using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostSurveyManager : MonoBehaviour
{
    [Header("Survey UI Panel")]
    public GameObject postSurveyPanel;

    [Header("Question 1: Rating (Optional)")]
    public Button[] starButtons; // 5 star buttons
    public Image[] starImages; // 5 star images to color
    public Color selectedStarColor = Color.yellow;
    public Color unselectedStarColor = Color.gray;
    private int selectedRating = 0;

    [Header("Question 2: Experienced Lag? (Optional)")]
    public Toggle lagYesToggle;
    public Toggle lagNoToggle;

    [Header("Question 3: Would Play Again? (Optional)")]
    public Toggle playAgainYesToggle;
    public Toggle playAgainNoToggle;

    [Header("Question 4: Feedback (Optional)")]
    public TMP_InputField feedbackInputField;
    public TextMeshProUGUI characterCountText;
    private int maxCharacters = 200;

    [Header("Buttons")]
    public Button submitButton;
    public Button skipButton;

    // Store survey responses
    private PostSurveyData surveyData;

    void Start()
    {
        // Setup star rating buttons
        for (int i = 0; i < starButtons.Length; i++)
        {
            int starIndex = i + 1; // 1-5 instead of 0-4
            starButtons[i].onClick.AddListener(() => OnStarClicked(starIndex));
        }

        // Setup lag toggles (mutually exclusive)
        lagYesToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) lagNoToggle.isOn = false;
        });
        lagNoToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) lagYesToggle.isOn = false;
        });

        // Setup play again toggles (mutually exclusive)
        playAgainYesToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) playAgainNoToggle.isOn = false;
        });
        playAgainNoToggle.onValueChanged.AddListener((isOn) => {
            if (isOn) playAgainYesToggle.isOn = false;
        });

        // Setup feedback character counter
        feedbackInputField.onValueChanged.AddListener(UpdateCharacterCount);
        feedbackInputField.characterLimit = maxCharacters;
        UpdateCharacterCount(feedbackInputField.text);

        // Setup buttons
        submitButton.onClick.AddListener(OnSubmitClicked);
        skipButton.onClick.AddListener(OnSkipClicked);

        // Initialize star colors
        UpdateStarDisplay();
    }

    void OnStarClicked(int rating)
    {
        selectedRating = rating;
        UpdateStarDisplay();
        Debug.Log($"⭐ Rating selected: {rating} stars");
    }

    void UpdateStarDisplay()
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < selectedRating)
                starImages[i].color = selectedStarColor;
            else
                starImages[i].color = unselectedStarColor;
        }
    }

    void UpdateCharacterCount(string text)
    {
        int remaining = maxCharacters - text.Length;
        characterCountText.text = $"{remaining} characters remaining";
    }

    void OnSubmitClicked()
    {
        // Collect responses (all optional)
        surveyData = new PostSurveyData
        {
            rating = selectedRating, // 0 if not selected
            experiencedLag = GetLagResponse(),
            wouldPlayAgain = GetPlayAgainResponse(),
            feedback = feedbackInputField.text.Trim(),
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        Debug.Log("✅ POST-SURVEY SUBMITTED!");
        Debug.Log($"Rating: {surveyData.rating} stars");
        Debug.Log($"Lag: {surveyData.experiencedLag}");
        Debug.Log($"Play again: {surveyData.wouldPlayAgain}");
        Debug.Log($"Feedback: {surveyData.feedback}");

        // Save to PlayerPrefs
        SaveSurveyData();

        // Continue to session summary
        ContinueToSummary();
    }

    void OnSkipClicked()
    {
        Debug.Log("⏭️ POST-SURVEY SKIPPED");

        // Create empty survey data
        surveyData = new PostSurveyData
        {
            rating = 0,
            experiencedLag = "Not answered",
            wouldPlayAgain = "Not answered",
            feedback = "",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // Save empty data
        SaveSurveyData();

        // Continue to session summary
        ContinueToSummary();
    }

    string GetLagResponse()
    {
        if (lagYesToggle.isOn)
            return "Yes";
        else if (lagNoToggle.isOn)
            return "No";
        else
            return "Not answered";
    }

    string GetPlayAgainResponse()
    {
        if (playAgainYesToggle.isOn)
            return "Yes";
        else if (playAgainNoToggle.isOn)
            return "No";
        else
            return "Not answered";
    }

    void SaveSurveyData()
    {
        PlayerPrefs.SetInt("PostSurvey_Rating", surveyData.rating);
        PlayerPrefs.SetString("PostSurvey_Lag", surveyData.experiencedLag);
        PlayerPrefs.SetString("PostSurvey_PlayAgain", surveyData.wouldPlayAgain);
        PlayerPrefs.SetString("PostSurvey_Feedback", surveyData.feedback);
        PlayerPrefs.Save();
    }

    void ContinueToSummary()
    {
        // Hide post-survey panel
        postSurveyPanel.SetActive(false);

        // Trigger GameManager to show session summary
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // GameManager should have a method to show session summary
            // gameManager.ShowSessionSummary();
            Debug.Log("✅ Triggering GameManager to show session summary");
        }
    }

    public PostSurveyData GetSurveyData()
    {
        return surveyData;
    }
}

// ========== POST-SURVEY DATA STRUCTURE ==========
[Serializable]
public class PostSurveyData
{
    public int rating; // 0-5 (0 = not rated)
    public string experiencedLag; // "Yes", "No", or "Not answered"
    public string wouldPlayAgain; // "Yes", "No", or "Not answered"
    public string feedback; // Free text (max 200 chars)
    public string timestamp;
}