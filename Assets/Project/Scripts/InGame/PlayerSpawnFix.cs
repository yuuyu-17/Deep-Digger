using UnityEngine;

public class PlayerSpawnFix : MonoBehaviour
{
    void Start()
    {
        // GridManager のインスタンスを取得
        GridManager gridManager = GridManager.instance;

        // GridManager が準備できていない可能性を考慮
        if (gridManager == null)
        {
            Debug.LogError("GridManagerが見つかりません。プレイヤーの初期配置をスキップします。");
            return;
        }

        // ----------------------------------------------------
        // ★★★ プレイヤーの位置をマップの中心、坑道入口に設定 ★★★
        // ----------------------------------------------------
        
        // X, Zはグリッドの中央
        float centerX = (float)gridManager.width / 2f;
        float centerZ = (float)gridManager.depth / 2f;
        
        // Yは安全に立つことができる足場の上（startYLevel）に設定
        float startY = (float)gridManager.startYLevel;

        // プレイヤーの最終的な初期位置を計算し、変数に定義
        Vector3 finalSpawnPosition = new Vector3(centerX, startY, centerZ);

        // このスクリプトがアタッチされているオブジェクト（プレイヤー本体またはカメラ）を移動
        transform.position = new Vector3(centerX, startY, centerZ);

        Debug.Log($"プレイヤーを初期位置へ移動: X={centerX}, Y={startY}, Z={centerZ}");

        Vector3 shopSpawnPosition = finalSpawnPosition + new Vector3(1.5f, 0, 0);
        ShopManager shopManager = FindFirstObjectByType<ShopManager>(); // FindObjectOfTypeの非推奨警告を避けるため
        if (shopManager != null)
        {
            shopManager.TeleportShop(shopSpawnPosition);
        }
        else
        {
            Debug.LogWarning("ShopManagerが見つかりません。ショップの初期配置をスキップしました。");
        }

        // ----------------------------------------------------
        // ★★★ ポータル生成をワープ完了後に指示 ★★★
        // ----------------------------------------------------
        if (EscapePortalSpawner.instance != null)
        {
            // ワープ後の確定した座標を使ってポータルを生成
            EscapePortalSpawner.instance.SpawnPortal(finalSpawnPosition);
        }
        else
        {
            Debug.LogError("EscapePortalSpawnerが見つかりません。MineSceneにスポーナーを配置し忘れていませんか？");
        }
    }
}
