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

    private PlayerInputAction inputActions;
    private float currentHeight = 0f;
    private SpriteRenderer[] forkSprites;
    private Vector3[] originalScales;

    public float CurrentHeight => currentHeight;


    private void Start()
    {
        inputActions = InputManager.InputActions;
        forkSprites = GetComponentsInChildren<SpriteRenderer>();
        originalScales = new Vector3[forkSprites.Length];
        for (int i = 0; i < forkSprites.Length; i++)
        {
            originalScales[i] = forkSprites[i].transform.localScale;
        }
    }

    private void Update()
    {
        // Lire l'input pour monter/descendre (à ajouter dans ton Input Actions)
        float liftInput = inputActions.Player.lift.ReadValue<float>();

        // Mise à jour de la hauteur
        currentHeight += liftInput * liftSpeed * Time.deltaTime;
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        // Feedback visuel : changer la couleur ou l'échelle selon la hauteur
        UpdateVisual();
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
}