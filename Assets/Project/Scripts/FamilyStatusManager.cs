using UnityEngine;

public class FamilyStatusManager : MonoBehaviour
{
    public static FamilyStatusManager instance;

    private const int MAX_GAUGE = 100;
    public int foodConsumptionPerShift = 10;

    [SerializeField] private int familyFoodGauge = MAX_GAUGE;
    [SerializeField] private int playerHungerGauge = MAX_GAUGE;

    public int FamilyFoodGauge => familyFoodGauge;
    public int PlayerHungerGauge => playerHungerGauge;

    private void Awake()
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

    public void ConsumeFood()
    {
        // 家族の食糧を消費
        familyFoodGauge -= foodConsumptionPerShift;
        if (familyFoodGauge < 0) familyFoodGauge = 0;

        // プレイヤーの空腹度を消費
        playerHungerGauge -= foodConsumptionPerShift;
        if (playerHungerGauge < 0) playerHungerGauge = 0;
        
        Debug.Log($"Shift ended. Family Food: {familyFoodGauge}, Player Hunger: {playerHungerGauge}");
        
        // ★ペナルティロジック（後で実装）: ゲージが0になった場合など
    }

    public void RecoverFoodForFamily(int amount)
    {
        familyFoodGauge = Mathf.Min(familyFoodGauge + amount, MAX_GAUGE);
        playerHungerGauge = Mathf.Min(playerHungerGauge + amount, MAX_GAUGE);
    }

    public void RecoverFoodForSelf(int amount)
    {
        // 家族の食糧ゲージは回復しない！
        playerHungerGauge = Mathf.Min(playerHungerGauge + amount, MAX_GAUGE);
    }
}
