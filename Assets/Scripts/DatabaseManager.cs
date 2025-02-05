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
            RetryConnection();
        }

        public void RetryConnection()
        {
            TryInternetConnection();
        }

        public async Task TryInternetConnection()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://google.com");
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response Code: " + www.responseCode);
                Debug.LogError("Response Body: " + www.downloadHandler.text);
                Debug.LogError("Failed to connect to internet");
                connected = false;
                IHMManager.instance.DatabaseConnectionUI(false);
            }
            else
            {
                Debug.Log("Connected to internet");
                connected = true;
                IHMManager.instance.DatabaseConnectionUI(true);
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

                UserData newUser = new UserData
                {
                    username = username,
                    password = passwordHash
                };

                string jsonData = JsonUtility.ToJson(newUser);

                UnityWebRequest www = new UnityWebRequest("https://weesek.ddns.net/MongoDB/api/users/register", "POST");
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error: " + www.error);
                    Debug.LogError("Response Code: " + www.responseCode);
                    Debug.LogError("Response Body: " + www.downloadHandler.text);
                }
                else
                {
                    Debug.Log("User registered successfully!");
                }
            }
        }
        #endregion

        #region NOSQL
        public async void OnLogin(string username, string password)
        {
            UserData user = new UserData
            {
                username = username,
                password = password
            };

            string jsonData = JsonUtility.ToJson(user);

            UnityWebRequest www = new UnityWebRequest("https://weesek.ddns.net/MongoDB/api/users/login", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response Code: " + www.responseCode);
                Debug.LogError("Response Body: " + www.downloadHandler.text);
                StartCoroutine(IHMManager.instance.ShowErrorMessages("Invalid password or username."));
            }
            else
            {
                currentUsername = username;
                await IHMManager.instance.RequestLeaderboardDatasNOSQL();
                await IHMManager.instance.AddCurrentUserOnLeaderboardNOSQL();
                IHMManager.instance.ShowLeaderboardDatas("Scores");
                IHMManager.instance.CloseLobbyUI();
                Debug.Log("User logged in successfully!");
            }
        }

        public async void OnRegister(string newUsername, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 13);

            UserData newUser = new UserData
            {
                username = newUsername,
                password = passwordHash
            };

            string jsonData = JsonUtility.ToJson(newUser);

            UnityWebRequest www = new UnityWebRequest("https://weesek.ddns.net/MongoDB/api/users/register", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response Code: " + www.responseCode);
                Debug.LogError("Response Body: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("User registered successfully!");
                SyncDatabases(newUsername, password);
                OnLogin(newUsername, password);
            }
        }

        public async Task UpdatePlayerScore(int newScore)
        {
            LeaderboardData leaderboardData = new LeaderboardData
            {
                username = currentUsername,
                score = newScore,
                dateTime = DateTime.UtcNow
            };

            string jsonData = JsonUtility.ToJson(leaderboardData);

            UnityWebRequest www = new UnityWebRequest("https://weesek.ddns.net/MongoDB/api/users/update", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response Code: " + www.responseCode);
                Debug.LogError("Response Body: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Score updated successfully!");
            }
        }

        public async Task<JObject> CheckUserInLeaderboard(string username)
        {
            string url = "https://weesek.ddns.net/MongoDB/api/users/usersdata/" + username;

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

                JObject jObject = JObject.Parse(webRequest.downloadHandler.text);

                return jObject;
            }
        }

        public async Task<JArray> GetLeaderboardDatasNOSQL()
        {
            string url = "https://weesek.ddns.net/MongoDB/api/users/leaderboard/get";

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

        public async Task<JArray> GetMonthlyLeaderboardDatasNOSQL()
        {
            string url = "https://weesek.ddns.net/MongoDB/api/users/monthlyleaderboard/get";

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
[System.Serializable]
public class UserData
{
    public string username;
    public string password;
}

[System.Serializable]
public class LeaderboardData
{
    public string username;
    public int score;
    public DateTime dateTime;
}