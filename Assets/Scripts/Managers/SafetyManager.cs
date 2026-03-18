using UnityEngine;
using System.Collections.Generic;
using Gameplay.Player;
using Gamplay.Player;

namespace Managers
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// SafetyManager : Gère les règles de sécurité CACES
    /// Détecte les violations et notifie le GameManager
    /// C'est le cœur du gameplay éducatif !
    /// </summary>
    public class SafetyManager : MonoBehaviour
    {
        #region Singleton

        public static SafetyManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Configuration

        [Header("Limites de sécurité")] [SerializeField]
        private float maxSpeedLoaded = 5f; // km/h avec charge

        [SerializeField] private float maxSpeedUnloaded = 10f; // km/h sans charge
        [SerializeField] private float maxLoadHeightMoving = 0.3f; // Hauteur max fourches en mouvement
        [SerializeField] private float intersectionHornRadius = 3f; // Rayon détection carrefour

        [Header("Pénalités (en points)")] [SerializeField]
        private int penaltySpeedExcess = 50;

        [SerializeField] private int penaltyNoHorn = 30;
        [SerializeField] private int penaltyWrongVision = 40;
        [SerializeField] private int penaltyLoadTooHigh = 60;

        [Header("Références")] [SerializeField]
        private PlayerMovement playerMovement;

        [SerializeField] private ForkController forkController;

        #endregion

        #region State Tracking

        private Dictionary<SafetyViolationType, float> lastViolationTime = new();
        private float violationCooldown = 2f; // Évite le spam de la même violation

        private int totalViolations = 0;
        private int totalPenaltyPoints = 0;

        public int TotalViolations => totalViolations;
        public int TotalPenaltyPoints => totalPenaltyPoints;

        #endregion

        #region Events

        public delegate void ViolationDetected(SafetyViolationType type, int penaltyPoints);

        public event ViolationDetected OnViolationDetected;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Auto-détection si non assigné
            if (playerMovement == null)
                playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (forkController == null)
                forkController = FindFirstObjectByType<ForkController>();
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
                return;

            CheckSpeedLimit();
            CheckLoadHeight();
            CheckVisionUsage();
        }

        #endregion

        #region Safety Checks

        private void CheckSpeedLimit()
        {
            if (playerMovement == null) return;

            float currentSpeed = playerMovement.CurrentSpeed;
            float maxSpeed = forkController?.HasPallet == true ? maxSpeedLoaded : maxSpeedUnloaded;

            if (Mathf.Abs(currentSpeed) > maxSpeed)
            {
                ReportViolationInternal(SafetyViolationType.SpeedExcess, penaltySpeedExcess);
            }
        }

        private void CheckLoadHeight()
        {
            if (forkController == null || playerMovement == null) return;

            // Si on se déplace avec la charge trop haute
            bool isMovingForward = playerMovement.CurrentSpeed > 0.1f;
            bool hasLoad = forkController.HasPallet;
            bool loadTooHigh = forkController.CurrentHeight > maxLoadHeightMoving;

            if (isMovingForward && hasLoad && loadTooHigh)
            {
                ReportViolationInternal(SafetyViolationType.LoadTooHigh, penaltyLoadTooHigh);
            }
        }

        private void CheckVisionUsage()
        {
            if (forkController == null || playerMovement == null) return;

            // Si on a une charge qui bloque la vue avant et qu'on avance
            // le joueur devrait utiliser la vision arrière
            bool hasLoad = forkController.HasPallet;
            bool isMovingForward = playerMovement.CurrentSpeed > 0.1f;
            bool loadBlocksVision = forkController.CurrentHeight > 0.5f; // Charge en position haute

            if (hasLoad && isMovingForward && loadBlocksVision)
            {
                // Vérifier via VisionManager si le joueur utilise la bonne vision
                // Cette logique sera connectée au VisionManager
                // ReportViolation(SafetyViolationType.WrongVisionUsed, penaltyWrongVision);
            }
        }

        /// <summary>
        /// Appelé par les zones de carrefour quand le chariot entre
        /// </summary>
        public void OnEnteredIntersection()
        {
            // Le joueur doit klaxonner dans les X secondes
            // À implémenter avec une coroutine ou un timer
            Debug.Log("[SafetyManager] Entrée dans un carrefour - Klaxon requis !");
        }

        /// <summary>
        /// Appelé quand le joueur klaxonne
        /// </summary>
        public void OnHornUsed()
        {
            Debug.Log("[SafetyManager] Klaxon utilisé");
            // Annule la pénalité de carrefour si applicable
        }

        #endregion

        #region Violation Reporting

        /// <summary>
        /// Signale une violation de sécurité (appelable par d'autres Managers comme VisionManager)
        /// </summary>
        public void ReportViolation(SafetyViolationType type)
        {
            int penalty = GetPenaltyForType(type);
            ReportViolationInternal(type, penalty);
        }

        private int GetPenaltyForType(SafetyViolationType type)
        {
            return type switch
            {
                SafetyViolationType.SpeedExcess => penaltySpeedExcess,
                SafetyViolationType.NoHornAtIntersection => penaltyNoHorn,
                SafetyViolationType.WrongVisionUsed => penaltyWrongVision,
                SafetyViolationType.LoadTooHigh => penaltyLoadTooHigh,
                _ => 25
            };
        }

        private void ReportViolationInternal(SafetyViolationType type, int penalty)
        {
            // Vérifie le cooldown pour éviter le spam
            if (lastViolationTime.TryGetValue(type, out float lastTime))
            {
                if (Time.time - lastTime < violationCooldown)
                    return;
            }

            lastViolationTime[type] = Time.time;
            totalViolations++;
            totalPenaltyPoints += penalty;

            Debug.Log($"[SafetyManager] VIOLATION: {type} - Pénalité: {penalty} points");

            // Notifie les listeners
            OnViolationDetected?.Invoke(type, penalty);

            // Notifie le GameManager
            GameManager.Instance?.OnSafetyViolation(type);
        }

        /// <summary>
        /// Appelé lors d'une collision grave (accident)
        /// </summary>
        public void ReportAccident(string description)
        {
            Debug.LogError($"[SafetyManager] ACCIDENT: {description}");
            GameManager.Instance?.OnAccident();
        }

        #endregion

        #region Reset

        public void ResetViolations()
        {
            totalViolations = 0;
            totalPenaltyPoints = 0;
            lastViolationTime.Clear();
            Debug.Log("[SafetyManager] Compteurs réinitialisés");
        }

        #endregion
    }
}