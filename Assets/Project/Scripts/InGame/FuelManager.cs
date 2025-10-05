using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FuelManager : MonoBehaviour
{
    public float maxFuel = 1f;
    public float timeConsumptionRate = 1.0f / 120.0f; // ライト使用時の1秒あたりの消費量
    public float miningConsumptionPenalty = 0.01f; // 採掘1回あたりの消費量
    [HideInInspector] public float currentFuel; // 外部参照のためpublic、Inspector表示は非表示に

    [Header("システム連携")]
    public FlashlightController flashlightController; // ライトの制御スクリプト
    public GameManager gameManager;

    [Header("UI連携")]
    public TextMeshProUGUI fuelText;
    public int scoreToFuelRate = 100; // 燃料補給に必要なジェムスコア

    private void Awake()
    {
        currentFuel = maxFuel;
    }

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.instance;
        }
        if (flashlightController == null)
        {
            Debug.LogError("FlashlightControllerが設定されていません。インスペクターで設定してください。");
        }
        UpdateFuelUI();
    }

    private void Update()
    {
        // ★ 1. 時間経過による燃料消費 (ライトがオンの場合のみ) ★
        if (flashlightController != null && flashlightController.IsLightOn() && currentFuel > 0)
        {
            currentFuel -= timeConsumptionRate * Time.deltaTime;
        }
        
        currentFuel = Mathf.Clamp(currentFuel, 0.0f, maxFuel);

        // ★ 2. 燃料切れのGame Over判定とライト制御の指示 ★
        if (currentFuel <= 0)
        {
            currentFuel = 0;
            if (flashlightController != null)
            {
                flashlightController.ForceOff(); // ライトを強制オフ
            }
            
            if (gameManager != null)
            {
                gameManager.LoadGameOverScene();
            }
            // ゲームオーバー処理に入ったら、これ以上Updateを実行しない
            this.enabled = false;
            return;
        }

        // ★ 3. 点滅/警告の指示 ★
        if (currentFuel < 0.2f * maxFuel)
        {
            flashlightController.SetBlinkMode(true);
        }
        else
        {
            flashlightController.SetBlinkMode(false);
        }

        UpdateFuelUI();
    }

    private void UpdateFuelUI()
    {
        if (fuelText != null)
        {
            int percentage = Mathf.RoundToInt(currentFuel / maxFuel * 100);
            fuelText.text = "燃料: " + percentage.ToString() + "%";
        }
    }

    // 採掘時にDrillから呼び出される燃料消費メソッド
    public void ConsumeFuelForMining(float multiplier = 1.0f)
    {
        currentFuel -= miningConsumptionPenalty * multiplier;
        currentFuel = Mathf.Clamp(currentFuel, 0.0f, maxFuel);
    }
    
    // 燃料を補給するメソッド (FuelStationから呼び出す想定)
    public void AddFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Clamp(currentFuel, 0.0f, maxFuel);
        // 補給でライトの状態をリセット（オンに戻す）
        if (flashlightController != null)
        {
            flashlightController.SetBlinkMode(false);
            flashlightController.TurnOn(); 
        }
    }

    // ジェムを消費して燃料を満タンにするメソッド (FuelStationから呼び出す想定)
    public bool BuyFullFuel()
    {
        // 既に満タンに近い場合は無駄遣いを防ぐ
        if (currentFuel >= maxFuel * 0.99f) return true;

        if (ScoreManager.instance.GetCurrentScore() >= scoreToFuelRate)
        {
            ScoreManager.instance.RemoveScore(scoreToFuelRate);
            AddFuel(maxFuel);
            Debug.Log("ジェムを消費して燃料を補給しました。");
            return true;
        }
        else
        {
            Debug.Log("ジェムが足りません。");
            return false;
        }
    }
}
