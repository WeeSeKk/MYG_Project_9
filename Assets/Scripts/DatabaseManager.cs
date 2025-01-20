using MongoDB.Driver;
using MongoDB.Bson;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using BCrypt.Net;
using BCrypt;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager instance;
    IMongoCollection<BsonDocument> _collection;
    MongoClient client;
    IMongoDatabase database;
    public string currentUsername;
    bool sql;

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

    #region NOSQL

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
            await IHMManager.instance.RequestLeaderboardDatasNOSQL();
            await IHMManager.instance.AddCurrentUserOnLeaderboardNOSQL();
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
                Debug.Log("Same score as databse");
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

    public async Task<List<BsonDocument>> GetLeaderboardDatasNOSQL()
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
    #endregion

    #region SQL

    public async Task<JArray> GetLeaderboardDatasSQL()
    {
        string url = "http://localhost/MYG9/index.php?leaderboard=get";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    return null;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    return null;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }

            JArray jArray = JArray.Parse(webRequest.downloadHandler.text);

            return jArray;
        }
    }

    public async Task<JArray> CheckUserInLeaderboardSQL()
    {
        string url = "http://localhost/MYG9/index.php?currentuser=" + currentUsername;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    return null;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    return null;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }

            JArray jArray = JArray.Parse(webRequest.downloadHandler.text);

            return jArray;
        }
    }

    public async void OnLoginSQL(string username, string password)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("action", "login"));
        formData.Add(new MultipartFormDataSection("username", username));
        formData.Add(new MultipartFormDataSection("password", password));

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/MYG9/insert.php", formData);
        await www.SendWebRequest();

        JObject jsonResponse = JObject.Parse(www.downloadHandler.text);
        string resultString = "";
        foreach (var key in jsonResponse)
        {
            resultString += $"{key.Key}: {key.Value}\n";
        }

        if (resultString.Contains("Success: True"))
        {
            currentUsername = username;
            await IHMManager.instance.RequestLeaderboardDatasSQL();
            await IHMManager.instance.AddCurrentUserOnLeaderboardSQL();
            IHMManager.instance.ShowLeaderboardDatas("Scores");
            IHMManager.instance.CloseLobbyUI();
            //loged in
        }
        else
        {
            //error
        }
    }

    public async void OnRegisterSQL(string username, string password)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("action", "register"));
        formData.Add(new MultipartFormDataSection("username", username));
        formData.Add(new MultipartFormDataSection("password", password));

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/MYG9/insert.php", formData);
        await www.SendWebRequest();

        JObject jsonResponse = JObject.Parse(www.downloadHandler.text);
        string resultString = "";
        foreach (var key in jsonResponse)
        {
            resultString += $"{key.Key}: {key.Value}\n";
        }

        if (resultString.Contains("Success: True"))
        {
            //registered
            OnLoginSQL(username, password);
            //OnRegister(username, password);
        }
        else
        {
            //error
        }
    }

    public async Task UpdatePlayerScoreSQL(int newScore)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("action", "updatescore"));
        formData.Add(new MultipartFormDataSection("username", currentUsername));
        formData.Add(new MultipartFormDataSection("score", newScore.ToString()));
        formData.Add(new MultipartFormDataSection("dateofscore", DateTime.UtcNow.ToString()));

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/MYG9/insert.php", formData);
        await www.SendWebRequest();

        JObject jsonResponse = JObject.Parse(www.downloadHandler.text);
        string resultString = "";
        foreach (var key in jsonResponse)
        {
            resultString += $"{key.Key}: {key.Value}\n";
        }

        if (resultString.Contains("Success: True"))
        {
            //score updated
        }
        else
        {
            //error
        }
    }

    #endregion
}