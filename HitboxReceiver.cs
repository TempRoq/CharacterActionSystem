  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class HitboxReceiver : MonoBehaviour
{
    public UnityEvent OnHit;

    public bool infiniteHealth;
    public bool takesHitstun;
    public bool takesKnockback;

    public struct AttackerInfo {
        public GameObject attacker;
        public Hitbox justHitBy;
        public bool attackerFaceRight;
        public float attackCoreX;
    }
    public AttackerInfo HitInfoReceived;

    // Start is called before the first frame update
    void Start()
    {
        if (OnHit == null)
        {
            OnHit = new UnityEvent();
        }
    }

    public virtual void TakeHit(GameObject hitter, bool enFaceRight, Hitbox h, float xOriginPointOffset)
    {
        HitInfoReceived.attacker = hitter;
        HitInfoReceived.justHitBy = h;
        HitInfoReceived.attackerFaceRight = enFaceRight;
        HitInfoReceived.attackCoreX = xOriginPointOffset;
        OnHit.Invoke();
    }

   
    

    
}
