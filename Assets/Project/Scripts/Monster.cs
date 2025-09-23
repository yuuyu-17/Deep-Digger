using UnityEngine;

public class Monster : MonoBehaviour
{
    // 化け物の移動速度
    public float moveSpeed = 10f;

    // プレイヤーへの参照
    private Transform playerTransform;

    private Rigidbody rb;

    private void Start()
    {
        // プレイヤーオブジェクトを探して、そのTransformを取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        rb = GetComponent<Rigidbody>();
    }

    // 外部から呼ばれて化け物を動かすメソッド
    public void MoveTowardsPlayer()
    {
       if (playerTransform == null) return;

    // プレイヤーの方向を計算
    Vector3 direction = (playerTransform.position - transform.position).normalized;

    // 目の前にブロックがあるかレイキャストで確認
    RaycastHit hit;
    if (Physics.Raycast(transform.position, direction, out hit, 1.5f)) // 1.5fは少し長めに設定
    {
        // レイキャストが当たったオブジェクトがブロックだったら
        if (hit.collider.CompareTag("Dirt")||hit.collider.CompareTag("Rock")||hit.collider.CompareTag("Gem"))
        {

            //ログ出力
            Debug.Log("モンスターが" + hit.collider.tag + "ブロックを掘りました！");

            // ブロックを破壊
            Vector3Int blockPos = Vector3Int.RoundToInt(hit.collider.transform.position);
            GridManager.instance.DestroyBlock(blockPos.x, blockPos.y, blockPos.z);
            return; // 掘ったので移動はしない
        }
    }

    // 目の前にブロックがなければ、プレイヤーに近づく
    Vector3 targetPosition = transform.position + direction;
    targetPosition.x = Mathf.RoundToInt(targetPosition.x);
    targetPosition.y = Mathf.RoundToInt(targetPosition.y);
    targetPosition.z = Mathf.RoundToInt(targetPosition.z);

    // Rigidbodyを使って位置を更新
    rb.MovePosition(targetPosition);
    }
}
