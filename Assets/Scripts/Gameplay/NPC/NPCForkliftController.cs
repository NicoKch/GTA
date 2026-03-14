using Managers;
using UnityEngine;

namespace Gameplay.NPC
{
    /// <summary>
    /// NPCForkliftController : Gère le déplacement autonome d'un chariot élévateur NPC
    /// Basé sur PlayerMovement mais piloté par une IA suivant des waypoints
    /// </summary>
    public class NPCForkliftController : MonoBehaviour
    {
        #region Configuration

        [Header("Vitesse")]
        [SerializeField] private float maxSpeed = 4f;
        [SerializeField] private float maxReverseSpeed = 2f;
        [SerializeField] private float acceleration = 2.5f;
        [SerializeField] private float deceleration = 4f;
        [SerializeField] private float brakeForce = 6f;

        [Header("Direction (Ackermann)")]
        [SerializeField] private float maxWheelAngle = 60f;
        [SerializeField] private float wheelTurnSpeed = 100f;
        [SerializeField] private float wheelBase = 2f;

        [Header("Navigation")]
        [SerializeField] private ForkliftWaypoint[] waypoints;
        [SerializeField] private float waypointReachDistance = 0.5f;
        [SerializeField] private float slowDownDistance = 3f;
        [SerializeField] private bool loopWaypoints = true;
        [SerializeField] private float waitTimeAtWaypoint = 2f;

        [Header("Détection d'obstacles")]
        [SerializeField] private float obstacleDetectionDistance = 3f;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float emergencyStopDistance = 1f;

        [Header("Références visuelles")]
        [SerializeField] private Transform rouesArriereGauche;
        [SerializeField] private Transform rouesArriereDroite;

        #endregion

        #region State

        private Rigidbody2D rb;
        private float currentWheelAngle = 0f;
        private float currentSpeed = 0f;
        private int currentWaypointIndex = 0;
        private float waitTimer = 0f;
        private bool isWaiting = false;
        private bool isActive = true;

        // Propriétés publiques
        public float CurrentSpeed => currentSpeed;
        public float MaxSpeed => maxSpeed;
        public float NormalizedSpeed => Mathf.Abs(currentSpeed) / maxSpeed;
        public bool IsReversing => currentSpeed < -0.1f;
        public bool IsMoving => Mathf.Abs(currentSpeed) > 0.1f;
        public bool IsActive => isActive;
        public ForkliftWaypoint CurrentWaypoint =>
            (waypoints != null && waypoints.Length > 0) ? waypoints[currentWaypointIndex] : null;

        #endregion

        #region Events

        public delegate void WaypointReached(ForkliftWaypoint waypoint, int index);
        public event WaypointReached OnWaypointReached;

        public delegate void PathCompleted();
        public event PathCompleted OnPathCompleted;

