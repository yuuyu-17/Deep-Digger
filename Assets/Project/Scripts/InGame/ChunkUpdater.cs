using UnityEngine;
using System.Collections.Generic;

public class ChunkUpdater : MonoBehaviour
{
    public GridManager gridManager;
    
    // ブロックを物理生成/破棄する範囲（視界距離）
    public int renderDistance = 40; 
    
    // 処理頻度: 1秒ごとにチャンクを更新（処理落ちを防ぐため）
    public float updateInterval = 1.0f; 

    void Start()
    {
        // GridManagerの参照取得
        gridManager = GridManager.instance;
        if (gridManager == null)
        {
            Debug.LogError("GridManagerが見つかりません。ChunkUpdaterは動作しません。");
            enabled = false;
            return;
        }
        
        // 処理落ちを防ぐため、1秒ごとに UpdateChunks を呼び出す
        InvokeRepeating("UpdateChunks", 0.0f, updateInterval);
        
        // 【重要】プレイヤーの初期位置修正（PlayerSpawnFixの役割を兼ねる）
        // GridManagerの幅/深さが変わっても中心からスタートさせる
        float centerX = (float)gridManager.width / 2f; 
        float centerZ = (float)gridManager.depth / 2f; 
        float startY = (float)gridManager.startYLevel; 
        transform.position = new Vector3(centerX, startY, centerZ);
    }
    
    // Update関数は削除済み

    private void UpdateChunks()
    {
        Vector3Int playerPos = Vector3Int.RoundToInt(transform.position);
        
        // 破棄するキーを一時的に格納
        List<Vector3Int> keysToDestroy = new List<Vector3Int>();

        // ----------------------------------------------------
        // 1. 既存のブロックをチェックし、遠すぎるものを破棄 (高速: 辞書を使用)
        // ----------------------------------------------------
        foreach (var kvp in gridManager.activeBlockObjects)
        {
            Vector3Int blockPos = kvp.Key;
            
            // プレイヤーからの距離が描画距離を超えているかチェック
            if (Vector3.Distance(blockPos, playerPos) > renderDistance)
            {
                keysToDestroy.Add(blockPos);
            }
        }

        // 辞書から削除し、オブジェクトを破棄
        foreach (Vector3Int pos in keysToDestroy)
        {
            // TryGetValueで安全にオブジェクトを取得しつつ、破棄
            if (gridManager.activeBlockObjects.TryGetValue(pos, out GameObject obj))
            {
                // Destroy(obj); // ★★★ コメントアウト: 後でまとめて破棄する ★★★
                
                // ★★★ オブジェクトを非アクティブ化して負荷を軽減（プーリングの簡易版） ★★★
                // Destroyの負荷が高すぎる場合、まずは非アクティブ化で対応します。
                obj.SetActive(false); 
                
                gridManager.activeBlockObjects.Remove(pos);
            }
        }
        
        // ----------------------------------------------------
        // 2. プレイヤーの周囲に、まだ生成されていないブロックを生成 (高速: 辞書を使用)
        // ----------------------------------------------------
        
        // Y軸の描画距離を短く設定
        int yRenderDistance = 15; // ★40ではなく、15など小さい値を試す★

        // GridManagerの境界を取得
        int w = gridManager.width;
        int d = gridManager.depth;
        int h = gridManager.height; // 高さ（Y方向のインデックス）

        for (int x = playerPos.x - renderDistance; x <= playerPos.x + renderDistance; x++)
        {
            for (int y = playerPos.y - renderDistance; y <= playerPos.y + renderDistance; y++)
            {
                for (int z = playerPos.z - renderDistance; z <= playerPos.z + renderDistance; z++)
                {
                    // ★★★ 境界チェック: グリッド配列の外側はスキップ ★★★
                    if (x < 0 || x >= w || z < 0 || z >= d)
                    {
                        continue; 
                    }
                    
                    Vector3Int physicalPos = new Vector3Int(x, y, z);
                    
                    // 辞書にキーが存在するかで、生成済みか高速チェック
                    if (!gridManager.activeBlockObjects.ContainsKey(physicalPos)) 
                    {
                        Block blockData = gridManager.GetBlock(x, y, z);
                        
                        // データが存在する場合のみ生成
                        if (blockData.type != Block.BlockType.Empty && blockData.type != Block.OUT_OF_BOUNDS.type)
                        {
                            // GridManagerの生成メソッドを呼び出し
                            gridManager.CreateBlockGameObject(physicalPos);
                        }
                    }
                }
            }
        }
    }
}