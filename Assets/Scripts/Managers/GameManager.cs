using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// GameManager : Chef d'orchestre central du jeu GTA Forklift
    /// Coordonne tous les sous-systèmes via le pattern Singleton
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu") return;

            // Recharge les références qui appartiennent à la scène
            missionManager = FindFirstObjectByType<MissionManager>();
            safetyManager  = FindFirstObjectByType<SafetyManager>();
            uiManager      = FindFirstObjectByType<UIManager>();
            audioManager   = FindFirstObjectByType<AudioManager>();

            // Remet le jeu en état Playing (reset depuis une éventuelle pause)
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            if (autoStartGame)
                StartGame();
        }

        #endregion

        #region Game State

        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver,
            MissionComplete
        }

        [Header("État du jeu")] [SerializeField]
        private GameState currentState = GameState.Playing; // Démarre en Playing pour les tests

        [SerializeField] private bool autoStartGame = true; // Démarre automatiquement la partie
        public GameState CurrentState => currentState;

        #endregion

        #region Manager References

        [Header("Références aux Managers")] [SerializeField]
        private MissionManager missionManager;

        [SerializeField] private SafetyManager safetyManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        // Accesseurs publics
        public MissionManager Mission => missionManager;
        public SafetyManager Safety => safetyManager;
        public UIManager UI => uiManager;
        public AudioManager Audio => audioManager;

        #endregion

        #region Events

        // Événements pour notifier les changements d'état
        public delegate void GameStateChanged(GameState newState);

        public event GameStateChanged OnGameStateChanged;

        public delegate void GamePaused(bool isPaused);

        public event GamePaused OnGamePaused;

        #endregion

        #region Initialization

        private void InitializeGame()
        {
            Debug.Log("[GameManager] Initialisation du jeu...");

            // Auto-détection des managers si non assignés
            if (missionManager == null)
                missionManager = FindFirstObjectByType<MissionManager>();
            if (safetyManager == null)
                safetyManager = FindFirstObjectByType<SafetyManager>();
            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();
            if (audioManager == null)
                audioManager = FindFirstObjectByType<AudioManager>();

            // Auto-start si configuré
            if (autoStartGame)
            {
                StartGame();
            }
        }

        #endregion

        #region Game Flow

        public void StartGame()
        {
            Debug.Log("[GameManager] Démarrage de la partie");
            SetGameState(GameState.Playing);

            // Réinitialise les systèmes
            missionManager?.ResetMission();
            missionManager?.StartMission();
            safetyManager?.ResetViolations();
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;

            Debug.Log("[GameManager] Jeu en pause");
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(true);
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;

            Debug.Log("[GameManager] Reprise du jeu");
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
            OnGamePaused?.Invoke(false);
        }

        public void EndGame(bool success)
        {
            Debug.Log($"[GameManager] Fin de partie - Succès: {success}");
            SetGameState(success ? GameState.MissionComplete : GameState.GameOver);
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        private void SetGameState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] État changé: {newState}");
        }

        #endregion

        #region Safety Events (appelés par SafetyManager)

        public void OnSafetyViolation(SafetyViolationType violation)
        {
            Debug.Log($"[GameManager] Violation de sécurité: {violation}");

            // Déclenche la pénalité via MissionManager
            missionManager?.ApplyPenalty(violation);

            // Feedback audio/visuel
            audioManager?.PlaySFX("violation_warning");
            uiManager?.ShowViolationWarning(violation);
        }

        public void OnAccident()
        {
            Debug.Log("[GameManager] ACCIDENT ! Fin de mission immédiate");
            EndGame(false);
        }

        #endregion
    }

    /// <summary>
    /// Types de violations de sécurité CACES
    /// </summary>
    public enum SafetyViolationType
    {
        SpeedExcess, // Excès de vitesse
        NoHornAtIntersection, // Pas de klaxon au carrefour
        WrongVisionUsed, // Mauvais champ de vision utilisé
        LoadTooHigh, // Charge trop haute en déplacement
        NoSeatbelt, // Pas de ceinture
        UnsafeReverse, // Marche arrière dangereuse
        PedestrianDanger // Mise en danger de piéton
    }
}