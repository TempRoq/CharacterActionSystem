using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HitboxEditingVisualizer : MonoBehaviour
{
    protected ActionHandler ah;
    public Action currentAction;
    public bool displayHitboxes;
    public int clusterNumber;
    public int hitboxNumber;

    public bool WireFrame = true;


    // Start is called before the first frame update
    void Start()
    {
        ah = GetComponent<ActionHandler>();
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAction != null)
        {
            clusterNumber = Mathf.Clamp(clusterNumber, 0, currentAction.clusters.Length - 1);
            hitboxNumber = Mathf.Clamp(hitboxNumber, -1, currentAction.clusters[clusterNumber].hitboxes.Length - 1);
        }
        else
        {
            displayHitboxes = false;
        }
        
    }

    private void OnDrawGizmos()
    {

        if (displayHitboxes && currentAction != null && currentAction.clusters[clusterNumber].hitboxes.Length != 0)
        {

            if (hitboxNumber == -1)
            {
                foreach (Hitbox h in currentAction.clusters[clusterNumber].hitboxes)
                {
                    Gizmos.color = Color.red;
                    if (WireFrame)
                    {
                        Gizmos.DrawWireCube(transform.position + new Vector3(h.offsetFromAnchor.x , h.offsetFromAnchor.y), h.dimensions);
                    }
                    else
                    {
                        Gizmos.DrawWireCube(transform.position + new Vector3(h.offsetFromAnchor.x, h.offsetFromAnchor.y), h.dimensions);
                        Gizmos.color = new Color(1f, .2f, .2f, .25f);
                        Gizmos.DrawCube(transform.position + new Vector3(h.offsetFromAnchor.x, h.offsetFromAnchor.y), h.dimensions);
                    }
                }
            }
            else
            {              
                Gizmos.color = Color.red;
                Hitbox h = currentAction.clusters[clusterNumber].hitboxes[hitboxNumber];
                if (WireFrame)
                {
                    Gizmos.DrawWireCube(transform.position + new Vector3(h.offsetFromAnchor.x, h.offsetFromAnchor.y), h.dimensions);
                }
                else
                {
                    Gizmos.DrawWireCube(transform.position + new Vector3(h.offsetFromAnchor.x, h.offsetFromAnchor.y), h.dimensions);
                    Gizmos.color = new Color(1f, .2f, .2f, .25f);
                    Gizmos.DrawCube(transform.position + new Vector3(h.offsetFromAnchor.x, h.offsetFromAnchor.y), h.dimensions);
                }
                Gizmos.color = Color.white;

                if (h.knockbackAngle <= 360f)
                {
                    Vector2 line = Vector2.Perpendicular(new Vector2(Mathf.Cos(h.knockbackAngle * Mathf.Deg2Rad), Mathf.Sin(h.knockbackAngle * Mathf.Deg2Rad)).normalized);


                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 startPos = transform.position + (Vector3)h.offsetFromAnchor + ((Vector3)line * .005f * (i-2f));
                        Vector2 endPos = transform.position + (Vector3)h.offsetFromAnchor + (new Vector3(Mathf.Cos(h.knockbackAngle * Mathf.Deg2Rad), Mathf.Sin(h.knockbackAngle * Mathf.Deg2Rad)) * (h.knockbackPower / 75f)) + ((Vector3)line * .005f);
                        Gizmos.DrawLine(startPos, endPos);
                    }
                }
                else
                {
                    //
                }
            }
        }
    }
}