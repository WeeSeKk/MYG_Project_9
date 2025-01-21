using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;
    public static event Action gameOver;
    public static event Action gameStart;
    public static event Action resetGame;
    public static event Action scroreUP;
    public static event Action jump;

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
        DontDestroyOnLoad(this.gameObject);
    }

    public static void GameOver()
    {
        gameOver?.Invoke();
    }

    public static void GameStart()
    {
        gameStart?.Invoke();
    }

    public static void ScroreUP()
    {
        scroreUP?.Invoke();
    }

    public static void ResetGame()
    {
        resetGame?.Invoke();
    }

    public static void Jump()
    {
        jump?.Invoke();
    }
}
