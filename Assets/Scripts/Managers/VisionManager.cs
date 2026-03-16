using UnityEngine;
using System.Collections.Generic;
using Events;
using Gameplay.Vision;
using Gamplay.Player;
using UnityEngine.InputSystem;

namespace Managers
{
    /// <summary>
    /// VisionManager : Gère le système de champ de vision du conducteur
    /// Intégré dans l'architecture Manager - Singleton
    /// Enseigne les règles CACES sur la visibilité (vue avant/arrière selon la charge)
    /// </summary>
    public class VisionManager : MonoBehaviour
    {
        #region Singleton

        public static VisionManager Instance { get; private set; }

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

        [Header("Cônes de vision")] [SerializeField]
        private VisionCone frontCone;

        [SerializeField] private VisionCone rearCone;

        [Header("Paramètres du cône")] [SerializeField]
        private float coneAngle = 80f;

        [SerializeField] private float coneDistance = 15f;
        [SerializeField] private int rayCount = 30;

        [Header("Layers")] [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private LayerMask detectableLayer;

        [Header("Règles de sécurité")] [Tooltip("Temps avant pénalité si mauvaise vue utilisée")] [SerializeField]
        private float wrongVisionGracePeriod = 2f;

        #endregion

        #region Runtime State

        private bool isFrontViewActive = true;
        private bool isLoadObstructingView;
        private HashSet<VisionTarget> visibleTargets = new HashSet<VisionTarget>();

        // Pour le suivi de la violation "mauvaise vue"
        private float wrongVisionTimer;
        private bool isUsingWrongVision;

        // Input actions (compatible avec les deux systèmes)
        private PlayerInputAction inputActions;

        // Propriétés publiques
        public bool IsFrontViewActive => isFrontViewActive;
        public bool IsViewObstructed => isLoadObstructingView;
        private VisionMode CurrentVisionMode => isFrontViewActive ? VisionMode.Forward : VisionMode.Rear;

        #endregion

        #region Events

        public event System.Action<bool> OnViewSwitched;
        public event System.Action<VisionTarget> OnTargetEnterView;
        public event System.Action<VisionTarget> OnTargetExitView;
        public event System.Action<bool> OnViewObstructed;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeInputs();
            InitializeCones();
            SetActiveView(isFrontViewActive);

            // Abonnement aux événements du jeu
            SubscribeToGameEvents();
        }

        private void InitializeInputs()
        {
            // Essaie d'abord le nouveau InputManager Singleton
            if (InputManager.Instance != null)
            {
                inputActions = InputManager.Instance.InputActions;
                Debug.Log("[VisionManager] Utilise InputManager Singleton");
            }
            // Sinon, crée directement les inputs (fallback).
            else
            {
                inputActions = new PlayerInputAction();
                inputActions.Player.Enable();
                Debug.Log("[VisionManager] Utilise PlayerInputAction directement (fallback)");
            }

            // Abonnement à l'input de changement de vue
            if (inputActions != null)
            {
                inputActions.Player.switchView.performed += OnSwitchViewPerformed;
                Debug.Log("[VisionManager] Abonné à switchView");
            }
        }

        private void Update()
        {
            // Vérifie si le jeu est en pause (si GameManager existe)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            UpdateVision();
            CheckVisionSafetyRules();
        }

        private void OnDestroy()
        {
            // Désabonnement de l'input
            if (inputActions != null)
            {
                inputActions.Player.switchView.performed -= OnSwitchViewPerformed;

                // Nettoie les inputs si on les a créés nous-mêmes
                if (InputManager.Instance == null)
                {
                    inputActions.Player.Disable();
                    inputActions.Dispose();
                }
            }

            UnsubscribeFromGameEvents();
        }

        #endregion

        #region Initialization

