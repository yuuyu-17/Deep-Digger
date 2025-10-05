using UnityEngine;
using TMPro;

public class FlashlightController : MonoBehaviour
{
    // ★★★ 外部参照の追加 ★★★
    [Header("システム連携")]
    public FuelManager fuelManager; // FuelManagerへの参照をインスペクターで設定

    private Light flashlight;
    private bool isBlinking = false;
    
    [Header("点滅設定")]
    public float blinkSpeed = 5f;

    private void Start()
    {
        flashlight = GetComponent<Light>();
        if (flashlight == null)
        {
            Debug.LogError("Lightコンポーネントが見つかりません！");
            enabled = false;
        }
        flashlight.enabled = true; // 初期状態はオン

        if (fuelManager == null)
        {
            // 同じオブジェクトにあるFuelManagerを取得（安全策）
            fuelManager = GetComponent<FuelManager>();
        }
    }

    private void Update()
    {
        // ★ 1. プレイヤーによるライトのオン/オフ切り替え ★
        if (Input.GetKeyDown(KeyCode.F))
        {
            // ★★★ 修正箇所: fuelManager.currentFuel を使用 ★★★
            if (fuelManager != null && fuelManager.currentFuel > 0)
            {
                flashlight.enabled = !flashlight.enabled;
            }
        }
        
        // ★ 2. 点滅演出ロジック（FuelManagerからの指示で実行） ★
        if (isBlinking)
        {
            // 燃料切れ後の点滅防止
            // ★★★ 修正箇所: fuelManager.currentFuel を使用 ★★★
            if (fuelManager != null && fuelManager.currentFuel > 0)
            {
                flashlight.enabled = (Mathf.Sin(Time.time * blinkSpeed) > 0);
            }
        }
    }

    // ★ FuelManagerから呼ばれるメソッド群 ★

    public bool IsLightOn()
    {
        return flashlight.enabled;
    }

    // 燃料切れ時の強制オフ
    public void ForceOff()
    {
        isBlinking = false;
        flashlight.enabled = false;
    }

    // 補給時の強制オン
    public void TurnOn()
    {
        flashlight.enabled = true;
    }

    // 点滅モードの制御
    public void SetBlinkMode(bool isEnabled)
    {
        isBlinking = isEnabled;
        
        // 点滅を解除する場合、ライトの状態をリセット
        if (!isEnabled && flashlight.enabled == false)
        {
            // ★★★ 修正箇所: fuelManager.currentFuel を使用 ★★★
            if (fuelManager != null && fuelManager.currentFuel > 0)
            {
                flashlight.enabled = true;
            }
        }
    }
}
