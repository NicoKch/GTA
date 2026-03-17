using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// SceneFader : Singleton DontDestroyOnLoad qui gère les transitions de scènes par fondu.
/// - Se crée automatiquement au premier appel (pas besoin de le placer dans chaque scène)
/// - FadeIn  : noir → transparent (à appeler au démarrage d'une scène)
/// - FadeOut : transparent → noir, puis charge la scène cible
/// </summary>
public class SceneFader : MonoBehaviour
{
    #region Singleton

    public static SceneFader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Configuration

    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    #endregion

    #region State

    private Image fadeImage;
    private bool isFading = false;
    public bool IsFading => isFading;

    #endregion

    #region Initialization

    private void InitializePanel()
    {
        // Crée le Canvas de fade dynamiquement (aucun prefab requis)
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // toujours au-dessus de tout

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Panel noir couvrant tout l'écran
        GameObject panelGO = new GameObject("FadePanel");
        panelGO.transform.SetParent(transform, false);

        fadeImage = panelGO.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.raycastTarget = false;

        RectTransform rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Commence transparent (le FadeIn sera appelé par la scène)
        SetAlpha(0f);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Fondu depuis le noir vers transparent.
    /// À appeler au Start() de chaque scène pour un fade-in d'entrée.
    /// </summary>
    public void FadeIn(float duration = -1f)
    {
        if (isFading) return;
        StartCoroutine(DoFade(1f, 0f, duration < 0 ? fadeDuration : duration, null));
    }

    /// <summary>
    /// Fondu vers le noir puis charge la scène indiquée.
    /// </summary>
    public void FadeToScene(string sceneName, float duration = -1f)
    {
        if (isFading) return;
        float d = duration < 0 ? fadeDuration : duration;
        StartCoroutine(DoFade(0f, 1f, d, () => SceneManager.LoadScene(sceneName)));
    }

    /// <summary>
    /// Fondu vers le noir puis charge la scène par index (Build Settings).
    /// </summary>
    public void FadeToScene(int sceneIndex, float duration = -1f)
    {
        if (isFading) return;
        float d = duration < 0 ? fadeDuration : duration;
        StartCoroutine(DoFade(0f, 1f, d, () => SceneManager.LoadScene(sceneIndex)));
    }

    #endregion

    #region Internal

    private IEnumerator DoFade(float fromAlpha, float toAlpha, float duration, System.Action onComplete)
    {
        isFading = true;
        SetAlpha(fromAlpha);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration));
            yield return null;
        }

        SetAlpha(toAlpha);
        isFading = false;
        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeColor;
        c.a = alpha;
        fadeImage.color = c;
    }

    #endregion

    #region Factory

    /// <summary>
    /// Crée le SceneFader s'il n'existe pas encore.
    /// Appelé automatiquement par MainMenuController et les scènes de jeu.
    /// </summary>
    public static SceneFader GetOrCreate()
    {
        if (Instance != null) return Instance;

        GameObject go = new GameObject("SceneFader");
        return go.AddComponent<SceneFader>();
    }

    #endregion
}
