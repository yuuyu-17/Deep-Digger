using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FuelManager : MonoBehaviour
{
    public static FuelManager instance;

    public float maxFuel = 1f;
    public float timeConsumptionRate = 1.0f / 120.0f; // ライト使用時の1秒あたりの消費量
    public float miningConsumptionPenalty = 0.01f; // 採掘1回あたりの消費量
    [HideInInspector] public float currentFuel; // 外部参照のためpublic、Inspector表示は非表示に

    [Header("システム連携")]
    public FlashlightController flashlightController; // ライトの制御スクリプト
    public GameManager gameManager;

    [Header("UI連携")]
    public TextMeshProUGUI fuelText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

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
            // ★★★ 修正: GameManager.instanceを直接再確認する ★★★
            if (gameManager == null)
            {
                // Start()で取得しきれていなかった場合のために、再度シングルトンから取得を試みる
                gameManager = GameManager.instance;
            }
            if (gameManager != null)
            {
                Debug.Log("燃料切れ！ゲームオーバーをロードします。");
                gameManager.LoadGameOverScene();
            }
            else
            {
                Debug.LogError("FATAL ERROR: GameManagerが見つからないため、ゲームオーバーをロードできません。");
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
}
