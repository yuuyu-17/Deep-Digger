using UnityEngine;

public class FuelItemConsumer : MonoBehaviour
{
    private FuelManager fuelManager;

    // どのキーでアイテムを使用するか
    public KeyCode useItemKey = KeyCode.R;

    private void Start()
    {
        // プレイヤーオブジェクト上のFuelManagerを取得
        fuelManager = GetComponent<FuelManager>();

        if (fuelManager == null)
        {
            Debug.LogError("FuelItemConsumer: FuelManagerが見つかりません！");
            enabled = false;
        }
    }

    private void Update()
    {
        // Rキーが押されたらアイテムを使用
        if (Input.GetKeyDown(useItemKey))
        {
            UseFuelPack();
        }
    }

    public void UseFuelPack()
    {
        // 既に満タンに近い場合は使用しない
        if (fuelManager.currentFuel >= fuelManager.maxFuel * 0.95f)
        {
            Debug.Log("燃料はほぼ満タンです。");
            return;
        }

        // InventoryManagerからアイテムを消費できるか確認
        if (InventoryManager.instance != null && InventoryManager.instance.ConsumeFuelPack())
        {
            // 消費に成功したら燃料を満タンに回復
            fuelManager.AddFuel(fuelManager.maxFuel);
            InGameUIManager uiManager = FindObjectOfType<InGameUIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateFuelPackUI();
            }
            Debug.Log("燃料パックを使用し、燃料を満タンに回復しました。");
        }
        else
        {
            Debug.Log("燃料パックがありません！");
        }
    }
}
