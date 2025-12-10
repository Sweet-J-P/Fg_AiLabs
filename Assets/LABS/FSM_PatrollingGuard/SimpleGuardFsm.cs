using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleGuardFsm : MonoBehaviour
{
    public enum SimpleGuardStates
    {
        Idle,
        Patrol,
        Chase,
        Return
    }
    
    public delegate void SimpleGuardStateChange(SimpleGuardStates newState);
    public static event SimpleGuardStateChange OnSimpleGuardStateChange;
    
    //Members
    private SimpleGuardStates m_currentState = SimpleGuardStates.Idle;
    private PlayerController m_playerController;
    private NavMeshAgent m_navMeshAgent;
    private float m_distanceToPlayer;
    private Transform m_currentPatrolTarget;
    private int m_currentPatrolPointIndex = 0;
    private bool m_isIdling;
    private float m_timeSinceLastPause;
    
    //Editor exposed
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float alertDistance = 5f;
    [SerializeField] private float stopChaseDistance = 7f;
    [SerializeField] private float patrolStoppingDistance = 1f;
    [SerializeField] private float chaseStoppingDistance = 2.2f;
    [SerializeField] private float timeBetweenPauses = 20f;


    private void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_playerController = FindAnyObjectByType<PlayerController>();
    }

    private void Start()
    {
        m_navMeshAgent.speed = patrolSpeed;
        m_currentPatrolPointIndex = FindClosestPatrolPointIndex();
        ChangeState(SimpleGuardStates.Return);
    }

    void Update()
    {
        CheckDistanceToPlayer();
        
        switch (m_currentState)
        {
            case SimpleGuardStates.Idle:
                DoIdleAction();
                break;
            case SimpleGuardStates.Patrol:
                DoPatrolAction();
                break;
            case SimpleGuardStates.Chase:
                DoChaseAction();
                break;
            case SimpleGuardStates.Return:
                DoReturnAction();
                break;
        }
    }

    private int FindClosestPatrolPointIndex()
    {
        float shortestDistance = Mathf.Infinity;
        int closestPointIndex = 0;
        
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distanceToPoint = Vector3.Distance(transform.position, patrolPoints[i].position);
            
            if (distanceToPoint < shortestDistance)
            {
                shortestDistance = distanceToPoint;
                closestPointIndex = i;
            }
        }
        return closestPointIndex;
    }

    private void CheckDistanceToPlayer()
    {
        m_distanceToPlayer = Vector3.Distance(transform.position, m_playerController.transform.position);
    }
    
    private void ChangeState(SimpleGuardStates newState)
    {
        m_currentState = newState;
        print("Entered new state: " + newState);
        OnSimpleGuardStateChange?.Invoke(newState);
    }
    
    private void DoIdleAction()
    {
        
        if (m_distanceToPlayer < alertDistance)
        {
            ChangeState(SimpleGuardStates.Chase);
            StopAllCoroutines();
            return;
        }
        if(m_isIdling) return; //Prevent multiple coroutines from being started (if the guard is already idling)
        StartCoroutine(PauseAgentAction(Random.Range(1.8f, 3.2f), SimpleGuardStates.Patrol));
        m_isIdling = true;
    }
    
    private void DoPatrolAction()
    {
        if (m_distanceToPlayer < alertDistance)
        {
            ChangeState(SimpleGuardStates.Chase);
            m_currentPatrolTarget = null;
            return;
        }
        
        m_timeSinceLastPause += Time.deltaTime;
        
        m_navMeshAgent.speed = patrolSpeed;
        m_navMeshAgent.stoppingDistance = patrolStoppingDistance;
        if(m_currentPatrolTarget == null)
            m_currentPatrolTarget = patrolPoints[FindClosestPatrolPointIndex()];
        
        m_navMeshAgent.SetDestination(m_currentPatrolTarget.position);
        m_navMeshAgent.isStopped = false;
        
        if (!m_navMeshAgent.pathPending && !m_navMeshAgent.isStopped && m_navMeshAgent.remainingDistance < m_navMeshAgent.stoppingDistance)
        {
            if (m_timeSinceLastPause > timeBetweenPauses)
            {
                m_timeSinceLastPause = 0f;
                ChangeState(SimpleGuardStates.Idle);
                return;
            }
            
            m_currentPatrolPointIndex = (m_currentPatrolPointIndex + 1) % patrolPoints.Length;
            m_currentPatrolTarget = patrolPoints[m_currentPatrolPointIndex];
        }
    }
    
    private void DoChaseAction()
    {
        if (m_distanceToPlayer > stopChaseDistance)
        {
            ChangeState(SimpleGuardStates.Return);
            return;
        }
        
        m_navMeshAgent.stoppingDistance = chaseStoppingDistance;
        m_navMeshAgent.speed = chaseSpeed;
        m_navMeshAgent.SetDestination(m_playerController.transform.position);

        if (m_navMeshAgent.remainingDistance > m_navMeshAgent.stoppingDistance)
        {
            m_navMeshAgent.isStopped = false;
        }
        else
        {
            m_navMeshAgent.isStopped = true;
        }

    }
    
    private void DoReturnAction()
    {
        if (m_isIdling) return;
        StartCoroutine(PauseAgentAction(2f, SimpleGuardStates.Patrol));
        m_isIdling = true;
    }

    IEnumerator PauseAgentAction(float duration, SimpleGuardStates stateAfterIdle)
    {
        m_navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(duration);
        m_navMeshAgent.isStopped = false;
        ChangeState(stateAfterIdle);
        m_isIdling = false;
    }
}
