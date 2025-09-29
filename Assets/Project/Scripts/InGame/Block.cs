using UnityEngine;

public class Block
{
    // ブロックの種類を定義
    public enum BlockType
    {
        Dirt,  // 土
        Rock,  // 岩
        Gem,   // 宝石
        Empty  // 掘られた後の空の状態
    }

    // このブロックのタイプ
    public BlockType type;

    // ブロックの耐久度
    public int health;

    // コンストラクタ（新しいブロックを作成する際の初期設定）
    public Block(BlockType type, int health)
    {
        this.type = type;
        this.health = health;
    }
}
