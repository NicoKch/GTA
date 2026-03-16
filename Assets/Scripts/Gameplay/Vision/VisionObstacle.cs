using UnityEngine;

namespace Gameplay.Vision
{
    /// <summary>
    /// VisionObstacle : Composant pour les objets qui bloquent la vision
    /// À attacher aux murs, étagères, et à la palette chargée sur les fourches
    /// Intégré avec le système de vision Manager
    /// </summary>
    public class VisionObstacle : MonoBehaviour
    {
        #region Configuration

        [Header("Collider")] [SerializeField] private Collider2D obstacleCollider;

        [Header("Comportement")] [Tooltip("Si true, l'obstacle est actif dès le départ")] [SerializeField]
        private bool activeOnStart = true;

        [Tooltip("Si true, bloque complètement la vision. Sinon, réduit juste la distance.")] [SerializeField]
        private bool fullyBlocking = true;

        [Header("Visuel (optionnel)")] [Tooltip("Sprite à afficher quand l'obstacle bloque la vue")] [SerializeField]
        private SpriteRenderer blockingIndicator;

        #endregion

        #region State

        private bool isActive = false;

        public bool IsActive => isActive;
        public bool IsFullyBlocking => fullyBlocking;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Auto-détection du collider si non assigné
            if (obstacleCollider == null)
            {
                obstacleCollider = GetComponent<Collider2D>();
            }

            if (obstacleCollider == null)
            {
                Debug.LogWarning($"[VisionObstacle] Pas de Collider2D trouvé sur {gameObject.name}");
            }
        }

        private void Start()
        {
            // État initial
            SetObstacleActive(activeOnStart);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Active ou désactive l'obstacle de vision
        /// Appelé notamment par ForkController quand une palette est chargée/déchargée
        /// </summary>
        public void SetObstacleActive(bool active)
        {
            isActive = active;

            if (obstacleCollider != null)
            {
                obstacleCollider.enabled = active;
            }

            // Indicateur visuel optionnel
            if (blockingIndicator != null)
            {
                blockingIndicator.enabled = active;
            }

            Debug.Log($"[VisionObstacle] {gameObject.name} - Actif: {active}");
        }

        /// <summary>
        /// Bascule l'état de l'obstacle
        /// </summary>
        public void ToggleObstacle()
        {
            SetObstacleActive(!isActive);
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            // Affiche un indicateur visuel dans l'éditeur
            Gizmos.color = isActive ? new Color(1f, 0f, 0f, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);

            if (obstacleCollider != null)
            {
                Gizmos.DrawWireCube(obstacleCollider.bounds.center, obstacleCollider.bounds.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
        }

        #endregion
    }
}