using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // スコアマネージャーのインスタンス
    public static ScoreManager instance;

    // 現在のスコアを格納する変数
    private int score = 0;

    // UIにスコアを表示するためのテキストコンポーネント
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        // シングルトンパターンの実装
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

    // スコアをUIに反映させるメソッド
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}
