using System;
using Unity.VisualScripting;
using UnityEngine;

public class ForkController : MonoBehaviour
{
    [Header("Hauteur")] [SerializeField] private float minHeight = 0f;
    [SerializeField] private float maxHeight = 3f;
    [SerializeField] private float liftSpeed = 2f;

    [Header("Visuel")] [SerializeField] private float minXScale = 1f;
    [SerializeField] private float maxXScale = 1.5f;
    [SerializeField] private float minYScale = 1f;
    [SerializeField] private float maxYScale = 1.3f;

    [Header("Prise de palette")] [SerializeField]
    private Transform palletAttachPoint; // Point d'ancrage pour la palette

    [Header("Sorting Order")] [SerializeField]
    private float heightThresholdForOverlay = 0.5f; // Hauteur à partir de laquelle les fourches passent au-dessus

    [SerializeField] private int sortingOrderBelow = 0; // Sorting order quand fourches sont basses
    [SerializeField] private int sortingOrderAbove = 10; // Sorting order quand fourches sont hautes


    [SerializeField] private float attachThreshold = 0.15f; // Hauteur minimale pour attacher
    [SerializeField] private float detachThreshold = 0.05f;
    private bool wasAboveOverlayThreshold = false;


    private PlayerInputAction inputActions;
    private float currentHeight = 0f;
    private SpriteRenderer[] forkSprites;
    private Vector3[] originalScales;

    private Pallet currentPallet;
    private Pallet palletInRange;
    private bool isPalletAttached = false;

    // Pour tracker l'état précédent
    private bool wasAboveAttachThreshold = false;
    private bool wasAboveDetachThreshold = false;
    public float CurrentHeight => currentHeight;
    public bool HasPallet => isPalletAttached;
    public Pallet CurrentPallet => currentPallet;

    private void Start()
    {
        inputActions = InputManager.InputActions;
        forkSprites = GetComponentsInChildren<SpriteRenderer>();
        originalScales = new Vector3[forkSprites.Length];
        for (int i = 0; i < forkSprites.Length; i++)
        {
            originalScales[i] = forkSprites[i].transform.localScale;
        }

        wasAboveAttachThreshold = currentHeight >= attachThreshold;
        wasAboveDetachThreshold = currentHeight > detachThreshold;
        // Initialise le sorting order
        UpdateSortingOrder();
    }

    private void Update()
    {
        // Lire l'input pour monter/descendre (à ajouter dans ton Input Actions)
        float liftInput = inputActions.Player.lift.ReadValue<float>();

        liftInput = FilterLiftInput(liftInput);
        // Mise à jour de la hauteur
        currentHeight += liftInput * liftSpeed * Time.deltaTime;
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
// Feedback visuel : changer la couleur ou l'échelle selon la hauteur
        UpdateVisual();
        if (!isPalletAttached)
        {
            UpdateSortingOrder();
        }

        CheckPalletAttachment();
    }

    private void UpdateVisual()
    {
        if (forkSprites == null || forkSprites.Length == 0) return;


        float t = (currentHeight - minHeight) / (maxHeight - minHeight);
        float xScaleFactor = Mathf.Lerp(minXScale, maxXScale, t);
        float yScaleFactor = Mathf.Lerp(minYScale, maxYScale, t);

        for (int i = 0; i < forkSprites.Length; i++)
        {
            Vector3 newScale = new Vector3(
                originalScales[i].x * xScaleFactor,
                originalScales[i].y * yScaleFactor,
                originalScales[i].z
            );
            // Scale : plus grand = plus haut
            forkSprites[i].transform.localScale = newScale;
        }
    }

    private void CheckPalletAttachment()
    {
        bool isAboveAttachThreshold = currentHeight >= attachThreshold;
        bool isAboveDetachThreshold = currentHeight > detachThreshold;

        // ATTACHER
        if (!isPalletAttached && isAboveAttachThreshold && !wasAboveAttachThreshold)
        {
            if (palletInRange != null && palletInRange.AreBothForksInserted)
            {
                AttachPallet(palletInRange);
            }
            else
            {
                Debug.Log(
                    $"[ForkController] Seuil atteint mais palletInRange={palletInRange}, bothForks={palletInRange?.AreBothForksInserted}");
            }
        }

        // DÉTACHER
        if (isPalletAttached && !isAboveDetachThreshold && wasAboveDetachThreshold)
        {
            DetachPallet();
        }

        wasAboveAttachThreshold = isAboveAttachThreshold;
        wasAboveDetachThreshold = isAboveDetachThreshold;
    }

    // Appelé par la Palette quand les fourches sont insérées
    public void RegisterPalletInRange(Pallet pallet)
    {
        if (!isPalletAttached)
        {
            palletInRange = pallet;
            Debug.Log($"[ForkController] Palette enregistrée : {pallet.name}");
        }
    }

    // Appelé par la Palette quand les fourches sont retirées
    public void UnregisterPalletInRange(Pallet pallet)
    {
        if (palletInRange == pallet && !isPalletAttached)
        {
            palletInRange = null;
            Debug.Log($"[ForkController] Palette désenregistrée : {pallet.name}");
        }
    }

    private void AttachPallet(Pallet pallet)
    {
        if (pallet == null) return;

        currentPallet = pallet;
        isPalletAttached = true;
        pallet.AttachToForks(palletAttachPoint);

        Debug.Log($"[ForkController] Palette attachée !");
    }

    private void DetachPallet()
    {
        if (currentPallet == null) return;

        currentPallet.DetachFromForks();
        currentPallet = null;
        isPalletAttached = false;
        palletInRange = null;

        Debug.Log($"[ForkController] Palette déposée !");
    }

    private void UpdateSortingOrder()
    {
        bool isAboveOverlayThreshold = currentHeight >= heightThresholdForOverlay;

        // Change seulement si on franchit le seuil
        if (isAboveOverlayThreshold != wasAboveOverlayThreshold)
        {
            int newSortingOrder = isAboveOverlayThreshold ? sortingOrderAbove : sortingOrderBelow;

            foreach (var sprite in forkSprites)
            {
                sprite.sortingOrder = newSortingOrder;
            }

            Debug.Log($"[ForkController] Sorting Order changé à {newSortingOrder} (hauteur: {currentHeight:F2})");

            wasAboveOverlayThreshold = isAboveOverlayThreshold;
        }
    }


    private float FilterLiftInput(float liftInput)
    {
        // Empêche de baisser les fourches au-dessus d'une palette
        if (!isPalletAttached && palletInRange != null)
        {
            if (currentHeight >= heightThresholdForOverlay && liftInput < 0)
            {
                Debug.Log("[ForkController] ALERTE : Impossible de baisser les fourches au-dessus d'une palette !");
                // TODO: Feedback visuel/sonore pour le joueur
                return 0f; // Bloque la descente
            }
        }

        return liftInput;
    }
}