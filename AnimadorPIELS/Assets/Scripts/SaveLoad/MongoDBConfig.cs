using UnityEngine;

/// <summary>
/// Configuration for MongoDB Atlas connection
/// Create this asset: Right-click > Create > MongoDB Config
/// </summary>
[CreateAssetMenu(fileName = "MongoDBConfig", menuName = "MongoDB Config", order = 1)]
public class MongoDBConfig : ScriptableObject
{
    [Header("MongoDB Atlas Connection")]
    [Tooltip("Your MongoDB Atlas connection string")]
    [TextArea(3, 5)]
    public string connectionString = "mongodb+srv://<username>:<password>@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority";

    [Header("Database Settings")]
    [Tooltip("Name of the database")]
    public string databaseName = "PIELSPosesDB";

    [Tooltip("Collection name for user poses")]
    public string userPosesCollection = "user_poses";

    [Tooltip("Collection name for system poses (T-pose, etc.)")]
    public string systemPosesCollection = "system_poses";

    [Header("Connection Settings")]
    [Tooltip("Connection timeout in milliseconds")]
    public int connectionTimeoutMs = 10000;
}
