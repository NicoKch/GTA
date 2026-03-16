using Managers;
using UnityEngine;

namespace Gamplay.Player
{
    /// <summary>
    /// PlayerMovement : Gère uniquement la physique du déplacement du chariot
    /// Compatible avec l'ancien InputManager statique ET le nouveau Singleton
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        #region Configuration

        [Header("Vitesse")] [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float maxReverseSpeed = 3f;
        [SerializeField] private float acceleration = 3f;
        [SerializeField] private float deceleration = 5f;
        [SerializeField] private float brakeForce = 8f;

        [Header("Direction (Ackermann)")] [SerializeField]
        private float maxWheelAngle = 60f;

        [SerializeField] private float wheelTurnSpeed = 120f;
        [SerializeField] private float wheelBase = 2f;

        [Header("Références visuelles")] [SerializeField]
        private Transform rouesArriereGauche;

        [SerializeField] private Transform rouesArriereDroite;

        #endregion

        #region State

        private Rigidbody2D rb;
        private PlayerInputAction inputActions;
        private float currentWheelAngle = 0f;
        private float currentSpeed = 0f;

        // Propriétés publiques pour les autres systèmes
        public float CurrentSpeed => currentSpeed;
        public float MaxSpeed => maxSpeed;
        public float NormalizedSpeed => Mathf.Abs(currentSpeed) / maxSpeed;
        public bool IsReversing => currentSpeed < -0.1f;
        public bool IsMoving => Mathf.Abs(currentSpeed) > 0.1f;

        #endregion

        #region Events

        public delegate void MovementStateChanged(bool isMoving, bool isReversing);

        public event MovementStateChanged OnMovementStateChanged;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError("[PlayerMovement] Rigidbody2D manquant !");
            }

            // Initialise les inputs - compatible avec les deux systèmes
            InitializeInputs();
        }

        private void InitializeInputs()
        {
            // Essaie d'abord le nouveau InputManager Singleton
            if (InputManager.Instance != null)
            {
                inputActions = InputManager.Instance.InputActions;
                Debug.Log("[PlayerMovement] Utilise InputManager Singleton");
            }
            // Sinon, crée directement les inputs (fallback)
            else
            {
                inputActions = new PlayerInputAction();
                inputActions.Player.Enable();
                Debug.Log("[PlayerMovement] Utilise PlayerInputAction directement (fallback)");
            }
        }

        private void Update()
        {
            // Vérifie si le jeu est en pause (si GameManager existe)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            UpdateWheelAngle();
            UpdateWheelVisuals();
        }

        private void FixedUpdate()
        {
            // Vérifie si le jeu est en pause (si GameManager existe)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            bool wasMoving = IsMoving;
            bool wasReversing = IsReversing;

            UpdateSpeed();
            ApplyMovement();

            // Notifie si l'état a changé
            if (wasMoving != IsMoving || wasReversing != IsReversing)
            {
                OnMovementStateChanged?.Invoke(IsMoving, IsReversing);
            }

            // Met à jour l'UI si disponible
            UIManager.Instance?.UpdateSpeedGauge(NormalizedSpeed);

            // Met à jour le son du moteur si disponible
            AudioManager.Instance?.UpdateEngineSound(NormalizedSpeed, IsReversing);
        }

        private void OnDestroy()
        {
            // Nettoie les inputs si on les a créés nous-mêmes
            if (InputManager.Instance == null && inputActions != null)
            {
                inputActions.Player.Disable();
                inputActions.Dispose();
            }
        }

        #endregion

        #region Wheel Control

        private void UpdateWheelAngle()
        {
            float rotateInput = GetRotateInput();

            // MAJ Angle roues
            currentWheelAngle += rotateInput * wheelTurnSpeed * Time.deltaTime;
            currentWheelAngle = Mathf.Clamp(currentWheelAngle, -maxWheelAngle, maxWheelAngle);

            // Retour progressif au centre si on ne tourne pas mais qu'on avance
            if (Mathf.Approximately(rotateInput, 0f) && IsMoving)
            {
                currentWheelAngle = Mathf.MoveTowards(currentWheelAngle, 0f, wheelTurnSpeed * 0.5f * Time.deltaTime);
            }
        }

        private void UpdateWheelVisuals()
        {
            // Rotation visuelle des roues arrière
            if (rouesArriereGauche != null)
                rouesArriereGauche.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);

            if (rouesArriereDroite != null)
                rouesArriereDroite.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);
        }

        #endregion

        #region Speed Control

        private void UpdateSpeed()
        {
            float moveInput = GetMoveInput();
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
                bool isBraking = (currentSpeed > 0f && moveInput < 0f) ||
                                 (currentSpeed < 0f && moveInput > 0f);

                if (isBraking)
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);
                }
                else
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
                }
            }
            else
            {
                // Décélération naturelle (inertie)
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Physics

        private void ApplyMovement()
        {
            // Pas de mouvement si quasi à l'arrêt
            if (Mathf.Abs(currentSpeed) < 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
                currentSpeed = 0f;
                return;
            }

            // Rotation basée sur l'angle des roues (géométrie d'Ackermann)
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

        #endregion

        #region Input Helpers

        private float GetMoveInput()
        {
            if (inputActions != null)
            {
                return inputActions.Player.move.ReadValue<float>();
            }

            return 0f;
        }

        private float GetRotateInput()
        {
            if (inputActions != null)
            {
                return inputActions.Player.rotate.ReadValue<float>();
            }

            return 0f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Force l'arrêt immédiat (pour accidents, etc.)
        /// </summary>
        public void ForceStop()
        {
            currentSpeed = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        /// <summary>
        /// Modifie la vitesse max (ex: zone de limitation)
        /// </summary>
        public void SetSpeedLimit(float newMaxSpeed)
        {
            maxSpeed = Mathf.Max(1f, newMaxSpeed);
        }

        #endregion
    }
}