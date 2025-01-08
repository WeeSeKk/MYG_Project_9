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
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] List<GameObject> playersScores;
    LeaderboardUserdata leaderboardUserdata = new LeaderboardUserdata();
    public static IHMManager instance;

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

    public void Test()
    {
        GetLeaderboardDatas();
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

    async Task GetLeaderboardDatas()
    {
        var topScores = await DatabaseManager.instance.GetTopScoresAsync();

        foreach (var score in topScores)
        {
            leaderboardUserdata.usernames.Add(score.Contains("username") ? score["username"].AsString : "Unknown");
            leaderboardUserdata.scores.Add(score.Contains("score") ? score["score"].AsInt32 : 0);
            leaderboardUserdata.dates.Add(score.Contains("date") ? score["date"].ToString() : "No date");
        }

        ShowLeaderboardDatas("Scores", true);
    }

    public void ShowLeaderboardDatas(string type, bool reverse)
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
                    dateText.text = leaderboardUserdata.dates[index];
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
                    dateText.text = leaderboardUserdata.dates[index];
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
                    dateText.text = leaderboardUserdata.dates[index];
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
                    dateText.text = leaderboardUserdata.dates[index];
                }
            }
        }
        else
        {
            if (!reverse)
            {
                var sortedByDate = leaderboardUserdata.dates.Select((date, index) => new { Date = DateTime.Parse(date), Index = index }).OrderBy(entry => entry.Date).ToList();

                for (int i = 0; i < sortedByDate.Count && i < playersScores.Count; i++)
                {
                    int index = sortedByDate[i].Index; 

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index];
                }
            }
            else
            {
                var sortedByDate = leaderboardUserdata.dates.Select((date, index) => new { Date = DateTime.Parse(date), Index = index }).OrderByDescending(entry => entry.Date).ToList();

                for (int i = 0; i < sortedByDate.Count && i < playersScores.Count; i++)
                {
                    int index = sortedByDate[i].Index; 

                    TMP_Text usernameText = playersScores[i].transform.GetChild(0).GetComponent<TMP_Text>();
                    TMP_Text scoreText = playersScores[i].transform.GetChild(1).GetComponent<TMP_Text>();
                    TMP_Text dateText = playersScores[i].transform.GetChild(2).GetComponent<TMP_Text>();

                    usernameText.text = leaderboardUserdata.usernames[index];
                    scoreText.text = leaderboardUserdata.scores[index].ToString();
                    dateText.text = leaderboardUserdata.dates[index];
                }
            }
        }
    }
}

public class LeaderboardUserdata
{
    public List<string> usernames { get; set; } = new List<string>();
    public List<int> scores { get; set; } = new List<int>();
    public List<string> dates { get; set; } = new List<string>();
}