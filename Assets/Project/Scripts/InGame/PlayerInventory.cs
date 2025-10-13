using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    [Header("通貨")]
    public int PlayerCredit = 0;

    [Header("資源")]
    // 辞書でジェムIDと個数を管理
    public Dictionary<int, int> GemInventory = new Dictionary<int, int>();

    // ジェムIDの定義（Drill.csと連動）
    public const int GEM_ID_COMMON = 1; // 浅層/岩 (Dirt, Rock)
    public const int GEM_ID_RARE = 2;   // 深層/ジェム (Gem)

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else if (instance != this) { Destroy(gameObject); }
    }

    // ジェム獲得 (Drill.csから呼ばれる)
    public void AddGem(int gemID, int amount = 1)
    {
        if (!GemInventory.ContainsKey(gemID)) GemInventory.Add(gemID, 0);
        GemInventory[gemID] += amount;
    }

    // クレジット加算 (ShopManagerから呼ばれる)
    public void AddCredit(int amount) { PlayerCredit += amount; }

    // クレジット消費 (ShopManagerから呼ばれる)
    public bool RemoveCredit(int amount)
    {
        if (PlayerCredit >= amount)
        {
            PlayerCredit -= amount;
            return true;
        }
        return false;
    }

    public int GetGemCount(int gemID) { return GemInventory.ContainsKey(gemID) ? GemInventory[gemID] : 0; }
    public void ClearAllGems() { GemInventory.Clear(); }
    public int GetCurrentCredit() { return PlayerCredit; }
}
