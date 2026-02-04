using UnityEngine;

/// <summary>
/// Composant pour les objets qui bloquent la vision.
/// À attacher aux murs, étagères, et à la palette quand elle est chargée.
/// </summary>
public class VisionObstacle : MonoBehaviour
{
    [SerializeField] private Collider2D obstacleCollider;

    private void Awake()
    {
        // S'assurer que l'objet est sur le bon layer
        if (obstacleCollider == null)
        {
            obstacleCollider = GetComponent<Collider2D>();
        }
    }

    public void SetObstacleActive(bool active)
    {
        if (obstacleCollider != null)
        {
            obstacleCollider.enabled = active;
        }
    }
}