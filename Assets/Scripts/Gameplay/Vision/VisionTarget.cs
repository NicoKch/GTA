using Managers;
using UnityEngine;

namespace Gameplay.Vision
{
    /// <summary>
    /// VisionTarget : Composant pour les objets détectables par le système de vision
    /// Gère l'apparence visuelle quand l'objet est visible/caché
    /// Types : Piétons, autres chariots, obstacles, dangers, palettes
    /// </summary>
    public class VisionTarget : MonoBehaviour
    {
        #region Enums

        public enum TargetType
        {
            Pedestrian, // Piéton - priorité haute, alerte sonore
            Forklift, // Autre chariot élévateur
            Obstacle, // Obstacle statique
            Hazard, // Zone dangereuse
            Pallet, // Palette
            Vehicle // Autre véhicule
        }

        private enum VisibilityBehavior
        {
            FadeAlpha, // Réduit l'opacité quand invisible
            HideCompletely, // Cache complètement
            ShowOutline, // Affiche un contour quand invisible
            NoChange // Aucun changement visuel
        }

        #endregion

        #region Configuration

        [Header("Type de cible")] [SerializeField]
        private TargetType targetType = TargetType.Obstacle;

        [Header("Comportement visuel")] [SerializeField]
        private VisibilityBehavior visibilityBehavior = VisibilityBehavior.FadeAlpha;

        [SerializeField] private Color hiddenColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] private float fadeSpeed = 5f;

        [Header("Danger")] [Tooltip("Distance à partir de laquelle une alerte est déclenchée")] [SerializeField]
        private float dangerDistance = 5f;

        [SerializeField] private bool triggerAlertWhenVisible = true;

        #endregion

        #region Runtime State

        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;
        private bool isVisible;
        private bool hasBeenInitialized;

        // Pour le fade progressif
        private float currentAlpha = 1f;
        private float targetAlpha = 1f;

        #endregion

        #region Properties

        public TargetType Type => targetType;
        public bool IsVisible => isVisible;
        public float DangerDistance => dangerDistance;

        #endregion

        #region Events

