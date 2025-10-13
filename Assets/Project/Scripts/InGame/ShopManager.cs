using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("参照")]
    public PlayerInventory inventory;
    public FuelManager fuelManager;
    public InGameUIManager uiManager;
    public GameObject ShopUI; // ショップのUIパネル

    [Header("換金レート")]
    public int CommonGemRate = 10;  // 浅層ジェムの換金レート
    public int RareGemRate = 50;    // 深層ジェムの換金レート

    [Header("燃料コスト")]
    public int FullFuelCost = 100; // 燃料満タン購入に必要なクレジット

    private void Start()
    {
        // シングルトン参照を優先
        if (inventory == null) inventory = PlayerInventory.instance;
        if (fuelManager == null) fuelManager = FindFirstObjectByType<FuelManager>();
        if (uiManager == null) uiManager = FindFirstObjectByType<InGameUIManager>();
        
        if (ShopUI != null)
        {
            ShopUI.SetActive(false); // 初期状態は非表示
        }

        if (ShopUI != null)
        {
            ShopUI.SetActive(false); // ★この行で確実に非表示にしておく
        }
    }

    // プレイヤーが触れたらUIを表示（このスクリプトを持つオブジェクトにTrigger Colliderが必要）
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && ShopUI != null)
        {
            ShopUI.SetActive(true);
            uiManager.UpdateAllUI(); // UIを更新して最新のクレジット/ジェムを表示

            // ★★★ 追加: マウスカーソルを解放し表示する ★★★
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && ShopUI != null)
        {
            ShopUI.SetActive(false);

            // ★★★ 追加: マウスカーソルをロックし非表示に戻す ★★★
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // UIボタンに接続: 全て換金
    public void SellAllGemsAndGetCredit()
    {
        if (inventory == null) return;

        int totalCreditEarned = 0;

        int commonAmount = inventory.GetGemCount(PlayerInventory.GEM_ID_COMMON);
        totalCreditEarned += commonAmount * CommonGemRate;
        
        int rareAmount = inventory.GetGemCount(PlayerInventory.GEM_ID_RARE);
        totalCreditEarned += rareAmount * RareGemRate;

        if(totalCreditEarned == 0) return;

        inventory.AddCredit(totalCreditEarned);
        inventory.ClearAllGems(); 

        Debug.Log($"全てのジェムを売却し、{totalCreditEarned}クレジットを獲得。");
        if (uiManager != null) uiManager.UpdateAllUI();
    }

    // UIボタンに接続: 燃料購入
    public void PurchaseFullFuel()
    {
        if (fuelManager == null || inventory == null) return;

        // ほぼ満タンなら購入不可
        if (fuelManager.currentFuel >= fuelManager.maxFuel * 0.99f) return;

        if (inventory.RemoveCredit(FullFuelCost))
        {
            // クレジット消費成功
            fuelManager.AddFuel(fuelManager.maxFuel); // フル回復
            Debug.Log($"クレジットを消費し、燃料を満タンに補給しました。");
        }
        else
        {
            Debug.Log($"クレジット不足。");
        }
        if (uiManager != null) uiManager.UpdateAllUI();
    }

    public void TeleportShop(Vector3 position)
    {
        // ショップオブジェクトをプレイヤーのスポーン位置に移動
        transform.position = position;
        Debug.Log($"ショップをプレイヤーの初期位置 ({position}) に移動しました。");
    }
}
