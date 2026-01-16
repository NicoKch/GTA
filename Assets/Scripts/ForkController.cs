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

    [SerializeField] private float attachThreshold = 0.15f; // Hauteur minimale pour attacher
    [SerializeField] private float detachThreshold = 0.05f;


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
    }

    private void Update()
    {
        // Lire l'input pour monter/descendre (à ajouter dans ton Input Actions)
        float liftInput = inputActions.Player.lift.ReadValue<float>();

        // Mise à jour de la hauteur
        float previousHeight = currentHeight;
        currentHeight += liftInput * liftSpeed * Time.deltaTime;
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        // Feedback visuel : changer la couleur ou l'échelle selon la hauteur
        UpdateVisual();
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
}