        public delegate void ObstacleDetected(Collider2D obstacle);
        public event ObstacleDetected OnObstacleDetected;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                Debug.LogError("[NPCForkliftController] Rigidbody2D manquant !");
            }

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning("[NPCForkliftController] Aucun waypoint configuré !");
                isActive = false;
            }
        }

        private void Update()
        {
            if (!isActive) return;

            // Vérifie si le jeu est en pause
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            if (isWaiting)
            {
                HandleWaiting();
            }
            else
            {
                UpdateSteering();
            }

            UpdateWheelVisuals();
        }

        private void FixedUpdate()
        {
            if (!isActive) return;

            // Vérifie si le jeu est en pause
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (!isWaiting)
            {
                CheckForObstacles();
                UpdateSpeed();
                ApplyMovement();
                CheckWaypointReached();
            }
            else
            {
                // À l'arrêt pendant l'attente
                rb.linearVelocity = Vector2.zero;
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Navigation

        private void UpdateSteering()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            ForkliftWaypoint targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint == null) return;

            Vector2 targetPosition = targetWaypoint.transform.position;
            Vector2 currentPosition = transform.position;
            Vector2 directionToTarget = (targetPosition - currentPosition).normalized;

            // Calcule l'angle vers la cible
            float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.y) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;

            // Normalise les angles
            float angleDiff = Mathf.DeltaAngle(-currentAngle, targetAngle);

            // Calcule l'angle des roues nécessaire
            float desiredWheelAngle = Mathf.Clamp(angleDiff, -maxWheelAngle, maxWheelAngle);

            // Transition douce vers l'angle désiré
            currentWheelAngle = Mathf.MoveTowards(currentWheelAngle, desiredWheelAngle, wheelTurnSpeed * Time.deltaTime);
        }

        private void CheckWaypointReached()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            ForkliftWaypoint targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint == null) return;

            float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.transform.position);

            if (distanceToWaypoint <= waypointReachDistance)
            {
                OnWaypointReached?.Invoke(targetWaypoint, currentWaypointIndex);

                // Commence l'attente si le waypoint le requiert
                float waitTime = targetWaypoint.WaitTime > 0 ? targetWaypoint.WaitTime : waitTimeAtWaypoint;
                if (waitTime > 0)
                {
                    isWaiting = true;
                    waitTimer = waitTime;
                }
                else
                {
                    MoveToNextWaypoint();
                }
            }
        }

        private void HandleWaiting()
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToNextWaypoint();
            }
        }

        private void MoveToNextWaypoint()
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopWaypoints)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    currentWaypointIndex = waypoints.Length - 1;
                    isActive = false;
                    OnPathCompleted?.Invoke();
                }
            }
        }

        #endregion

        #region Speed Control

        private void UpdateSpeed()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            ForkliftWaypoint targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint == null) return;

            float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.transform.position);

            // Calcule la vitesse cible en fonction de la distance
            float targetSpeed = maxSpeed;

            // Ralentit à l'approche du waypoint
            if (distanceToWaypoint < slowDownDistance)
            {
                float slowdownFactor = distanceToWaypoint / slowDownDistance;
                targetSpeed = Mathf.Lerp(maxSpeed * 0.3f, maxSpeed, slowdownFactor);
            }

            // Applique la limite de vitesse du waypoint si définie
            if (targetWaypoint.SpeedLimit > 0)
            {
                targetSpeed = Mathf.Min(targetSpeed, targetWaypoint.SpeedLimit);
            }

            // Accélération vers la vitesse cible
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }

        #endregion

        #region Obstacle Detection

        private void CheckForObstacles()
        {
            Vector2 origin = transform.position;
            Vector2 direction = transform.up;

            RaycastHit2D hit = Physics2D.Raycast(origin, direction, obstacleDetectionDistance, obstacleLayer);

            if (hit.collider != null)
            {
                OnObstacleDetected?.Invoke(hit.collider);

                float distanceToObstacle = hit.distance;

                if (distanceToObstacle < emergencyStopDistance)
                {
                    // Arrêt d'urgence
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * 2f * Time.fixedDeltaTime);
                }
                else
                {
                    // Ralentissement proportionnel à la distance
                    float slowdownFactor = (distanceToObstacle - emergencyStopDistance) /
                                          (obstacleDetectionDistance - emergencyStopDistance);
                    float limitedSpeed = maxSpeed * slowdownFactor * 0.5f;
                    currentSpeed = Mathf.Min(currentSpeed, limitedSpeed);
                }
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

        #region Visual Updates

        private void UpdateWheelVisuals()
        {
            if (rouesArriereGauche != null)
                rouesArriereGauche.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);

            if (rouesArriereDroite != null)
                rouesArriereDroite.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Démarre ou reprend le mouvement du NPC
        /// </summary>
        public void StartMoving()
        {
            isActive = true;
            isWaiting = false;
        }

        /// <summary>
        /// Arrête le NPC
        /// </summary>
        public void StopMoving()
        {
            isActive = false;
            currentSpeed = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// Force l'arrêt immédiat
        /// </summary>
        public void ForceStop()
        {
            currentSpeed = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        /// <summary>
        /// Définit un nouveau parcours de waypoints
        /// </summary>
        public void SetWaypoints(ForkliftWaypoint[] newWaypoints)
        {
            waypoints = newWaypoints;
            currentWaypointIndex = 0;
            isWaiting = false;
            isActive = waypoints != null && waypoints.Length > 0;
        }

        /// <summary>
        /// Va directement à un waypoint spécifique
        /// </summary>
        public void GoToWaypoint(int index)
        {
            if (waypoints != null && index >= 0 && index < waypoints.Length)
            {
                currentWaypointIndex = index;
                isWaiting = false;
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            // Affiche la détection d'obstacles
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.up * obstacleDetectionDistance);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + transform.up * emergencyStopDistance, 0.2f);

            // Affiche le waypoint actuel
            if (waypoints != null && waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
            {
                ForkliftWaypoint target = waypoints[currentWaypointIndex];
                if (target != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, target.transform.position);
                    Gizmos.DrawWireSphere(target.transform.position, waypointReachDistance);
                }
            }
        }

        #endregion
    }
}