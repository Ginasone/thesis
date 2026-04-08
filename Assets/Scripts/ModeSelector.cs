using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelector : MonoBehaviour
{
    // UI Elements
    private Canvas modeCanvas;
    private Text titleText;
    private Text recommendationText;
    private Button offlineButton;
    private Button turnBasedButton;
    private Button realTimeButton;

    // Selected mode
    public string selectedMode = "offline";
    public bool modeSelected = false;

    // Reference to infrastructure manager
    private InfrastructureManager infraManager;

    void Start()
    {
        // Find infrastructure manager
        infraManager = FindObjectOfType<InfrastructureManager>();

        // Create mode selection UI
        CreateModeSelectionUI();

        // Wait for infrastructure measurements, then show recommendation
        StartCoroutine(WaitForMeasurements());
    }

    IEnumerator WaitForMeasurements()
    {
        // Wait for infrastructure detection to complete
        if (infraManager != null)
        {
            while (!infraManager.AreMeasurementsComplete())
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Update recommendation
            string recommended = infraManager.GetRecommendedMode();
            UpdateRecommendation(recommended);
        }
        else
        {
            // No infrastructure manager - default to offline
            UpdateRecommendation("offline");
        }
    }

    void CreateModeSelectionUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("ModeSelectionCanvas");
        modeCanvas = canvasObj.AddComponent<Canvas>();
        modeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== TITLE =====
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform);
        titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.text = "SELECT GAME MODE";
        titleText.fontSize = 60;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyle.Bold;

        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = Color.black;
        titleOutline.effectDistance = new Vector2(4, 4);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(1000, 100);

        // ===== RECOMMENDATION TEXT =====
        GameObject recObj = new GameObject("Recommendation");
        recObj.transform.SetParent(canvasObj.transform);
        recommendationText = recObj.AddComponent<Text>();
        recommendationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        recommendationText.text = "Analyzing your connection...";
        recommendationText.fontSize = 40;
        recommendationText.alignment = TextAnchor.MiddleCenter;
        recommendationText.color = new Color(0.8f, 0.9f, 1f); // Light blue

        Outline recOutline = recObj.AddComponent<Outline>();
        recOutline.effectColor = Color.black;
        recOutline.effectDistance = new Vector2(3, 3);

        RectTransform recRect = recObj.GetComponent<RectTransform>();
        recRect.anchorMin = new Vector2(0.5f, 0.65f);
        recRect.anchorMax = new Vector2(0.5f, 0.65f);
        recRect.pivot = new Vector2(0.5f, 0.5f);
        recRect.anchoredPosition = Vector2.zero;
        recRect.sizeDelta = new Vector2(1200, 80);

        // ===== OFFLINE BUTTON =====
        offlineButton = CreateModeButton("OFFLINE MODE", new Vector2(0.5f, 0.5f), new Color(0.3f, 0.3f, 0.8f));
        offlineButton.onClick.AddListener(() => SelectMode("offline"));

        // ===== TURN-BASED BUTTON =====
        turnBasedButton = CreateModeButton("TURN-BASED ONLINE", new Vector2(0.5f, 0.35f), new Color(0.3f, 0.7f, 0.3f));
        turnBasedButton.onClick.AddListener(() => SelectMode("turn-based"));

        // ===== REAL-TIME BUTTON =====
        realTimeButton = CreateModeButton("REAL-TIME MULTIPLAYER", new Vector2(0.5f, 0.2f), new Color(0.8f, 0.3f, 0.3f));
        realTimeButton.onClick.AddListener(() => SelectMode("real-time"));

        Debug.Log("Mode selection UI created");
    }

    Button CreateModeButton(string buttonText, Vector2 position, Color buttonColor)
    {
        GameObject buttonObj = new GameObject(buttonText + "Button");
        buttonObj.transform.SetParent(modeCanvas.transform);

        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = position;
        buttonRect.anchorMax = position;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(600, 100);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = buttonText;
        text.fontSize = 36;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, 2);

        return button;
    }

    void UpdateRecommendation(string recommendedMode)
    {
        string speedInfo = infraManager != null ? $"{infraManager.downloadSpeedMbps:F1} Mbps" : "Unknown";
        string latencyInfo = infraManager != null ? $"{infraManager.latencyMs:F0}ms" : "Unknown";

        recommendationText.text = $"Your Connection: {speedInfo}, {latencyInfo}\n";
        recommendationText.text += $"✅ Recommended: {recommendedMode.ToUpper()}";

        // Highlight recommended button
        HighlightRecommendedButton(recommendedMode);

        Debug.Log($"Recommended mode: {recommendedMode}");
    }

    void HighlightRecommendedButton(string recommendedMode)
    {
        // Add a glow/outline to recommended button
        if (recommendedMode == "offline" && offlineButton != null)
        {
            offlineButton.GetComponent<Image>().color = new Color(0.4f, 0.4f, 1f); // Brighter blue
        }
        else if (recommendedMode == "turn-based" && turnBasedButton != null)
        {
            turnBasedButton.GetComponent<Image>().color = new Color(0.4f, 0.9f, 0.4f); // Brighter green
        }
        else if (recommendedMode == "real-time" && realTimeButton != null)
        {
            realTimeButton.GetComponent<Image>().color = new Color(1f, 0.4f, 0.4f); // Brighter red
        }
    }

    void SelectMode(string mode)
    {
        selectedMode = mode;
        modeSelected = true;

        Debug.Log($"========================================");
        Debug.Log($"USER SELECTED MODE: {mode.ToUpper()}");

        if (infraManager != null)
        {
            string recommended = infraManager.GetRecommendedMode();
            bool followedRecommendation = (mode == recommended);

            Debug.Log($"Recommended: {recommended.ToUpper()}");
            Debug.Log($"Chosen: {mode.ToUpper()}");
            Debug.Log($"Followed Recommendation: {followedRecommendation}");
        }

        Debug.Log($"========================================");

        // Hide mode selection UI
        if (modeCanvas != null)
        {
            modeCanvas.gameObject.SetActive(false);
        }

        // Notify GameManager to start - FIXED METHOD NAME
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnModeSelected(mode); // FIXED!
        }
    }

    public string GetSelectedMode()
    {
        return selectedMode;
    }

    public bool IsModeSelected()
    {
        return modeSelected;
    }

    public void OnOfflineButtonClicked()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.OnModeSelected("offline");
    }

    public void OnTurnBasedButtonClicked()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.OnModeSelected("turn-based");
    }

    public void OnRealTimeButtonClicked()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.OnModeSelected("real-time");
    }
}