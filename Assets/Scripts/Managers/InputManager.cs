using UnityEngine;

namespace Managers
{
    /// <summary>
    /// InputManager : Gestion centralisée des entrées utilisateur
    /// Singleton MonoBehaviour pour plus de flexibilité que la version statique
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Singleton

        public static InputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeInputs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Input Actions

        private PlayerInputAction inputActions;
        public PlayerInputAction InputActions => inputActions;

        // Raccourcis pour un accès facile
        public float MoveInput => inputActions.Player.move.ReadValue<float>();
        public float RotateInput => inputActions.Player.rotate.ReadValue<float>();
        public float LiftInput => inputActions.Player.lift.ReadValue<float>();

        public float switchViewInput => inputActions.Player.switchView.ReadValue<float>();

        // Pour le klaxon (si ajouté dans PlayerInputAction)
        public bool HornPressed { get; private set; }

        #endregion

        #region Initialization

        private void InitializeInputs()
        {
            inputActions = new PlayerInputAction();
            inputActions.Player.Enable();

            Debug.Log("[InputManager] Inputs initialisés");

            // Abonnement aux événements spécifiques si besoin
            // inputActions.Player.horn.performed += ctx => OnHornPressed();
        }

        #endregion

        #region Enable/Disable

        public void EnablePlayerInputs()
        {
            inputActions?.Player.Enable();
        }

        public void DisablePlayerInputs()
        {
            inputActions?.Player.Disable();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.Disable();
                inputActions.Dispose();
                inputActions = null;
            }
        }

        private void OnApplicationQuit()
        {
            OnDestroy();
        }

        #endregion
    }
}