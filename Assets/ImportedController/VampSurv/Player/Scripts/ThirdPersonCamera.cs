using System;
using System.Collections;
using Input;
using Player;
using UnityEngine;

//using DG.Tweening;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private float stationaryDistanceFromTarget = 6.0f;
    [SerializeField] private float movingDistanceFromTarget = 8.5f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private float distanceSmoothTime = 0.2f;
    [SerializeField] private Vector2 rotationXMinMax = new Vector2(-7, 40);
    [SerializeField] ScriptableFloatCurve smoothCurve;
    [SerializeField] private ScriptablePlayerPrefsKeys playerPrefsKeys;

    private float m_cameraRotSensitivity = 0.151f;
    private float m_rotationY;
    private float m_rotationX;
    private Vector3 m_currentRotation;
    private Vector3 m_smoothVelocity = Vector3.zero;
    private float m_distanceToTarget;
    private float m_targetDistanceToTarget;
    private float m_refVelocity;
    private CameraInputHandler  m_cameraInputHandler;
    private bool m_isDead = false;

    private void OnEnable()
    {
        //Do things on death or when settings change
        //PlayerData.OnPlayerDeath += OnDeath;
        //SetCamRotSpeed.OnCamRotSpeedChanged += UpdateCamRotSpeed;
    }

    private void OnDisable()
    {
        //Do things on death or when settings change
        //PlayerData.OnPlayerDeath -= OnDeath;
        //SetCamRotSpeed.OnCamRotSpeedChanged -= UpdateCamRotSpeed;
    }

    private void OnDeath()
    {
        m_isDead = true;
    }

    private void UpdateCamRotSpeed()
    {
        if (!PlayerPrefs.HasKey(playerPrefsKeys.cameraRotationSpeedKey))
        {
            PlayerPrefs.SetFloat(playerPrefsKeys.cameraRotationSpeedKey, m_cameraRotSensitivity);
        }
        m_cameraRotSensitivity = PlayerPrefs.GetFloat(playerPrefsKeys.cameraRotationSpeedKey);
        print("updated cam rot speed??");
    }

    private void Start()
    {
        m_cameraInputHandler = GetComponent<CameraInputHandler>();
        Cursor.visible = false;
        UpdateCamRotSpeed();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }
        
        if (m_isDead)
        {
            m_currentRotation = Vector3.SmoothDamp(m_currentRotation, Vector3.zero, ref m_smoothVelocity, 0.1f);
      
            transform.localEulerAngles = m_currentRotation;
        }
        
        float mouseX = m_cameraInputHandler.lookAction.ReadValue<Vector2>().x * m_cameraRotSensitivity;
        float mouseY = m_cameraInputHandler.lookAction.ReadValue<Vector2>().y * m_cameraRotSensitivity * -1f;

        m_rotationY += mouseX;
        m_rotationX += mouseY;

        // Apply clamping for x rotation 
        m_rotationX = Mathf.Clamp(m_rotationX, rotationXMinMax.x, rotationXMinMax.y);
        
        float evaluatedSmoothCurve = smoothCurve.floatCurve.Evaluate(m_cameraInputHandler.lookAction.ReadValue<Vector2>().magnitude);
        
        Vector3 nextRotation = new Vector3(m_rotationX, m_rotationY);

        // Apply damping between rotation changes
        m_currentRotation = Vector3.SmoothDamp(m_currentRotation, nextRotation, ref m_smoothVelocity, evaluatedSmoothCurve);
      
        transform.localEulerAngles = m_currentRotation;
        float distanceMoveVelocity = player._moveVelocity.normalized.magnitude;
        
        if (distanceMoveVelocity > 0.01f)
        {
            m_targetDistanceToTarget = movingDistanceFromTarget;
        }
        else
        {
            m_targetDistanceToTarget = stationaryDistanceFromTarget;
        }
        
        m_distanceToTarget = Mathf.SmoothDamp(m_distanceToTarget, m_targetDistanceToTarget, ref m_refVelocity, distanceSmoothTime);
        
        // Subtract the forward vector of the GameObject to point its forward vector to the target
        transform.position = player.transform.position - 
            transform.forward * m_distanceToTarget + transform.up * heightOffset;
    }
}