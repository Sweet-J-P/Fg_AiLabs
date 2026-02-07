using UnityEngine;
public class GuardSensors : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private Transform player;
    [SerializeField] private float viewRange = 10f;
    [SerializeField] private LayerMask occluders = ~0; // everything by default
    [SerializeField] private bool useLineOfSightRaycast = true;
    
    //Public
    public bool SeesPlayer { get; private set; }
    
    void Update()
    {
        SeesPlayer = false;
        
        if (player == null) 
            return;
        
        float dist = Vector3.Distance(transform.position, player.position);
        
        if (dist > viewRange)
            return;
        
        if (!useLineOfSightRaycast)
        {
            SeesPlayer = true;
            return;
        }
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;
        Vector3 dir = (target - origin);
        float len = dir.magnitude;
        
        if (len < 0.001f) { SeesPlayer = true; return; }
        
        if (Physics.Raycast(origin, dir / len, out RaycastHit hit, len, occluders))
        {
            // Sees player only if the first thing hit is the player
            if (hit.transform == player) SeesPlayer = true;
        }
    }
}
