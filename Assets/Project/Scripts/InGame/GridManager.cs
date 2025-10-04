using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("洞窟生成設定")]
    // ノイズのスケール: 小さいほど大きな空洞ができる
    public float noiseScale = 0.1f;
    // ブロック生成の閾値: この値を超えた時だけブロックを生成
    public float noiseThreshold = 0.3f;
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

                    // ★★★ 境界の強制：外側の層を破壊不可能な岩にする ★★★
                    // XZの境界 (x=0, x=width-1, z=0, z=depth-1)
                    bool isXZBoundary = (x == 0 || x == width - 1 || z == 0 || z == depth - 1);
                    // Yの境界 (yIndex=0=最も深い、yIndex=height-1=最も浅い)
                    bool isYBoundary = (yIndex == 0 || yIndex == height - 1);
                    
                    // 地上（Y>0）を完全に岩で埋める
                    bool isAboveGround = (yPhysical > 0); 

                    if (isXZBoundary || isYBoundary || isAboveGround)
                    {
                        // 破壊不可能な岩で完全に壁を構成 (耐久度9999)
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Rock, 9999);
                        continue; // 以下のノイズ生成ロジックをスキップ
                    }
                    // ★★★ 境界の強制 終了 ★★★

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
                        if (yPhysical == startYLevel - 1)
                        {
                            forceSolidFloor = true;
                        }
                        else if (yPhysical >= startYLevel && yPhysical <= startYLevel + 2)
                        {
                            isPlayerSafeZone = true;
                        }
                    }

                    // 5. ブロックの割り当て
                    if (isPlayerSafeZone)
                    {
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Empty, 0);
                    }
                    else if (forceSolidFloor)
                    {
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Dirt, 3);
                    }
                    else if (shouldBeSolid)
                    {
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
                        gridData[x, yIndex, z] = new Block(Block.BlockType.Empty, 0);
                    }
                }
            }
        }
    }

    // 最初のブロック群を生成
    private void GenerateInitialBlocks(Vector3Int centerPhysicalPos)
    {
        // 処理落ち対策のため、初期生成範囲を限定（safeZoneRadius + 5を推奨）
        int initialGenerateRadius = safeZoneRadius + 5; 

        for (int x = centerPhysicalPos.x - initialGenerateRadius; x <= centerPhysicalPos.x + initialGenerateRadius; x++)
        {
            for (int yPhysical = centerPhysicalPos.y - initialGenerateRadius; yPhysical <= centerPhysicalPos.y + initialGenerateRadius; yPhysical++)
            {
                for (int z = centerPhysicalPos.z - initialGenerateRadius; z <= centerPhysicalPos.z + initialGenerateRadius; z++)
                {
                    Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, yPhysical, z));

                    if (gridPos.x >= 0 && gridPos.x < width &&
                        gridPos.y >= 0 && gridPos.y < height &&
                        gridPos.z >= 0 && gridPos.z < depth)
                    {
                        if (gridData[gridPos.x, gridPos.y, gridPos.z].type != Block.BlockType.Empty)
                        {
                             // 重複生成を防ぐために辞書チェック
                             Vector3Int physicalPos = new Vector3Int(x, yPhysical, z);
                             if (!activeBlockObjects.ContainsKey(physicalPos))
                             {
                                CreateBlockGameObject(physicalPos);
                             }
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

    // 物理座標をグリッドインデックスに変換するヘルパー関数
    private Vector3Int GetGridIndexFromPhysicalPos(Vector3Int physicalPos)
    {
        int maxGridYIndex = height - 1;
        int yIndex = physicalPos.y + maxGridYIndex;

        return new Vector3Int(physicalPos.x, yIndex, physicalPos.z);
    }

    // 外部からブロックデータを取得するメソッド (引数は物理座標)
    // ★★★ 境界チェックロジックを削除し、純粋なデータアクセスにシンプル化 ★★★
    public Block GetBlock(int x, int y, int z)
    {
        Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, y, z));
        int xIndex = gridPos.x;
        int yIndex = gridPos.y;
        int zIndex = gridPos.z;

        // 単純な配列境界チェック
        if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height && zIndex >= 0 && zIndex < depth)
        {
            return gridData[xIndex, yIndex, zIndex];
        }

        // データ配列の外側であれば、OUT_OF_BOUNDS（空）を返す
        return Block.OUT_OF_BOUNDS;
    }

    public void DestroyBlock(int x, int y, int z)
    {
        Vector3Int gridPos = GetGridIndexFromPhysicalPos(new Vector3Int(x, y, z));
        Vector3Int physicalPos = new Vector3Int(x, y, z);

        int xIndex = gridPos.x;
        int yIndex = gridPos.y;
        int zIndex = gridPos.z;

        // 1. グリッドデータ（配列）の破壊
        if (xIndex >= 0 && xIndex < width && yIndex >= 0 && yIndex < height && zIndex >= 0 && zIndex < depth)
        {
            // グリッドのデータを「空」に設定
            gridData[xIndex, yIndex, zIndex].type = Block.BlockType.Empty;
        }

        // 2. ★★★ 物理オブジェクト（GameObject）の破壊 ★★★
        // Dictionaryからオブジェクトを探して破壊
        if (activeBlockObjects.TryGetValue(physicalPos, out GameObject blockObject))
        {
            // Destroy(blockObject); // 処理落ち対策のため、非アクティブ化を試す場合はこちら
            blockObject.SetActive(false); // ★★★ 処理落ち対策として非アクティブ化を使用 ★★★
            activeBlockObjects.Remove(physicalPos);
        }
    }
}