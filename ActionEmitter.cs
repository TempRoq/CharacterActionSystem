using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
public class ActionEmitter : MonoBehaviour
{
    protected bool attackedThisFrame;
    public LayerMask targetsToHit; //What layers can this attacker hit
    Dictionary<int, HitboxCluster> frameToAction;

    public GameObject parentObject = null;
    public Action currAction;
    [HideInInspector]
    protected HitboxCluster currCluster;
    [SerializeField]
    protected int currentFrame;
    [SerializeField]
    protected int framesRemaining;
    [SerializeField]
    protected int numClustersRemaining = 0;




    public bool drawGizmo = false;
    protected List<GameObject> alreadyHit;

    public UnityEvent OnStartAction;
    public UnityEvent OnFinishAction;

    public bool paused;
    //protected AudioSource audioPlayer;

    public bool facingRight;
    public UnityEvent OnLandedHit;

    private void Start()
    {
        
        alreadyHit = new List<GameObject>();
        frameToAction = new Dictionary<int, HitboxCluster>();
        SetupAction();
    }

    public void BeginEmission(bool newRight)
    {
        Debug.Log("BE1");

        if (alreadyHit == null)
        {
            alreadyHit = new List<GameObject>();
        }
        alreadyHit.Clear();
        facingRight = newRight;
        Debug.Log("BE2");
        DoActionFromStart();
    }

    public void SetupAction()
    {
        frameToAction.Clear();
        alreadyHit.Clear();
        for (int i = 0; i < currAction.clusters.Length; i++)
        {
            frameToAction.Add(currAction.clusterFrames[i], currAction.clusters[i]);
        }
    }

    public void DoActionFromStart()
    {
        numClustersRemaining = currAction.clusters.Length;
        currentFrame = 0;
        currAction.OnInvoked(gameObject);
        framesRemaining = 0;
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
  

        UpdateInfoFrame(currentFrame);
        if (framesRemaining <= 0)
        {  
            drawGizmo = false;
            currCluster = null;
        }
        else //if there are frames remaining, just build the damn hitboxes.
        {
            BuildHitboxes();
        }
        currentFrame += 1;

        if (currentFrame >= currAction.attackDuration)
        {
            DoActionFromStart();
        }

    }

    void BuildHitboxes()
    {
        drawGizmo = true;
        currCluster.OnUpdate(gameObject);
        foreach (Hitbox h in currCluster.hitboxes)
        {
            Vector3 additional = h.offsetFromAnchor;

            Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + new Vector3(h.offsetFromAnchor.x * (facingRight ? 1 : -1), h.offsetFromAnchor.y), h.dimensions, 0f, targetsToHit);
            foreach (Collider2D c in cols)
            {
                GameObject g = c.transform.root.gameObject;
                try
                {
                    if (!alreadyHit.Contains(g))
                    {
                        g.GetComponent<HitboxReceiver>().TakeHit(gameObject, facingRight, h, transform.position.x + (currCluster.xOriginPointOffset * (facingRight ? 1 : -1)));
                        alreadyHit.Add(g);
                        h.OnHit(parentObject, g);
                        OnLandedHit.Invoke();
                        //if (h.HitSFX != null) audioPlayer.PlayOneShot(h.HitSFX);;
                    }
                }
                catch (Exception)
                {

                    Debug.LogError("GameObject " + g.name + " has the wrong Layer!");
                }
            }
        }
        framesRemaining -= 1;
    }



    public void SetParent(GameObject g)
    {
        parentObject = g;
    }


    void UpdateInfoFrame(int frame)
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
            currCluster.OnStart(gameObject);
        }
    }




    private void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            foreach (Hitbox h in currCluster.hitboxes)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position + new Vector3(h.offsetFromAnchor.x * (facingRight ? 1 : -1), h.offsetFromAnchor.y), h.dimensions);
                Gizmos.color = new Color(1f, .2f, .2f, .25f);
                Gizmos.DrawCube(transform.position + new Vector3(h.offsetFromAnchor.x * (facingRight ? 1 : -1), h.offsetFromAnchor.y), h.dimensions);

            }
        }
    }


}
