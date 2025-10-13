using UnityEngine;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    [Header("スコアUI")]
    // 洞窟シーンのCanvasに配置したスコア表示用のUIを設定
    public TextMeshProUGUI inGameScoreText;

    [Header("燃料パックUI")]
    // 洞窟シーンのCanvasに配置した燃料パック数表示用のUI
    public TextMeshProUGUI fuelPackCountText;

    private PlayerInventory inventory;
    private InventoryManager inventoryManager;

    private void Start()
    {
        inventory = PlayerInventory.instance;
        inventoryManager = InventoryManager.instance;
        
        // 最初の表示を更新
        UpdateAllUI();
    }

    // スコア、燃料、アイテム数など、全てのUIを更新するメインメソッド
    public void UpdateAllUI()
    {
        if (inGameScoreText != null && inventory != null)
        {
            int commonGems = inventory.GetGemCount(PlayerInventory.GEM_ID_COMMON);
            int rareGems = inventory.GetGemCount(PlayerInventory.GEM_ID_RARE);
            int credits = inventory.GetCurrentCredit();

            // 発表資料のコンセプトが伝わるように表示を調整
            inGameScoreText.text = $"クレジット: {credits}\n" +
                                   $"未換金ジェム: x{rareGems}";
        }
        
        // ★★★ 燃料パック数の更新ロジックをここで実行 ★★★
        if (fuelPackCountText != null && inventoryManager != null)
        {
            inventoryManager.UpdateUI(fuelPackCountText); // InventoryManagerのデータを使って表示
        }
    }

    // ★★★ 洞窟シーンで燃料パックを使用した際にUIを更新するために必要 ★★★
    public void UpdateFuelPackUI()
    {
        if (fuelPackCountText != null && inventoryManager != null)
        {
            inventoryManager.UpdateUI(fuelPackCountText);
        }
    }
}
