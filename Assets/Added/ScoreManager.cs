using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // Singleton to access from anywhere
    private int score = 0;
    public Text scoreText; // Reference to the UI Text element
    
    void Awake()
    {
        // Set up singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateScoreUI();
    }
    
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
        UpdateScoreUI();
        
        // Check win condition with GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.CheckWinCondition(score);
        }
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogWarning("Score Text UI not assigned in Inspector!");
        }
    }
    
    public int GetScore()
    {
        return score;
    }
    
    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }
}