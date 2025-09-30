using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // シングルトンパターン
    public static GameStateManager instance;

    // プレイヤーの総資産（シーンをまたいで引き継がれる貯金）
    [Header("総資産")]
    [SerializeField] private int totalAssets = 0;
    public int TotalAssets => totalAssets;

    // 今シフトで稼いだスコア（リザルト画面への一時的な保存場所）
    [Header("今シフトの稼ぎ（一時保存）")]
    public int currentShiftEarnings = 0;

    // 今シフトの固定費用（GameManagerから受け取る）
    [Header("今シフトの固定費用 (納税額)")]
    public int totalShiftCost = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateAssets(int amount)
    {
        int previousAssets = totalAssets;
        totalAssets += amount;
        // デバッグログ
        Debug.Log($"[Assets] 総資産を {amount} 更新しました。({previousAssets} -> {totalAssets})。");
    }
}