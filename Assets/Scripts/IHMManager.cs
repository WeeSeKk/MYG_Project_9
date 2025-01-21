using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Database;
using System.Collections;

namespace IHM
{
    public class IHMManager : MonoBehaviour
    {
        public static IHMManager instance;
        [SerializeField] GameObject gameOverScreenUI;
        [SerializeField] GameObject lobbyUI;
        [SerializeField] GameObject leaderboardUI;
        [SerializeField] GameObject databaseChoiceUI;
        [SerializeField] GameObject scoreCountGO;
        [SerializeField] TMP_InputField usernameInputField;
        [SerializeField] TMP_InputField passwordInputField;
        [SerializeField] GameObject currentUserScore;
        [SerializeField] List<GameObject> playersScores;
        [SerializeField] Image usernameArrow;
        [SerializeField] Image scoreArrow;
        [SerializeField] Image dateArrow;
        [SerializeField] TMP_Text scoreCount;
        [SerializeField] TMP_Text errorMessage;
        [SerializeField] TMP_Text databaseConnectionText;
        [SerializeField] GameObject databaseConnectionButtons;
        [SerializeField] GameObject jumpButtonAndroid;
        [SerializeField] Button retryConnectionButton;
        [SerializeField] Button showAllButton;
        [SerializeField] Button showMonthlyButton;
        public LeaderboardUserdata leaderboardUserdata = new LeaderboardUserdata();
        bool reverse;
        bool monthly = false;

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
            EventManager.gameOver += GameOverScreen;
            EventManager.resetGame += ResetGame;
            showAllButton.interactable = false;
        }

        public IEnumerator ShowErrorMessages(string message)
        {
            errorMessage.text = message;
            yield return new WaitForSeconds(3f);
            errorMessage.text = "";
        }

        public void MonthlyOrAllLeaderboard(Button button)
        {
            if (button == showMonthlyButton)
            {
                monthly = true;
                ShowLeaderboardDatas("Scores");
                showMonthlyButton.interactable = false;
                showAllButton.interactable = true;
            }
            else
            {
                monthly = false;
                ShowLeaderboardDatas("Scores");
                showMonthlyButton.interactable = true;
                showAllButton.interactable = false;
            }
        }

        public void ShowScoreCount()
        {
            if (scoreCountGO.activeSelf == false)
            {
                scoreCountGO.SetActive(true);
                jumpButtonAndroid.SetActive(true);
            }
            else
            {
                scoreCountGO.SetActive(false);
                jumpButtonAndroid.SetActive(false);
            }
        }

        public void DatabaseConnectionUI(bool connected)
        {
            if (!connected)
            {
                databaseConnectionText.text = "no internet connection";
                retryConnectionButton.gameObject.SetActive(true);
                databaseConnectionButtons.gameObject.SetActive(false);
            }
            else
            {
                databaseConnectionText.text = "connected";
                retryConnectionButton.gameObject.SetActive(false);
                databaseConnectionButtons.gameObject.SetActive(true);
            }
        }

        public void ShowLoginUI()
        {
            lobbyUI.SetActive(true);
            databaseChoiceUI.SetActive(false);
        }

        public void UpdateScoreCount(int score)
        {
            scoreCount.text = "Score : " + score;
        }

        public void OnLoginRegisterClicked(bool register)
        {
            DatabaseManager.instance.LoginRegister(register, usernameInputField.text, passwordInputField.text);
        }

        void GameOverScreen()
        {
            gameOverScreenUI.SetActive(true);
        }

        void ResetGame()
        {
            gameOverScreenUI.SetActive(false);
        }

        public void CloseLobbyUI()
        {
            lobbyUI.SetActive(false);
            leaderboardUI.SetActive(true);
        }

        public void ShowLeaderboard()
        {
            if (leaderboardUI.gameObject.activeSelf == true)
            {
                leaderboardUI.SetActive(false);
            }
            else
            {
                gameOverScreenUI.SetActive(false);
                leaderboardUI.SetActive(true);
            }
        }

        public async Task AddCurrentUserOnLeaderboardNOSQL()
        {
            var currentUserDatas = await DatabaseManager.instance.CheckUserInLeaderboard(DatabaseManager.instance.currentUsername);

            if (currentUserDatas == null)
            {
                return;
            }

            TMP_Text usernameText = currentUserScore.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text scoreText = currentUserScore.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text dateText = currentUserScore.transform.GetChild(2).GetComponent<TMP_Text>();

            usernameText.text = currentUserDatas.Contains("username") ? currentUserDatas["username"].AsString : "Unknown";
            scoreText.text = (currentUserDatas.Contains("score") ? currentUserDatas["score"].AsInt32 : 0).ToString();
            dateText.text = (currentUserDatas.Contains("dateofscore") ? currentUserDatas["dateofscore"].ToUniversalTime() : DateTime.MinValue).ToString().Substring(0, currentUserDatas["dateofscore"].ToString().Length - 13);
        }

