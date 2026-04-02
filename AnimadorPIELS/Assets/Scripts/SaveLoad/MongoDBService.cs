using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// MongoDB document structure for poses
/// </summary>
[System.Serializable]
public class PoseDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string id;

    public string poseName;
    public string createdBy = "user"; // "user" or "system" (default to "user")
    public DateTime createdAt;
    public DateTime updatedAt;
    public List<BoneData> bones = new List<BoneData>();
    public FacialExpressionData facialExpression = new FacialExpressionData();
}

/// <summary>
/// Service for connecting to MongoDB Atlas and managing pose data
/// </summary>
public class MongoDBService
{
    private MongoClient client;
    private IMongoDatabase database;
    private IMongoCollection<PoseDocument> userPosesCollection;
    private IMongoCollection<PoseDocument> systemPosesCollection;

    private bool isConnected = false;
    private MongoDBConfig config;

    public bool IsConnected => isConnected;

    /// <summary>
    /// Initialize the MongoDB connection
    /// </summary>
    public async Task<bool> Initialize(MongoDBConfig mongoConfig)
    {
        try
        {
            config = mongoConfig;

            if (string.IsNullOrEmpty(config.connectionString) ||
                config.connectionString.Contains("<username>") ||
                config.connectionString.Contains("<password>"))
            {
                Debug.LogError("MongoDB: Invalid connection string");
                return false;
            }

            // Create MongoDB client
            var settings = MongoClientSettings.FromConnectionString(config.connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(config.connectionTimeoutMs);
            settings.ConnectTimeout = TimeSpan.FromMilliseconds(config.connectionTimeoutMs);

            client = new MongoClient(settings);

            // Test connection
            await client.ListDatabaseNamesAsync();

            database = client.GetDatabase(config.databaseName);
            userPosesCollection = database.GetCollection<PoseDocument>(config.userPosesCollection);
            systemPosesCollection = database.GetCollection<PoseDocument>(config.systemPosesCollection);

            isConnected = true;
            Debug.Log("MongoDB: Successfully connected to Atlas");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"MongoDB connection failed: {ex.Message}");
            isConnected = false;
            return false;
        }
    }

    /// <summary>
    /// Save a pose to MongoDB
    /// </summary>
    public async Task<bool> SavePose(string poseName, PoseData poseData, bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("MongoDB: Not connected. Can't save pose.");
            return false;
        }

        try
        {
            var collection = isSystemPose ? systemPosesCollection : userPosesCollection;

            // Check if pose already exists
            var filter = Builders<PoseDocument>.Filter.Eq("poseName", poseName);
            var existingPose = await collection.Find(filter).FirstOrDefaultAsync();

            if (existingPose != null)
            {
                // Update existing pose
                existingPose.bones = poseData.bones;
                existingPose.facialExpression = poseData.facialExpression;
                existingPose.updatedAt = DateTime.UtcNow;

                await collection.ReplaceOneAsync(filter, existingPose);
                Debug.Log($"MongoDB: Updated pose '{poseName}'");
            }
            else
            {
                // Create new pose document
                var document = new PoseDocument
                {
                    poseName = poseName,
                    createdBy = isSystemPose ? "system" : "user",
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    bones = poseData.bones,
                    facialExpression = poseData.facialExpression
                };

                await collection.InsertOneAsync(document);
                Debug.Log($"MongoDB: Saved new pose '{poseName}'");
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"MongoDB: Failed to save pose: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load a pose from by name
    /// </summary>
    public async Task<PoseData> LoadPose(string poseName, bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("MongoDB: Not connected. Can't load pose.");
            return null;
        }

        try
        {
            var collection = isSystemPose ? systemPosesCollection : userPosesCollection;
            var filter = Builders<PoseDocument>.Filter.Eq("poseName", poseName);
            var document = await collection.Find(filter).FirstOrDefaultAsync();

            if (document == null)
            {
                Debug.LogWarning($"MongoDB: Pose '{poseName}' not found");
                return null;
            }

            var poseData = new PoseData
            {
                bones = document.bones,
                facialExpression = document.facialExpression ?? new FacialExpressionData()
            };

            Debug.Log($"MongoDB: Loaded pose '{poseName}'");
            return poseData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"MongoDB: Failed to load pose: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get all pose names
    /// </summary>
    public async Task<List<string>> GetAllPoseNames(bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("MongoDB: Not connected. Can't retrieve pose names.");
            return new List<string>();
        }

        try
        {
            var collection = isSystemPose ? systemPosesCollection : userPosesCollection;
            var projection = Builders<PoseDocument>.Projection.Include("poseName");
            var documents = await collection.Find(new BsonDocument()).Project<BsonDocument>(projection).ToListAsync();

            var poseNames = new List<string>();
            foreach (var doc in documents)
            {
                if (doc.Contains("poseName"))
                {
                    poseNames.Add(doc["poseName"].AsString);
                }
            }

            Debug.Log($"MongoDB: Retrieved {poseNames.Count} pose names");
            return poseNames;
        }
        catch (Exception ex)
        {
            Debug.LogError($"MongoDB: Failed to retrieve pose names: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Delete a pose
    /// </summary>
    public async Task<bool> DeletePose(string poseName, bool isSystemPose = false)
    {
        if (!isConnected)
        {
            Debug.LogError("MongoDB: Not connected. Can't delete pose.");
            return false;
        }

        try
        {
            var collection = isSystemPose ? systemPosesCollection : userPosesCollection;
            var filter = Builders<PoseDocument>.Filter.Eq("poseName", poseName);
            var result = await collection.DeleteOneAsync(filter);

            if (result.DeletedCount > 0)
            {
                Debug.Log($"MongoDB: Deleted pose '{poseName}'");
                return true;
            }
            else
            {
                Debug.LogWarning($"MongoDB: Pose '{poseName}' not found for deletion");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"MongoDB: Failed to delete pose: {ex.Message}");
            return false;
        }
    }
}
