using UnityEngine;
using UnityEngine.AI;

public enum GoapStatus { Running, Success, Failure }
public class GoapContext
{
    public NavMeshAgent Agent;
    public Transform Player;
    public Transform Weapon;
    public Transform[] PatrolWaypoints;
    public GuardSensors Sensors;
    public int PatrolIndex;
}

public abstract class GoapActionBase : MonoBehaviour
{
    [Header("GOAP (planner-visible)")]
    public string actionName = "Action";
    public float cost = 1f;
    
    // planner-visible masks (pre/add/del)
    public ulong preMask;
    public ulong addMask;
    public ulong delMask;
    
    // Runtime (procedural)
    public virtual bool CheckProcedural(GoapContext ctx) => true;
    public virtual void OnEnter(GoapContext ctx) { }
    public abstract GoapStatus Tick(GoapContext ctx);
    public virtual void OnExit(GoapContext ctx) { }
    
    // Planner helpers
    public bool CanApplyTo(GoapState s) => (s.Bits & preMask) == preMask;
    public GoapState ApplyTo(GoapState s)
    {
        ulong bits = s.Bits;
        bits &= ~delMask;
        bits |= addMask;
        return new GoapState(bits);
    }
}