        public async Task RequestLeaderboardDatasNOSQL()
        {
            var topScores = await DatabaseManager.instance.GetLeaderboardDatasNOSQL();

            foreach (var score in topScores)
            {
                leaderboardUserdata.usernames.Add(score.Contains("username") ? score["username"].AsString : "Unknown");
                leaderboardUserdata.scores.Add(score.Contains("score") ? score["score"].AsInt32 : 0);
                leaderboardUserdata.dates.Add(score.Contains("dateofscore") ? score["dateofscore"].ToUniversalTime() : DateTime.MinValue);
            }

            var topMonthlyScores = await DatabaseManager.instance.GetMonthlyLeaderboardDatasNOSQL();

            foreach (var score in topMonthlyScores)
            {
                leaderboardUserdata.monthlyUsernames.Add(score.Contains("username") ? score["username"].AsString : "Unknown");
                leaderboardUserdata.monthlyScores.Add(score.Contains("score") ? score["score"].AsInt32 : 0);
                leaderboardUserdata.monthlyDates.Add(score.Contains("dateofscore") ? score["dateofscore"].ToUniversalTime() : DateTime.MinValue);
            }

            ShowLeaderboardDatas("Scores");
        }

        public async Task RequestLeaderboardDatasSQL()
        {
            JArray leaderboard = await DatabaseManager.instance.GetLeaderboardDatasSQL();

            foreach (JObject keys in leaderboard)
            {
                leaderboardUserdata.usernames.Add((string)keys.GetValue("username"));
                leaderboardUserdata.scores.Add((int)keys.GetValue("score"));
                leaderboardUserdata.dates.Add(Convert.ToDateTime(keys.GetValue("dateofscore")));
            }

            JArray monthlyLeaderboard = await DatabaseManager.instance.GetMonthlyLeaderboardDatasSQL();

            foreach (JObject keys in monthlyLeaderboard)
            {
                leaderboardUserdata.monthlyUsernames.Add((string)keys.GetValue("username"));
                leaderboardUserdata.monthlyScores.Add((int)keys.GetValue("score"));
                leaderboardUserdata.monthlyDates.Add(Convert.ToDateTime(keys.GetValue("dateofscore")));
            }

            ShowLeaderboardDatas("Scores");
        }

        public async Task AddCurrentUserOnLeaderboardSQL()
        {
            JArray jArray = await DatabaseManager.instance.CheckUserInLeaderboardSQL();

            TMP_Text usernameText = currentUserScore.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text scoreText = currentUserScore.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text dateText = currentUserScore.transform.GetChild(2).GetComponent<TMP_Text>();

            foreach (JObject keys in jArray)
            {
                usernameText.text = (string)keys.GetValue("username");
                scoreText.text = (string)keys.GetValue("score");
                dateText.text = Convert.ToDateTime(keys.GetValue("dateofscore")).ToString().Substring(0, Convert.ToDateTime(keys.GetValue("dateofscore")).ToString().Length - 9);
            }
        }

