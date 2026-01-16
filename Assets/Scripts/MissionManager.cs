using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [Header("Zones de dépôt")] [SerializeField]
    private DropZone[] dropZones;

    [Header("Score")] [SerializeField] private int pointsPerDelivery = 100;

    private int currentScore = 0;
    private int palletesDelivered = 0;

    public int CurrentScore => currentScore;
    public int PalletesDelivered => palletesDelivered;

    private void Start()
    {
        // S'abonne aux événements de chaque zone
        foreach (var zone in dropZones)
        {
            zone.onPalletDropped.AddListener(OnPalletDelivered);
            zone.onPalletPickedUp.AddListener(OnPalletRemoved);
        }
    }

    private void OnDestroy()
    {
        // Se désabonne proprement
        foreach (var zone in dropZones)
        {
            if (zone != null)
            {
                zone.onPalletDropped.RemoveListener(OnPalletDelivered);
                zone.onPalletPickedUp.RemoveListener(OnPalletRemoved);
            }
        }
    }

    private void OnPalletDelivered(Pallet pallet)
    {
        palletesDelivered++;
        currentScore += pointsPerDelivery;

        Debug.Log($"[MissionManager] Palette livrée ! Score: {currentScore}, Total livré: {palletesDelivered}");

        // TODO: Mettre à jour l'UI
        // UIManager.Instance.UpdateScore(currentScore);
    }

    private void OnPalletRemoved(Pallet pallet)
    {
        // Optionnel : pénalité si on reprend une palette déjà livrée ?
        Debug.Log($"[MissionManager] Palette reprise de la zone");
    }
}