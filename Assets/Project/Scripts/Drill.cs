using UnityEngine;

public class Drill : MonoBehaviour
{
   // Raycastの最大距離
    public float maxDrillDistance = 5f;
    public Monster monster;

    private void Update()
    {
        // マウスの左クリックを検知
        if (Input.GetMouseButtonDown(0))
        {
            // Raycastの情報を格納する変数
            RaycastHit hit;
            
            // 画面の中央からRaycastを飛ばす
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDrillDistance))
            {
                // ヒットしたオブジェクトのタグを取得
                string hitTag = hit.collider.tag;

                // タグによって処理を分岐
                switch (hitTag)
                {
                    case "Dirt":
                        Debug.Log("土のブロックを掘りました！");
                        break;
                    case "Rock":
                        Debug.Log("岩のブロックを掘りました！");
                        break;
                    case "Gem":
                        Debug.Log("宝石を掘り当てました！");
                        break;
                    default:
                        Debug.Log("掘るべきブロックではありません。");
                        return;
                }

                // どのブロックでも共通の破壊処理
                Vector3Int gridPos = Vector3Int.RoundToInt(hit.collider.transform.position);
                GridManager.instance.DestroyBlock(gridPos.x, gridPos.y, gridPos.z);

                // 掘った後に化け物を動かす
                if (monster != null)
                {
                    monster.MoveTowardsPlayer();
                }
            }
        }
    }
}
