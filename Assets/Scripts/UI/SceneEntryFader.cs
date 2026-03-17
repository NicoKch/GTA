using UnityEngine;

/// <summary>
/// SceneEntryFader : Déclenche un fade-in au chargement de la scène.
/// À placer sur n'importe quel GameObject de la scène (pas DontDestroyOnLoad).
/// Compatible avec toutes les scènes futures.
/// </summary>
public class SceneEntryFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        SceneFader.GetOrCreate().FadeIn(fadeDuration);
    }
}
