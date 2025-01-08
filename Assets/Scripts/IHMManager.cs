using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IHMManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverScreenUI;
    [SerializeField] GameObject lobbyUI;
    [SerializeField] GameObject LeaderboardUI;
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] GameObject currentUserScore;
    [SerializeField] List<GameObject> playersScores;
    LeaderboardUserdata leaderboardUserdata = new LeaderboardUserdata();
    public static IHMManager instance;
    bool reverse;

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
    }

    public void OnLoginClicked()
    {
        DatabaseManager.instance.OnLogin(usernameInputField.text, passwordInputField.text);
    }

    public void OnRegisterClicked()
    {
        DatabaseManager.instance.OnRegister(usernameInputField.text, passwordInputField.text);
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
        LeaderboardUI.SetActive(true);
    }

    public async Task AddCurrentUserOnLeaderboard()
    {
        var currentUserDatas = await DatabaseManager.instance.CheckUserInLeaderboard(DatabaseManager.instance.currentUsername);

        TMP_Text usernameText = currentUserScore.transform.GetChild(0).GetComponent<TMP_Text>();
        TMP_Text scoreText = currentUserScore.transform.GetChild(1).GetComponent<TMP_Text>();
        TMP_Text dateText = currentUserScore.transform.GetChild(2).GetComponent<TMP_Text>();

        usernameText.text = currentUserDatas.Contains("username") ? currentUserDatas["username"].AsString : "Unknown";
        scoreText.text = (currentUserDatas.Contains("score") ? currentUserDatas["score"].AsInt32 : 0).ToString();
        dateText.text = (currentUserDatas.Contains("dateofscore") ? currentUserDatas["dateofscore"].ToUniversalTime() : DateTime.MinValue).ToString().Substring(0, currentUserDatas["dateofscore"].ToString().Length - 9);
    }

    public async Task RequestLeaderboardDatas()
    {
        var topScores = await DatabaseManager.instance.GetLeaderboardDatas();

        foreach (var score in topScores)
        {
            leaderboardUserdata.usernames.Add(score.Contains("username") ? score["username"].AsString : "Unknown");
            leaderboardUserdata.scores.Add(score.Contains("score") ? score["score"].AsInt32 : 0);
            leaderboardUserdata.dates.Add(score.Contains("dateofscore") ? score["dateofscore"].ToUniversalTime() : DateTime.MinValue);
        }

        ShowLeaderboardDatas("Scores");
    }

    public void ShowLeaderboardDatas(string type)
    {
        if (type == "Usernames")
        {
            if (!reverse)
            {
                var sortedUsernames = leaderboardUserdata.usernames.OrderBy(name => name).ToList();

                for (int i = 0; i < sortedUsernames.Count && i < playersScores.Count; i++)
                {
                    int index = leaderboardUserdata.usernames.IndexOf(sortedUsernames[i]);

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);

                    reverse = true;
                }
            }
            else
            {
                var sortedUsernames = leaderboardUserdata.usernames.OrderByDescending(name => name).ToList();

                for (int i = 0; i < sortedUsernames.Count && i < playersScores.Count; i++)
                {
                    int index = leaderboardUserdata.usernames.IndexOf(sortedUsernames[i]);

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);

                    reverse = false;
                }
            }
        }
        else if (type == "Scores")
        {
            if (!reverse)
            {
                var sortedScores = leaderboardUserdata.scores.OrderBy(score => score).ToList();

                for (int i = 0; i < sortedScores.Count && i < playersScores.Count; i++)
                {
                    int index = leaderboardUserdata.scores.IndexOf(sortedScores[i]);

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);

                    reverse = true;
                }
            }
            else
            {
                var sortedScores = leaderboardUserdata.scores.OrderByDescending(score => score).ToList();

                for (int i = 0; i < sortedScores.Count && i < playersScores.Count; i++)
                {
                    int index = leaderboardUserdata.scores.IndexOf(sortedScores[i]);

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);

                    reverse = false;
                }
            }
        }
        else
        {
            if (!reverse)
            {
                var sortedByDate = leaderboardUserdata.dates.Select((date, index) => new { Date = date, Index = index }).OrderBy(entry => entry.Date).ToList();

                for (int i = 0; i < sortedByDate.Count && i < playersScores.Count; i++)
                {
                    int index = sortedByDate[i].Index; 

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);

                    reverse = true;
                }
            }
            else
            {
                var sortedByDate = leaderboardUserdata.dates.Select((date, index) => new { Date = date, Index = index }).OrderByDescending(entry => entry.Date).ToList();

                for (int i = 0; i < sortedByDate.Count && i < playersScores.Count; i++)
                {
                    int index = sortedByDate[i].Index; 

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index].ToString().Substring(0, leaderboardUserdata.dates[index].ToString().Length - 9);

                    reverse = false;
                }
            }
        }
    }
}

public class LeaderboardUserdata
{
    public List<string> usernames { get; set; } = new List<string>();
    public List<int> scores { get; set; } = new List<int>();
    public List<DateTime> dates { get; set; } = new List<DateTime>();
}