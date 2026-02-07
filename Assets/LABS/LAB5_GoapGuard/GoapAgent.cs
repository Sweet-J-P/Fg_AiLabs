using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
public class GoapAgent : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private GuardSensors sensors;
    [SerializeField] private Transform player;
    [SerializeField] private Transform weaponPickup;
    [SerializeField] private Transform[] patrolWaypoints;
    
    [Header("Debug")]
    [SerializeField] private bool logPlans = true;
    
    [Header("Planning")]
    [Tooltip("Minimum seconds between replans (prevents spam when facts flicker).")]
    [SerializeField] private float minSecondsBetweenReplans = 0.20f;
    
    private float m_nextAllowedReplanTime = 0f;
    private NavMeshAgent m_agent;
    private GoapContext m_ctx;
    private List<GoapActionBase> m_allActions;
    private Queue<GoapActionBase> m_plan;
    private GoapActionBase m_currentAction;
    // “Owned” facts: memory/execution facts (e.g., HasWeapon, AtWeapon, AtPlayer, PatrolStepDone, PlayerTagged)
    // Sensor/world facts (SeesPlayer, WeaponExists) are refreshed each tick.
    private ulong m_ownedFactsBits = 0;
    void Awake()
    {
        m_agent = GetComponent<NavMeshAgent>();
        
        m_ctx = new GoapContext
        {
            Agent = m_agent,
            Player = player,
            Weapon = weaponPickup,
            PatrolWaypoints = patrolWaypoints,
            Sensors = sensors,
            PatrolIndex = 0
        };
        
        m_allActions = new
        List<GoapActionBase>(GetComponents<GoapActionBase>());
    }
    void Update()
    {
        GoapState current = BuildCurrentState();
        ulong goalMask = SelectGoalMask(current);
        // If we have no plan, request one (throttled).
        if ((m_plan == null || m_plan.Count == 0) && Time.time >= m_nextAllowedReplanTime)
        {
            MakePlan(current, goalMask);
        }
        if (m_plan == null || m_plan.Count == 0) return;
        // Start next action if needed
        if (m_currentAction == null)
        {
            m_currentAction = m_plan.Dequeue();
            // Procedural check at runtime (not planner-visible)
            if (!m_currentAction.CheckProcedural(m_ctx))
            {
                InvalidatePlan(throttle: true);
                return;
            }
            m_currentAction.OnEnter(m_ctx);
        }
        var status = m_currentAction.Tick(m_ctx);
        if (status == GoapStatus.Running) return;
        
        if (status == GoapStatus.Success)
        {
            // Apply effects only on success
            ApplyActionEffectsToOwnedFacts(m_currentAction);
            m_currentAction.OnExit(m_ctx);
            m_currentAction = null;
            return;
        }
        
        // Failure: action did not complete; invalidate and replan (throttled)
        m_currentAction.OnExit(m_ctx);
        m_currentAction = null;
        InvalidatePlan(throttle: true);
    }
    private GoapState BuildCurrentState()
    {
        ulong bits = m_ownedFactsBits;
        
        // Determine owned HasWeapon first (used to interpret WeaponExists as "pickup available to THIS agent")
        bool hasWeapon = (bits & GoapBits.Mask(GoapFact.HasWeapon)) != 0;
        
        // Sensor-driven fact (fresh each tick)
        if (sensors != null && sensors.SeesPlayer) bits |= GoapBits.Mask(GoapFact.SeesPlayer);
        else bits &= ~GoapBits.Mask(GoapFact.SeesPlayer);
        
        // World-driven fact (fresh each tick)
        // Interpret WeaponExists as "pickup available to pick up".
        // If the agent already has a weapon, treat WeaponExists as false for planning purposes.
        bool pickupActive = weaponPickup != null && weaponPickup.gameObject.activeInHierarchy;
        bool weaponAvailable = pickupActive && !hasWeapon;
        
        if (weaponAvailable) bits |= GoapBits.Mask(GoapFact.WeaponExists);
        else bits &= ~GoapBits.Mask(GoapFact.WeaponExists);
        
        return new GoapState(bits);
    }
    private ulong SelectGoalMask(GoapState current)
    {
        // Simple rule set:
        // - If player seen: goal is to tag the player
        // - Else: goal is to complete one patrol step
        if (current.Has(GoapFact.SeesPlayer))
            return GoapBits.Mask(GoapFact.PlayerTagged);
        
        return GoapBits.Mask(GoapFact.PatrolStepDone);
    }
    
    private void MakePlan(GoapState current, ulong goalMask)
    {
        var res = GoapPlanner.Plan(current, goalMask, m_allActions);
        if (res == null)
        {
            if (logPlans) Debug.LogWarning("GOAP: No plan found.");
                m_plan = null;
                
            return;
        }
        
        m_plan = new Queue<GoapActionBase>(res.Actions);
        
        if (logPlans)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"GOAP Plan (cost {res.TotalCost:0.0}):");
            foreach (var a in res.Actions) sb.AppendLine($"-{a.actionName} (cost {a.cost:0.0})");
            Debug.Log(sb.ToString());
        }
    }
    
    private void InvalidatePlan(bool throttle)
    {
        m_plan = null;
        m_currentAction = null;
        if (throttle)
            m_nextAllowedReplanTime = Time.time + minSecondsBetweenReplans;
    }
    
    private void ApplyActionEffectsToOwnedFacts(GoapActionBase a)
    {
        m_ownedFactsBits &= ~a.delMask;
        m_ownedFactsBits |= a.addMask;
    }
}
