using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPauseMenu : MonoBehaviour
{
    [SerializeField] private string townSceneName = "HealthSystemTest";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isMenuOpen;
    private SimpleRuntimeMenu runtimeMenu;

    private void Awake()
    {
        runtimeMenu = GetComponent<SimpleRuntimeMenu>();
        if (runtimeMenu == null)
        {
            runtimeMenu = gameObject.AddComponent<SimpleRuntimeMenu>();
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == townSceneName)
        {
            if (isMenuOpen)
            {
                Resume();
            }

            return;
        }

        if (Input.GetKeyDown(pauseKey))
        {
            if (isMenuOpen)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (!isMenuOpen)
        {
            return;
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        if (runtimeMenu != null)
        {
            runtimeMenu.Hide();
        }
    }

    private void Pause()
    {
        isMenuOpen = true;
        Time.timeScale = 0f;
        runtimeMenu.Show(
            "Level Menu",
            "Press Esc again to close this menu.",
            new SimpleRuntimeMenu.MenuOption("Continue", Resume),
            new SimpleRuntimeMenu.MenuOption("Restart Level", RestartLevel),
            new SimpleRuntimeMenu.MenuOption("Return To Town", ReturnToTown));
    }

    private void Resume()
    {
        isMenuOpen = false;
        Time.timeScale = 1f;
        runtimeMenu.Hide();
    }

    private void RestartLevel()
    {
        Resume();
        PlayerRuntimeState.Instance.RestoreLevelEntrySnapshot();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToTown()
    {
        Resume();
        PlayerRuntimeState.Instance.CaptureCurrentState();
        SceneManager.LoadScene(townSceneName);
    }
}

public class SimpleRuntimeMenu : MonoBehaviour
{
    private const int MaxButtons = 6;

    [SerializeField] private string canvasName = "RuntimeMenuCanvas";

    private Canvas menuCanvas;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Button[] buttons;
    private TMP_Text[] buttonLabels;

    private void Awake()
    {
        BuildIfNeeded();
        Hide();
    }

    public void Show(string title, string body, params MenuOption[] options)
    {
        BuildIfNeeded();

        titleText.text = title;
        bodyText.text = body;

        for (int i = 0; i < buttons.Length; i++)
        {
            bool shouldShow = options != null && i < options.Length;
            buttons[i].gameObject.SetActive(shouldShow);

            if (!shouldShow)
            {
                continue;
            }

            buttons[i].onClick.RemoveAllListeners();
            buttonLabels[i].text = options[i].Label;
            Action callback = options[i].Callback;
            buttons[i].onClick.AddListener(() => callback?.Invoke());
        }

        menuCanvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }
    }

    private void BuildIfNeeded()
    {
        if (menuCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new(canvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        menuCanvas = canvasObject.GetComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform panelRect = CreateUIObject("Panel", canvasObject.transform).GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 280f);

        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        titleText = CreateText("Title", panelRect, new Vector2(0f, -30f), new Vector2(360f, 40f), 30);
        titleText.alignment = TextAlignmentOptions.Center;

        bodyText = CreateText("Body", panelRect, new Vector2(0f, -75f), new Vector2(360f, 30f), 20);
        bodyText.alignment = TextAlignmentOptions.Center;

        buttons = new Button[MaxButtons];
        buttonLabels = new TMP_Text[MaxButtons];

        for (int i = 0; i < MaxButtons; i++)
        {
            RectTransform buttonRect = CreateUIObject("Button" + i, panelRect).GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.anchoredPosition = new Vector2(0f, -120f - i * 45f);
            buttonRect.sizeDelta = new Vector2(340f, 35f);

            Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.18f, 0.18f, 1f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            colors.highlightedColor = new Color(0.28f, 0.28f, 0.28f, 1f);
            colors.pressedColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            button.colors = colors;

            TMP_Text label = CreateText("Label", buttonRect, Vector2.zero, new Vector2(320f, 30f), 18);
            label.alignment = TextAlignmentOptions.Center;

            buttons[i] = button;
            buttonLabels[i] = label;
        }
    }

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new(objectName, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static TMP_Text CreateText(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.text = string.Empty;
        return text;
    }

    public readonly struct MenuOption
    {
        public string Label { get; }
        public Action Callback { get; }

        public MenuOption(string label, Action callback)
        {
            Label = label;
            Callback = callback;
        }
    }
}
