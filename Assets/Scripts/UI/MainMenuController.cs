using UnityEngine;

/// <summary>
/// MainMenuController : Gère l'écran principal du jeu.
/// À placer sur un GameObject dans la scène MainMenu.
/// Le bouton "Jouer" doit appeler OnPlayClicked() via son événement OnClick().
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private string gameSceneName = "MainScene";

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        // Crée le fader si absent et fait un fade-in d'entrée
        SceneFader.GetOrCreate().FadeIn(fadeDuration);
    }

    /// <summary>
    /// À assigner dans l'Inspector sur l'événement OnClick() du bouton Jouer.
    /// </summary>
    public void OnPlayClicked()
    {
        SceneFader.GetOrCreate().FadeToScene(gameSceneName, fadeDuration);
    }
}