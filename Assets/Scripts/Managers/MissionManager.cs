using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Managers
{
    /// <summary>
    /// MissionManager : Gère les missions, le score et la progression
    /// Refactorisé pour utiliser le pattern Manager
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        #region Singleton

        public static MissionManager Instance { get; private set; }

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

        #region Configuration

        [Header("Configuration Mission")] [SerializeField]
        private int basePointsPerDelivery = 100;

        [SerializeField] private int bonusTimePoints = 50;
        [SerializeField] private float missionTimeLimit = 300f; // 5 minutes
        [SerializeField] private int targetDeliveries = 5;

        [Header("Zones de dépôt")] [SerializeField]
        private List<DropZone> dropZones = new();

        #endregion

        #region State

        private int currentScore;
        private int palletsDelivered;
        private float missionTimer;
        private bool missionActive;

        // Propriétés publiques
        public int CurrentScore => currentScore;
        public int PalletsDelivered => palletsDelivered;
        public float RemainingTime => Mathf.Max(0, missionTimeLimit - missionTimer);
        public float MissionProgress => (float)palletsDelivered / targetDeliveries;
        public bool IsMissionActive => missionActive;

        #endregion

        #region Events

        [Header("Événements")] public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnPalletDelivered;
        public UnityEvent OnMissionComplete;
        public UnityEvent OnMissionFailed;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Auto-détection des zones si non assignées
            if (dropZones.Count == 0)
            {
                dropZones.AddRange(FindObjectsByType<DropZone>(FindObjectsSortMode.None));
            }

            // Abonnement aux événements des zones
            foreach (var zone in dropZones)
            {
                zone.onPalletDropped.AddListener(HandlePalletDelivered);
                zone.onPalletPickedUp.AddListener(HandlePalletRemoved);
            }
        }

        private void Update()
        {
            if (!missionActive) return;

            // Timer de mission
            missionTimer += Time.deltaTime;

            if (missionTimer >= missionTimeLimit)
            {
                EndMission(false);
            }
        }

        private void OnDestroy()
        {
            // Désabonnement propre
            foreach (var zone in dropZones)
            {
                if (zone != null)
                {
                    zone.onPalletDropped.RemoveListener(HandlePalletDelivered);
                    zone.onPalletPickedUp.RemoveListener(HandlePalletRemoved);
                }
            }
        }

        #endregion

        #region Mission Control

        public void StartMission()
        {
            missionActive = true;
            missionTimer = 0f;
            Debug.Log("[MissionManager] Mission démarrée !");
        }

        public void ResetMission()
        {
            currentScore = 0;
            palletsDelivered = 0;
            missionTimer = 0f;
            missionActive = false;

            OnScoreChanged?.Invoke(currentScore);
            Debug.Log("[MissionManager] Mission réinitialisée");
        }

        private void EndMission(bool success)
        {
            missionActive = false;

            if (success)
            {
                // Bonus temps restant
                int timeBonus = Mathf.FloorToInt(RemainingTime * 0.5f);
                AddScore(timeBonus);

                Debug.Log($"[MissionManager] Mission réussie ! Score final: {currentScore}");
                OnMissionComplete?.Invoke();
                GameManager.Instance?.EndGame(true);
            }
            else
            {
                Debug.Log("[MissionManager] Mission échouée - Temps écoulé");
                OnMissionFailed?.Invoke();
                GameManager.Instance?.EndGame(false);
            }
        }

        #endregion

        #region Score Management

        private void HandlePalletDelivered(Pallet pallet)
        {
            palletsDelivered++;

            // Calcul du score avec bonus potentiel
            int points = CalculateDeliveryPoints();
            AddScore(points);

            Debug.Log(
                $"[MissionManager] Palette livrée ! +{points} points. Total: {palletsDelivered}/{targetDeliveries}");

            OnPalletDelivered?.Invoke(palletsDelivered);

            // Vérifie si mission complète
            if (palletsDelivered >= targetDeliveries)
            {
                EndMission(true);
            }
        }

        private void HandlePalletRemoved(Pallet pallet)
        {
            // Optionnel: pénalité si on reprend une palette déjà livrée
            Debug.Log("[MissionManager] Palette reprise de la zone");
        }

        private int CalculateDeliveryPoints()
        {
            int points = basePointsPerDelivery;

            // Bonus si livraison rapide (dans le premier tiers du temps)
            if (missionTimer < missionTimeLimit / 3f)
            {
                points += bonusTimePoints;
            }

            // Bonus combo si plusieurs livraisons consécutives sans violation
            // (à connecter avec SafetyManager)

            return points;
        }

        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Applique une pénalité suite à une violation de sécurité
        /// </summary>
        public void ApplyPenalty(SafetyViolationType violation)
        {
            int penalty = GetPenaltyForViolation(violation);
            currentScore = Mathf.Max(0, currentScore - penalty);

            Debug.Log($"[MissionManager] Pénalité appliquée: -{penalty} points pour {violation}");
            OnScoreChanged?.Invoke(currentScore);
        }

        private int GetPenaltyForViolation(SafetyViolationType violation)
        {
            return violation switch
            {
                SafetyViolationType.SpeedExcess => 50,
                SafetyViolationType.NoHornAtIntersection => 30,
                SafetyViolationType.WrongVisionUsed => 40,
                SafetyViolationType.LoadTooHigh => 60,
                SafetyViolationType.NoSeatbelt => 20,
                SafetyViolationType.UnsafeReverse => 35,
                SafetyViolationType.PedestrianDanger => 100,
                _ => 25
            };
        }

        #endregion
    }
}