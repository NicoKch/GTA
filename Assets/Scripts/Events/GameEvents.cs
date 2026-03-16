using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    /// <summary>
    /// GameEvents : Système d'événements centralisé pour la communication entre scripts
    /// Évite le couplage fort entre les composants
    /// Usage: GameEvents.OnPalletDelivered?.Invoke(pallet);
    /// </summary>
    public static class GameEvents
    {
        #region Gameplay Events

        /// <summary>Déclenché quand une palette est ramassée</summary>
        public static UnityAction<Pallet> OnPalletPickedUp;

        /// <summary>Déclenché quand une palette est déposée</summary>
        public static UnityAction<Pallet> OnPalletDropped;

        /// <summary>Déclenché quand une palette est livrée dans une zone</summary>
        public static UnityAction<Pallet, DropZone> OnPalletDelivered;

        /// <summary>Déclenché quand le chariot entre dans une zone de dépôt</summary>
        public static UnityAction<DropZone> OnEnteredDropZone;

        /// <summary>Déclenché quand le chariot entre dans un carrefour</summary>
        public static UnityAction OnEnteredIntersection;

        #endregion

        #region Safety Events

        /// <summary>Déclenché lors d'une violation de sécurité</summary>
        public static UnityAction<SafetyViolationType, int> OnSafetyViolation;

        /// <summary>Déclenché lors d'un accident</summary>
        public static UnityAction<string> OnAccident;

        /// <summary>Déclenché quand le klaxon est utilisé</summary>
        public static UnityAction OnHornUsed;

        #endregion

        #region Score Events

        /// <summary>Déclenché quand le score change</summary>
        public static UnityAction<int> OnScoreChanged;

        /// <summary>Déclenché quand une pénalité est appliquée</summary>
        public static UnityAction<int> OnPenaltyApplied;

        /// <summary>Déclenché quand un bonus est obtenu</summary>
        public static UnityAction<int, string> OnBonusEarned;

        #endregion

        #region Mission Events

        /// <summary>Déclenché quand une mission commence</summary>
        public static UnityAction OnMissionStarted;

        /// <summary>Déclenché quand une mission est réussie</summary>
        public static UnityAction<int> OnMissionCompleted; // int = score final

        /// <summary>Déclenché quand une mission échoue</summary>
        public static UnityAction<string> OnMissionFailed; // string = raison

        /// <summary>Déclenché quand le temps restant est critique</summary>
        public static UnityAction<float> OnTimeCritical; // float = temps restant

        #endregion

        #region Player Events

        /// <summary>Déclenché quand le joueur commence à bouger</summary>
        public static UnityAction OnPlayerStartedMoving;

        /// <summary>Déclenché quand le joueur s'arrête</summary>
        public static UnityAction OnPlayerStopped;

        /// <summary>Déclenché quand le joueur commence la marche arrière</summary>
        public static UnityAction OnPlayerStartedReversing;

        #endregion

        #region Vision Events

        /// <summary>Déclenché quand le mode de vision change</summary>
        public static UnityAction<VisionMode> OnVisionModeChanged;

        /// <summary>Déclenché quand la vision est obstruée par une charge</summary>
        public static UnityAction<bool> OnVisionObstructed;

        #endregion

        #region UI Events

        /// <summary>Déclenché pour afficher un message à l'écran</summary>
        public static UnityAction<string, float> OnShowMessage; // message, durée

        /// <summary>Déclenché pour afficher un tutoriel</summary>
        public static UnityAction<string> OnShowTutorial;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Réinitialise tous les événements (utile entre les scènes)
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPalletPickedUp = null;
            OnPalletDropped = null;
            OnPalletDelivered = null;
            OnEnteredDropZone = null;
            OnEnteredIntersection = null;

            OnSafetyViolation = null;
            OnAccident = null;
            OnHornUsed = null;

            OnScoreChanged = null;
            OnPenaltyApplied = null;
            OnBonusEarned = null;

            OnMissionStarted = null;
            OnMissionCompleted = null;
            OnMissionFailed = null;
            OnTimeCritical = null;

            OnPlayerStartedMoving = null;
            OnPlayerStopped = null;
            OnPlayerStartedReversing = null;

            OnVisionModeChanged = null;
            OnVisionObstructed = null;

            OnShowMessage = null;
            OnShowTutorial = null;

            Debug.Log("[GameEvents] Tous les événements ont été réinitialisés");
        }

        #endregion
    }

    /// <summary>
    /// Modes de vision disponibles pour le conducteur
    /// </summary>
    public enum VisionMode
    {
        Forward, // Vision avant (normale)
        Rear, // Vision arrière (rétroviseur/caméra)
        Panoramic // Vision 360° (pour les manœuvres)
    }
}