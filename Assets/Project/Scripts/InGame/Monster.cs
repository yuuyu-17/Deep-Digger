using UnityEngine;

public class Monster : MonoBehaviour
{
    // 化け物の移動速度
    public float moveSpeed = 10f;
    public float patrolSpeed = 3f;           // 通常時の移動速度
    public float chaseSpeedMultiplier = 3f;  // 追跡時の速度倍率

    [Header("徘徊設定")]
    public float directionChangeInterval = 4f; // 4秒ごとに方向を変更
    private float patrolTimer = 0f;
    private Vector3 patrolDirection;

    [Header("追跡設定")]
    private Vector3 targetPosition;
    private bool isChasing = false; // 追跡モードフラグ
    private float chaseDuration = 5f; // 追跡を続ける時間
    private float chaseTimer = 0f;

    [Header("撃退設定")]
    public float largeFuelPenalty = 0.5f; // 撃退時に消費する燃料 (例: 50%)

    [Header("Sound")]
    public AudioClip chaseLoopClip; // 追跡中にループ再生する音（足音、唸り声など）
    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("MonsterにAudioSourceコンポーネントが見つかりません。");
        }

        patrolDirection = GetRandomDirection();

        SetInitialSpawnPosition();
    }

    private void Update()
    {
        if (isChasing)
        {
            // 追跡タイマーを減らす
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0)
            {
                isChasing = false;
                Debug.Log("モンスターは追跡を諦め、徘徊に戻りました。");

                // ★★★ 追記: 追跡SEの停止 ★★★
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
            // 追跡移動 (moveSpeed * chaseSpeedMultiplierを使用)
            MoveTowardsPlayer(moveSpeed * chaseSpeedMultiplier);
        }
        else
        {
            // 通常の徘徊移動 (後述のPatrol()を呼び出す)
            Patrol();
        }
    }

    private void SetInitialSpawnPosition()
    {
        GridManager gridManager = GridManager.instance;
        
        if (gridManager == null)
        {
            Debug.LogError("GridManagerが見つかりません。モンスターの初期配置をスキップします。");
            return;
        }

        // プレイヤーのスポーン位置を取得 (PlayerSpawnFix.csと同じロジックを想定)
        float playerX = (float)gridManager.width / 2f;
        float playerZ = (float)gridManager.depth / 2f;
        float spawnY = (float)gridManager.startYLevel; 
        
        // ★★★ 修正箇所: プレイヤー位置から最低限離れた位置をランダムに探す ★★★
        
        Vector3 initialPosition;
        float minDistance = 10f; // プレイヤーから最低10ブロック離す
        int maxAttempts = 50;    // 最大試行回数
        int attempt = 0;

        do
        {
            // プレイヤー位置から一定距離離れたランダムなX, Z座標を生成
            float randomAngle = Random.Range(0f, 360f);
            float randomRadius = Random.Range(minDistance, Mathf.Min(gridManager.width, gridManager.depth) / 2f); // フィールドサイズに応じて調整
            
            // プレイヤー位置を基準としたスポーン位置
            float spawnX = playerX + Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomRadius;
            float spawnZ = playerZ + Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomRadius;

            initialPosition = new Vector3(
                Mathf.RoundToInt(spawnX), // グリッドの整数座標に丸める
                spawnY, 
                Mathf.RoundToInt(spawnZ)  // グリッドの整数座標に丸める
            );

            attempt++;

            // 試行回数を超えたら、デバッグ用に強制的に中心から離れた場所に設定する
            if (attempt >= maxAttempts)
            {
                Debug.LogWarning("ランダムな安全スポーン位置が見つかりませんでした。強制的に初期配置を設定します。");
                initialPosition = new Vector3(playerX + minDistance, spawnY, playerZ);
                break; 
            }

        // プレイヤーの位置と同一でなく、かつグリッドの境界内にあることを確認する
        } while (Vector3.Distance(initialPosition, new Vector3(playerX, spawnY, playerZ)) < minDistance);

        transform.position = initialPosition;
        Debug.Log($"モンスターをプレイヤーから隔離された位置 ({initialPosition}) に移動しました。");
    }

    // ----------------------------------------------------
    // ★★★ 採掘音（外部）から追跡をトリガーするメソッド ★★★
    // ----------------------------------------------------
    public void StartChasing()
    {
        isChasing = true;
        chaseTimer = chaseDuration;
        Debug.Log("採掘音！モンスターがプレイヤーを追跡開始！");

        // ★★★ 追記: 追跡SEの再生を開始 ★★★
        if (audioSource != null && chaseLoopClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = chaseLoopClip;
            audioSource.loop = true; // ループ再生を有効化
            audioSource.Play();
        }
    }

    // ----------------------------------------------------
    // ★★★ ライトによる撃退判定（ライトのコライダーが当たったとき） ★★★
    // ----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // 1. プレイヤーライトによる撃退判定
        if (other.CompareTag("Flashlight"))
        {
            FlashlightController flashlight = other.GetComponentInParent<FlashlightController>();
            if (flashlight != null && flashlight.IsLightOn())
            {
                // ★★★ 撃退ロジック: 燃料消費とテレポート ★★★
                
                // 燃料ペナルティを課す (1回だけ実行)
                FuelManager.instance?.ConsumeFuelForMining(largeFuelPenalty);

                // ★★★ 追記: 追跡SEの停止 ★★★
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                
                // テレポートして追跡を終了
                TeleportToSafeLocation(); 
                isChasing = false;
                
                Debug.Log($"ライトの光でモンスターを撃退！燃料{largeFuelPenalty * 100}%消費し、テレポートさせました。");
                return; // 撃退が成功したら、これ以上の処理（ゲームオーバー判定）は行わない
            }
        }
        
        // 2. プレイヤー接触によるゲームオーバー判定
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゲームオーバー！モンスターに捕まりました。");
            // GameManagerのインスタンスがnullでないか確認する
            GameManager.instance?.LoadGameOverScene();
        }
    }

    // ----------------------------------------------------
    // ★★★ 追跡移動のロジック (速度を引数として受け取る) ★★★
    // ----------------------------------------------------
    public void MoveTowardsPlayer(float currentSpeed)
    {
       if (playerTransform == null) return;
        
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // 進行方向にブロックがあるかレイキャストで確認
        RaycastHit hit;
        // 速度に基づいてターゲット位置を計算 *これは移動処理なので先に実行
        Vector3 targetPosition = transform.position + direction * currentSpeed * Time.deltaTime; 

        if (Physics.Raycast(transform.position, direction, out hit, 1.5f)) 
        {
            if (hit.collider.CompareTag("Dirt") || hit.collider.CompareTag("Rock") || hit.collider.CompareTag("Gem"))
            {
                // 掘る処理。このフレームは移動せず、破壊に専念させても良い
                Vector3Int blockPos = Vector3Int.RoundToInt(hit.collider.transform.position);
                GridManager.instance.DestroyBlock(blockPos.x, blockPos.y, blockPos.z);
                // 掘った場合はこのフレームの移動をキャンセルする（ブロックを突き抜けないように）
                // return; // 掘った場合は移動をスキップ
            }
        }

        // Rigidbodyを使って位置をスムーズに更新
        rb.MovePosition(targetPosition);
    }

    // ----------------------------------------------------
    // ★★★ 徘徊ロジック (シンプルにランダムに移動) ★★★
    // ----------------------------------------------------
    private void Patrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            // 一定時間経過で新しいランダムな方向に変更
            patrolDirection = GetRandomDirection();
            patrolTimer = directionChangeInterval;
        }

        // 徘徊移動を実行
        rb.MovePosition(transform.position + patrolDirection * patrolSpeed * Time.deltaTime);

        // 壁に当たった場合の方向転換ロジックなどを追加しても良い
    }

    private Vector3 GetRandomDirection()
    {
        // X-Z平面でのランダムな方向を生成
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        return randomDir;
    }

    private void TeleportToSafeLocation()
    {
        GridManager gridManager = GridManager.instance;
        if (gridManager == null) return;

        Vector3 newPosition;
        float teleportDistance = 20f; // プレイヤーから最低20ブロック離れた場所へテレポート
        int maxAttempts = 10;
        int attempt = 0;

        // プレイヤーの位置を取得
        Vector3 playerPos = playerTransform.position;
        float spawnY = (float)gridManager.startYLevel; 

        do
        {
            // プレイヤーの周囲のランダムな位置
            float randomAngle = Random.Range(0f, 360f);
            
            // プレイヤーから最低20ブロック離れた場所を探す
            float spawnX = playerPos.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad) * teleportDistance;
            float spawnZ = playerPos.z + Mathf.Sin(randomAngle * Mathf.Deg2Rad) * teleportDistance;

            newPosition = new Vector3(
                Mathf.RoundToInt(spawnX),
                spawnY, 
                Mathf.RoundToInt(spawnZ)
            );

            attempt++;

            // 試行回数やグリッド境界外の場合のチェック（省略可）
            if (attempt >= maxAttempts) break;

        // プレイヤーの位置に近すぎる場合（念のため）、または移動可能でない場合は再試行
        } while (Vector3.Distance(newPosition, playerPos) < teleportDistance);
        
        // テレポートを実行
        transform.position = newPosition;
        
        Debug.Log($"モンスターを ({newPosition}) へテレポートさせました。");
    }
}
