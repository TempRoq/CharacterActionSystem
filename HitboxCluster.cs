using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "ActionCreator/Cluster/Basic", order = 0)]
[System.Serializable]

public class HitboxCluster
{
    public enum HitboxClusterType
    {
        NORMAL,
        JUMP,
        DASH,
        INSTANTIATE,
        FORCE
    }


    public string Comments;
    public bool? test;
    [Space(20)]
    public HitboxClusterType clusterType;
    public Hitbox[] hitboxes; //all hitboxes that will be active while this cluster is active. Hitbox priority is based on how low the index of a hitbox is.
    [Space(30)]
    [Header("Attack Data")]
    [Space(5)]
    public int durationInFrames; //Assume 60fps
    public bool refreshHitList; //Will this cluster clear "alreadyHit" in the attackHandler? TRUE = yes (good for multihits), False = no (good for lingeringHitboxes)
    public float xOriginPointOffset; //how far in the X direction is the "core" of the move? The player must be blocking in this direction in order 
    [Space(30)]
    [Header("TYPE SPECIFIC VALUES")]
    [Space(5)]
    public Vector2[] vecRef;
    public int[] intRef;
    public bool[] boolRef;
    public float[] floatRef;
    [Space(30)]
    [Header("OTHER")]
    [Space(5)]
    public AudioClip sound;
    public MyEnums.Conditional checkForRekka;
    public MyEnums.Conditional setCancel;





    public void OnStart(GameObject self) //Called on the first frame a cluster is active
    {
        if (sound != null)
        {
            self.GetComponent<AudioSource>().PlayOneShot(sound);
        }

        switch (clusterType)
        {
            case HitboxClusterType.INSTANTIATE:
                ClSpawn.OnStart(self, intRef[0], vecRef[0]);
                break;
            case HitboxClusterType.JUMP:
                CLJump.OnStart(self, boolRef[0], boolRef[1], intRef[0]);
                break;
            case HitboxClusterType.FORCE:
                CLForce.OnStart(self, boolRef[0], vecRef[0], floatRef[0]);
                break;
            case HitboxClusterType.DASH:
                CLDash.OnStart(self, boolRef[0], floatRef[0]);
                break;
            default:
                break;

        }

    }

    public void OnUpdate(GameObject self) //Called every frame a cluster is active
    {
        switch (clusterType)
        {

            case HitboxClusterType.FORCE:
                CLForce.OnUpdate(self, boolRef[1], vecRef[0], floatRef[0]);
                break;
            case HitboxClusterType.DASH:
                CLDash.OnStart(self, boolRef[0], floatRef[0]);
                break;
            default:
                break;
        }
    }
}

public class ClSpawn
{
    public static void OnStart(GameObject self, int projectileHash, Vector2 localOffsetCoords)
    {
        GameObject g = self.GetComponent<Character>().GetInactiveInstance(projectileHash);
        g.GetComponent<ActionEmitter>().SetParent(self);
        g.GetComponent<Projectile>().ActivateProjectile(self.GetComponent<Character>().facingRight, self.GetComponent<Character>().transform.position + new Vector3(localOffsetCoords.x * (self.GetComponent<Character>().facingRight ? 1 : -1), localOffsetCoords.y));

    }
}

public class CLJump
{
    public static void OnStart(GameObject self, bool shortHop, bool doubleJump, int direction)
    {
       
        self.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        PlayableCharacter pc = self.GetComponent<PlayableCharacter>();


        Vector2 scl = new(
        (pc.facingRight ? 1 : -1) * direction * pc.VelocityAirMax,
        (shortHop ? pc.VelocityJumpShort : pc.VelocityJumpFull * (doubleJump ? pc.DoubleJumpMultiplier : 1))
        );
        self.GetComponent<Rigidbody2D>().velocity = scl;

    }
}

public class CLForce
{
    public static void OnStart(GameObject self, bool zeroOutMovement, Vector2 directionVector, float force)
    {

        if (zeroOutMovement)
        {
            self.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        Vector2 scl = new Vector2((self.GetComponent<PlayableCharacter>().facingRight ? 1 : -1), 1);
        self.GetComponent<Rigidbody2D>().velocity = directionVector * scl * force;

    }
    // Start is called before the first frame update
    public static void OnUpdate(GameObject self, bool continueForce, Vector2 directionVector, float force)
    {
        if (continueForce)
        {
            Vector2 scl = new Vector2((self.GetComponent<PlayableCharacter>().facingRight ? 1 : -1), 1);
            self.GetComponent<Rigidbody2D>().velocity = directionVector * scl * force;
        }
    }
}

public class CLDash
{

    //B[0] = GroundedDashSpeed?
    //F[0] = multiplier
    public static void OnStart(GameObject self, bool grounded, float multiplier)
    {
       
        Character c = self.GetComponent<Character>();
        c.BeginDash();
        Vector2 scl = new Vector2((c.facingRight ? 1f : -1f) * (grounded ? c.DashSpeedGround : c.DashSpeedAir) * multiplier, 0f);
        self.GetComponent<Rigidbody2D>().velocity = scl;

    }

    public static void OnUpdate(GameObject self, bool grounded, float multiplier)
    {
        Character c = self.GetComponent<Character>();
        Vector2 scl = new Vector2((c.facingRight ? 1f : -1f) * (grounded ? c.DashSpeedGround : c.DashSpeedAir) * multiplier, 0f);
        self.GetComponent<Rigidbody2D>().velocity = scl;
    }
}

