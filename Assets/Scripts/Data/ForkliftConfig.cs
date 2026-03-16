using UnityEngine;

namespace Data
{
    /// <summary>
    /// ForkliftConfig : Configuration centralisée du chariot élévateur
    /// Utilise ScriptableObject pour faciliter le balancing et les tests
    /// </summary>
    [CreateAssetMenu(fileName = "ForkliftConfig", menuName = "GTA Forklift/Forklift Configuration")]
    public class ForkliftConfig : ScriptableObject
    {
        [Header("=== MOUVEMENT ===")] [Tooltip("Vitesse maximale en avant (unités/sec)")] [Range(1f, 20f)]
        public float maxSpeed = 5f;

        [Tooltip("Vitesse maximale en marche arrière")] [Range(1f, 10f)]
        public float maxReverseSpeed = 3f;

        [Tooltip("Force d'accélération")] [Range(1f, 10f)]
        public float acceleration = 3f;

        [Tooltip("Force de décélération naturelle (inertie)")] [Range(1f, 10f)]
        public float deceleration = 5f;

        [Tooltip("Force de freinage actif")] [Range(5f, 20f)]
        public float brakeForce = 8f;

        [Header("=== DIRECTION ===")] [Tooltip("Angle maximum des roues directrices")] [Range(30f, 90f)]
        public float maxWheelAngle = 60f;

        [Tooltip("Vitesse de rotation du volant")] [Range(60f, 240f)]
        public float wheelTurnSpeed = 120f;

        [Tooltip("Empattement du chariot (distance entre essieux)")] [Range(1f, 5f)]
        public float wheelBase = 2f;

        [Header("=== FOURCHES ===")] [Tooltip("Hauteur minimale des fourches")]
        public float forkMinHeight = 0f;

        [Tooltip("Hauteur maximale des fourches")] [Range(1f, 5f)]
        public float forkMaxHeight = 3f;

        [Tooltip("Vitesse de levage des fourches")] [Range(0.5f, 5f)]
        public float forkLiftSpeed = 2f;

        [Tooltip("Hauteur pour attacher une palette")] [Range(0.1f, 0.5f)]
        public float attachThreshold = 0.15f;

        [Tooltip("Hauteur pour détacher une palette")] [Range(0.1f, 0.5f)]
        public float detachThreshold = 0.20f;

        [Header("=== SÉCURITÉ (CACES) ===")] [Tooltip("Vitesse max autorisée avec charge (km/h)")] [Range(1f, 10f)]
        public float maxSpeedWithLoad = 5f;

        [Tooltip("Vitesse max autorisée sans charge (km/h)")] [Range(5f, 15f)]
        public float maxSpeedWithoutLoad = 10f;

        [Tooltip("Hauteur max des fourches en mouvement")] [Range(0.1f, 1f)]
        public float maxForkHeightWhileMoving = 0.3f;

        [Header("=== CAPACITÉS ===")] [Tooltip("Capacité de charge maximale (kg)")]
        public float maxLoadCapacity = 2000f;

        [Tooltip("Rayon de braquage minimum (mètres)")]
        public float minTurningRadius = 2f;

        #region Validation

        private void OnValidate()
        {
            // S'assure que les seuils sont cohérents
            if (detachThreshold < attachThreshold)
            {
                detachThreshold = attachThreshold + 0.05f;
            }

            // S'assure que la vitesse avec charge est inférieure à sans charge
            if (maxSpeedWithLoad > maxSpeedWithoutLoad)
            {
                maxSpeedWithLoad = maxSpeedWithoutLoad;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retourne la vitesse max en fonction de l'état de charge
        /// </summary>
        public float GetMaxSpeed(bool hasLoad)
        {
            return hasLoad ? maxSpeedWithLoad : maxSpeedWithoutLoad;
        }

        /// <summary>
        /// Vérifie si une vitesse donnée est dans les limites autorisées
        /// </summary>
        public bool IsSpeedSafe(float currentSpeed, bool hasLoad)
        {
            float maxAllowed = GetMaxSpeed(hasLoad);
            return Mathf.Abs(currentSpeed) <= maxAllowed;
        }

        /// <summary>
        /// Vérifie si la hauteur des fourches est sécuritaire pour se déplacer
        /// </summary>
        public bool IsForkHeightSafeForMovement(float currentHeight)
        {
            return currentHeight <= maxForkHeightWhileMoving;
        }

        #endregion
    }
}