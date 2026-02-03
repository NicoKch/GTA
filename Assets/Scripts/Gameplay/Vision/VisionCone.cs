using UnityEngine;
using System.Collections.Generic;

public class VisionCone : MonoBehaviour
{
    [Header("Visuel")] [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material activeConeMaterial;
    [SerializeField] private Material inactiveConeMaterial;

    // Paramètres configurés par VisionManager
    private float angle;
    private float distance;
    private int rayCount;
    private LayerMask obstacleLayer;
    private LayerMask detectableLayer;

    private Mesh coneMesh;
    private bool isActive = false;
    private HashSet<VisionTarget> visibleTargets = new HashSet<VisionTarget>();

    public void Initialize(float angle, float distance, int rayCount,
        LayerMask obstacleLayer, LayerMask detectableLayer)
    {
        this.angle = angle;
        this.distance = distance;
        this.rayCount = rayCount;
        this.obstacleLayer = obstacleLayer;
        this.detectableLayer = detectableLayer;

        coneMesh = new Mesh();
        if (meshFilter != null)
        {
            meshFilter.mesh = coneMesh;
        }
    }

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
            visibleTargets.Clear();
        }
    }

    private void Update()
    {
        UpdateConeMesh();

        if (isActive)
        {
            UpdateVisibleTargets();
        }
    }

    private void UpdateConeMesh()
    {
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        // Point central (origine du cône)
        vertices[0] = Vector3.zero;

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
        }

        // Créer les triangles
        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
    }

    private void UpdateVisibleTargets()
    {
        visibleTargets.Clear();

        float halfAngle = angle / 2f;

        // Trouver tous les objets détectables dans la portée
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(
            transform.position, distance, detectableLayer);

        foreach (var collider in potentialTargets)
        {
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

    public HashSet<VisionTarget> GetVisibleTargets()
    {
        return new HashSet<VisionTarget>(visibleTargets);
    }

    // Debug visuel dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.green : Color.gray;

        float halfAngle = angle / 2f;
        Vector3 leftBound = Quaternion.Euler(0, 0, -halfAngle) * transform.up * distance;
        Vector3 rightBound = Quaternion.Euler(0, 0, halfAngle) * transform.up * distance;

        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}