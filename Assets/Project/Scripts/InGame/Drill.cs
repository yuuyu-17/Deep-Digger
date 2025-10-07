using UnityEngine;

public class Drill : MonoBehaviour
{
    [Header("Sound Effects")]
    public AudioClip dirtDrillClip;   // 土を掘る音
    public AudioClip rockDrillClip;   // 岩を掘る音
    public AudioClip gemDrillClip;    // ジェムを掘り当てた音
    private AudioSource audioSource;  // 追加

   // Raycastの最大距離
    public float maxDrillDistance = 5f;

    public Monster monster;

    private FuelManager fuelManager;

    private void Start()
    {
        // FuelManagerの参照を取得 (同じプレイヤーオブジェクトにアタッチされている前提)
        fuelManager = GetComponent<FuelManager>();
        if (fuelManager == null)
        {
            Debug.LogError("FuelManagerが見つかりません。");
            enabled = false;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("DrillにAudioSourceコンポーネントが見つかりません。");
        }
    }

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
                // 採掘時のFuel消費を最初に実行
                if (fuelManager.currentFuel <= 0)
                {
                    Debug.Log("燃料が切れたため掘れません。");
                    return;
                }

                // ヒットしたオブジェクトのタグを取得
                string hitTag = hit.collider.tag;
                float consumptionMultiplier = (hitTag == "Gem") ? 2.0f : 1.0f;
                fuelManager.ConsumeFuelForMining(consumptionMultiplier);
                if (hitTag == "CoreGem")
                {
                    Debug.Log("ゲームクリア！コア・ジェムを見つけました。");
                    GameManager.instance.LoadWinScene();
                    return;
                }

                // タグによって処理を分岐
                int scoreToAdd = 0;
                AudioClip clipToPlay = null;
                switch (hitTag)
                {
                    case "Dirt":
                        scoreToAdd = 0;
                        clipToPlay = dirtDrillClip; // 土の音を選択
                        break;
                    case "Rock":
                        scoreToAdd = 0;
                        clipToPlay = rockDrillClip; // 岩の音を選択
                        break;
                    case "Gem":
                        scoreToAdd = 100;
                        clipToPlay = gemDrillClip; // ジェムの音を選択
                        break;
                    default:
                        Debug.Log("掘るべきブロックではありません。");
                        return;
                }

                // ScoreManagerのインスタンスを通じてスコアを加算
                ScoreManager.instance.AddScore(scoreToAdd);

                // InGameUIManagerへの参照を取得または検索（最も簡単な方法）
                InGameUIManager uiManager = FindObjectOfType<InGameUIManager>();
                if (uiManager != null)
                {
                    uiManager.UpdateAllUI();
                }

                // ★★★ SEの再生 ★★★
                if (audioSource != null && clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);
                }

                // どのブロックでも共通の破壊処理
                Vector3Int gridPos = Vector3Int.RoundToInt(hit.collider.transform.position);

                // 1. GridManagerからブロックのデータ（耐久度情報）を取得
                Block blockToDestroy = GridManager.instance.GetBlock(gridPos.x, gridPos.y, gridPos.z);

                // 2. 耐久度が9999以上（破壊不可能な壁）かチェック
                if (blockToDestroy.durability >= 9999)
                {
                    Debug.Log("この壁はフィールドの境界です。破壊できません！");
                    return; // 破壊処理を中断してメソッドを終了
                }

                GridManager.instance.DestroyBlock(gridPos.x, gridPos.y, gridPos.z);

                // ★★★ モンスターの追跡をトリガー ★★★
                Monster monster = FindObjectOfType<Monster>(); // シーン内のモンスターを探す
                if (monster != null)
                {
                    monster.StartChasing();
                }
            }
        }
    }
}
