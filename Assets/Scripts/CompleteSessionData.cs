using System;
using UnityEngine;

// ========== COMPLETE SESSION DATA WITH SURVEYS ==========
[Serializable]
public class CompleteSessionData
{
    // ===== PRE-SURVEY DATA =====
    public string country;
    public string city;
    public string ageRange;
    public string gender;
    public string gamingFrequency;
    public string platformsUsed;
    public string preferredGenres;

    // ===== INFRASTRUCTURE MEASUREMENTS =====
    public float downloadSpeedMbps;
    public float latencyMs;
    public float batteryDrainPercent;
    public float dataUsageMB;

    // ===== MODE INFORMATION =====
    public string recommendedMode;
    public string chosenMode;
    public bool followedRecommendation;

    // ===== GAMEPLAY RESULTS =====
    public bool playerWon;
    public int territoriesControlled;
    public int totalTerritories;
    public string gameMode;
    public float sessionDurationMinutes;

    // ===== POST-SURVEY DATA =====
    public int rating; // 0-5 (0 = not rated)
    public string experiencedLag; // "Yes", "No", or "Not answered"
    public string wouldPlayAgain; // "Yes", "No", or "Not answered"
    public string feedback; // Free text (max 200 chars)

    // ===== METADATA =====
    public string timestamp;
    public string sessionId;

    // Constructor
    public CompleteSessionData()
    {
        // Load pre-survey data from PlayerPrefs
        country = PlayerPrefs.GetString("PreSurvey_Country", "Unknown");
        city = PlayerPrefs.GetString("PreSurvey_City", "Unknown");
        ageRange = PlayerPrefs.GetString("PreSurvey_AgeRange", "Unknown");
        gender = PlayerPrefs.GetString("PreSurvey_Gender", "Prefer not to say");
        gamingFrequency = PlayerPrefs.GetString("PreSurvey_Frequency", "Unknown");
        platformsUsed = PlayerPrefs.GetString("PreSurvey_Platforms", "Unknown");
        preferredGenres = PlayerPrefs.GetString("PreSurvey_Genres", "Unknown");

        // Load post-survey data from PlayerPrefs
        rating = PlayerPrefs.GetInt("PostSurvey_Rating", 0);
        experiencedLag = PlayerPrefs.GetString("PostSurvey_Lag", "Not answered");
        wouldPlayAgain = PlayerPrefs.GetString("PostSurvey_PlayAgain", "Not answered");
        feedback = PlayerPrefs.GetString("PostSurvey_Feedback", "");

        // Initialize metadata
        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        sessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + UnityEngine.Random.Range(1000, 9999);
    }

    // Method to set infrastructure data
    public void SetInfrastructureData(float speed, float latency, float battery, float data)
    {
        downloadSpeedMbps = speed;
        latencyMs = latency;
        batteryDrainPercent = battery;
        dataUsageMB = data;
    }

    // Method to set mode data
    public void SetModeData(string recommended, string chosen, bool followed)
    {
        recommendedMode = recommended;
        chosenMode = chosen;
        followedRecommendation = followed;
    }

    // Method to set gameplay data
    public void SetGameplayData(bool won, int territories, int total, string mode, float duration)
    {
        playerWon = won;
        territoriesControlled = territories;
        totalTerritories = total;
        gameMode = mode;
        sessionDurationMinutes = duration;
    }
}