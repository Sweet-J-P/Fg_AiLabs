using System;
using System.Collections;
using Input;
using Tiny;
using UnityEngine;

//Add DOTween package to use this, is then used to move the camera to death position
//using DG.Tweening;

namespace Player
{ 
    public class PlayerController : MonoBehaviour
    {
        public delegate void DeathCamMoved();
        public static event DeathCamMoved OnDeathCamMoved;
        
        //Animator parameters
        private static readonly int NormalAttack = Animator.StringToHash("Attack");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int GravityIntensity = Animator.StringToHash("GravityIntensity");
        private static readonly int AttackMomentum = Animator.StringToHash("AttackMomentum");
        private static readonly int AttackTag = Animator.StringToHash("Attack");
        private static readonly int JumpTag = Animator.StringToHash("Jump");
        private static readonly int Died = Animator.StringToHash("Died");

        //Editor editable
        [Tooltip("How fast we can jump after landing, in seconds")] [SerializeField] private float jumpFrequency = 0.4f;
        [Tooltip("Downward force intensity")] [SerializeField] private float gravityIntensity = -6f;
        [Tooltip("How fast the player rotates")] [SerializeField] private float rotationSpeed = 10f;
        [Tooltip("The camera used by the player")] [SerializeField] private Camera playerCamera;
        [SerializeField] Trail trail;
        [SerializeField] private Transform deathCamTransform;
        
        //Privates
        private Animator m_animator;
        private PlayerInputHandler m_PlayerInputHandler;
        private CharacterController m_characterController;
        private Vector3 m_playerVelocity;
        private bool m_groundedPlayer;
        private bool m_allowJump = true;
        private bool m_isDead;
        private bool m_canAttack = true;
        private bool m_inputWindowOpen;
        private float m_accumulatedAttackTime;
        private float m_attackTimer;
        private float m_movementMultiplier = 0.5f;
        private float m_jumpTimer;
        
        //Read-only
        private readonly float m_attackInputFrequency = 0.25f;
        
        //Publics
        [HideInInspector] public Vector3 _moveVelocity;

        private void OnEnable()
        {
            //Disable things when the player dies
            // PlayerData.OnPlayerDeath += OnDeath;
        }

        private void OnDisable()
        {
            //Disable things when the player dies
            //PlayerData.OnPlayerDeath -= OnDeath;
        }


        void Start()
        {
            m_PlayerInputHandler = GetComponent<PlayerInputHandler>();
            m_characterController = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            trail.enabled = false;
        }
        
        void Update()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }
            
            Attack();
            Movement();
        }

        private void OnDeath()
        {
            //Move camera to nice cinematic location on death
            //StartCoroutine(MovecameraOnDeath());
            
            m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            m_animator.SetTrigger(Died);
            m_isDead = true;
            trail.gameObject.SetActive(false);
        }
        
        void Attack()
        {
            if (m_canAttack && m_inputWindowOpen && m_PlayerInputHandler.attackAction.IsPressed())
            {
                m_animator.SetTrigger(NormalAttack);
                m_accumulatedAttackTime = Time.time;
                m_attackTimer = m_accumulatedAttackTime + m_attackInputFrequency;
                m_inputWindowOpen = false;
                m_canAttack = false;
            }
            
            if (m_canAttack && m_PlayerInputHandler.attackAction.IsPressed())
            {
                m_animator.SetTrigger(NormalAttack);
                m_accumulatedAttackTime = Time.time;
                m_attackTimer = m_accumulatedAttackTime + m_attackInputFrequency;
                m_canAttack = false;
            }
            
            if (!m_canAttack)
            {
                m_accumulatedAttackTime += Time.deltaTime;
                if (m_accumulatedAttackTime >= m_attackTimer)
                {
                    m_canAttack = true;
                    m_attackTimer = 0f;
                    m_animator.ResetTrigger(NormalAttack);
                }
            }
        }
        
        void Movement()
        {
            m_groundedPlayer = m_characterController.isGrounded;

            if (m_groundedPlayer && !m_allowJump)
            {
                m_jumpTimer += Time.deltaTime;
                if(m_jumpTimer >= jumpFrequency)
                {
                    m_allowJump = true;
                }
            }

            if (!m_animator.IsInTransition(0)) //Stop updating if we are in attack animation
            {
                if(m_animator.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
                    return;
            }
            
            Vector2 input = m_PlayerInputHandler.moveAction.ReadValue<Vector2>();
            float horizontalInput = input.x;
            float verticalInput = input.y;
            Vector3 movementInput = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0) * new Vector3(horizontalInput, 0, verticalInput);
            Vector3 movementDirection = movementInput.normalized;
            
            if (m_PlayerInputHandler.sprintAction.IsPressed())
            {
                m_movementMultiplier = 1f;
            }
            else if(movementDirection.magnitude < 0.1f )
            {
                m_movementMultiplier = 0.5f;
            }
            
            _moveVelocity = movementDirection * Time.deltaTime;
            m_animator.SetFloat(Speed, _moveVelocity.normalized.magnitude * m_movementMultiplier, 0.05f, Time.deltaTime);
            
            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            if (m_groundedPlayer && m_playerVelocity.y < 0)
            {
                m_playerVelocity.y = 0f;
                m_animator.SetBool(IsGrounded, m_groundedPlayer);
            }
            
            if (m_allowJump && m_groundedPlayer&& m_PlayerInputHandler.jumpAction.IsPressed())
            {
                m_allowJump = false;
                m_jumpTimer = 0f;
                m_animator.SetTrigger(Jump);
            }
            
            if (m_animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag)
            {
               m_playerVelocity.y += gravityIntensity * Time.deltaTime;
               m_characterController.Move(m_playerVelocity * Time.deltaTime); 
            }
            
            if (m_animator.GetCurrentAnimatorStateInfo(0).tagHash != AttackTag)
            {
                trail.enabled = false;
            }
        }

        private void OnAnimatorMove() //Animator movement - root movement
        {
            Vector3 velocity = m_animator.deltaPosition;
            if (m_animator.GetCurrentAnimatorStateInfo(0).tagHash == JumpTag)
            {
                gravityIntensity = m_animator.GetFloat(GravityIntensity);
                velocity.y += gravityIntensity * Time.deltaTime;
            }
            else
            {
                gravityIntensity = -8f;
            }

            if (m_animator.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
            {
                velocity += transform.forward * m_animator.GetFloat(AttackMomentum) * Time.deltaTime;
                velocity.y += gravityIntensity * Time.deltaTime;
                trail.enabled = true;
            }
            
            m_characterController.Move(velocity);
        }
        
        //Called from animation events
        public void InputStart()
        {
            m_inputWindowOpen = true;
        }
        
        public void InputEnd()
        {
            m_inputWindowOpen = false;
        }
        
        IEnumerator MovecameraOnDeath()
        {
            float time = 1.2f;
            
            //Add DOTween package to use this
            //playerCamera.transform.DOMove(deathCamTransform.position, time);
            //playerCamera.transform.DORotate(deathCamTransform.eulerAngles, time, RotateMode.FastBeyond360);
            
            yield return new WaitForSecondsRealtime(time);
            print("After cam death move");
            OnDeathCamMoved?.Invoke();
        }
    }
}