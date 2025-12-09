using System;
using UnityEngine;

public class PlayerWorldAudio : MonoBehaviour
{
    [SerializeField] private AudioClip swordStrike;
    [SerializeField] private AudioClip playerDamaged;
    
    private AudioSource m_audioSource;
    private float m_lowVolume = 0.6f;
    private float m_highVolume = 1f;

    private void OnEnable()
    {
        //Play a sound when we are damaged
        //PlayerData.OnPlayerTakeDamage += OnDamaged;
    }

    private void OnDisable()
    {
        //Play a sound when we are damaged
        //PlayerData.OnPlayerTakeDamage -= OnDamaged;
    }

    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    public void OnSwordHit()
    {
        m_audioSource.PlayOneShot(swordStrike, m_lowVolume);
    }

    public void OnDamaged()
    {
        m_audioSource.PlayOneShot(playerDamaged, m_highVolume);
    }
}
