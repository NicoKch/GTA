using UnityEngine;

namespace Gameplay.NPC
{
    /// <summary>
    /// ForkliftWaypoint : Point de passage pour les NPC forklifts
    /// </summary>
    public class ForkliftWaypoint : MonoBehaviour
    {
        [Header("Comportement")]
        [Tooltip("Temps d'attente à ce waypoint (0 = pas d'attente)")]
        [SerializeField] private float waitTime = 0f;

        [Tooltip("Limite de vitesse à l'approche de ce waypoint (0 = pas de limite)")]
        [SerializeField] private float speedLimit = 0f;

        [Header("Connexions")]
        [Tooltip("Waypoint suivant (pour visualisation uniquement si le NPC utilise un tableau)")]
        [SerializeField] private ForkliftWaypoint nextWaypoint;

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.cyan;
        [SerializeField] private float gizmoRadius = 0.5f;

        // Propriétés publiques
        public float WaitTime => waitTime;
        public float SpeedLimit => speedLimit;
        public ForkliftWaypoint NextWaypoint => nextWaypoint;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);

            // Dessine une flèche vers le waypoint suivant
            if (nextWaypoint != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = (nextWaypoint.transform.position - transform.position).normalized;
                Gizmos.DrawLine(transform.position, nextWaypoint.transform.position);

                // Pointe de flèche
                Vector3 arrowEnd = nextWaypoint.transform.position - direction * 0.5f;
                Vector3 right = Quaternion.Euler(0, 0, 30) * -direction * 0.3f;
                Vector3 left = Quaternion.Euler(0, 0, -30) * -direction * 0.3f;
                Gizmos.DrawLine(nextWaypoint.transform.position, arrowEnd + right);
                Gizmos.DrawLine(nextWaypoint.transform.position, arrowEnd + left);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Affiche plus d'infos quand sélectionné
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, gizmoRadius * 0.3f);
        }
    }
}