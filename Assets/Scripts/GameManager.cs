using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System;

//Todo this just tracks lives, and lets you restart game
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int p1Lives;
    public int p2Lives;

    public event Action<int, int> OnLivesChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateLives(int playerIndex, int lives)
    {
        if (playerIndex == 0)
            p1Lives = lives;
        else if (playerIndex == 1)
            p2Lives = lives;

        OnLivesChanged?.Invoke(p1Lives, p2Lives);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //scene currently active for player
    }
}
