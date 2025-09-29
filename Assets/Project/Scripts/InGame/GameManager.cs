using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("シフト設定")]
    public float shiftDuration = 300f; // 1シフトの時間（5分）
    private float shiftTimer;
    public TextMeshProUGUI timerText; // 労働時間UI

    [Header("費用設定")]
    public int taxCost = 500;    // 納税額
    public int foodCost = 200;   // 食費・生活費
    private int totalShiftCost;   // 固定費の合計

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

    void Start()
    {
        shiftTimer = shiftDuration;
        totalShiftCost = taxCost + foodCost;
    }

    void Update()
    {
        if (shiftTimer > 0)
        {
            shiftTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            EndShift();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(shiftTimer / 60F);
            int seconds = Mathf.FloorToInt(shiftTimer % 60F);
            timerText.text = string.Format("労働時間: {0:00}:{1:00}", minutes, seconds);
        }
    }

    void EndShift()
    {
        // 労働時間終了後の処理は一度だけ実行
        if (shiftTimer == 0) return;
        shiftTimer = 0;

        Debug.Log("労働時間終了！清算準備へ。");

        // スコアを取得し、ScoreManagerのスコアをリセット
        int earnings = ScoreManager.instance.EndShiftAndGetScore();

        // GameStateManagerに今回の稼ぎと固定費用を託す
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.currentShiftEarnings = earnings;
            GameStateManager.instance.totalShiftCost = totalShiftCost;
        }

        // リザルト画面へ遷移
        SceneManager.LoadScene("ResultScene");
    }

    public void LoadWinScene()
    {
        // ゲームのクリア処理を実行
        Debug.Log("ゲームクリア！WinSceneに遷移します。");
        
        // 現在のシーンを停止し、WinSceneへ移動
        SceneManager.LoadScene("WinScene");
    }
}