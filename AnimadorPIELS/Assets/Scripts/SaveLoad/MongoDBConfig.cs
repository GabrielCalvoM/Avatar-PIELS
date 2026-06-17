using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Configuration for pose backend API connection
/// Create this asset: Right-click > Create > MongoDB Config
/// </summary>
[CreateAssetMenu(fileName = "MongoDBConfig", menuName = "Pose API Config", order = 1)]
public class MongoDBConfig : ScriptableObject
{
    [Header("Backend API")]
    [Tooltip("Base URL of backend API, default: http://localhost:3000/api")]
    [TextArea(3, 5)]
    public string apiBaseUrl = "http://localhost:3000/api";

    [Tooltip("Optional API key sent as x-api-key header")]
    public string apiKey = "";

    [Header("Request Settings")]
    [Tooltip("Request timeout in milliseconds")]
    [FormerlySerializedAs("connectionTimeoutMs")]
    public int requestTimeoutMs = 10000;
}
