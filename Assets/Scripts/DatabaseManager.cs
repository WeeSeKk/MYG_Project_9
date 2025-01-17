using MongoDB.Driver;
using MongoDB.Bson;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using BCrypt.Net;
using BCrypt;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;
    IMongoCollection<BsonDocument> _collection;
    MongoClient client;
    IMongoDatabase database;
    public string currentUsername;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        ConnectToMongoDB();
    }

    void ConnectToMongoDB()
    {
        try
        {
            string connectionString = "mongodb+srv://weesekk:2zmqnh0FQW74CnPd@cluster0.5zx1h.mongodb.net/MYG_Project_9?authMechanism=SCRAM-SHA-1&retryWrites=true&w=majority";
            client = new MongoClient(connectionString);
            database = client.GetDatabase("MYG_Project_9");

            var command = new BsonDocument("ping", 1);
            database.RunCommand<BsonDocument>(command);

            Debug.Log("Connected to MongoDB successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect to MongoDB: " + ex.Message);
        }
    }

    public async void OnLogin(string username, string password)
    {
        var collection = database.GetCollection<BsonDocument>("Users");

        if (collection == null)
        {
            Debug.LogError("Collection 'Users' not found!");
            return;
        }

        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("username", username),
            Builders<BsonDocument>.Filter.Eq("password", password)
        );

        var user = await collection.Find(filter).FirstOrDefaultAsync();

        if (user != null)
        {
            Debug.Log("User found, login successful!");
            currentUsername = username;
            await IHMManager.instance.RequestLeaderboardDatas();
            await IHMManager.instance.AddCurrentUserOnLeaderboard();
            IHMManager.instance.ShowLeaderboardDatas("Scores");
            IHMManager.instance.CloseLobbyUI();
        }
        else
        {
            Debug.LogError("Invalid username or password.");
            //call IHMManager
        }
    }

    public async void OnRegister(string username, string password)
    {
        var collection = database.GetCollection<BsonDocument>("Users");

        if (collection == null)
        {
            Debug.LogError("Collection 'Users' not found!");
            return;
        }

        var filter = Builders<BsonDocument>.Filter.Eq("username", username);
        var existingUser = await collection.Find(filter).FirstOrDefaultAsync();

        if (existingUser != null)
        {
            Debug.LogError("Username is already taken");
            //call IHMManager
            return;
        }

        var newUser = new BsonDocument
        {
            { "username", username },
            { "password", password }
        };

        await collection.InsertOneAsync(newUser);
        Debug.Log("User registered successfully!");
        OnLogin(username, password);
    }

    public async Task UpdatePlayerScore(int newScore)
    {
        var collection = database.GetCollection<BsonDocument>("Leaderboard");

        var filter = Builders<BsonDocument>.Filter.Eq("username", currentUsername);

        var userDocument = await collection.Find(filter).FirstOrDefaultAsync();

        if (userDocument == null)
        {
            var newUserDocument = new BsonDocument
            {
                { "username", currentUsername },
                { "score", newScore },
                { "dateofscore", DateTime.UtcNow }
            };

            await collection.InsertOneAsync(newUserDocument);
        }
        else
        {
            int currentScore = userDocument["score"].AsInt32;

            if (newScore > currentScore)
            {
                var update = Builders<BsonDocument>.Update
                    .Set("score", newScore)
                    .Set("dateofscore", DateTime.UtcNow);

                await collection.UpdateOneAsync(filter, update);
            }
            else
            {
                Debug.Log("same score");
            }
        }
    }

    public async Task<BsonDocument> CheckUserInLeaderboard(string username)
    {
        var collection = database.GetCollection<BsonDocument>("Leaderboard");

        var filter = Builders<BsonDocument>.Filter.Eq("username", username);

        try
        {
            var userDocument = await collection.Find(filter).FirstOrDefaultAsync();

            if (userDocument != null)
            {
                return userDocument;
            }
            else
            {
                Debug.Log($"User '{username}' does not exist in the leaderboard.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
            return null;
        }
    }

    public async Task<List<BsonDocument>> GetLeaderboardDatas()
    {
        var collection = database.GetCollection<BsonDocument>("Leaderboard");
        int limit = 10;

        try
        {
            var sort = Builders<BsonDocument>.Sort.Descending("score");
            var topScores = await collection
                .Find(new BsonDocument()) 
                .Sort(sort)
                .Limit(limit)
                .ToListAsync();
            return topScores;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error fetching top scores: {ex.Message}");
            return null;
        }
    }
}