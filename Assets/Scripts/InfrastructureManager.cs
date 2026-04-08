
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InfrastructureManager : MonoBehaviour
{
    // Infrastructure Measurements
    public float downloadSpeedMbps = 0f;
    public float latencyMs = 0f;
    public float batteryLevelStart = 0f;
    public float batteryLevelCurrent = 0f;
    public float batteryDrainRate = 0f;
    public float estimatedDataUsageMB = 0f;

    // Recommendation
    public string recommendedMode = "offline";

    // Measurement status
    public bool measurementsComplete = false;

    // Timing
    private float sessionStartTime;
    private float lastBatteryCheckTime;

    void Start()
    {
        Debug.Log("=== INFRASTRUCTURE MANAGER STARTED ===");

        // Record session start time
        sessionStartTime = Time.time;
        lastBatteryCheckTime = Time.time;

        // Get initial battery level
        batteryLevelStart = SystemInfo.batteryLevel;
        batteryLevelCurrent = batteryLevelStart;

        Debug.Log($"Initial battery: {batteryLevelStart * 100}%");

        // Start infrastructure measurements
        StartCoroutine(MeasureInfrastructure());
    }

    void Update()
    {
        // Update battery monitoring every 5 seconds
        if (Time.time - lastBatteryCheckTime >= 5f)
        {
            UpdateBatteryMetrics();
            lastBatteryCheckTime = Time.time;
        }
    }

    IEnumerator MeasureInfrastructure()
    {
        Debug.Log("Starting infrastructure measurements...");

        // Step 1: Measure Internet Speed
        yield return StartCoroutine(MeasureDownloadSpeed());

        // Step 2: Measure Latency
        yield return StartCoroutine(MeasureLatency());

        // Step 3: Calculate recommendation
        CalculateRecommendation();

        // Mark measurements as complete
        measurementsComplete = true;

        Debug.Log("=== INFRASTRUCTURE MEASUREMENTS COMPLETE ===");
        LogAllMeasurements();
    }

    // ========== INTERNET SPEED MEASUREMENT ==========
    IEnumerator MeasureDownloadSpeed()
    {
        Debug.Log("Measuring download speed...");

        // Download a small test file (100KB)
        // Using a reliable CDN test file
        string testFileUrl = "https://proof.ovh.net/files/100Kb.dat";

        float startTime = Time.realtimeSinceStartup;

        UnityWebRequest request = UnityWebRequest.Get(testFileUrl);
        yield return request.SendWebRequest();

        float endTime = Time.realtimeSinceStartup;
        float downloadTime = endTime - startTime;

        if (request.result == UnityWebRequest.Result.Success)
        {
            // File is 100KB = 0.0976 MB
            float fileSizeMB = 0.0976f;

            // Calculate speed: MB / seconds = MB/s
            float speedMBps = fileSizeMB / downloadTime;

            // Convert to Mbps (Megabits per second)
            // 1 MB = 8 Megabits
            downloadSpeedMbps = speedMBps * 8;

            // Add estimated data usage
            estimatedDataUsageMB += fileSizeMB;

            Debug.Log($"Download speed: {downloadSpeedMbps:F2} Mbps");
            Debug.Log($"Download time: {downloadTime:F2} seconds");
        }
        else
        {
            Debug.LogWarning("Speed test failed - assuming low speed");
            downloadSpeedMbps = 2.0f; // Assume low speed if test fails
        }
    }

    // ========== LATENCY MEASUREMENT ==========
    IEnumerator MeasureLatency()
    {
        Debug.Log("Measuring latency...");

        // Ping Google's DNS server
        string pingTarget = "https://dns.google";

        float totalLatency = 0f;
        int pingCount = 3;
        int successfulPings = 0;

        for (int i = 0; i < pingCount; i++)
        {
            float startTime = Time.realtimeSinceStartup;

            UnityWebRequest request = UnityWebRequest.Head(pingTarget);
            yield return request.SendWebRequest();

            float endTime = Time.realtimeSinceStartup;
            float pingTime = (endTime - startTime) * 1000f; // Convert to milliseconds

            if (request.result == UnityWebRequest.Result.Success)
            {
                totalLatency += pingTime;
                successfulPings++;
                Debug.Log($"Ping {i + 1}: {pingTime:F1}ms");
            }

            // Small delay between pings
            yield return new WaitForSeconds(0.5f);
        }

        if (successfulPings > 0)
        {
            latencyMs = totalLatency / successfulPings;
            Debug.Log($"Average latency: {latencyMs:F1}ms");
        }
        else
        {
            Debug.LogWarning("Latency test failed - assuming high latency");
            latencyMs = 300f; // Assume high latency if test fails
        }
    }

    // ========== BATTERY MONITORING ==========
    void UpdateBatteryMetrics()
    {
        batteryLevelCurrent = SystemInfo.batteryLevel;

        // Calculate drain rate (percentage per minute)
        float sessionDurationMinutes = (Time.time - sessionStartTime) / 60f;

        if (sessionDurationMinutes > 0)
        {
            float batteryDrainPercent = (batteryLevelStart - batteryLevelCurrent) * 100f;
            batteryDrainRate = batteryDrainPercent / sessionDurationMinutes;
        }

        Debug.Log($"Battery: {batteryLevelCurrent * 100:F1}% (Drain: {batteryDrainRate:F2}%/min)");
    }

    // ========== MODE RECOMMENDATION ALGORITHM ==========
    void CalculateRecommendation()
    {
        Debug.Log("Calculating recommended mode...");

        // Thresholds (based on your methodology)
        float realTimeSpeedThreshold = 10f;  // Mbps
        float turnBasedSpeedThreshold = 3f;  // Mbps
        float realTimeLatencyThreshold = 150f; // ms

        // Decision logic
        if (downloadSpeedMbps >= realTimeSpeedThreshold && latencyMs < realTimeLatencyThreshold)
        {
            recommendedMode = "real-time";
            Debug.Log("✅ RECOMMENDED: Real-Time Multiplayer");
        }
        else if (downloadSpeedMbps >= turnBasedSpeedThreshold)
        {
            recommendedMode = "turn-based";
            Debug.Log("✅ RECOMMENDED: Turn-Based Online");
        }
        else
        {
            recommendedMode = "offline";
            Debug.Log("✅ RECOMMENDED: Offline Mode");
        }
    }

    // ========== DATA LOGGING ==========
    void LogAllMeasurements()
    {
        Debug.Log("===========================================");
        Debug.Log("INFRASTRUCTURE MEASUREMENTS SUMMARY:");
        Debug.Log($"Download Speed: {downloadSpeedMbps:F2} Mbps");
        Debug.Log($"Latency: {latencyMs:F1} ms");
        Debug.Log($"Battery Start: {batteryLevelStart * 100:F1}%");
        Debug.Log($"Battery Current: {batteryLevelCurrent * 100:F1}%");
        Debug.Log($"Battery Drain Rate: {batteryDrainRate:F2}%/min");
        Debug.Log($"Data Usage: {estimatedDataUsageMB:F3} MB");
        Debug.Log($"Recommended Mode: {recommendedMode.ToUpper()}");
        Debug.Log("===========================================");
    }

    // ========== PUBLIC METHODS FOR GAME MANAGER ==========

    public string GetRecommendedMode()
    {
        return recommendedMode;
    }

    public bool AreMeasurementsComplete()
    {
        return measurementsComplete;
    }

    public Dictionary<string, float> GetAllMetrics()
    {
        Dictionary<string, float> metrics = new Dictionary<string, float>();
        metrics["downloadSpeed"] = downloadSpeedMbps;
        metrics["latency"] = latencyMs;
        metrics["batteryStart"] = batteryLevelStart * 100f;
        metrics["batteryCurrent"] = batteryLevelCurrent * 100f;
        metrics["batteryDrain"] = batteryDrainRate;
        metrics["dataUsage"] = estimatedDataUsageMB;

        return metrics;
    }

    // ========== SAVE DATA FOR ANALYSIS ==========
    public string GetMeasurementsAsJSON()
    {
        // Create a simple JSON string manually
        string json = "{";
        json += $"\"downloadSpeedMbps\": {downloadSpeedMbps:F2},";
        json += $"\"latencyMs\": {latencyMs:F1},";
        json += $"\"batteryStart\": {batteryLevelStart * 100:F1},";
        json += $"\"batteryCurrent\": {batteryLevelCurrent * 100:F1},";
        json += $"\"batteryDrainRate\": {batteryDrainRate:F2},";
        json += $"\"dataUsageMB\": {estimatedDataUsageMB:F3},";
        json += $"\"recommendedMode\": \"{recommendedMode}\",";
        json += $"\"timestamp\": \"{System.DateTime.Now}\"";
        json += "}";

        return json;
    }

    // Call this to save data to a file
    public void SaveMeasurements()
    {
        string data = GetMeasurementsAsJSON();
        Debug.Log("SAVE THIS DATA:");
        Debug.Log(data);

        // In Week 4, we'll add Firebase here
        // For now, just log it
    }
}