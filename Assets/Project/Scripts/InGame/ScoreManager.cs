using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private int score = 0;
    public TextMeshProUGUI scoreText; // スコア表示UI

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // スコアを加算するメソッド
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    // 現在のスコアを返すメソッド
    public int GetCurrentScore()
    {
        return score;
    }

    // スコアを減らすメソッド (バッテリー回復などに使用)
    public void RemoveScore(int amount)
    {
        Debug.Log($"[Score] {amount} スコアを消費しました。(現在の稼ぎ: {score} -> {score - amount})");
        score -= amount;
        if (score < 0) score = 0;
        UpdateScoreUI();
    }

    public int EndShiftAndGetScore()
    {
        int scoreToClear = score;
        score = 0;
        UpdateScoreUI();
        return scoreToClear;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "稼ぎ: " + score.ToString();
        }
    }
}