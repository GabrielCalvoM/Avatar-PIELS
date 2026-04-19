using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Payload document used by backend API.
/// </summary>
[System.Serializable]
public class PoseDocument
{
    public string id;

    public string poseName;
    public string createdBy = "user";
    public List<BoneData> bones = new List<BoneData>();
    public FacialExpressionData facialExpression = new FacialExpressionData();
}

[System.Serializable]
public class PoseNamesResponse
{
    public List<string> poseNames = new List<string>();
}

[System.Serializable]
public class PoseResponse
{
    public string poseName;
    public PoseData pose;
}

[System.Serializable]
public class SavePoseRequest
{
    public string poseName;
    public PoseData pose;
    public bool isSystemPose;
}

/// <summary>
/// Service for calling backend API that manages pose data in MongoDB
/// </summary>
public class MongoDBService
{
    private bool isConnected = false;
    private MongoDBConfig config;

    public bool IsConnected => isConnected;

    /// <summary>
    /// Initialize and verify backend API connection
    /// </summary>
    public async Task<bool> Initialize(MongoDBConfig mongoConfig)
    {
        try
        {
            config = mongoConfig;

            if (config == null || string.IsNullOrWhiteSpace(config.apiBaseUrl))
            {
                Debug.LogError("Pose API: Invalid API base URL");
                return false;
            }

            var healthUrl = BuildUrl("health");
            using UnityWebRequest request = UnityWebRequest.Get(healthUrl);
            ApplyCommonRequestOptions(request);
            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Pose API health check failed: {request.error}");
                isConnected = false;
                return false;
            }

            isConnected = true;
            Debug.Log("Pose API: Successfully connected");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pose API connection failed: {ex.Message}");
            isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Save a pose through backend API
    /// </summary>
    public async Task<bool> SavePose(string poseName, PoseData poseData, bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("Pose API: Not connected. Can't save pose.");
            return false;
        }

        try
        {
            SavePoseRequest payload = new SavePoseRequest
            {
                poseName = poseName,
                pose = poseData,
                isSystemPose = isSystemPose
            };

            string json = JsonUtility.ToJson(payload);
            var url = BuildUrl($"poses/{UnityWebRequest.EscapeURL(poseName)}?system={isSystemPose.ToString().ToLowerInvariant()}");
            using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT);
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyCommonRequestOptions(request);
            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Pose API: Failed to save pose: {request.error}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pose API: Failed to save pose: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load a pose by name through backend API
    /// </summary>
    public async Task<PoseData> LoadPose(string poseName, bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("Pose API: Not connected. Can't load pose.");
            return null;
        }

        try
        {
            var url = BuildUrl($"poses/{UnityWebRequest.EscapeURL(poseName)}?system={isSystemPose.ToString().ToLowerInvariant()}");
            using UnityWebRequest request = UnityWebRequest.Get(url);
            ApplyCommonRequestOptions(request);
            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 404)
                {
                    Debug.LogWarning($"Pose API: Pose '{poseName}' not found");
                }
                else
                {
                    Debug.LogError($"Pose API: Failed to load pose: {request.error}");
                }
                return null;
            }

            PoseResponse response = JsonUtility.FromJson<PoseResponse>(request.downloadHandler.text);
            if (response == null || response.pose == null)
            {
                Debug.LogError($"Pose API: Invalid response while loading pose '{poseName}'");
                return null;
            }

            return response.pose;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pose API: Failed to load pose: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get all pose names through backend API
    /// </summary>
    public async Task<List<string>> GetAllPoseNames(bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("Pose API: Not connected. Can't retrieve pose names.");
            return new List<string>();
        }

        try
        {
            var url = BuildUrl($"poses?system={isSystemPose.ToString().ToLowerInvariant()}");
            using UnityWebRequest request = UnityWebRequest.Get(url);
            ApplyCommonRequestOptions(request);
            await SendRequest(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Pose API: Failed to retrieve pose names: {request.error}");
                return new List<string>();
            }

            PoseNamesResponse response = JsonUtility.FromJson<PoseNamesResponse>(request.downloadHandler.text);
            if (response == null || response.poseNames == null)
            {
                return new List<string>();
            }

            return response.poseNames;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pose API: Failed to retrieve pose names: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Delete a pose through backend API
    /// </summary>
    public async Task<bool> DeletePose(string poseName, bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("Pose API: Not connected. Can't delete pose.");
            return false;
        }

        try
        {
            var url = BuildUrl($"poses/{UnityWebRequest.EscapeURL(poseName)}?system={isSystemPose.ToString().ToLowerInvariant()}");
            using UnityWebRequest request = UnityWebRequest.Delete(url);
            ApplyCommonRequestOptions(request);
            await SendRequest(request);

            if (request.result == UnityWebRequest.Result.Success)
            {
                return true;
            }

            if (request.responseCode == 404)
            {
                Debug.LogWarning($"Pose API: Pose '{poseName}' not found for deletion");
                return false;
            }

            Debug.LogError($"Pose API: Failed to delete pose: {request.error}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Pose API: Failed to delete pose: {ex.Message}");
            return false;
        }
    }

    private async Task SendRequest(UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<bool>();
        var operation = request.SendWebRequest();
        operation.completed += _ => tcs.TrySetResult(true);
        await tcs.Task;
    }

    private void ApplyCommonRequestOptions(UnityWebRequest request)
    {
        int timeoutMs = Mathf.Max(1000, config != null ? config.requestTimeoutMs : 10000);
        request.timeout = Mathf.CeilToInt(timeoutMs / 1000f);

        if (config != null && !string.IsNullOrWhiteSpace(config.apiKey))
        {
            request.SetRequestHeader("x-api-key", config.apiKey);
        }
    }

    private string BuildUrl(string path)
    {
        string baseUrl = config.apiBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{path.TrimStart('/')}";
    }
}
