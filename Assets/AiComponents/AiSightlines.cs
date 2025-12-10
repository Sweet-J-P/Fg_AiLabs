using System;
using Player;
using UnityEngine;

public class AiSightlines : MonoBehaviour
{
    private PlayerController m_player;

    private void Awake()
    {
        m_player = FindAnyObjectByType<PlayerController>();
    }

    private void TracePlayer()
    {
        
    }
}
