using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private float fadeDuration = 1.2f;

    private Image fadePanel;
    private bool isTransitioning = false;

    private void Awake()
    {
        Debug.Log("[ScreenFader] Awoke. Instance = " + (Instance ? Instance.name : "null"));
        if (Instance != null && Instance != this)
        {
            Debug.Log("[ScreenFader] Destroying duplicate ScreenFader: " + name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ScreenFader] Set Instance and DontDestroyOnLoad on: " + name);

        // Find or create the fade panel UI
        EnsureFadePanelExists();

        SetAlpha(1f);
    }

    private IEnumerator Start()
    {
        Debug.Log("[ScreenFader] Start coroutine begin.");
        yield return Fade(1f, 0f);
        Debug.Log("[ScreenFader] Start coroutine end.");
    }

    public void LoadNextScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionRoutine(sceneName));
        }
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        Debug.Log("[ScreenFader] TransitionRoutine start to scene: " + sceneName);

        // Fade to black.
        yield return Fade(0f, 1f);

        AsyncOperation load =
            SceneManager.LoadSceneAsync(sceneName);

        if (load == null)
        {
            Debug.LogError("Could not load scene: " + sceneName);
            yield return Fade(1f, 0f);
            isTransitioning = false;
            yield break;
        }

        while (!load.isDone)
        {
            yield return null;
        }

        // Fade back in after loading Scene 2.
        yield return Fade(1f, 0f);

        isTransitioning = false;
        Debug.Log("[ScreenFader] TransitionRoutine end.");
    }

    private IEnumerator Fade(float from, float to)
    {
        Debug.Log("[ScreenFader] Fade called from " + from + " to " + to + ". fadePanel = " + (fadePanel ? fadePanel.name : "null"));
        if (fadePanel == null)
        {
            Debug.LogError(
                "ScreenFader: Fade panel reference is missing."
            );
            yield break;
        }

        fadePanel.raycastTarget = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(Mathf.Lerp(from, to, progress));

            yield return null;
        }

        SetAlpha(to);
        fadePanel.raycastTarget = to > 0.99f;
        Debug.Log("[ScreenFader] Fade finished. alpha = " + to);
    }

    private void SetAlpha(float alpha)
    {
        if (fadePanel == null)
            return;

        Color color = fadePanel.color;
        color.a = alpha;
        fadePanel.color = color;
    }

    private void EnsureFadePanelExists()
    {
        Debug.Log("[ScreenFader] EnsureFadePanelExists called.");
        // Try to find existing fade panel in current scene
        GameObject fadePanelObj = GameObject.Find("FadePanel");
        if (fadePanelObj != null)
        {
            fadePanel = fadePanelObj.GetComponent<Image>();
            if (fadePanel != null)
            {
                Debug.Log("[ScreenFader] Found existing FadePanel: " + fadePanelObj.name + " in scene: " + fadePanelObj.scene.name);
                return;
            }
        }

        // If not found, create a new fade panel
        Debug.Log("[ScreenFader] Creating new FadePanel.");
        fadePanelObj = new GameObject("FadePanel");
        fadePanelObj.transform.SetParent(transform, false); // Make it a child of ScreenFader for organization

        // Add RectTransform
        RectTransform rt = fadePanelObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // Add CanvasRenderer
        fadePanelObj.AddComponent<CanvasRenderer>();

        // Add Image component
        fadePanel = fadePanelObj.AddComponent<Image>();
        fadePanel.color = new Color(0f, 0f, 0f, 1f); // Black, fully opaque
        fadePanel.raycastTarget = true;

        Debug.Log("[ScreenFader] Created FadePanel object: " + fadePanelObj.name + ", parent: " + fadePanelObj.transform.parent?.name + ", scene: " + fadePanelObj.scene.name);
    }
}