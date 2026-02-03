using UnityEngine;
using System.Collections.Generic;

public class VisionTarget : MonoBehaviour
{
    public enum TargetType
    {
        Pedestrian,
        Forklift,
        Obstacle,
        Hazard,
        Pallet
    }

    [SerializeField] private TargetType targetType;
    [SerializeField] private Color hiddenColor = new Color(1, 1, 1, 0.3f);

    [Header("Comportement quand invisible")] [SerializeField]
    private bool hideCompletely = false;

    // Tous les sprites enfants avec leurs couleurs d'origine
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private bool isVisible = false;

    public TargetType Type => targetType;
    public bool IsVisible => isVisible;

    private void Awake()
    {
        // Récupère TOUS les SpriteRenderers enfants automatiquement
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length == 0)
        {
            Debug.LogWarning($"[VisionTarget] Aucun SpriteRenderer trouvé sur {gameObject.name} ou ses enfants !");
            return;
        }

        // Sauvegarde les couleurs d'origine
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        Debug.Log($"[VisionTarget] {spriteRenderers.Length} SpriteRenderer(s) trouvé(s) sur {gameObject.name}");
    }

    private void Start()
    {
        OnBecomeHidden();
    }

    public void OnBecomeVisible()
    {
        isVisible = true;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].enabled = true;
                spriteRenderers[i].color = originalColors[i]; // Restaure la couleur d'origine
            }
        }
    }

    public void OnBecomeHidden()
    {
        isVisible = false;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                if (hideCompletely)
                {
                    spriteRenderers[i].enabled = false;
                }
                else
                {
                    // Applique la couleur cachée en conservant l'alpha de hiddenColor
                    Color hidden = originalColors[i];
                    hidden.a = hiddenColor.a;
                    spriteRenderers[i].color = hidden;
                }
            }
        }
    }
}