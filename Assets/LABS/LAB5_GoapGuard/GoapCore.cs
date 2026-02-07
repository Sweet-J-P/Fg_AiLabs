using System;
using System.Collections.Generic;

public enum GoapFact
{
    SeesPlayer = 0,
    WeaponExists = 1,
    HasWeapon = 2, 
    AtWeapon = 3,
    AtPlayer = 4,
    PatrolStepDone = 5, 
    PlayerTagged = 6
}

public readonly struct GoapState : IEquatable<GoapState>
{
    public readonly ulong Bits;
    public GoapState(ulong bits) => Bits = bits;
    public bool Has(GoapFact f) => (Bits & (1UL << (int)f)) != 0;
    public GoapState With(GoapFact f) => new GoapState(Bits | (1UL << (int)f));
    public GoapState Without(GoapFact f) => new GoapState(Bits & ~(1UL << (int)f));
    public bool Satisfies(ulong goalMask) => (Bits & goalMask) == goalMask;
    public bool Equals(GoapState other) => Bits == other.Bits;
    public override bool Equals(object obj) => obj is GoapState s &&
                                               
    Equals(s);
        public override int GetHashCode() => Bits.GetHashCode();
}

public static class GoapBits
{
    public static ulong Mask(params GoapFact[] facts)
    {
        ulong m = 0;
        foreach (var f in facts) m |= 1UL << (int)f;
        return m;
    }
}

public sealed class GoapPlanResult
{
    public readonly List<GoapActionBase> Actions = new();
    public float TotalCost;
}

public static class GoapPlanner
{
    private struct CameFrom
    {
        public GoapState Prev;
        public GoapActionBase Action;
        public bool HasPrev;
    }
    
    // Dijkstra in state-space (small action sets => simple open list is fine)
    public static GoapPlanResult Plan(GoapState start, ulong
    goalMask, List<GoapActionBase> actions)
    {
        var open = new List<GoapState> { start };
        var cost = new Dictionary<GoapState, float> { [start] = 0f
    };
    var came = new Dictionary<GoapState, CameFrom>();
        while (open.Count > 0)
        {
            // pick lowest-cost state (O(n); OK for small demos)
            int bestIdx = 0;
            float bestCost = cost[open[0]];
            for (int i = 1; i < open.Count; i++)
            {
                float c = cost[open[i]];
                if (c < bestCost)
                {
                    bestCost = c; bestIdx = i; 
                }
            }
            var current = open[bestIdx];
            open.RemoveAt(bestIdx);
            if (current.Satisfies(goalMask))
                return Reconstruct(current, came, cost[current]);
                
            foreach (var a in actions)
            {
                if (!a.CanApplyTo(current)) continue;
                
                var next = a.ApplyTo(current);
                
                float newCost = cost[current] + a.cost;
                
                if (!cost.TryGetValue(next, out float old) || newCost < old) 
                {
                    cost[next] = newCost;
                    came[next] = new CameFrom { Prev = current,
                    Action = a, HasPrev = true };
                    if (!open.Contains(next)) open.Add(next);
                }
            }
        }
        return null;
    }
    private static GoapPlanResult Reconstruct(GoapState goalState,
    Dictionary<GoapState, CameFrom> came, float totalCost)
    {
        var result = new GoapPlanResult { TotalCost = totalCost };
        var cur = goalState;
        while (came.TryGetValue(cur, out var step) && step.HasPrev)
        {
            result.Actions.Add(step.Action);
            cur = step.Prev;
        }
        result.Actions.Reverse();
        return result;
    }
}
