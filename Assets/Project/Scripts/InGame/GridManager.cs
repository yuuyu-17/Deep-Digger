using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    // グリッドのサイズ
    public int width = 50;
    public int height = 50; // この高さ（50）が地下の深さの次元となる
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
        // 生成の中心点を、地下の入り口付近（物理座標Y=0付近）に対応するグリッドインデックスに設定
        // centerPosの Y はグリッド配列のインデックス (0～49)
        // プレイヤーを X=25, Y=0, Z=25 付近に配置することを想定し、グリッドの中心を調整
        GenerateInitialBlocks(new Vector3Int(width / 2, height - 1, depth / 2));
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
                    // ランダムにブロックを割り当てる (このロジックはそのまま)
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
        // Y軸のグリッド配列インデックスの最大値 (この例では 49)
        int maxGridYIndex = height - 1;

        // 最初のブロック群を生成
        for (int x = centerPos.x - initialGenerateRadius; x <= centerPos.x + initialGenerateRadius; x++)
        {
            // Y座標を地下（負の値）に対応させる。グリッドインデックスの「上部」から生成を開始
            for (int yIndex = centerPos.y - initialGenerateRadius; yIndex <= centerPos.y + initialGenerateRadius; yIndex++)
            {
                for (int z = centerPos.z - initialGenerateRadius; z <= centerPos.z + initialGenerateRadius; z++)
                {
                    // 境界チェック
                    if (x >= 0 && x < width && yIndex >= 0 && yIndex < height && z >= 0 && z < depth)
                    {
                        // 物理座標 Y を計算: yIndex=49 (一番上) -> 0 になる
                        int yPhysical = yIndex - maxGridYIndex;

                        // 物理座標 (X, Y_physical, Z) でブロックを生成する
                        CreateBlockGameObject(new Vector3Int(x, yPhysical, z));
                    }
                }
            }
        }
    }

    // グリッドデータに基づいて実際のブロック（GameObject）を生成
    // 引数は物理座標 (Physical Position)
    private void CreateBlockGameObject(Vector3Int physicalPos)
    {
        // 物理座標からグリッドインデックスを取得
        Vector3Int gridPos = GetGridIndexFromPhysicalPos(physicalPos);

        // 境界チェック
        if (gridPos.y < 0 || gridPos.y >= height || gridPos.x < 0 || gridPos.x >= width || gridPos.z < 0 || gridPos.z >= depth) return;

        Block block = GetBlock(physicalPos.x, physicalPos.y, physicalPos.z); // 物理座標でGetBlockを呼ぶ
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
            // ★配置には物理座標 (physicalPos) を使用
            GameObject newBlock = Instantiate(prefabToInstantiate, physicalPos, Quaternion.identity);
            newBlock.transform.parent = this.transform;
            // ブロック名も物理座標で設定
            newBlock.name = "Block_" + physicalPos.x + "_" + physicalPos.y + "_" + physicalPos.z; 
        }
    }

    // ★新規メソッド: 物理座標をグリッドインデックスに変換するヘルパー関数
    private Vector3Int GetGridIndexFromPhysicalPos(Vector3Int physicalPos)
    {
        int maxGridYIndex = height - 1;

        // Y物理座標 (0, -1, -2...) を Yインデックス (49, 48, 47...) に変換
        // 例: physicalPos.y=0 -> yIndex=49
        // 例: physicalPos.y=-49 -> yIndex=0
        int yIndex = physicalPos.y + maxGridYIndex;

        return new Vector3Int(physicalPos.x, yIndex, physicalPos.z);
    }

    // 外部からブロックデータを取得するメソッド (引数は物理座標)
    public Block GetBlock(int x, int y, int z)
    {
        Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, y, z));

        int xIndex = gridPos.x;
        int yIndex = gridPos.y;
        int zIndex = gridPos.z;

        if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height && zIndex >= 0 && zIndex < depth)
        {
            return gridData[xIndex, yIndex, zIndex];
        }
        return null;
    }

    // 外部からブロックを破壊するメソッド（引数は物理座標）
    public void DestroyBlock(int x, int y, int z)
    {
        Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, y, z));

        int xIndex = gridPos.x;
        int yIndex = gridPos.y;
        int zIndex = gridPos.z;

        if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height && zIndex >= 0 && zIndex < depth)
        {
            // グリッドのデータを「空」に設定
            gridData[xIndex, yIndex, zIndex].type = Block.BlockType.Empty;
        }

        // 対応するゲームオブジェクトを探して破壊 (名前は物理座標で検索)
        GameObject blockObject = GameObject.Find("Block_" + x + "_" + y + "_" + z); 
        if (blockObject != null)
        {
            Destroy(blockObject);
        }
    }
}