        private void InitializeCones()
        {
            if (frontCone != null)
            {
                frontCone.Initialize(coneAngle, coneDistance, rayCount, obstacleLayer, detectableLayer);
            }

            if (rearCone != null)
            {
                rearCone.Initialize(coneAngle, coneDistance, rayCount, obstacleLayer, detectableLayer);
            }

            Debug.Log("[VisionManager] Cônes de vision initialisés");
        }

        private void SubscribeToGameEvents()
        {
            // S'abonne aux événements globaux via GameEvents
            GameEvents.OnPalletPickedUp += HandlePalletPickedUp;
            GameEvents.OnPalletDropped += HandlePalletDropped;
        }

        private void UnsubscribeFromGameEvents()
        {
            GameEvents.OnPalletPickedUp -= HandlePalletPickedUp;
            GameEvents.OnPalletDropped -= HandlePalletDropped;
        }

        #endregion

        #region View Switching

        /// <summary>
        /// Bascule entre la vue avant et arrière
        /// </summary>
        private void SwitchView()
        {
            isFrontViewActive = !isFrontViewActive;
            SetActiveView(isFrontViewActive);

            // Notifie les listeners locaux
            OnViewSwitched?.Invoke(isFrontViewActive);

            // Notifie via le système d'événements global
            GameEvents.OnVisionModeChanged?.Invoke(CurrentVisionMode);

            // Feedback audio
            AudioManager.Instance?.PlaySFX("view_switch");

            Debug.Log($"[VisionManager] Vue basculée : {(isFrontViewActive ? "AVANT" : "ARRIÈRE")}");
        }

        private void SetActiveView(bool frontActive)
        {
            if (frontCone != null)
            {
                // Vue avant désactivée si charge obstrue
                frontCone.SetActive(frontActive && !isLoadObstructingView);
            }

            if (rearCone != null)
            {
                rearCone.SetActive(!frontActive);
            }
        }

        private void OnSwitchViewPerformed(InputAction.CallbackContext context)
        {
            SwitchView();
        }

        #endregion

        #region Load Obstruction

        /// <summary>
        /// Appelé par ForkController quand une charge est prise/déposée
        /// </summary>
        public void SetLoadObstruction(bool isObstructed)
        {
            if (isLoadObstructingView == isObstructed) return;

            isLoadObstructingView = isObstructed;

            // Notifie les listeners
            OnViewObstructed?.Invoke(isObstructed);
            GameEvents.OnVisionObstructed?.Invoke(isObstructed);

            // Si la vue avant est active et qu'une charge obstrue
            if (isFrontViewActive && isObstructed)
            {
                frontCone?.SetActive(false);

                // Affiche un avertissement via UIManager
                UIManager.Instance?.ShowViolationWarning(SafetyViolationType.WrongVisionUsed);

                Debug.LogWarning(
                    "[VisionManager] ATTENTION : Charge obstruant la vue frontale ! Utilisez la marche arrière.");
            }
            else if (isFrontViewActive && !isObstructed)
            {
                frontCone?.SetActive(true);
            }

            Debug.Log($"[VisionManager] Obstruction de vue : {isObstructed}");
        }

        private void HandlePalletPickedUp(Pallet pallet)
        {
            // La charge est haute, elle obstrue potentiellement la vue
            // Note : La vraie logique est gérée par ForkController qui appelle SetLoadObstruction.
        }

        private void HandlePalletDropped(Pallet pallet)
        {
            // La charge est déposée, la vue est dégagée
            // Note : La vraie logique est gérée par ForkController qui appelle SetLoadObstruction.
        }

        #endregion

        #region Vision Updates

        private void UpdateVision()
        {
            VisionCone activeCone = GetActiveCone();
            if (activeCone == null) return;

            HashSet<VisionTarget> currentlyVisible = activeCone.GetVisibleTargets();

            // Détecter les nouvelles cibles visibles
            foreach (var target in currentlyVisible)
            {
                if (!visibleTargets.Contains(target))
                {
                    target.OnBecomeVisible();
                    OnTargetEnterView?.Invoke(target);

                    // Alerte si c'est un piéton ou un danger
                    HandleTargetDetected(target);
                }
            }

            // Détecter les cibles qui ne sont plus visibles
            foreach (var target in visibleTargets)
            {
                if (!currentlyVisible.Contains(target))
                {
                    target.OnBecomeHidden();
                    OnTargetExitView?.Invoke(target);
                }
            }

            visibleTargets = currentlyVisible;
        }

