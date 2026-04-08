using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    // Firebase references
    private DatabaseReference databaseReference;
    private FirebaseAuth auth;
    private string userId;

    // Initialization status
    public bool isInitialized = false;

    void Start()
    {
        Debug.Log("=== FIREBASE MANAGER STARTING ===");
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        // Check Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("✅ Firebase dependencies OK!");

                // Initialize Firebase
                FirebaseApp app = FirebaseApp.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                auth = FirebaseAuth.DefaultInstance;

                // Sign in anonymously
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"❌ Firebase dependency error: {task.Result}");
                isInitialized = false;
            }
        });
    }

    void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("❌ Anonymous sign-in was canceled");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"❌ Anonymous sign-in failed: {task.Exception}");
                return;
            }

            // Success! FIXED: Use task.Result.User instead of task.Result
            AuthResult authResult = task.Result;
            FirebaseUser user = authResult.User;
            userId = user.UserId;
            isInitialized = true;

            Debug.Log($"✅ Signed in anonymously! User ID: {userId}");
        });
    }

    // ========== SAVE SESSION DATA ==========
    public void SaveSessionData(SessionData data)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ Firebase not initialized - data not saved");
            return;
        }

        // Create unique session ID
        string sessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + UnityEngine.Random.Range(1000, 9999);
        string path = $"sessions/{userId}/{sessionId}";

        // Convert to JSON
        string json = JsonUtility.ToJson(data);

        // Upload to Firebase
        databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"✅ SESSION DATA SAVED TO FIREBASE!");
                Debug.Log($"Path: {path}");
            }
            else
            {
                Debug.LogError($"❌ Failed to save data: {task.Exception}");
            }
        });
    }

    // ========== SAVE SESSION DATA JSON (NEW METHOD) ==========
    public void SaveSessionDataJSON(string json, string sessionId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ Firebase not initialized - data not saved");
            return;
        }

        string path = $"sessions/{userId}/{sessionId}";

        // Upload to Firebase
        databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"✅ SESSION DATA SAVED TO FIREBASE!");
                Debug.Log($"Path: {path}");
                Debug.Log($"Session ID: {sessionId}");
            }
            else
            {
                Debug.LogError($"❌ Failed to save data: {task.Exception}");
            }
        });
    }

    // ========== GET USER ID ==========
    public string GetUserId()
    {
        return userId;
    }
}

// ========== DATA STRUCTURE ==========
[Serializable]
public class SessionData
{
    // Infrastructure measurements
    public float downloadSpeedMbps;
    public float latencyMs;
    public float batteryDrainPercent;
    public float dataUsageMB;

    // Mode information
    public string recommendedMode;
    public string chosenMode;
    public bool followedRecommendation;

    // Gameplay results
    public bool playerWon;
    public int territoriesControlled;
    public int totalTerritories;
    public string gameMode;

    // Session metadata
    public string timestamp;
    public string country; // "Ghana" or "South Africa"
    public string city;

    // Constructor
    public SessionData(
        float speed, float latency, float battery, float data,
        string recommended, string chosen, bool followed,
        bool won, int territories, int total, string mode,
        string countryName, string cityName)
    {
        downloadSpeedMbps = speed;
        latencyMs = latency;
        batteryDrainPercent = battery;
        dataUsageMB = data;

        recommendedMode = recommended;
        chosenMode = chosen;
        followedRecommendation = followed;

        playerWon = won;
        territoriesControlled = territories;
        totalTerritories = total;
        gameMode = mode;

        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        country = countryName;
        city = cityName;
    }
}