        public void ShowLeaderboardDatas(string type)// please be mercyfull when you see this monster
        {
            CleanLeaderboard();

            if (type == "Usernames")
            {
                var sortedUsernames = leaderboardUserdata.usernames.OrderByDescending(name => name).ToList();
                usernameArrow.transform.rotation = new Quaternion(180, 0, 0, 0);

                if (monthly)
                {
                    sortedUsernames = leaderboardUserdata.monthlyUsernames.OrderByDescending(name => name).ToList();
                }

                if (!reverse)
                {
                    sortedUsernames = leaderboardUserdata.usernames.OrderBy(name => name).ToList();

                    if (monthly)
                    {
                        sortedUsernames = leaderboardUserdata.monthlyUsernames.OrderBy(name => name).ToList();
                    }

                    usernameArrow.transform.rotation = new Quaternion(0, 0, 0, 0);
                    reverse = true;
                }
                else
                {
                    reverse = false;
                }

                for (int i = 0; i < sortedUsernames.Count && i < playersScores.Count; i++)
                {
                    int index = leaderboardUserdata.usernames.IndexOf(sortedUsernames[i]);

                    if (monthly)
                        index = leaderboardUserdata.monthlyUsernames.IndexOf(sortedUsernames[i]);

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    if (monthly)
                    {
                        usernameText.text = leaderboardUserdata.monthlyUsernames[index];
                        scoreText.text = leaderboardUserdata.monthlyScores[index].ToString();
                        dateText.text = leaderboardUserdata.monthlyDates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);
                    }
                    else
                    {
                        usernameText.text = leaderboardUserdata.usernames[index];
                        scoreText.text = leaderboardUserdata.scores[index].ToString();
                        dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);
                    }

                    usernameArrow.gameObject.SetActive(true);
                    scoreArrow.gameObject.SetActive(false);
                    dateArrow.gameObject.SetActive(false);
                }
            }
            else if (type == "Scores")
            {
                var sortedScores = leaderboardUserdata.scores.OrderByDescending(score => score).ToList();
                scoreArrow.transform.rotation = new Quaternion(0, 0, 0, 0);

                if (monthly)
                {
                    sortedScores = leaderboardUserdata.monthlyScores.OrderByDescending(score => score).ToList();
                }

                if (!reverse)
                {
                    sortedScores = leaderboardUserdata.scores.OrderBy(score => score).ToList();

                    if (monthly)
                    {
                        sortedScores = leaderboardUserdata.monthlyScores.OrderBy(score => score).ToList();
                    }

                    scoreArrow.transform.rotation = new Quaternion(180, 0, 0, 0);
                    reverse = true;
                }
                else
                {
                    reverse = false;
                }

                for (int i = 0; i < sortedScores.Count && i < playersScores.Count; i++)
                {
                    int index = leaderboardUserdata.scores.IndexOf(sortedScores[i]);

                    if (monthly)
                        index = leaderboardUserdata.monthlyScores.IndexOf(sortedScores[i]);

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    if (monthly)
                    {
                        usernameText.text = leaderboardUserdata.monthlyUsernames[index];
                        scoreText.text = leaderboardUserdata.monthlyScores[index].ToString();
                        dateText.text = leaderboardUserdata.monthlyDates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);
                    }
                    else
                    {
                        usernameText.text = leaderboardUserdata.usernames[index];
                        scoreText.text = leaderboardUserdata.scores[index].ToString();
                        dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);
                    }

                    scoreArrow.gameObject.SetActive(true);
                    dateArrow.gameObject.SetActive(false);
                    usernameArrow.gameObject.SetActive(false);
                }
            }
            else
            {
                var sortedByDate = leaderboardUserdata.dates.Select((date, index) => new { Date = date, Index = index }).OrderByDescending(entry => entry.Date).ToList();
                dateArrow.transform.rotation = new Quaternion(0, 0, 0, 0);

                if (monthly)
                {
                    sortedByDate = leaderboardUserdata.monthlyDates.Select((date, index) => new { Date = date, Index = index }).OrderByDescending(entry => entry.Date).ToList();
                }

                if (!reverse)
                {
                    sortedByDate = leaderboardUserdata.dates.Select((date, index) => new { Date = date, Index = index }).OrderBy(entry => entry.Date).ToList();

                    if (monthly)
                    {
                        sortedByDate = leaderboardUserdata.monthlyDates.Select((date, index) => new { Date = date, Index = index }).OrderBy(entry => entry.Date).ToList();
                    }

                    dateArrow.transform.rotation = new Quaternion(180, 0, 0, 0);
                    reverse = true;
                }
                else
                {
                    reverse = false;
                }

                for (int i = 0; i < sortedByDate.Count && i < playersScores.Count; i++)
                {
                    int index = sortedByDate[i].Index;

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    if (monthly)
                    {
                        usernameText.text = leaderboardUserdata.monthlyUsernames[index];
                        scoreText.text = leaderboardUserdata.monthlyScores[index].ToString();
                        dateText.text = leaderboardUserdata.monthlyDates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);
                    }
                    else
                    {
                        usernameText.text = leaderboardUserdata.usernames[index];
                        scoreText.text = leaderboardUserdata.scores[index].ToString();
                        dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);
                    }

                    dateArrow.gameObject.SetActive(true);
                    usernameArrow.gameObject.SetActive(false);
                    scoreArrow.gameObject.SetActive(false);
                }
            }
        }

        void CleanLeaderboard()
        {
            for (int i = 0; i < playersScores.Count; i++)
            {
                TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                usernameText.text = "";
                scoreText.text = "";
                dateText.text = "";
            }
        }
    }

    public class LeaderboardUserdata
    {
        public List<string> usernames { get; set; } = new List<string>();
        public List<int> scores { get; set; } = new List<int>();
        public List<DateTime> dates { get; set; } = new List<DateTime>();
        public List<string> monthlyUsernames { get; set; } = new List<string>();
        public List<int> monthlyScores { get; set; } = new List<int>();
        public List<DateTime> monthlyDates { get; set; } = new List<DateTime>();
    }
}