        private VisionCone GetActiveCone()
        {
            if (isFrontViewActive && !isLoadObstructingView)
            {
                return frontCone;
            }

            return isFrontViewActive ? null : rearCone;
        }

        /// <summary>
        /// Gère la détection d'une nouvelle cible
        /// </summary>
        private void HandleTargetDetected(VisionTarget target)
        {
            switch (target.Type)
            {
                case VisionTarget.TargetType.Pedestrian:
                    // Alerte piéton !
                    AudioManager.Instance?.PlaySFX("pedestrian_alert");
                    Debug.Log("[VisionManager] PIÉTON DÉTECTÉ !");
                    break;

                case VisionTarget.TargetType.Hazard:
                    // Alerte danger
                    AudioManager.Instance?.PlaySFX("hazard_alert");
                    Debug.Log("[VisionManager] DANGER DÉTECTÉ !");
                    break;

                case VisionTarget.TargetType.Forklift:
                    // Autre chariot
                    Debug.Log("[VisionManager] Autre chariot détecté");
                    break;
            }
        }

        #endregion

        #region Safety Rules Check

        /// <summary>
        /// Vérifie les règles de sécurité liées à la vision
        /// </summary>
        private void CheckVisionSafetyRules()
        {
            // Règle CACES : Si charge haute, on doit utiliser la vision arrière
            bool shouldUseRearVision = isLoadObstructingView;
            bool isMovingForward = IsPlayerMovingForward();

            if (shouldUseRearVision && isFrontViewActive && isMovingForward)
            {
                // Le joueur avance avec une charge haute en utilisant la vue avant (bloquée).
                if (!isUsingWrongVision)
                {
                    isUsingWrongVision = true;
                    wrongVisionTimer = 0f;
                }

                wrongVisionTimer += Time.deltaTime;

                // Après la période de grâce, signale la violation
                if (wrongVisionTimer >= wrongVisionGracePeriod)
                {
                    SafetyManager.Instance?.ReportViolation(SafetyViolationType.WrongVisionUsed);
                    wrongVisionTimer = 0f; // Reset pour éviter le spam
                }
            }
            else
            {
                isUsingWrongVision = false;
                wrongVisionTimer = 0f;
            }
        }

        private bool IsPlayerMovingForward()
        {
            // Vérifie si le joueur avance via le PlayerMovement
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            return playerMovement != null && playerMovement.CurrentSpeed > 0.1f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retourne les cibles actuellement visibles
        /// </summary>
        public HashSet<VisionTarget> GetVisibleTargets()
        {
            return new HashSet<VisionTarget>(visibleTargets);
        }

        /// <summary>
        /// Vérifie si une cible spécifique est visible
        /// </summary>
        public bool IsTargetVisible(VisionTarget target)
        {
            return visibleTargets.Contains(target);
        }

        /// <summary>
        /// Retourne le nombre de cibles visibles par type
        /// </summary>
        public int CountVisibleTargetsOfType(VisionTarget.TargetType type)
        {
            int count = 0;
            foreach (var target in visibleTargets)
            {
                if (target.Type == type) count++;
            }

            return count;
        }

        /// <summary>
        /// Force la vue arrière (utile pour les tutoriels)
        /// </summary>
        public void ForceRearView()
        {
            if (isFrontViewActive)
            {
                SwitchView();
            }
        }

        /// <summary>
        /// Force la vue avant
        /// </summary>
        public void ForceFrontView()
        {
            if (!isFrontViewActive)
            {
                SwitchView();
            }
        }

        #endregion
    }
}