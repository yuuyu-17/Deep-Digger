// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class TownManager : MonoBehaviour
// {
//     [Header("UI要素")]
//     // 画面上に配置したTextMeshProUGUIのコンポーネントをインスペクターで設定
//     public TextMeshProUGUI scoreText;
//     public TextMeshProUGUI fuelPackText;

//     // 画面上に配置したButtonコンポーネントをインスペクターで設定
//     public Button buyButton;
//     public Button goToMineButton;

//     private InventoryManager inventoryManager;
//     private ScoreManager scoreManager;
//     private GameManager gameManager;

//     // 一度に購入できる燃料パックの数
//     private const int ITEMS_TO_BUY_COUNT = 1;

//     private void Start()
//     {
//         // シングルトン参照の取得
//         inventoryManager = InventoryManager.instance;
//         scoreManager = ScoreManager.instance;
//         gameManager = GameManager.instance;

//         // ボタンのクリックイベントにメソッドを割り当てる
//         // ※この設定により、Unity Editorのインスペクターでの手動設定は不要になります。
//         if (buyButton != null)
//         {
//             buyButton.onClick.AddListener(TryBuyFuelPacks);
//         }
        
//         if (goToMineButton != null)
//         {
//             // 洞窟へ行くボタンはGameManagerのGoToMineを呼び出す
//             goToMineButton.onClick.AddListener(gameManager.GoToMine);
//         }

//         // カーソルを明示的に表示
//         Cursor.lockState = CursorLockMode.None;
//         Cursor.visible = true;
        
//         // UIを更新
//         UpdateTownUI();
//     }

//     // 街のUIを常に最新の状態に更新する
//     public void UpdateTownUI()
//     {
//         // ジェムスコアの表示
//         if (scoreText != null && scoreManager != null)
//         {
//             scoreText.text = "所持ジェム: " + scoreManager.GetCurrentScore().ToString();
//         }
        
//         // 燃料パック数の表示
//         if (fuelPackText != null && inventoryManager != null)
//         {
//             fuelPackText.text = "燃料パック: x" + inventoryManager.fuelPacks.ToString();
//         }
        
//         // 購入ボタンの状態とテキストを更新
//         if (buyButton != null && scoreManager != null && inventoryManager != null)
//         {
//             int cost = inventoryManager.itemCost;
//             int currentScore = scoreManager.GetCurrentScore();
//             bool canBuy = currentScore >= cost;
            
//             // 購入可能でない場合はボタンを無効化
//             buyButton.interactable = canBuy; 
            
//             // ボタンのテキストを動的に変更
//             TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
//             if (buttonText != null)
//             {
//                 buttonText.text = canBuy 
//                     ? $"燃料パック購入 (x{ITEMS_TO_BUY_COUNT}) \n[ジェム {cost}]" 
//                     : $"ジェム不足 (必要: {cost})";
//             }
//         }
//     }

//     // 購入ボタンが押されたときに実行されるロジック
//     void TryBuyFuelPacks()
//     {
//         if (scoreManager == null || inventoryManager == null) return;

//         int cost = inventoryManager.itemCost;

//         if (scoreManager.GetCurrentScore() >= cost)
//         {
//             // スコアを消費
//             scoreManager.RemoveScore(cost);
//             // アイテムを追加
//             inventoryManager.AddFuelPack(ITEMS_TO_BUY_COUNT);
            
//             Debug.Log($"購入成功！ジェムを{cost}消費し、燃料パックを{ITEMS_TO_BUY_COUNT}個手に入れました。");
//         }
        
//         // 購入後、UIを更新
//         UpdateTownUI();
//     }
// }
