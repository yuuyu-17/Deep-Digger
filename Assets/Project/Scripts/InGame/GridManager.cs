using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("洞窟生成設定")]
    // ノイズのスケール: 小さいほど大きな空洞ができる
    public float noiseScale = 0.08f;
    // ブロック生成の閾値: この値を超えた時だけブロックを生成
    public float noiseThreshold = 0.5f;
    // 毎回異なる世界を作るためのノイズの開始座標
    private Vector3 noiseOffset;

    [Header("プレイヤー開始地点")]
    // プレイヤーがスタートするY座標。この周辺は空洞を保証します。
    public int startYLevel = -5;
    // プレイヤーのスタート位置を確保するための安全ゾーンの半径
    public int safeZoneRadius = 5;

    // グリッドのサイズ
    public int width = 50;
    public int height = 50; // この高さ（50）が地下の深さの次元となる
    public int depth = 50;

    // ブロックのデータを格納する3次元配列
    private Block[,,] gridData;

    // ★★★ 追加: アクティブなブロックGameObjectを管理する辞書 ★★★
    public Dictionary<Vector3Int, GameObject> activeBlockObjects;

    // ブロックのプレハブ
    public GameObject dirtPrefab;
    public GameObject rockPrefab;
    public GameObject gemPrefab;

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
        // ★★★ 初期化 ★★★
        activeBlockObjects = new Dictionary<Vector3Int, GameObject>();
    }

    private void Start()
    {
        // 毎回異なる世界を生成するためのランダムなオフセット
        // 永続化（セーブ/ロード）を実装する際には、この値を保存・ロードします。
        noiseOffset = new Vector3(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f)
        );

        GenerateWorldData();
        Vector3Int playerStartPhysicalPos = new Vector3Int(width / 2, startYLevel, depth / 2);
        GenerateInitialBlocks(playerStartPhysicalPos);
    }

    // ランダムなワールドのデータを生成
    private void GenerateWorldData()
    {
        int maxGridYIndex = height - 1;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int yIndex = 0; yIndex < height; yIndex++)
                {
                    // 1. 物理座標Yを計算 (Y=0, -1, -2... または正の値)
                    int yPhysical = yIndex - maxGridYIndex;

                    // 2. ノイズの計算
                    float xCoord = (x + noiseOffset.x) * noiseScale;
                    float yCoord = (yPhysical + noiseOffset.y) * noiseScale;
                    float zCoord = (z + noiseOffset.z) * noiseScale;

                    float noiseValue = (
                        Mathf.PerlinNoise(xCoord, yCoord) +
                        Mathf.PerlinNoise(yCoord, zCoord) +
                        Mathf.PerlinNoise(xCoord, zCoord)
                    ) / 3.0f;

                    // 3. ブロック生成の基本判定
                    bool shouldBeSolid = (noiseValue > noiseThreshold);

                    // 4. 強制空洞化/強制足場の判定 (プレイヤーのスタート地点)
                    int playerStartX = width / 2;
                    int playerStartZ = depth / 2;

                    bool isPlayerSafeZone = false;
                    bool forceSolidFloor = false;

                    if (x >= playerStartX - safeZoneRadius && x <= playerStartX + safeZoneRadius &&
                        z >= playerStartZ - safeZoneRadius && z <= playerStartZ + safeZoneRadius)
                    {
                        // プレイヤーの足場（startYLevel の1つ下）
                        if (yPhysical == startYLevel - 1)
                        {
                            forceSolidFloor = true;
                        }
                        // プレイヤーが立つ空間とその上
                        else if (yPhysical >= startYLevel && yPhysical <= startYLevel + 2)
                        {
                            isPlayerSafeZone = true;
                        }
                    }

                    // 5. ブロックの割り当て
                    if (isPlayerSafeZone)
                    {
                        // 強制空洞エリア
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Empty, 0);
                    }
                    else if (forceSolidFloor)
                    {
                        // 強制足場エリア (Dirtを生成)
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Dirt, 3);
                    }
                    else if (shouldBeSolid)
                    {
                        // ノイズが閾値を超えた場合、ブロックの種類を決定
                        float randomValue = Random.value;
                        if (randomValue < 0.8f)
                        {
                            gridData[x, yIndex, z] = new Block(Block.BlockType.Dirt, 3);
                        }
                        else if (randomValue < 0.95f)
                        {
                            gridData[x, yIndex, z] = new Block(Block.BlockType.Rock, 5);
                        }
                        else
                        {
                            gridData[x, yIndex, z] = new Block(Block.BlockType.Gem, 1);
                        }
                    }
                    else
                    {
                        // ノイズが閾値未満 または 強制空洞の場合
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Empty, 0);
                    }
                }
            }
        }
    }

    // 最初のブロック群を生成
    private void GenerateInitialBlocks(Vector3Int centerPhysicalPos)
    {
        // 安全ゾーンより少し広く生成
        int initialGenerateRadius = 40;

        for (int x = centerPhysicalPos.x - initialGenerateRadius; x <= centerPhysicalPos.x + initialGenerateRadius; x++)
        {
            for (int yPhysical = centerPhysicalPos.y - initialGenerateRadius; yPhysical <= centerPhysicalPos.y + initialGenerateRadius; yPhysical++)
            {
                for (int z = centerPhysicalPos.z - initialGenerateRadius; z <= centerPhysicalPos.z + initialGenerateRadius; z++)
                {
                    Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, yPhysical, z));

                    // 境界チェックと、データがEmptyではないかチェック
                    if (gridPos.x >= 0 && gridPos.x < width &&
                        gridPos.y >= 0 && gridPos.y < height &&
                        gridPos.z >= 0 && gridPos.z < depth)
                    {
                         // データが Empty でない場合にのみ物理オブジェクトを生成
                        if (gridData[gridPos.x, gridPos.y, gridPos.z].type != Block.BlockType.Empty)
                        {
                             CreateBlockGameObject(new Vector3Int(x, yPhysical, z));
                        }
                    }
                }
            }
        }
    }

    // グリッドデータに基づいて実際のブロック（GameObject）を生成
    // 引数は物理座標 (Physical Position)
    public void CreateBlockGameObject(Vector3Int physicalPos)
    {
        // 境界チェックは GetBlock で行われるため、ここでは簡略化
        Block block = GetBlock(physicalPos.x, physicalPos.y, physicalPos.z);
        if (block.type == Block.BlockType.Empty || block.type == Block.OUT_OF_BOUNDS.type) return;

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
        }

        if (prefabToInstantiate != null)
        {
            GameObject newBlock = Instantiate(prefabToInstantiate, physicalPos, Quaternion.identity);
            newBlock.transform.parent = this.transform;
            newBlock.name = "Block_" + physicalPos.x + "_" + physicalPos.y + "_" + physicalPos.z;

            // ★★★ 辞書にオブジェクトを追加 ★★★
            activeBlockObjects[physicalPos] = newBlock;
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
        return Block.OUT_OF_BOUNDS;
    }

    // 外部からブロックを破壊するメソッド（引数は物理座標）
    public void DestroyBlock(int x, int y, int z)
    {
        Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, y, z));
        Vector3Int physicalPos = new Vector3Int(x, y, z);

        int xIndex = gridPos.x;
        int yIndex = gridPos.y;
        int zIndex = gridPos.z;

        if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height && zIndex >= 0 && zIndex < depth)
        {
            // グリッドのデータを「空」に設定
            gridData[xIndex, yIndex, zIndex].type = Block.BlockType.Empty;
        }

        // ★★★ Dictionaryからオブジェクトを探して破壊 ★★★
        if (activeBlockObjects.TryGetValue(physicalPos, out GameObject blockObject))
        {
            Destroy(blockObject);
            activeBlockObjects.Remove(physicalPos);
        }
    }
}