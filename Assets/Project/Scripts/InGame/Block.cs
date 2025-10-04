using UnityEngine;

public struct Block // ★クラスから構造体へ変更
{
    public enum BlockType { Empty, Dirt, Rock, Gem }
    public BlockType type;
    public int durability; // 耐久度

    public static readonly Block OUT_OF_BOUNDS = new Block(BlockType.Empty, 0);

    public Block(BlockType type, int durability)
    {
        this.type = type;
        this.durability = durability;
    }
}
