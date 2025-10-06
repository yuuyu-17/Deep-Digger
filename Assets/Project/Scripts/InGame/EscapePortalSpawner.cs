using UnityEngine;

public class EscapePortalSpawner : MonoBehaviour
{
    [Header("設定")]
    public GameObject portalPrefab; // 帰還ポータルのPrefab
    public float spawnOffset = 2f; // プレイヤーの足元からポータルを少し浮かす高さ

    public static EscapePortalSpawner instance;

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
    }

    // Start() メソッドのロジックを削除し、外部から呼び出せるメソッドに変更
    public void SpawnPortal(Vector3 playerFinalPosition)
    {
        // プレイヤーのワープ後の位置をスタート地点の座標として使用
        Vector3 startPos = playerFinalPosition; 

        // ポータルの座標を計算: プレイヤーの足元から少し上に配置
        Vector3 spawnPosition = new Vector3(
            startPos.x, 
            startPos.y + spawnOffset, 
            startPos.z
        );

        // ポータルをインスタンス化
        if (portalPrefab != null)
        {
            GameObject newPortal = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
            newPortal.name = "Escape Portal (Auto)";
            Debug.Log("帰還ポータルをワープ後の地点 (" + spawnPosition + ") に配置しました。");
        }
        else
        {
            Debug.LogError("ポータルのPrefabが設定されていません！");
        }
    }
}
