using UnityEngine;
using System.Collections.Generic;

public class ChunkUpdater : MonoBehaviour
{
    public GridManager gridManager;
    
    // ブロックを物理生成/破棄する範囲（視界距離）
    public int renderDistance = 8; // XZ軸の描画距離
    public int yRenderDistance = 8; // Y軸の描画距離
    
    // 処理頻度: 処理落ちを防ぐため、1秒ごとにチャンクを更新
    public float updateInterval = 1.0f; 

    void Start()
    {
        gridManager = GridManager.instance;
        if (gridManager == null)
        {
            Debug.LogError("GridManagerが見つかりません。ChunkUpdaterは動作しません。");
            enabled = false;
            return;
        }
        
        InvokeRepeating("UpdateChunks", 0.0f, updateInterval);
        
        // プレイヤーの初期位置修正
        float centerX = (float)gridManager.width / 2f; 
        float centerZ = (float)gridManager.depth / 2f; 
        float startY = (float)gridManager.startYLevel; 
        transform.position = new Vector3(centerX, startY, centerZ);
    }
    
    private void UpdateChunks()
    {
        Vector3Int playerPos = Vector3Int.RoundToInt(transform.position);
        
        List<Vector3Int> keysToDestroy = new List<Vector3Int>();

        // ----------------------------------------------------
        // 1. 既存のブロックをチェックし、遠すぎるものを破棄
        // ----------------------------------------------------
        foreach (var kvp in gridManager.activeBlockObjects)
        {
            Vector3Int blockPos = kvp.Key;
            
            if (Vector3.Distance(blockPos, playerPos) > renderDistance)
            {
                keysToDestroy.Add(blockPos);
            }
        }

        // 辞書から削除し、オブジェクトを非アクティブ化 (処理落ち対策)
        foreach (Vector3Int pos in keysToDestroy)
        {
            if (gridManager.activeBlockObjects.TryGetValue(pos, out GameObject obj))
            {
                obj.SetActive(false); // Destroyではなく非アクティブ化
                gridManager.activeBlockObjects.Remove(pos); 
            }
        }
        
        // ----------------------------------------------------
        // 2. プレイヤーの周囲に、まだ生成されていないブロックを生成
        // ----------------------------------------------------
        
        int w = gridManager.width;

        for (int x = playerPos.x - renderDistance; x <= playerPos.x + renderDistance; x++)
        {
            for (int y = playerPos.y - yRenderDistance; y <= playerPos.y + yRenderDistance; y++)
            {
                for (int z = playerPos.z - renderDistance; z <= playerPos.z + renderDistance; z++)
                {
                    // X, Z の境界チェック (無駄な計算を防ぐ)
                    if (x < 0 || x >= w || z < 0 || z >= gridManager.depth) // zのチェックを追加
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
                            // CreateBlockGameObjectを呼び出す前に、非アクティブなオブジェクトの再利用を検討することも可能ですが、
                            // デモ版ではシンプルに新規生成します。
                            gridManager.CreateBlockGameObject(physicalPos);
                        }
                    }
                }
            }
        }
    }
}