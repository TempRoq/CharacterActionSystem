using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif


[CreateAssetMenu (menuName = "Action")]
public class Action : ScriptableObject
{
    [System.Serializable]
    public struct PerformLimits
    {
        public int meterCost;
        public int usesInAir;
    }
    [System.Serializable]
    public struct ActionInfo
    {
        public bool stopUser;
        public bool canMoveDuring;
        public bool negateGravity;
        public bool restoreGravityAfter;
        public bool endOnLanding;
        public bool skipAnimCall;
    }
    public enum MoveType
    {
        LIGHT = 0,
        MEDIUM = 1,
        HEAVY = 2,
        ASSIST = 3,
        REKKA = 4,
        MISCELLANEOUS = 5
    }
    public enum MoveInput
    {
        NEUTRAL = 0,
        DOWN = 3,
        FORWARD = 6,
        QCF = 9,
        HCF = 12,
        DP = 15,
        SUPER = 18,
    }


    public MoveType type;
    public MoveInput input;
    public HitboxCluster[] clusters; //All clusters. Order matters.
    // The frame (assuming 60fps) when a cluster at the same index in clusters is called
    public int[] clusterFrames; 
    //How long from the beginning of the move until the player can move again in frames?
    public int attackDuration;
    public int landingLag;
    public Rekka[] rekkas;
    public ActionInfo info;
    public PerformLimits limits;

    public bool IsSpecialMove { get { return input >= MoveInput.QCF && input != MoveInput.SUPER; } }
    public bool IsSuper {  get { return input == MoveInput.SUPER; } }
    public int MeterCost { get { return limits.meterCost; } }
    public virtual void OnInvoked(GameObject self) //Attack handler calls this on the first frame that the move is active (Frame 0)
    {
        

    }
    public bool CancelsInto(Action a, bool canMovementCancel, out bool willMovementCancel, bool canSpecialToSpecialCancel, out bool willSpecialToSpecialCancel, bool canReverseBeat, out bool willReverseBeat)
    {
        willSpecialToSpecialCancel = false;
        willReverseBeat = false;
        willMovementCancel = false;
        if (a.type == MoveType.MISCELLANEOUS) //If the move is able to be movement cancelled and this is a movement cancel move, then you're chilling!
        {
            willMovementCancel = true;
            return canMovementCancel;
        }

        if (IsSuper) //Supers can't cancel into other supers (yet...)
        {
            return false;
        }
        if (a.IsSuper) //Going to Super from something that isn't super is always True
        {
            return true;
        }

        if (IsSpecialMove && a.IsSpecialMove) //If this is a special move and there is no special to special cancel, then there is nothing to link to. Beyond this, IsSpecialMove is always false
        {
            willSpecialToSpecialCancel = true;
            return canSpecialToSpecialCancel;
        }

        if (!a.IsSpecialMove && IsSpecialMove)
        {
            willReverseBeat = true;
            return canReverseBeat;

        }
        if (a.IsSpecialMove) // if this is not a special move and a is a special move, then you are free to act, as that is n-> s cancelling. Beyond here, a cannot be a special move. Both are normals
        {
            return true;
        }

        /*
        if (a.type == MoveType.LIGHT && type == MoveType.LIGHT)
        {
            return true;
        }
        */
        if (a.type <= type) //If Trying to reverse beat
        {

            willReverseBeat = true;
            return canReverseBeat;
        }

        if (a.type > type) //L->M->H is cancellable.
        {
            return true;
        }

        return false; //Lights Cancel into Lights is the rule of the wild west
    }



}
