using MongoDB.Driver;
using MongoDB.Bson;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using IHM;

namespace Database
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager instance;
        MongoClient client;
        IMongoDatabase database;
        public string currentUsername;
        public bool connected;
        bool sql;

        #region Utility
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

        public void ConnectToMongoDB()
        {
            try
            {
                string connectionString = "mongodb://weesekk:2zmqnh0FQW74CnPd@cluster0-shard-00-00.5zx1h.mongodb.net:27017,cluster0-shard-00-01.5zx1h.mongodb.net:27017,cluster0-shard-00-02.5zx1h.mongodb.net:27017/?replicaSet=atlas-cvvjrc-shard-0&authSource=admin&retryWrites=true&w=majority&tls=true";
                client = new MongoClient(connectionString);
                database = client.GetDatabase("MYG_Project_9");

                var command = new BsonDocument("ping", 1);
                database.RunCommand<BsonDocument>(command);

                Debug.Log("Connected to MongoDB successfully!");
                connected = true;
                IHMManager.instance.DatabaseConnectionUI(true);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to connect to MongoDB: " + ex.Message);
                connected = false;
                IHMManager.instance.DatabaseConnectionUI(false);
            }
        }

        public void ChooseDatabase(bool nosql)
        {
            if (nosql)
            {
                sql = false;
            }
            else
            {
                sql = true;
            }

            IHMManager.instance.ShowLoginUI();
        }

        public void LoginRegister(bool register, string username, string password)
        {
            if (register)
            {
                if (sql)
                {
                    OnRegisterSQL(username, password);
                }
                else
                {
                    OnRegister(username, password);
                }
            }
            else
            {
                if (sql)
                {
                    OnLoginSQL(username, password);
                }
                else
                {
                    OnLogin(username, password);
                }
            }
        }

        public async void SyncDatabases(string username, string password)
        {
            if (!sql)
            {
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormDataSection("action", "register"));
                formData.Add(new MultipartFormDataSection("username", username));
                formData.Add(new MultipartFormDataSection("password", password));

                UnityWebRequest www = UnityWebRequest.Post("https://weesek.ddns.net/MYG9/insert.php", formData);
                await www.SendWebRequest();
            }
            else
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 13);

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
                    StartCoroutine(IHMManager.instance.ShowErrorMessages("Username is already taken."));
                    return;
                }

                var newUser = new BsonDocument
                {
                    { "username", username },
                    { "password", passwordHash }
                };

                await collection.InsertOneAsync(newUser);
            }
        }
        #endregion

        #region NOSQL

        public async void OnLogin(string username, string password)
        {
            var collection = database.GetCollection<BsonDocument>("Users");

            if (collection == null)
            {
                Debug.LogError("Collection 'Users' not found!");
                return;
            }

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("username", username)
            );

            var user = await collection.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                string passwordHash = user.Contains("password") ? user["password"].AsString : "password";

                if (BCrypt.Net.BCrypt.Verify(password, passwordHash))
                {
                    currentUsername = username;
                    await IHMManager.instance.RequestLeaderboardDatasNOSQL();
                    await IHMManager.instance.AddCurrentUserOnLeaderboardNOSQL();
                    IHMManager.instance.ShowLeaderboardDatas("Scores");
                    IHMManager.instance.CloseLobbyUI();
                }
                else
                {
                    StartCoroutine(IHMManager.instance.ShowErrorMessages("Invalid password."));
                }
            }
            else
            {
                StartCoroutine(IHMManager.instance.ShowErrorMessages("Invalid username."));
                Debug.LogError("Invalid username");
            }
        }

        public async void OnRegister(string username, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 13);

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
                StartCoroutine(IHMManager.instance.ShowErrorMessages("Username is already taken."));
                return;
            }

            var newUser = new BsonDocument
        {
            { "username", username },
            { "password", passwordHash }
        };

            await collection.InsertOneAsync(newUser);
            Debug.Log("User registered successfully!");
            SyncDatabases(username, password);
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
                    Debug.Log("Score in database is higher or equal NOSQL");
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

        public async Task<List<BsonDocument>> GetMonthlyLeaderboardDatasNOSQL()
        {
            var collection = database.GetCollection<BsonDocument>("Leaderboard");
            int limit = 10;

            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var startOfNextMonth = startOfMonth.AddMonths(1);

                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("dateofscore", startOfMonth),
                    Builders<BsonDocument>.Filter.Lt("dateofscore", startOfNextMonth)
                );

                var sort = Builders<BsonDocument>.Sort.Descending("score");
                var topScores = await collection
                    .Find(filter)
                    .Sort(sort)
                    .Limit(limit)
                    .ToListAsync();

                return topScores;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching monthly top scores: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region SQL
        public async Task<JArray> GetLeaderboardDatasSQL()
        {
            string url = "https://weesek.ddns.net/MYG9/index.php?leaderboard=get";

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

        public async Task<JArray> GetMonthlyLeaderboardDatasSQL()
        {
            string url = "https://weesek.ddns.net/MYG9/index.php?monthlyleaderboard=get";

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
            string url = "https://weesek.ddns.net/MYG9/index.php?currentuser=" + currentUsername;

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

            UnityWebRequest www = UnityWebRequest.Post("https://weesek.ddns.net/MYG9/insert.php", formData);
            await www.SendWebRequest();

            JObject jsonResponse = JObject.Parse(www.downloadHandler.text);
            string resultString = "";
            foreach (var key in jsonResponse)
            {
                resultString += $"{key.Key}: {key.Value}\n";
            }

            if (resultString.Contains("Success: True"))
            {
                Debug.Log(www.downloadHandler.text);
                currentUsername = username;
                await IHMManager.instance.RequestLeaderboardDatasSQL();
                await IHMManager.instance.AddCurrentUserOnLeaderboardSQL();
                IHMManager.instance.ShowLeaderboardDatas("Scores");
                IHMManager.instance.CloseLobbyUI();
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                StartCoroutine(IHMManager.instance.ShowErrorMessages("Invalid username or password."));
            }
        }

        public async void OnRegisterSQL(string username, string password)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("action", "register"));
            formData.Add(new MultipartFormDataSection("username", username));
            formData.Add(new MultipartFormDataSection("password", password));

            UnityWebRequest www = UnityWebRequest.Post("https://weesek.ddns.net/MYG9/insert.php", formData);
            await www.SendWebRequest();

            JObject jsonResponse = JObject.Parse(www.downloadHandler.text);
            string resultString = "";
            foreach (var key in jsonResponse)
            {
                resultString += $"{key.Key}: {key.Value}\n";
            }

            if (resultString.Contains("Success: True"))
            {
                Debug.Log(www.downloadHandler.text);
                OnLoginSQL(username, password);
                SyncDatabases(username, password);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                StartCoroutine(IHMManager.instance.ShowErrorMessages("Username is already taken."));
            }

            //Debug.Log(www.downloadHandler.text);
        }

        public async Task UpdatePlayerScoreSQL(int newScore)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("action", "updatescore"));
            formData.Add(new MultipartFormDataSection("username", currentUsername));
            formData.Add(new MultipartFormDataSection("score", newScore.ToString()));
            formData.Add(new MultipartFormDataSection("dateofscore", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));

            UnityWebRequest www = UnityWebRequest.Post("https://weesek.ddns.net/MYG9/insert.php", formData);
            await www.SendWebRequest();

            JArray jsonResponse = JArray.Parse(www.downloadHandler.text);

            string resultString = "";
            foreach (var item in jsonResponse)
            {
                resultString += $"{item}\n";
            }

            if (www.downloadHandler.text.Contains("Score in database is higher or equal"))
            {
                Debug.Log("Score in database is higher or equal SQL");
            }
        }
    }
    #endregion
}