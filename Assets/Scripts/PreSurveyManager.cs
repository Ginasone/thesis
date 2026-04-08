using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreSurveyManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager; // CONNECT THIS IN INSPECTOR!

    [Header("Question 1: Country (Required)")]
    public TMP_Dropdown countryDropdown;

    [Header("Question 2: City (Required)")]
    public TMP_InputField cityInputField;

    [Header("Question 3: Age Range (Required)")]
    public TMP_Dropdown ageDropdown;

    [Header("Question 4: Gender (Optional)")]
    public TMP_Dropdown genderDropdown;

    [Header("Question 5: Gaming Frequency (Required)")]
    public TMP_Dropdown gamingFrequencyDropdown;

    [Header("Question 6: Platforms Used (Required)")]
    public TMP_Dropdown platformsDropdown;

    [Header("Question 7: Preferred Genres (Required)")]
    public TMP_Dropdown genresDropdown;

    [Header("Submit Button")]
    public Button submitButton;

    [Header("Error Message")]
    public TextMeshProUGUI errorText;

    // Store survey responses
    private PreSurveyData surveyData;

    void Start()
    {
        // Setup dropdowns
        SetupDropdowns();

        // Attach submit button
        submitButton.onClick.AddListener(OnSubmitClicked);

        // Hide error message initially
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        Debug.Log("✅ PreSurveyManager started");
    }

    void SetupDropdowns()
    {
        // Country dropdown
        countryDropdown.ClearOptions();
        countryDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "-- Select Country --",
            "Ghana",
            "South Africa",
            "Other"
        });

        // Age range dropdown
        ageDropdown.ClearOptions();
        ageDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "-- Select Age Range --",
            "15-20",
            "21-30",
            "31-40",
            "40+"
        });

        // Gender dropdown (optional)
        genderDropdown.ClearOptions();
        genderDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Prefer not to say",
            "Male",
            "Female",
            "Non-binary",
            "Other"
        });

        // Gaming frequency dropdown
        gamingFrequencyDropdown.ClearOptions();
        gamingFrequencyDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "-- Select Frequency --",
            "Daily",
            "Several times per week",
            "Weekly",
            "Less than weekly"
        });

        // Platforms used dropdown
        platformsDropdown.ClearOptions();
        platformsDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "-- Select Platform(s) --",
            "Mobile only",
            "PC only",
            "Console only",
            "Mobile + PC",
            "Mobile + Console",
            "PC + Console",
            "All platforms (Mobile + PC + Console)"
        });

        // Preferred genres dropdown
        genresDropdown.ClearOptions();
        genresDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "-- Select Genre(s) --",
            "Strategy",
            "Action/Shooter",
            "RPG (Role-Playing)",
            "Sports",
            "Puzzle",
            "Multiple genres"
        });
    }

    void OnSubmitClicked()
    {
        Debug.Log("🔘 Submit button clicked!");

        // Validate required fields
        if (!ValidateInputs())
        {
            ShowError("Please complete all required fields (marked with *)");
            Debug.LogWarning("⚠️ Validation failed");
            return;
        }

        // Collect responses
        surveyData = new PreSurveyData
        {
            country = countryDropdown.options[countryDropdown.value].text,
            city = cityInputField.text.Trim(),
            ageRange = ageDropdown.options[ageDropdown.value].text,
            gender = genderDropdown.options[genderDropdown.value].text,
            gamingFrequency = gamingFrequencyDropdown.options[gamingFrequencyDropdown.value].text,
            platformsUsed = platformsDropdown.options[platformsDropdown.value].text,
            preferredGenres = genresDropdown.options[genresDropdown.value].text,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        Debug.Log("✅ PRE-SURVEY COMPLETED!");
        Debug.Log($"Country: {surveyData.country}, City: {surveyData.city}");

        // Save to PlayerPrefs (so GameManager can access later)
        PlayerPrefs.SetString("PreSurvey_Country", surveyData.country);
        PlayerPrefs.SetString("PreSurvey_City", surveyData.city);
        PlayerPrefs.SetString("PreSurvey_AgeRange", surveyData.ageRange);
        PlayerPrefs.SetString("PreSurvey_Gender", surveyData.gender);
        PlayerPrefs.SetString("PreSurvey_Frequency", surveyData.gamingFrequency);
        PlayerPrefs.SetString("PreSurvey_Platforms", surveyData.platformsUsed);
        PlayerPrefs.SetString("PreSurvey_Genres", surveyData.preferredGenres);
        PlayerPrefs.Save();

        // NOW enable infrastructure and mode selector
        InfrastructureManager infraManager = FindObjectOfType<InfrastructureManager>();
        if (infraManager != null)
        {
            infraManager.enabled = true;
            Debug.Log("✅ InfrastructureManager ENABLED after survey");
        }

        ModeSelector modeSelector = FindObjectOfType<ModeSelector>();
        if (modeSelector != null)
        {
            modeSelector.enabled = true;
            Debug.Log("✅ ModeSelector ENABLED after survey");
        }

        // Notify GameManager
        if (gameManager != null)
        {
            gameManager.OnPreSurveyCompleted();
            Debug.Log("✅ Notified GameManager");
        }
    }

    bool ValidateInputs()
    {
        // Check country (not first option)
        if (countryDropdown.value == 0)
        {
            Debug.Log("❌ Country not selected");
            return false;
        }

        // Check city (not empty)
        if (string.IsNullOrWhiteSpace(cityInputField.text))
        {
            Debug.Log("❌ City is empty");
            return false;
        }

        // Check age range (not first option)
        if (ageDropdown.value == 0)
        {
            Debug.Log("❌ Age range not selected");
            return false;
        }

        // Gender is optional, no validation needed

        // Check gaming frequency (not first option)
        if (gamingFrequencyDropdown.value == 0)
        {
            Debug.Log("❌ Gaming frequency not selected");
            return false;
        }

        // Check platforms (not first option)
        if (platformsDropdown.value == 0)
        {
            Debug.Log("❌ Platforms not selected");
            return false;
        }

        // Check genres (not first option)
        if (genresDropdown.value == 0)
        {
            Debug.Log("❌ Genres not selected");
            return false;
        }

        Debug.Log("✅ All validations passed");
        return true;
    }

    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            Invoke("HideError", 3f); // Hide after 3 seconds
        }
    }

    void HideError()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    public PreSurveyData GetSurveyData()
    {
        return surveyData;
    }
}

// ========== PRE-SURVEY DATA STRUCTURE ==========
[Serializable]
public class PreSurveyData
{
    public string country;
    public string city;
    public string ageRange;
    public string gender;
    public string gamingFrequency;
    public string platformsUsed;
    public string preferredGenres;
    public string timestamp;
}