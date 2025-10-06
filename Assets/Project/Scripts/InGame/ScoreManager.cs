using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private int score = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
    }
}