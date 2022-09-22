using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class ActionHandler : MonoBehaviour
{
    protected bool attackedThisFrame;

    public LayerMask targetsToHit;
    [HideInInspector]
    public bool currMoveFacingRight;
    Dictionary<int, HitboxCluster> frameToAction;
   
    protected Action currAction;

    protected HitboxCluster currCluster;

    protected int currentFrame;

    protected int framesRemaining;

    protected int numClustersRemaining = 0;
    [HideInInspector]
    public bool performingAction = false;
    [HideInInspector]
    public bool endLag = false;
    //public bool landedHit = false;
    [HideInInspector]
    public bool canCancel;
    [HideInInspector]
    public bool shouldCheckForRekkas = false;

    [NonSerialized]
    public bool drawGizmo = false;
    protected List<GameObject> alreadyHit;
    [HideInInspector]
    public UnityEvent OnStartAction;
    [HideInInspector]
    public UnityEvent OnFinishAction;

    [HideInInspector]
    public bool paused;
    protected AudioSource audioPlayer;

    [HideInInspector]
    public Action nextAction;
    [HideInInspector]
    public bool nextnewfacingright;

    [HideInInspector]
    public List<Action> airActions;
    public bool trackAirUses;
    public bool PerformingAction { get { return performingAction; } }
    public bool InStartup { get { return performingAction && currentFrame < currAction.clusterFrames[0]; } }
    public bool InEndLag {  get { return endLag; } }

    // Start is called before the first frame update
    void Start()
    {
        alreadyHit = new List<GameObject>();
        frameToAction = new Dictionary<int, HitboxCluster>();
        audioPlayer = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        frameToAction = new Dictionary<int, HitboxCluster>();
    }




    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (attackedThisFrame)
        {
            attackedThisFrame = false;
            return;
        }
        DoMainStuff();
    }
    public void EndAction()
    {
        if (performingAction)
        {
          
            //print(currAction.name + "<== current Action");
            OnFinishAction.Invoke();
            
        }

        performingAction = false;

        if (currAction != null && currentFrame < currAction.attackDuration)
        {
            endLag = true;
        }
        else
        {
            endLag = false;
        }
        //landedHit = false;
        //canCancel = false;

    }
    public void PerformAction(Action a, bool newfacingRight)
    {
        if (paused)
        {
            nextAction = a;
            nextnewfacingright = newfacingRight;
            return;
        }

        if (trackAirUses && (a.type == Action.MoveType.MISCELLANEOUS || a.IsSpecialMove))
        {
            if (!CanBeUsed(a))
            {
              //  Debug.Log("Action " + a.name + " exceeded max air uses");
                return;
            }
            else
            {
                airActions.Add(a);
            }
        }

        canCancel = false;
        

        shouldCheckForRekkas = false;
        frameToAction.Clear();
        alreadyHit.Clear();
        for (int i = 0; i < a.clusters.Length; i++) {
            frameToAction.Add(a.clusterFrames[i], a.clusters[i]);
        }
        currAction = a;
        numClustersRemaining = a.clusters.Length;
        currMoveFacingRight = newfacingRight;
        performingAction = true;
        endLag = false;
        currentFrame = 0;
        currAction.OnInvoked(gameObject);
        framesRemaining = 0;
        attackedThisFrame = true;
        OnStartAction.Invoke();
        DoMainStuff();
       
    }
    public bool CanBeUsed(Action a)
    {
        if (a.limits.usesInAir == -1)
        {
            return true;
        }
        int count = 0;
        for (int i = 0; i < airActions.Count; i++)
        {
            if (airActions[i] == a)
            {
                count += 1;
            }
        }
        if (count >= a.limits.usesInAir)
        {
            return false;
        }

        return true;
    }
    public void TrackAirUses()
    {
        trackAirUses = true;
    }

    public void StopTrackAirUses()
    {
        trackAirUses = false;
        airActions.Clear();
    }


    public void UpdateInfoFrame(int frame)
    {
        if (frameToAction.TryGetValue(frame, out HitboxCluster hbc))
        {
            currCluster = hbc;
            framesRemaining = hbc.durationInFrames;
            numClustersRemaining -= 1;
            if (hbc.refreshHitList)
            {
                alreadyHit.Clear();
            }
            if (hbc.checkForRekka != MyEnums.Conditional.IGNORE)
            {
                shouldCheckForRekkas = hbc.checkForRekka == MyEnums.Conditional.TRUE;
            }
            if (hbc.setCancel != MyEnums.Conditional.IGNORE)
            {
                canCancel = hbc.setCancel == MyEnums.Conditional.TRUE;
            }
            currCluster.OnStart(gameObject);
        }
    }
    public void BuildHitboxes() //Place the hitboxes into the scene and see if they interact with anything 
    {
        drawGizmo = true;
        currCluster.OnUpdate(gameObject);
        // Debug.Log("BUILDING HITBOXES! TIME.TIME = " + Time.time + ". CURRENT ACTION = " + currAction.name + ". Current Frame = " + currentFrame);
        // StartCoroutine(DEBUGPAUSEENDFRAME());

        foreach (Hitbox h in currCluster.hitboxes)
        {
            Vector3 additional = h.offsetFromAnchor;

            Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + new Vector3(h.offsetFromAnchor.x * (currMoveFacingRight ? 1 : -1), h.offsetFromAnchor.y), h.dimensions, 0f, targetsToHit);      
            foreach (Collider2D c in cols)
            {
                GameObject g = c.transform.root.gameObject;
                try
                {
                    if (!alreadyHit.Contains(g))
                    {
                        g.GetComponent<HitboxReceiver>().TakeHit(gameObject, currMoveFacingRight, h, transform.position.x + (currCluster.xOriginPointOffset * (currMoveFacingRight ? 1 : -1)));
                        alreadyHit.Add(g);                     
                        h.OnHit(gameObject, g);
                        if (h.checkRekkaOnHit && currAction.rekkas.Length > 0)
                        {
                            shouldCheckForRekkas = true;
                            Debug.Log("AAA");
                        }
                        audioPlayer.PlayOneShot(h.GetSFXHit());
                        //if (h.HitSFX != null) audioPlayer.PlayOneShot(h.HitSFX);
                        //landedHit = true;

                        //canCancel = h.setCancel;
                    }
                }
                catch (Exception)
                {

                    Debug.LogError("GameObject " + g.name + " has the wrong Layer!");
                }
            }
        }
        framesRemaining -= 1;
        if (framesRemaining == 0 && numClustersRemaining == 0)
        {
           // EndAction();
        }
    }
    public void CancelAction()
    {
        EndAction();
        endLag = false;
        canCancel = false;
        drawGizmo = false;  
    }
    public Action GetCurrentAction()
    {
        return currAction;
    }
    public bool CanCheckForRekkas()
    {
        return (performingAction && currAction.rekkas.Length > 0 && shouldCheckForRekkas);
    }
    public bool CheckForRekkas(out Rekka r, bool facingRight, bool isGrounded, Vector2 stickDirectionHeld, MyEnums.ButtonInput b, int rekkaToCheck)
    {
        r = null;
        if (currAction.rekkas[rekkaToCheck].ConditionMatched(facingRight, isGrounded, stickDirectionHeld, b))
        {

            r = currAction.rekkas[rekkaToCheck];

            return true;

        }

        return false;
    }
    private void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            foreach(Hitbox h in currCluster.hitboxes)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + new Vector3(h.offsetFromAnchor.x * (currMoveFacingRight ? 1 : -1), h.offsetFromAnchor.y), h.dimensions);
                Gizmos.color = new Color(1f, .2f, .2f, .25f);
                Gizmos.DrawCube(transform.position + new Vector3(h.offsetFromAnchor.x * (currMoveFacingRight ? 1 : -1), h.offsetFromAnchor.y), h.dimensions);

                Gizmos.color = Color.white;

                if (h.knockbackAngle <= 360f)
                {
                    float direc = currMoveFacingRight ? 1 : -1;
                    Vector2 line = Vector2.Perpendicular(new Vector2(Mathf.Cos(h.knockbackAngle * Mathf.Deg2Rad), Mathf.Sin(h.knockbackAngle * Mathf.Deg2Rad)).normalized);
                    if (!currMoveFacingRight)
                    {
                        line = Vector2.Perpendicular(line);
                    }



                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 startPos = transform.position + Vector3.Scale(h.offsetFromAnchor, new Vector3(direc, 1)) + ((i - 3f) * .005f * (Vector3)line);
                        Vector2 endPos = transform.position + Vector3.Scale(h.offsetFromAnchor, new Vector3(direc, 1)) + 
                            new Vector3(Mathf.Cos(h.knockbackAngle * Mathf.Deg2Rad) * direc, Mathf.Sin(h.knockbackAngle * Mathf.Deg2Rad)) * (h.knockbackPower / 75f)
                            + ((Vector3)line * .005f);
                        Gizmos.DrawLine(startPos, endPos);
                    }
                }

            }
        }
    }
    public void DoMainStuff()
    {
        if (paused)
        {
            return;
        }
        if (endLag)
        {
            if (currentFrame != currAction.attackDuration)
            {
                currentFrame += 1;
                if (currentFrame == currAction.attackDuration)
                {
                    endLag = false;
                }
            }
        }

        if (performingAction)
        {
            if (numClustersRemaining > 0)
            {
                UpdateInfoFrame(currentFrame);
            }

            if (framesRemaining <= 0)
            { //if, even after that, the number of frames remaining is still less than or equal to 0, do not show gizmos. if there are clusters remaining, currCluster is null. 
                drawGizmo = false;
                currCluster = null;
                if (numClustersRemaining == 0)
                {
                    EndAction();
                }
            }
            else //if there are frames remaining, just build the damn hitboxes.
            {
                BuildHitboxes();
            }
            currentFrame += 1;

        }
       
       
        
    }
    public void Pause()
    {
        paused = true;
    }
    public void Play()
    {
        paused = false;
        if (nextAction != null)
        {
            PerformAction(nextAction, nextnewfacingright);
            nextAction = null;
        }
        attackedThisFrame = true;

    }

    public IEnumerator DEBUGPAUSEENDFRAME()
    {
        yield return new WaitForEndOfFrame();
        Debug.Break();
    }

    public void SetCancel(bool b)
    {
        canCancel = b;
    }
}
