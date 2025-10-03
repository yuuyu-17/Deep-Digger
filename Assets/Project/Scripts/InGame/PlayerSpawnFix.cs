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

        // このスクリプトがアタッチされているオブジェクト（プレイヤー本体またはカメラ）を移動
        transform.position = new Vector3(centerX, startY, centerZ);

        Debug.Log($"プレイヤーを初期位置へ移動: X={centerX}, Y={startY}, Z={centerZ}");
    }
}
