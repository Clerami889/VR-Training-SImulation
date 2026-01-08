using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int score = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // optional if you want it across scenes
    }

    public void AddPoints(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
        // You could also update UI here
    }

    public int GetScore()
    {
        return score;
    }
}