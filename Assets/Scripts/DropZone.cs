using UnityEngine;
using UnityEngine.Events;

public class DropZone : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private string zoneName = "Zone de dépôt";

    [SerializeField] private Color zoneColor = new Color(0, 1, 0, 0.3f); // Vert transparent
    [SerializeField] private Color zoneActiveColor = new Color(0, 1, 0, 0.6f); // Vert plus visible

    [Header("Événements")] public UnityEvent<Pallet> onPalletDropped; // Déclenché quand une palette est déposée
    public UnityEvent<Pallet> onPalletPickedUp; // Déclenché quand une palette est reprise

    private SpriteRenderer spriteRenderer;
    private Pallet palletInZone;
    private bool hasPalletDeposited;

    public bool HasPallet => hasPalletDeposited;
    public Pallet CurrentPallet => palletInZone;
    public string ZoneName => zoneName;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = zoneColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si c'est une palette
        Pallet pallet = other.GetComponentInParent<Pallet>();
        if (pallet != null)
        {
            Debug.Log($"[DropZone] {zoneName} - Palette '{pallet.name}' entre dans la zone");
            palletInZone = pallet;
            UpdateVisual();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Pallet pallet = other.GetComponentInParent<Pallet>();
        if (pallet != null && pallet == palletInZone)
        {
            Debug.Log($"[DropZone] {zoneName} - Palette '{pallet.name}' sort de la zone");

            // Si la palette était déposée et qu'elle part, c'est qu'on l'a reprise
            if (hasPalletDeposited)
            {
                hasPalletDeposited = false;
                onPalletPickedUp?.Invoke(pallet);
                Debug.Log($"[DropZone] {zoneName} - Palette reprise !");
            }

            palletInZone = null;
            UpdateVisual();
        }
    }

    private void Update()
    {
        // Vérifie si une palette dans la zone vient d'être déposée (n'est plus attachée)
        if (palletInZone != null && !hasPalletDeposited)
        {
            if (!palletInZone.IsAttached)
            {
                // La palette vient d'être déposée !
                hasPalletDeposited = true;
                onPalletDropped?.Invoke(palletInZone);
                Debug.Log($"[DropZone] {zoneName} - Palette déposée avec succès !");
                UpdateVisual();
            }
        }
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            if (hasPalletDeposited)
            {
                spriteRenderer.color = zoneActiveColor;
            }
            else if (palletInZone != null)
            {
                // Palette au-dessus, mais pas encore déposée
                spriteRenderer.color = Color.Lerp(zoneColor, zoneActiveColor, 0.5f);
            }
            else
            {
                spriteRenderer.color = zoneColor;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Affiche la zone dans l'éditeur
        Gizmos.color = zoneColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}