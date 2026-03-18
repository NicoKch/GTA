using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Managers
{
    /// <summary>
    /// UIManager : Gère toute l'interface utilisateur
    /// Centralise les mises à jour de l'UI pour éviter les références directes
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton

        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region UI References - HUD

        [Header("HUD - Gameplay")] [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI deliveriesText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image speedGauge;
        [SerializeField] private Image forkHeightIndicator;

        #endregion

        #region UI References - Panels

        [Header("Panneaux")] [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject missionCompletePanel;
        [SerializeField] private GameObject violationWarningPanel;

        [Header("Mission Complete")] [SerializeField]
        private TextMeshProUGUI finalScoreText;

        #endregion

        #region UI References - Warning

        [Header("Alertes de sécurité")] [SerializeField]
        private TextMeshProUGUI violationText;

        [SerializeField] private Image violationIcon;
        [SerializeField] private float warningDisplayDuration = 2f;

        private float warningTimer = 0f;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Abonnement aux événements du GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            // Abonnement aux événements du MissionManager
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnScoreChanged.AddListener(UpdateScore);
                MissionManager.Instance.OnPalletDelivered.AddListener(UpdateDeliveries);
            }

            // État initial
            Debug.Log($"[UIManager] Start — hudPanel: {hudPanel}, pausePanel: {pausePanel}");
            HideAllPanels();
            ShowHUD();
            Debug.Log($"[UIManager] Après ShowHUD — HUD actif: {hudPanel?.activeSelf}");
        }

        private void Update()
        {
            UpdateTimer();
            UpdateWarningVisibility();
        }

        private void OnDestroy()
        {
            // Désabonnement
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnScoreChanged.RemoveListener(UpdateScore);
                MissionManager.Instance.OnPalletDelivered.RemoveListener(UpdateDeliveries);
            }
        }

        #endregion

        #region HUD Updates

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateDeliveries(int count)
        {
            if (deliveriesText != null)
            {
                int target = 5; // À récupérer du MissionManager
                deliveriesText.text = $"Livraisons: {count}/{target}";
            }

            if (progressBar != null && MissionManager.Instance != null)
            {
                progressBar.value = MissionManager.Instance.MissionProgress;
            }
        }

        private void UpdateTimer()
        {
            if (timerText == null || MissionManager.Instance == null) return;

            float remaining = MissionManager.Instance.RemainingTime;
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";

            // Change de couleur si temps critique
            if (remaining < 30f)
            {
                timerText.color = Color.red;
            }
            else if (remaining < 60f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        /// <summary>
        /// Met à jour l'indicateur de vitesse
        /// </summary>
        public void UpdateSpeedGauge(float normalizedSpeed)
        {
            if (speedGauge != null)
            {
                speedGauge.fillAmount = Mathf.Clamp01(normalizedSpeed);

                // Rouge si trop rapide
                speedGauge.color = normalizedSpeed > 0.8f ? Color.red : Color.green;
            }
        }

        /// <summary>
        /// Met à jour l'indicateur de hauteur des fourches
        /// </summary>
        public void UpdateForkHeight(float normalizedHeight)
        {
            if (forkHeightIndicator != null)
            {
                forkHeightIndicator.fillAmount = Mathf.Clamp01(normalizedHeight);
            }
        }

        #endregion

        #region Warning System

        public void ShowViolationWarning(SafetyViolationType violation)
        {
            if (violationWarningPanel == null) return;

            violationWarningPanel.SetActive(true);
            warningTimer = warningDisplayDuration;

            if (violationText != null)
            {
                violationText.text = GetViolationMessage(violation);
            }

            Debug.Log($"[UIManager] Affichage alerte: {violation}");
        }

        private void UpdateWarningVisibility()
        {
            if (violationWarningPanel == null || !violationWarningPanel.activeSelf) return;

            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
            {
                violationWarningPanel.SetActive(false);
            }
        }

        private string GetViolationMessage(SafetyViolationType violation)
        {
            return violation switch
            {
                SafetyViolationType.SpeedExcess => "⚠️ EXCÈS DE VITESSE !",
                SafetyViolationType.NoHornAtIntersection => "⚠️ KLAXON OBLIGATOIRE AU CARREFOUR !",
                SafetyViolationType.WrongVisionUsed => "⚠️ UTILISEZ LA VISION ARRIÈRE !",
                SafetyViolationType.LoadTooHigh => "⚠️ BAISSEZ LA CHARGE AVANT DE ROULER !",
                SafetyViolationType.NoSeatbelt => "⚠️ CEINTURE DE SÉCURITÉ !",
                SafetyViolationType.UnsafeReverse => "⚠️ MARCHE ARRIÈRE DANGEREUSE !",
                SafetyViolationType.PedestrianDanger => "⚠️ ATTENTION AUX PIÉTONS !",
                _ => "⚠️ VIOLATION DE SÉCURITÉ !"
            };
        }

        #endregion

        #region Panel Management

        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            HideAllPanels();

            switch (newState)
            {
                case GameManager.GameState.Playing:
                    ShowHUD();
                    break;
                case GameManager.GameState.Paused:
                    ShowHUD();
                    ShowPause();
                    break;
                case GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;
                case GameManager.GameState.MissionComplete:
                    ShowMissionComplete();
                    break;
            }
        }

        private void HideAllPanels()
        {
            hudPanel?.SetActive(false);
            pausePanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            missionCompletePanel?.SetActive(false);
            violationWarningPanel?.SetActive(false);
        }

        private void ShowHUD()
        {
            hudPanel?.SetActive(true);
        }

        private void ShowPause()
        {
            pausePanel?.SetActive(true);
        }

        private void ShowGameOver()
        {
            gameOverPanel?.SetActive(true);
        }

        private void ShowMissionComplete()
        {
            missionCompletePanel?.SetActive(true);

            if (MissionManager.Instance != null && finalScoreText != null)
            {
                finalScoreText.text = $"Score : {MissionManager.Instance.CurrentScore}";
            }
        }

        #endregion

        #region Button Handlers (à connecter via l'Inspector)

        public void OnPauseButtonClicked()
        {
            GameManager.Instance?.PauseGame();
        }

        public void OnResumeButtonClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        public void OnRestartButtonClicked()
        {
            Time.timeScale = 1f;
            GameManager.Instance?.RestartGame();
        }

        public void OnMainMenuButtonClicked()
        {
            Time.timeScale = 1f;
            SceneFader.GetOrCreate().FadeToScene("MainMenu");
        }

        public void OnNextLevelButtonClicked()
        {
            Time.timeScale = 1f;
            int nextIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            if (nextIndex < sceneCount)
                SceneFader.GetOrCreate().FadeToScene(nextIndex);
            else
                SceneFader.GetOrCreate().FadeToScene("MainMenu");
        }

        public void OnQuitButtonClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}