        public event System.Action<VisionTarget> OnBecameVisible;
        public event System.Action<VisionTarget> OnBecameHidden;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeRenderers();
        }

        [Header("Initialisation")] [Tooltip("Si false, l'objet reste visible au démarrage")] [SerializeField]
        private bool startHidden = true;

        private void Start()
        {
            // Commence caché par défaut SEULEMENT si startHidden est true
            if (startHidden && visibilityBehavior != VisibilityBehavior.NoChange)
            {
                ForceHidden(); // Utilise ForceHidden au lieu de OnBecomeHidden
            }
        }

        /// <summary>
        /// Force l'état hidden au démarrage (ignore la condition isVisible)
        /// </summary>
        private void ForceHidden()
        {
            if (!hasBeenInitialized) return;

            isVisible = false;
            targetAlpha = hiddenColor.a;
            currentAlpha = hiddenColor.a; // Applique immédiatement, pas de fade

            switch (visibilityBehavior)
            {
                case VisibilityBehavior.FadeAlpha:
                    ApplyAlpha(hiddenColor.a); // Applique directement
                    break;

                case VisibilityBehavior.HideCompletely:
                    SetRenderersEnabled(false);
                    break;

                case VisibilityBehavior.ShowOutline:
                    ApplyHiddenColor();
                    break;

                case VisibilityBehavior.NoChange:
                    // Rien à faire
                    break;
            }

            Debug.Log($"[VisionTarget] {gameObject.name} initialisé en mode HIDDEN");
        }

        private void Update()
        {
            // Animation de fade progressive
            if (visibilityBehavior == VisibilityBehavior.FadeAlpha && Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
            {
                currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
                ApplyAlpha(currentAlpha);
            }
        }

        #endregion

        #region Initialization

        private void InitializeRenderers()
        {
            // Récupère TOUS les SpriteRenderers enfants automatiquement
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

            if (spriteRenderers.Length == 0)
            {
                Debug.LogWarning($"[VisionTarget] Aucun SpriteRenderer trouvé sur {gameObject.name} ou ses enfants !");
                return;
            }

            // Sauvegarde les couleurs d'origine
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
            }

            hasBeenInitialized = true;
        }

        #endregion

        #region Visibility Control

        /// <summary>
        /// Appelé par VisionManager quand la cible devient visible
        /// </summary>
        public void OnBecomeVisible()
        {
            if (!hasBeenInitialized) return;
            if (isVisible) return; // Déjà visible

            isVisible = true;
            targetAlpha = 1f;

            switch (visibilityBehavior)
            {
                case VisibilityBehavior.FadeAlpha:
                    // Le fade se fait dans Update()
                    break;

                case VisibilityBehavior.HideCompletely:
                    SetRenderersEnabled(true);
                    break;

                case VisibilityBehavior.ShowOutline:
                    RestoreOriginalColors();
                    break;

                case VisibilityBehavior.NoChange:
                    // Rien à faire
                    break;
            }

            // Notifie les listeners
            OnBecameVisible?.Invoke(this);

            // Alerte si nécessaire
            if (triggerAlertWhenVisible)
            {
                TriggerVisibilityAlert();
            }
        }

        /// <summary>
        /// Appelé par VisionManager quand la cible n'est plus visible
        /// </summary>
        public void OnBecomeHidden()
        {
            if (!hasBeenInitialized) return;
            if (!isVisible && visibilityBehavior != VisibilityBehavior.NoChange) return; // Déjà caché

            isVisible = false;
            targetAlpha = hiddenColor.a;

            switch (visibilityBehavior)
            {
                case VisibilityBehavior.FadeAlpha:
                    // Le fade se fait dans Update()
                    break;

                case VisibilityBehavior.HideCompletely:
                    SetRenderersEnabled(false);
                    break;

                case VisibilityBehavior.ShowOutline:
                    ApplyHiddenColor();
                    break;

                case VisibilityBehavior.NoChange:
                    // Rien à faire
                    break;
            }

            // Notifie les listeners
            OnBecameHidden?.Invoke(this);
        }

        #endregion

        #region Visual Helpers

        private void SetRenderersEnabled(bool enabled)
        {
            foreach (var renderer in spriteRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
        }

        private void ApplyAlpha(float alpha)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null && originalColors != null && i < originalColors.Length)
                {
                    Color newColor = originalColors[i];
                    newColor.a = alpha;
                    spriteRenderers[i].color = newColor;
                }
            }
        }

        private void ApplyHiddenColor()
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null && originalColors != null && i < originalColors.Length)
                {
                    Color hidden = originalColors[i];
                    hidden.a = hiddenColor.a;
                    spriteRenderers[i].color = hidden;
                }
            }
        }

        private void RestoreOriginalColors()
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null && originalColors != null && i < originalColors.Length)
                {
                    spriteRenderers[i].color = originalColors[i];
                }
            }
        }

        #endregion

        #region Alerts

        private void TriggerVisibilityAlert()
        {
            switch (targetType)
            {
                case TargetType.Pedestrian:
                    // Alerte prioritaire pour les piétons
                    AudioManager.Instance?.PlaySFX("pedestrian_alert");
                    UIManager.Instance?.ShowViolationWarning(SafetyViolationType.PedestrianDanger);
                    Debug.Log($"[VisionTarget] ALERTE PIÉTON : {gameObject.name}");
                    break;

                case TargetType.Hazard:
                    AudioManager.Instance?.PlaySFX("hazard_alert");
                    Debug.Log($"[VisionTarget] ALERTE DANGER : {gameObject.name}");
                    break;

                case TargetType.Forklift:
                    AudioManager.Instance?.PlaySFX("forklift_alert");
                    Debug.Log($"[VisionTarget] Autre chariot détecté : {gameObject.name}");
                    break;

                case TargetType.Vehicle:
                    AudioManager.Instance?.PlaySFX("vehicle_alert");
                    Debug.Log($"[VisionTarget] Véhicule détecté : {gameObject.name}");
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retourne la distance par rapport à un point (généralement le joueur)
        /// </summary>
        public float GetDistanceFrom(Vector3 position)
        {
            return Vector3.Distance(transform.position, position);
        }

        /// <summary>
        /// Vérifie si la cible est dans la zone de danger
        /// </summary>
        public bool IsInDangerZone(Vector3 playerPosition)
        {
            return GetDistanceFrom(playerPosition) <= dangerDistance;
        }

        /// <summary>
        /// Change le type de cible à runtime (utile pour les tests)
        /// </summary>
        public void SetTargetType(TargetType newType)
        {
            targetType = newType;
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            // Affiche la zone de danger
            if (dangerDistance > 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, dangerDistance);
            }
        }

        #endregion
    }
}