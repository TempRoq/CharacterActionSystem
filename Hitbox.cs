using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All knockback and positions assume that the player is facing right.
[System.Serializable]
public class Hitbox
{
    #region Data Structures
    public enum Knockdown
    {
        NONE = 0,
        SOFT = 1,
        HARD = 2
    }

    public enum Launcher
    {
        FALSE = 0,
        TRUE = 1,
        TRUE_FORCE = 2
    }

    public enum StageInteraction
    {
        NONE = 0,
        WALLBOUNCE = 1,
        WALLSPLAT = 2,
        GROUNDBOUNCE = 3,
        GROUNDSPLAT = 3,   
    }

    public enum BaseSound
    {
        BLUNT = 0,
        SLASHING = 1,
        PIERCING = 2,
        SLAM = 3,
        FIRE = 4,
        TEST = 5
    }
    #endregion

    [Header("LOCATION")]
    public Vector2 dimensions;
    public Vector2 offsetFromAnchor;

    [Space]
    [Header("DAMAGE")]
    public int damage;
    public int chipDamage;

    [Space]
    [Header("KNOCKBACK")]
    [Tooltip("361 = Auto Link Angle")]
    public float knockbackAngle;
    public float knockbackPower;
    public float knockbackPowerOnBlock;
    [Space]
    [Header("STUN")]
    public int framesHitstun;
    public int framesBlockstun;

    [Space]
    [Header("HITSTOP")]
    public int framesHitstop;
    public int framesHitstopBl;

    [Header("HITBOX INTERACTIONS")]

    public Launcher launcher;
    public StageInteraction interaction;
    [Space(10)]
    public bool blockable;
    public bool techable = true;
    public MyEnums.HitPosition hitPosition;
    //REMOVE TECHING, ADD LIMIT FOR LAUNCHERS!
    // public float bounceForce; //FIND ME AS A FUNCTION OF VELOCITY!

    [Space]
    [Header("EFFECTS")]
    public float shakeStrength;
    public int meterReward;
    public BaseSound sound;
    public uint soundVariation;

    [Header("Other")]
    
    public bool setCancel = true;
    public bool checkRekkaOnHit;
  

    public bool IsLauncher { get { return launcher > Launcher.FALSE; } }
   

    public virtual void OnHit(GameObject self, GameObject target) //Called when a hitbox successfully connects against a target. NOTE: Will not be called if the hit target is in "alreadyHit".
    {
        if (setCancel)
        {
            self.GetComponent<ActionHandler>().SetCancel(true);
        }
        try
        {
            self.GetComponent<Character>().ChangeMeter(meterReward);
        }
        catch
        {
            Debug.Log("No character component on " + self.name);
        }
       
    }


    public AudioClip GetSFXHit()
    {
        return Resources.Load<AudioClip>("SFX/" + sound.ToString() + "_" + soundVariation);
    }

    public AudioClip GetSFXBlock()
    {
        return Resources.Load<AudioClip>("SFX/" + sound.ToString() + "_B" );
    }
}
