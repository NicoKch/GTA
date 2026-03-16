using Managers;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// ForkController : Gère la mécanique des fourches du chariot élévateur
    /// Refactorisé pour utiliser le pattern Manager
    /// </summary>
    public class ForkController : MonoBehaviour
    {
        #region Configuration

        [Header("Hauteur")] [SerializeField] private float minHeight = 0f;
        [SerializeField] private float maxHeight = 3f;
        [SerializeField] private float liftSpeed = 2f;

        [Header("Seuils d'action")] [SerializeField]
        private float attachThreshold = 0.15f;

        [SerializeField] private float detachThreshold = 0.20f;
        [SerializeField] private float heightThresholdForOverlay = 0.5f;

        [Header("Visuel - Scaling")] [SerializeField]
        private float minXScale = 1f;

        [SerializeField] private float maxXScale = 1.5f;
        [SerializeField] private float minYScale = 1f;
        [SerializeField] private float maxYScale = 1.3f;

        [Header("Sorting Order")] [SerializeField]
        private int sortingOrderBelow = 0;

        [SerializeField] private int sortingOrderAbove = 10;

        [Header("Références")] [SerializeField]
        private Transform palletAttachPoint;

        #endregion

        #region Runtime State

        // Paramètres configurés par VisionManager
        private PlayerInputAction inputActions;
        private float currentHeight = 0f;
        private SpriteRenderer[] forkSprites;
        private Vector3[] originalScales;

        private Pallet currentPallet;
        private Pallet palletInRange;
        private bool isPalletAttached = false;

        // Pour tracker les seuils
        private bool wasAboveAttachThreshold = false;
        private bool wasAboveDetachThreshold = false;
        private bool wasAboveOverlayThreshold = false;

        // Propriétés publiques
        public float CurrentHeight => currentHeight;
        public float NormalizedHeight => (currentHeight - minHeight) / (maxHeight - minHeight);
        public bool HasPallet => isPalletAttached;
        public Pallet CurrentPallet => currentPallet;

        #endregion

        #region Events

        public delegate void PalletAttached(Pallet pallet);

        public event PalletAttached OnPalletAttached;

        public delegate void PalletDetached(Pallet pallet);

        public event PalletDetached OnPalletDetached;

        public delegate void HeightChanged(float normalizedHeight);

        public event HeightChanged OnHeightChanged;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeSprites();
            InitializeInputs();

            wasAboveAttachThreshold = currentHeight >= attachThreshold;
            wasAboveDetachThreshold = currentHeight > detachThreshold;
            wasAboveOverlayThreshold = currentHeight >= heightThresholdForOverlay;

            UpdateSortingOrder();
        }

        private void InitializeInputs()
        {
            // Essaie d'abord le nouveau InputManager Singleton
            if (InputManager.Instance != null)
            {
                inputActions = InputManager.Instance.InputActions;
                Debug.Log("[ForkController] Utilise InputManager Singleton");
            }
            // Sinon, crée directement les inputs (fallback)
            else
            {
                inputActions = new PlayerInputAction();
                inputActions.Player.Enable();
                Debug.Log("[ForkController] Utilise PlayerInputAction directement (fallback)");
            }
        }

        private void Update()
        {
            // Vérifie si le jeu est en pause (si GameManager existe)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            ProcessLiftInput();
            UpdateVisual();
            CheckPalletAttachment();

            if (!isPalletAttached)
            {
                UpdateSortingOrder();
            }

            // Met à jour l'UI si disponible
            UIManager.Instance?.UpdateForkHeight(NormalizedHeight);
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

        #region Initialization

        private void InitializeSprites()
        {
            forkSprites = GetComponentsInChildren<SpriteRenderer>();
            originalScales = new Vector3[forkSprites.Length];

            for (int i = 0; i < forkSprites.Length; i++)
            {
                originalScales[i] = forkSprites[i].transform.localScale;
            }
        }

        #endregion

        #region Input Processing

        private void ProcessLiftInput()
        {
            float liftInput = GetLiftInput();
            liftInput = FilterLiftInput(liftInput);

            float previousHeight = currentHeight;

            currentHeight += liftInput * liftSpeed * Time.deltaTime;
            currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

            // Notifie si la hauteur a changé significativement
            if (Mathf.Abs(currentHeight - previousHeight) > 0.01f)
            {
                OnHeightChanged?.Invoke(NormalizedHeight);

                // Son de levage/descente
                if (liftInput > 0)
                    AudioManager.Instance?.PlaySFX("fork_lift");
                else if (liftInput < 0)
                    AudioManager.Instance?.PlaySFX("fork_lower");
            }
        }

        private float GetLiftInput()
        {
            if (InputManager.Instance != null) return InputManager.Instance.GetLift();
            if (inputActions != null) return inputActions.Player.lift.ReadValue<float>();
            return 0f;
        }

        /// <summary>
        /// Filtre l'input pour appliquer les règles de sécurité
        /// </summary>
        private float FilterLiftInput(float liftInput)
        {
            // Empêche de baisser les fourches au-dessus d'une palette
            if (!isPalletAttached && palletInRange != null)
            {
                if (currentHeight >= heightThresholdForOverlay && liftInput < 0)
                {
                    Debug.Log("[ForkController] ALERTE : Impossible de baisser les fourches au-dessus d'une palette !");

                    // Feedback via les managers
                    UIManager.Instance?.ShowViolationWarning(SafetyViolationType.LoadTooHigh);
                    AudioManager.Instance?.PlaySFX("violation_warning");

                    return 0f; // Bloque la descente
                }
            }

            return liftInput;
        }

        #endregion

        #region Visual Updates

        private void UpdateVisual()
        {
            if (forkSprites == null || forkSprites.Length == 0) return;

            float t = NormalizedHeight;
            float xScaleFactor = Mathf.Lerp(minXScale, maxXScale, t);
            float yScaleFactor = Mathf.Lerp(minYScale, maxYScale, t);

            for (int i = 0; i < forkSprites.Length; i++)
            {
                Vector3 newScale = new Vector3(
                    originalScales[i].x * xScaleFactor,
                    originalScales[i].y * yScaleFactor,
                    originalScales[i].z
                );
                forkSprites[i].transform.localScale = newScale;
            }
        }

        private void UpdateSortingOrder()
        {
            bool isAboveOverlayThreshold = currentHeight >= heightThresholdForOverlay;

            if (isAboveOverlayThreshold != wasAboveOverlayThreshold)
            {
                int newSortingOrder = isAboveOverlayThreshold ? sortingOrderAbove : sortingOrderBelow;

                foreach (var sprite in forkSprites)
                {
                    sprite.sortingOrder = newSortingOrder;
                }

                wasAboveOverlayThreshold = isAboveOverlayThreshold;
            }
        }

        #endregion

        #region Pallet Attachment

        private void CheckPalletAttachment()
        {
            bool isAboveAttachThreshold = currentHeight >= attachThreshold;
            bool isBelowDetachThreshold = currentHeight < detachThreshold;

            // ATTACHER - quand on monte et passe le seuil
            if (!isPalletAttached && isAboveAttachThreshold && !wasAboveAttachThreshold)
            {
                if (palletInRange != null && palletInRange.AreBothForksInserted)
                {
                    AttachPallet(palletInRange);
                }
            }

            // DÉTACHER - quand on descend et passe sous le seuil
            if (isPalletAttached && isBelowDetachThreshold && wasAboveDetachThreshold)
            {
                DetachPallet();
            }

            wasAboveAttachThreshold = isAboveAttachThreshold;
            wasAboveDetachThreshold = !isBelowDetachThreshold;
        }

        /// <summary>
        /// Appelé par la Pallet quand les fourches sont insérées
        /// </summary>
        public void RegisterPalletInRange(Pallet pallet)
        {
            if (!isPalletAttached)
            {
                palletInRange = pallet;
                Debug.Log($"[ForkController] Palette enregistrée : {pallet.name}");
            }
        }

        /// <summary>
        /// Appelé par la Pallet quand les fourches sont retirées
        /// </summary>
        public void UnregisterPalletInRange(Pallet pallet)
        {
            if (palletInRange == pallet && !isPalletAttached)
            {
                palletInRange = null;
                Debug.Log($"[ForkController] Palette désenregistrée : {pallet.name}");
            }
        }

        private void AttachPallet(Pallet pallet)
        {
            if (pallet == null) return;

            currentPallet = pallet;
            isPalletAttached = true;
            pallet.AttachToForks(palletAttachPoint);

            Debug.Log("[ForkController] Palette attachée !");

            // Notifie les listeners
            OnPalletAttached?.Invoke(pallet);

            // Audio feedback
            AudioManager.Instance?.PlaySFX("pallet_pickup");

            // Notifie le VisionManager que la vue est obstruée
            VisionManager.Instance?.SetLoadObstruction(true);
        }

        private void DetachPallet()
        {
            if (currentPallet == null) return;

            Pallet detachedPallet = currentPallet;

            currentPallet.DetachFromForks();
            currentPallet = null;
            isPalletAttached = false;
            palletInRange = null;

            UpdateSortingOrder();

            Debug.Log("[ForkController] Palette déposée !");

            // Notifie les listeners
            OnPalletDetached?.Invoke(detachedPallet);

            // Audio feedback
            AudioManager.Instance?.PlaySFX("pallet_drop");

            // Notifie le VisionManager que la vue est dégagée
            VisionManager.Instance?.SetLoadObstruction(false);
        }

        #endregion
    }
}