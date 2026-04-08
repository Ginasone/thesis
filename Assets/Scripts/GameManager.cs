
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // ========== THE GAME LOGIC ==========
    public List<Territory> territories;
    private Territory selectedTerritory;
    private int currentPlayer = 1;
    private int playerUnitsToPlace = 3;

    // UI Elements
    private Text statusText;
    private Button endTurnButton;
    private Canvas canvas;

    // Game mode
    private string currentGameMode = "offline";

    // ========== ONLY WHAT I HAVE ==========
    [Header("UI Panels - Connect These!")]
    public GameObject preSurveyPanel;      // PreSurveyPanel
    public GameObject gameBoard;            // GameBoard
    public GameObject postSurveyPanel;      // PostSurveyPanel

    [Header("Optional")]
    public FirebaseManager firebaseManager; 

    // ModeSelector is attached to this same GameObject
    private ModeSelector modeSelector;

    [Header("Game State")]
    public int maxRounds = 10;
    private int currentRound = 0;
    private bool gameEnded = false;

    // Session timing
    private float sessionStartTime;
    private float sessionEndTime;

    // ADDED: Reference to the Canvas that contains the survey
    private GameObject surveyCanvas;

    void Start()
    {
        EnsureEventSystem();
        territories = new List<Territory>(FindObjectsOfType<Territory>());

        // Get ModeSelector component (attached to this same GameObject)
        modeSelector = GetComponent<ModeSelector>();

        // FIND THE CANVAS PARENT
        if (preSurveyPanel != null)
        {
            surveyCanvas = preSurveyPanel.transform.parent.gameObject;
            Debug.Log($"📋 Found survey canvas: {surveyCanvas.name}");
        }

        // Wait a frame for ModeSelector to create its UI, then hide it
        StartCoroutine(HideModeSelectorUI());

        Debug.Log("✅ GameManager started - showing pre-survey");
    }

    IEnumerator HideModeSelectorUI()
    {
        // Wait for ModeSelector.Start() to create the canvas
        yield return new WaitForEndOfFrame();

        // Find and hide the mode selection canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (c.gameObject.name == "ModeSelectionCanvas")
            {
                c.gameObject.SetActive(false);
                Debug.Log("✅ ModeSelector UI hidden until after pre-survey");
                break;
            }
        }

        // Now show pre-survey
        ShowPreSurvey();
    }

    void ShowPreSurvey()
    {
        Debug.Log("📋 Showing Pre-Survey");

        // Show the survey canvas
        if (surveyCanvas != null)
        {
            surveyCanvas.SetActive(true);
        }
        else if (preSurveyPanel != null)
        {
            preSurveyPanel.SetActive(true);
        }

        if (gameBoard != null)
            gameBoard.SetActive(false);
        if (postSurveyPanel != null)
            postSurveyPanel.SetActive(false);
    }

    // Called by PreSurveyManager when user submits
    public void OnPreSurveyCompleted()
    {
        Debug.Log("✅ Pre-Survey Done - Now choose your mode!");

        // HIDE THE ENTIRE SURVEY CANVAS!
        if (surveyCanvas != null)
        {
            Debug.Log($"🚫 Hiding survey canvas: {surveyCanvas.name}");
            surveyCanvas.SetActive(false);
        }
        else if (preSurveyPanel != null)
        {
            Debug.Log("🚫 Hiding pre-survey panel");
            preSurveyPanel.SetActive(false);
        }

        // Show ModeSelector UI
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (c.gameObject.name == "ModeSelectionCanvas")
            {
                c.gameObject.SetActive(true);
                Debug.Log("📱 Mode selector shown - pick your mode!");
                return;
            }
        }

        // Fallback if no mode selector found
        Debug.LogWarning("⚠️ No ModeSelector canvas found! Starting in offline mode...");
        OnModeSelected("offline");
    }

    // Called by ModeSelector when user picks a mode
    public void OnModeSelected(string mode)
    {
        Debug.Log($"✅ Mode Selected: {mode} - Starting game!");

        currentGameMode = mode;

        // MAKE SURE SURVEY CANVAS IS HIDDEN
        if (surveyCanvas != null)
        {
            surveyCanvas.SetActive(false);
        }
        else if (preSurveyPanel != null)
        {
            preSurveyPanel.SetActive(false);
        }

        // ModeSelector already hides itself, so just show game
        if (gameBoard != null)
            gameBoard.SetActive(true);

        // Start game in chosen mode
        sessionStartTime = Time.time;
        currentRound = 0;

        CreateUICanvas();
        InitializeGame();

        Debug.Log($"🎮 Game started in {mode} mode!");
    }

    // Called when game ends
    public void EndGame(bool playerWon)
    {
        if (gameEnded) return;

        gameEnded = true;
        sessionEndTime = Time.time;

        Debug.Log($"🏁 Game ended - Player won: {playerWon}");

        // Destroy game UI
        if (canvas != null)
            Destroy(canvas.gameObject);

        // Hide game board
        if (gameBoard != null)
            gameBoard.SetActive(false);

        // Skip post-survey, go straight to summary!
        Debug.Log("🎉 Skipping post-survey - showing victory summary");
        SaveSessionData();
        ShowSimpleSummary();
    }

    // Called by PostSurveyManager when user submits/skips
    public void OnPostSurveyCompleted()
    {
        Debug.Log("✅ Post-Survey Done");

        // Save data
        SaveSessionData();

        // Show simple summary
        ShowSimpleSummary();
    }

    void SaveSessionData()
    {
        Debug.Log("=== SAVING SESSION DATA ===");

        // Create complete data with surveys
        CompleteSessionData data = new CompleteSessionData();

        // Add gameplay data
        float duration = (sessionEndTime - sessionStartTime) / 60f;
        int playerTerritories = territories.FindAll(t => t.ownerPlayer == 1).Count;
        bool won = playerTerritories > (territories.Count / 2);

        data.SetGameplayData(won, playerTerritories, territories.Count, currentGameMode, duration);

        // Convert to JSON
        string json = JsonUtility.ToJson(data, true);
        Debug.Log($"📄 Data:\n{json}");

        // Save to Firebase if available
        if (firebaseManager != null && firebaseManager.isInitialized)
        {
            firebaseManager.SaveSessionDataJSON(json, data.sessionId);
            Debug.Log("✅ Saved to Firebase!");
        }
        else
        {
            Debug.LogWarning("⚠️ Firebase not available - data only in console");
        }
    }

    void ShowSimpleSummary()
    {
        if (postSurveyPanel != null)
            postSurveyPanel.SetActive(false);

        // Create summary canvas
        GameObject summaryCanvas = new GameObject("SummaryCanvas");
        Canvas sumCanvas = summaryCanvas.AddComponent<Canvas>();
        sumCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = summaryCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        summaryCanvas.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(summaryCanvas.transform);

        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(700, 600);

        // Title
        CreateText(panel.transform, "✅ SESSION COMPLETE!", new Vector2(0, 250), 50, Color.green);
        CreateText(panel.transform, "Thank you for playing!", new Vector2(0, 190), 30, Color.white);

        // Game Results
        int playerTerr = territories.FindAll(t => t.ownerPlayer == 1).Count;
        float duration = (sessionEndTime - sessionStartTime) / 60f;

        CreateText(panel.transform, $"Territories: {playerTerr}/{territories.Count}", new Vector2(0, 130), 25, Color.white);
        CreateText(panel.transform, $"Game Mode: {currentGameMode.ToUpper()}", new Vector2(0, 100), 25, Color.white);
        CreateText(panel.transform, $"Duration: {duration:F1} minutes", new Vector2(0, 70), 25, Color.white);

        // Infrastructure Info (if available)
        InfrastructureManager infraMgr = FindObjectOfType<InfrastructureManager>();
        if (infraMgr != null)
        {
            CreateText(panel.transform, "=== INFRASTRUCTURE ===", new Vector2(0, 30), 22, Color.cyan);
            CreateText(panel.transform, $"Speed: {infraMgr.downloadSpeedMbps:F1} Mbps", new Vector2(0, 0), 20, Color.white);
            CreateText(panel.transform, $"Latency: {infraMgr.latencyMs:F1} ms", new Vector2(0, -25), 20, Color.white);
            CreateText(panel.transform, $"Battery: {infraMgr.batteryLevelStart * 100:F0}% → {infraMgr.batteryLevelCurrent * 100:F0}%", new Vector2(0, -50), 20, Color.white);
            CreateText(panel.transform, $"Data Used: {infraMgr.estimatedDataUsageMB:F2} MB", new Vector2(0, -75), 20, Color.white);
        }

        // Buttons
        CreateButton(panel.transform, "PLAY AGAIN", new Vector2(-120, -200), () => {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        CreateButton(panel.transform, "EXIT", new Vector2(120, -200), () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    void CreateText(Transform parent, string content, Vector2 pos, int fontSize, Color color)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent);

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(550, 100);
    }

    void CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(parent);

        Button btn = btnObj.AddComponent<Button>();
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.7f, 0.2f);

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = new Vector2(200, 60);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform);

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = label;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        btn.onClick.AddListener(onClick);
    }

    // ========== MY GAME LOGIC ==========

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("✅ EventSystem created");
        }
    }

    void CreateUICanvas()
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Status text
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(canvasObj.transform);
        statusText = textObj.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.text = "Your Turn - Place 3 Units";
        statusText.fontSize = 50;
        statusText.alignment = TextAnchor.UpperCenter;
        statusText.color = Color.white;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4, 4);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0, -20);
        textRect.sizeDelta = new Vector2(1200, 100);

        // End turn button
        GameObject buttonObj = new GameObject("EndTurnButton");
        buttonObj.transform.SetParent(canvasObj.transform);
        endTurnButton = buttonObj.AddComponent<Button>();

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.25f, 0.25f, 0.25f);

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-50, 50);
        buttonRect.sizeDelta = new Vector2(350, 120);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(buttonObj.transform);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "END TURN";
        btnText.fontSize = 40;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontStyle = FontStyle.Bold;

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        Outline btnOutline = btnTextObj.AddComponent<Outline>();
        btnOutline.effectColor = Color.black;
        btnOutline.effectDistance = new Vector2(3, 3);

        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        endTurnButton.interactable = false;
    }

    void InitializeGame()
    {
        if (territories.Count >= 5)
        {
            territories[0].SetOwner(1);
            territories[1].SetOwner(1);
            territories[2].SetOwner(0);
            territories[3].SetOwner(2);
            territories[4].SetOwner(2);
        }
    }

    public void OnTerritoryClicked(Territory territory)
    {
        if (currentPlayer != 1) return;

        if (playerUnitsToPlace > 0)
        {
            if (territory.ownerPlayer == 1)
            {
                territory.AddUnits(1);
                playerUnitsToPlace--;

                if (playerUnitsToPlace == 0)
                {
                    statusText.text = "Attack or END TURN";
                    endTurnButton.interactable = true;
                    endTurnButton.GetComponent<Image>().color = new Color(0.15f, 0.85f, 0.15f);
                }
                else
                {
                    statusText.text = $"Place {playerUnitsToPlace} Units";
                }
            }
        }
        else
        {
            if (selectedTerritory == null)
            {
                if (territory.ownerPlayer == 1 && territory.unitCount > 1)
                {
                    selectedTerritory = territory;
                    statusText.text = "Tap Enemy to Attack";
                }
            }
            else
            {
                if (territory.ownerPlayer != 1 && selectedTerritory.IsAdjacentTo(territory))
                {
                    Attack(selectedTerritory, territory);
                    selectedTerritory = null;
                }
                else if (territory == selectedTerritory)
                {
                    selectedTerritory = null;
                    statusText.text = "Attack or END TURN";
                }
            }
        }
    }

    void Attack(Territory attacker, Territory defender)
    {
        int atkPower = attacker.unitCount - 1;
        int defPower = defender.unitCount;

        int atkRoll = UnityEngine.Random.Range(1, 7) * atkPower;
        int defRoll = UnityEngine.Random.Range(1, 7) * defPower;

        if (atkRoll > defRoll)
        {
            defender.SetOwner(attacker.ownerPlayer);
            defender.SetUnits(atkPower);
            attacker.SetUnits(1);
            statusText.text = "🎉 Captured!";
            CheckWin();
        }
        else
        {
            attacker.SetUnits(1);
            statusText.text = "❌ Failed!";
        }
    }

    void CheckWin()
    {
        int p1 = territories.FindAll(t => t.ownerPlayer == 1).Count;
        int p2 = territories.FindAll(t => t.ownerPlayer == 2).Count;
        int requiredToWin = territories.Count - 1; // Need 4 out of 5 to win

        // Win if you control 4+ territories OR all territories
        if (p1 >= requiredToWin) EndGame(true);
        else if (p2 >= requiredToWin) EndGame(false);
        else if (currentRound >= maxRounds) EndGame(p1 > p2);
    }

    void OnEndTurnClicked()
    {
        endTurnButton.interactable = false;
        endTurnButton.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);

        currentPlayer = 2;
        currentRound++;
        statusText.text = "AI Turn...";

        Invoke("AITurn", 1.5f);
    }

    void AITurn()
    {
        List<Territory> aiTerr = territories.FindAll(t => t.ownerPlayer == 2);
        foreach (Territory t in aiTerr) t.AddUnits(1);

        if (aiTerr.Count > 0)
        {
            Territory attacker = aiTerr[UnityEngine.Random.Range(0, aiTerr.Count)];
            List<Territory> enemies = new List<Territory>();

            foreach (Territory t in territories)
            {
                if (t.ownerPlayer != 2 && attacker.IsAdjacentTo(t))
                    enemies.Add(t);
            }

            if (enemies.Count > 0 && attacker.unitCount > 1)
            {
                Territory target = enemies[UnityEngine.Random.Range(0, enemies.Count)];

                int atkPower = attacker.unitCount - 1;
                int defPower = target.unitCount;
                int atkRoll = UnityEngine.Random.Range(1, 7) * atkPower;
                int defRoll = UnityEngine.Random.Range(1, 7) * defPower;

                if (atkRoll > defRoll)
                {
                    target.SetOwner(2);
                    target.SetUnits(atkPower);
                    attacker.SetUnits(1);
                }
                else
                {
                    attacker.SetUnits(1);
                }
            }
        }

        currentPlayer = 1;
        playerUnitsToPlace = 3;
        statusText.text = $"Your Turn - Place {playerUnitsToPlace} Units";

        CheckWin();
    }
}
