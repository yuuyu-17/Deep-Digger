using UnityEngine;
using TMPro;

public class FlashlightManager : MonoBehaviour
{
    // バッテリーの初期残量（0.0から1.0）
    public float maxBatteryLife = 1f;

    // バッテリーが減る速さ
    public float drainRate = 0.1f;

    // バッテリー残量
    private float currentBatteryLife;

    // 懐中電灯のライトコンポーネント
    private Light flashlight;

    // バッテリーUIテキスト
    public TextMeshProUGUI batteryText;

    private void Start()
    {
        currentBatteryLife = maxBatteryLife;
        flashlight = GetComponent<Light>();

        // 懐中電灯の初期状態
        flashlight.enabled = true;
    }

    private void Update()
    {
        // バッテリー残量を減らす
        if (currentBatteryLife > 0)
        {
            currentBatteryLife -= drainRate * Time.deltaTime;
        }
        else
        {
            // バッテリーがゼロになったら懐中電灯を消す
            currentBatteryLife = 0;
            flashlight.enabled = false;
        }

        if (currentBatteryLife <= 0)
    {
        currentBatteryLife = 0;
        flashlight.enabled = false;
        // バッテリーが切れたらゲームオーバーシーンへ
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }

        // UIを更新
        UpdateBatteryUI();

        // 懐中電灯の点滅演出（例：残量が20%以下になったら）
        if (currentBatteryLife < 0.2f)
        {
            float blinkSpeed = 5f;
            flashlight.enabled = (Mathf.Sin(Time.time * blinkSpeed) > 0);
        }
    }

    private void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            int percentage = Mathf.RoundToInt(currentBatteryLife * 100);
            batteryText.text = "Battery: " + percentage.ToString() + "%";
        }
    }
}
