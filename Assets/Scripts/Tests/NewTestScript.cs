using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Database;
using IHM;
using Level;

public class NewTestScript
{
    [UnityTest]
    public IEnumerator _01InternetConnection()
    {
        yield return SceneManager.LoadSceneAsync(0);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(DatabaseManager.instance.connected);
    }

    [UnityTest]
    public IEnumerator _02GetDatasNOSQL()
    {
        DatabaseManager.instance.ChooseDatabase(false);
        DatabaseManager.instance.LoginRegister(false, "Test", "123456");

        yield return new WaitForSeconds(1);

        Assert.IsTrue(DatabaseManager.instance.currentUsername == "Test");
        Assert.IsNotNull(IHMManager.instance.leaderboardUserdata.usernames);
    }

    [UnityTest]
    public IEnumerator _03SendDatasNOSQLAndSQL()
    {
        LevelManager levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        IHMManager.instance.ShowLeaderboard();
        IHMManager.instance.ShowScoreCount();
        levelManager.StartGame();

        Time.timeScale = 10;
        yield return new WaitForSeconds(5);
        Time.timeScale = 1;
        yield return new WaitForSeconds(1);

        LogAssert.Expect(LogType.Log, "Score in database is higher or equal NOSQL");
        LogAssert.Expect(LogType.Log, "Score in database is higher or equal SQL");
    }

    [UnityTest]
    public IEnumerator _04GetDatasSQL()
    {
        yield return SceneManager.LoadSceneAsync(1);
        yield return SceneManager.LoadSceneAsync(0);
        yield return new WaitForSeconds(1);

        Assert.IsTrue(DatabaseManager.instance.connected);

        DatabaseManager.instance.ChooseDatabase(true);
        DatabaseManager.instance.LoginRegister(false, "Test", "123456");

        yield return new WaitForSeconds(1);

        Assert.IsTrue(DatabaseManager.instance.currentUsername == "Test");
        Assert.IsNotNull(IHMManager.instance.leaderboardUserdata.usernames);
    }
/*
    [UnityTest]
    public IEnumerator _05SendDatasSQL()
    {
        LevelManager levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        IHMManager.instance.ShowLeaderboard();
        IHMManager.instance.ShowScoreCount();
        levelManager.StartGame();

        Time.timeScale = 10;
        yield return new WaitForSeconds(5);
        Time.timeScale = 1;

        LogAssert.Expect(LogType.Log, "Score in database is higher or equal");
    }*/
}
