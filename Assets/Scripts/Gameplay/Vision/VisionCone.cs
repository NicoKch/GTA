using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Vision
{
    /// <summary>
    /// VisionCone : Représente un cône de vision (avant ou arrière)
    /// Gère le mesh visuel et la détection des cibles
    /// Utilisé par VisionManager
    /// </summary>
    public class VisionCone : MonoBehaviour
    {
        #region Configuration

        [Header("Visuel")] [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material activeConeMaterial;
        [SerializeField] private Material inactiveConeMaterial;

        [Header("Couleurs")] [SerializeField] private Color activeColor = new Color(0f, 1f, 0.4f, 0.3f);
        [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.15f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.3f, 0f, 0.4f);

        [Header("Debug")] [SerializeField] private bool showDebugLogs = false;

        #endregion

        #region Runtime State

        // Paramètres configurés par VisionManager
        private float angle;
        private float distance;
        private int rayCount;
        private LayerMask obstacleLayer;
        private LayerMask detectableLayer;

        private Mesh coneMesh;
        private bool isActive = false;
        private bool isInitialized = false;

        private HashSet<VisionTarget> visibleTargets = new HashSet<VisionTarget>();

        // Cache pour éviter les allocations
        private Vector3[] vertices;
        private int[] triangles;
        private Color[] colors;

        #endregion

        #region Properties

        public bool IsActive => isActive;
        public int VisibleTargetCount => visibleTargets.Count;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialise le cône avec les paramètres du VisionManager
        /// </summary>
        public void Initialize(float angle, float distance, int rayCount,
            LayerMask obstacleLayer, LayerMask detectableLayer)
        {
            this.angle = angle;
            this.distance = distance;
            this.rayCount = rayCount;
            this.obstacleLayer = obstacleLayer;
            this.detectableLayer = detectableLayer;

            // Crée le mesh
            coneMesh = new Mesh();
            coneMesh.name = $"VisionCone_{gameObject.name}";

            if (meshFilter != null)
            {
                meshFilter.mesh = coneMesh;
            }

            // Pré-alloue les tableaux pour éviter les allocations répétées
            vertices = new Vector3[rayCount + 2];
            triangles = new int[rayCount * 3];
            colors = new Color[rayCount + 2];

            // Pré-calcule les triangles (ne changent jamais)
            for (int i = 0; i < rayCount; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            isInitialized = true;

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[VisionCone] {gameObject.name} initialisé - Angle: {angle}°, Distance: {distance}, Rays: {rayCount}");
            }
        }

        #endregion

        #region Activation

        /// <summary>
        /// Active ou désactive le cône de vision
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            if (meshRenderer != null)
            {
                meshRenderer.material = active ? activeConeMaterial : inactiveConeMaterial;
                meshRenderer.enabled = true; // Toujours visible mais avec matériau différent
            }

            if (!active)
            {
                // Notifie que toutes les cibles ne sont plus visibles
                foreach (var target in visibleTargets)
                {
                    target?.OnBecomeHidden();
                }

                visibleTargets.Clear();
            }

            if (showDebugLogs)
            {
                Debug.Log($"[VisionCone] {gameObject.name} - Active: {active}");
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!isInitialized) return;

            UpdateConeMesh();

            if (isActive)
            {
                UpdateVisibleTargets();
            }
        }

        #endregion

        #region Mesh Generation

        private void UpdateConeMesh()
        {
            Color currentColor = isActive ? activeColor : inactiveColor;

            // Point central (origine du cône)
            vertices[0] = Vector3.zero;
            colors[0] = currentColor;

            float halfAngle = angle / 2f;
            float angleStep = angle / rayCount;

            for (int i = 0; i <= rayCount; i++)
            {
                float currentAngle = -halfAngle + (angleStep * i);
                Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * transform.up;

                // Raycast pour détecter les obstacles
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

                float rayDistance = hit.collider != null ? hit.distance : distance;

                // Convertir en espace local
                vertices[i + 1] = Quaternion.Euler(0, 0, currentAngle) * Vector3.up * rayDistance;

                // Couleur différente si obstacle détecté (optionnel)
                colors[i + 1] = currentColor;
            }

            // Met à jour le mesh
            coneMesh.Clear();
            coneMesh.vertices = vertices;
            coneMesh.triangles = triangles;
            coneMesh.colors = colors;
            coneMesh.RecalculateNormals();
            coneMesh.RecalculateBounds();
        }

        #endregion

        #region Target Detection

        private void UpdateVisibleTargets()
        {
            visibleTargets.Clear();

            float halfAngle = angle / 2f;

            // Trouver tous les objets détectables dans la portée
            Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(
                transform.position, distance, detectableLayer);

            foreach (var collider in potentialTargets)
            {
                if (collider == null) continue;

                VisionTarget target = collider.GetComponent<VisionTarget>();
                if (target == null) continue;

                Vector2 directionToTarget = (collider.transform.position - transform.position).normalized;
                float angleToTarget = Vector2.SignedAngle(transform.up, directionToTarget);

                // Vérifier si dans l'angle du cône
                if (Mathf.Abs(angleToTarget) > halfAngle) continue;

                float distanceToTarget = Vector2.Distance(transform.position, collider.transform.position);

                // Raycast pour vérifier qu'aucun obstacle ne bloque la vue
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    directionToTarget,
                    distanceToTarget,
                    obstacleLayer);

                // Si pas d'obstacle entre nous et la cible, elle est visible
                if (hit.collider == null)
                {
                    visibleTargets.Add(target);
                }
            }
        }

        /// <summary>
        /// Retourne une copie du set des cibles visibles
        /// </summary>
        public HashSet<VisionTarget> GetVisibleTargets()
        {
            return new HashSet<VisionTarget>(visibleTargets);
        }

        /// <summary>
        /// Vérifie si une cible spécifique est visible par ce cône
        /// </summary>
        public bool CanSeeTarget(VisionTarget target)
        {
            return isActive && visibleTargets.Contains(target);
        }

        #endregion

        #region Color Control

        /// <summary>
        /// Change temporairement la couleur du cône (pour les alertes)
        /// </summary>
        public void SetDangerMode(bool isDanger)
        {
            if (!isActive) return;

            Color targetColor = isDanger ? dangerColor : activeColor;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = targetColor;
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!isInitialized && angle == 0) return;

            Gizmos.color = isActive ? Color.green : Color.gray;

            float displayAngle = angle > 0 ? angle : 80f;
            float displayDistance = distance > 0 ? distance : 15f;

            float halfAngle = displayAngle / 2f;
            Vector3 leftBound = Quaternion.Euler(0, 0, -halfAngle) * transform.up * displayDistance;
            Vector3 rightBound = Quaternion.Euler(0, 0, halfAngle) * transform.up * displayDistance;

            Gizmos.DrawLine(transform.position, transform.position + leftBound);
            Gizmos.DrawLine(transform.position, transform.position + rightBound);

            // Arc de cercle (approximation)
            int segments = 20;
            float angleStep = displayAngle / segments;
            Vector3 prevPoint = transform.position + leftBound;

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (angleStep * i);
                Vector3 point = transform.position +
                                Quaternion.Euler(0, 0, currentAngle) * transform.up * displayDistance;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }

        #endregion
    }
}