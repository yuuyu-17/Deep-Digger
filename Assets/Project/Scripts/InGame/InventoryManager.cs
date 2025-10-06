using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    // 現在の燃料回復アイテムの数
    [HideInInspector] public int fuelPacks = 0;

    [Header("アイテム設定")]
    public int itemCost = 100; // 街でアイテムを買うのに必要なジェムスコア

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // シーンをまたいでも破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // アイテム数を増やす（街のショップで購入時）
    public void AddFuelPack(int amount)
    {
        fuelPacks += amount;
    }

    // アイテムを消費する（洞窟内で使用時）
    public bool ConsumeFuelPack()
    {
        if (fuelPacks > 0)
        {
            fuelPacks--;
            return true;
        }
        return false;
    }

    public void UpdateUI(TextMeshProUGUI textComponent)
    {
        if (textComponent != null)
        {
            textComponent.text = "燃料パック: x" + fuelPacks.ToString();
        }
    }
}
