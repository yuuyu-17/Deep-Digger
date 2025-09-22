using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    // グリッドのサイズ
    public int width = 50;
    public int height = 50;
    public int depth = 50;

    // ブロックのデータを格納する3次元配列
    private Block[,,] gridData;

    // ブロックのプレハブ
    public GameObject dirtPrefab;
    public GameObject rockPrefab;
    public GameObject gemPrefab;

    // 最初に生成するブロックの範囲
    public int initialGenerateRadius = 3;

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

        // グリッドデータを初期化
        gridData = new Block[width, height, depth];
        GenerateWorldData();
        GenerateInitialBlocks(new Vector3Int(25, 25, 25));
    }

    // ランダムなワールドのデータを生成
    private void GenerateWorldData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // ランダムにブロックを割り当てる
                    float randomValue = Random.value;
                    if (randomValue < 0.8f)
                    {
                        gridData[x, y, z] = new Block(Block.BlockType.Dirt, 3);
                    }
                    else if (randomValue < 0.95f)
                    {
                        gridData[x, y, z] = new Block(Block.BlockType.Rock, 5);
                    }
                    else
                    {
                        gridData[x, y, z] = new Block(Block.BlockType.Gem, 1);
                    }
                }
            }
        }
    }

    // 最初のブロック群を生成
    private void GenerateInitialBlocks(Vector3Int centerPos)
    {
        for (int x = centerPos.x - initialGenerateRadius; x <= centerPos.x + initialGenerateRadius; x++)
        {
            for (int y = centerPos.y - initialGenerateRadius; y <= centerPos.y + initialGenerateRadius; y++)
            {
                for (int z = centerPos.z - initialGenerateRadius; z <= centerPos.z + initialGenerateRadius; z++)
                {
                    // 境界チェック
                    if (x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth)
                    {
                        CreateBlockGameObject(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    // グリッドデータに基づいて実際のブロック（GameObject）を生成
    private void CreateBlockGameObject(Vector3Int gridPos)
    {
        Block block = GetBlock(gridPos.x, gridPos.y, gridPos.z);
        GameObject prefabToInstantiate = null;

        switch (block.type)
        {
            case Block.BlockType.Dirt:
                prefabToInstantiate = dirtPrefab;
                break;
            case Block.BlockType.Rock:
                prefabToInstantiate = rockPrefab;
                break;
            case Block.BlockType.Gem:
                prefabToInstantiate = gemPrefab;
                break;
            default:
                return; // Emptyの場合は生成しない
        }

        if (prefabToInstantiate != null)
        {
            // ブロックを生成し、親オブジェクトを設定して階層を整理
            GameObject newBlock = Instantiate(prefabToInstantiate, gridPos, Quaternion.identity);
            newBlock.transform.parent = this.transform;
            newBlock.name = "Block_" + gridPos.x + "_" + gridPos.y + "_" + gridPos.z;
        }
    }

    // 外部からブロックデータを取得するメソッド
    public Block GetBlock(int x, int y, int z)
    {
        if (x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth)
        {
            return gridData[x, y, z];
        }
        return null;
    }

    // 外部からブロックを破壊するメソッド（今後実装）
    public void DestroyBlock(int x, int y, int z)
    {
        // グリッドのデータを「空」に設定
        gridData[x, y, z].type = Block.BlockType.Empty;

        // 対応するゲームオブジェクトを探して破壊
        // これまでのステップでブロックに名前を付けているので、名前で検索できる
        GameObject blockObject = GameObject.Find("Block_" + x + "_" + y + "_" + z);
        if (blockObject != null)
        {
            Destroy(blockObject);
        }

    }
}
