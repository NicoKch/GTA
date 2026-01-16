using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxReverseSpeed = 3f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float brakeForce = 1f;

    [Header("Direction")] [SerializeField] private float maxWheelAngle = 60f;
    [SerializeField] private float wheelTurnSpeed = 120f;

    [Header("Chariot")] [SerializeField] private float wheelBase = 2f;

    [Header("References")] [SerializeField]
    private Transform rouesArriereGauche;

    [SerializeField] private Transform rouesArriereDroite;

    private Rigidbody2D rb;
    private PlayerInputAction inputActions;
    private float moveInput;
    private float rotateInput;
    private float currentWheelAngle = 0f;
    private float currentSpeed = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = InputManager.InputActions;
    }

    private void Update()
    {
        // Lecture des inputs
        moveInput = inputActions.Player.move.ReadValue<float>();
        rotateInput = inputActions.Player.rotate.ReadValue<float>();


        // MAJ Angle roues
        currentWheelAngle += rotateInput * wheelTurnSpeed * Time.deltaTime;
        currentWheelAngle = Mathf.Clamp(currentWheelAngle, -maxWheelAngle, maxWheelAngle);

        // Retour progressif au centre si on ne tourne pas
        if (Mathf.Approximately(rotateInput, 0f) && !Mathf.Approximately(moveInput, 0f))
        {
            currentWheelAngle = Mathf.MoveTowards(currentWheelAngle, 0f, wheelTurnSpeed * 0.5f * Time.deltaTime);
        }

        // Rotation roues arrières
        if (rouesArriereGauche && rouesArriereDroite)
        {
            rouesArriereGauche.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);
            rouesArriereDroite.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);
        }
    }

    private void FixedUpdate()
    {
        moveInput = inputActions.Player.move.ReadValue<float>();
        rotateInput = inputActions.Player.rotate.ReadValue<float>();
        UpdateSpeed();
        ApplyMovement();
    }

    private void UpdateSpeed()
    {
        float targetSpeed = 0f;

        if (moveInput > 0f)
        {
            targetSpeed = maxSpeed;
        }
        else if (moveInput < 0f)
        {
            targetSpeed = -maxReverseSpeed;
        }

        if (!Mathf.Approximately(moveInput, 0f))
        {
            // Le joueur appuie sur une touche
            bool isBraking = (currentSpeed > 0f && moveInput < 0f) || (currentSpeed < 0f && moveInput > 0f);

            if (isBraking)
            {
                // Freinage
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, brakeForce * Time.deltaTime);
            }
            else
            {
                // Accélération normale
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            }
        }
        else
        {
            // décélération naturelle
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);
        }
    }

    private void ApplyMovement()
    {
        // Pas de mouvement si quasi à l'arrêt
        if (Mathf.Abs(currentSpeed) < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            currentSpeed = 0f;
            return;
        }

        // Rotation basée sur l'angle des roues
        float angularVelocity = 0f;

        if (!Mathf.Approximately(currentWheelAngle, 0f))
        {
            float turnRadius = wheelBase / Mathf.Tan(currentWheelAngle * Mathf.Deg2Rad);
            angularVelocity = currentSpeed / turnRadius;
        }

        rb.MoveRotation(rb.rotation - angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime);

        // Déplacement
        Vector2 direction = transform.up * currentSpeed;
        rb.linearVelocity = direction;
    }
}