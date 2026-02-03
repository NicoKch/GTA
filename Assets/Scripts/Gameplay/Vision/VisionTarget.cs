using UnityEngine;

public class VisionTarget : MonoBehaviour
{
    public enum TargetType
    {
        Pedestrian, // Piéton
        Forklift, // Autre chariot
        Obstacle, // Obstacle mobile
        Hazard // Danger
    }

    [SerializeField] private TargetType targetType;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color visibleColor = Color.white;
    [SerializeField] private Color hiddenColor = new Color(1, 1, 1, 0.3f);

    [Header("Comportement quand invisible")] [SerializeField]
    private bool hideCompletely = false;

    private bool isVisible = false;

    public TargetType Type => targetType;
    public bool IsVisible => isVisible;

    private void Start()
    {
        // Initialement caché
        OnBecomeHidden();
    }

    public void OnBecomeVisible()
    {
        isVisible = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = visibleColor;
        }
    }

    public void OnBecomeHidden()
    {
        isVisible = false;

        if (spriteRenderer != null)
        {
            if (hideCompletely)
            {
                spriteRenderer.enabled = false;
            }
            else
            {
                spriteRenderer.color = hiddenColor;
            }
        }
    }
}