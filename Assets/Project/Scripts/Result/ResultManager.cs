using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    [Header("UI要素")]
    public TextMeshProUGUI earningsText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI profitText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI totalAssetsText; // 総資産表示用

    [Header("操作ボタン")]
    public GameObject continueButtonObject;
    private Button continueButtonComponent; // ボタンのイベント登録に使う

    private bool gameOverFlag = false;
    private int shiftEarnings;
    private int totalCost;

    private void Start()
    {
        if (GameStateManager.instance == null)
        {
            Debug.LogError("GameStateManagerが見つかりません。");
            SceneManager.LoadScene("GameOver");
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Buttonコンポーネントを取得し、イベントを登録
        if (continueButtonObject != null)
        {
            continueButtonComponent = continueButtonObject.GetComponent<Button>();
            if (continueButtonComponent != null)
            {
                continueButtonComponent.onClick.AddListener(OnContinueClicked);
            }
        }

        // GameStateManagerからデータ取得
        shiftEarnings = GameStateManager.instance.currentShiftEarnings;
        totalCost = GameStateManager.instance.totalShiftCost;

        CalculateAndDisplayResults();
    }

    private void CalculateAndDisplayResults()
    {
        int netProfit = shiftEarnings - totalCost;

        // UI表示の更新
        earningsText.text = "採掘スコア: " + shiftEarnings.ToString();
        costText.text = "固定費用 (納税/食費): -" + totalCost.ToString();

        // 清算後の総資産を計算し、GameStateManagerを更新（総資産の引き継ぎ）
        GameStateManager.instance.UpdateAssets(netProfit);

        profitText.text = "純利益: " + netProfit.ToString();

        // 総資産の最終値をUIに表示
        if (totalAssetsText != null)
        {
            totalAssetsText.text = "現在の総資産: " + GameStateManager.instance.totalAssets.ToString();
        }

        // 追放判定とUIの表示制御
        if (GameStateManager.instance.totalAssets < 0)
        {
            gameOverFlag = true;
            statusText.text = "【追放確定】\n総資産が尽きました。あなたは闇に飲み込まれます。";
            if (continueButtonObject != null)
            {
                continueButtonObject.SetActive(false);
            }
            Invoke("LoadGameOverScene", 5f);
        }
        else if (netProfit < 0)
        {
            statusText.text = "【負債発生】\n貯金で補填されましたが、危険な状態です。";
            if (continueButtonObject != null)
            {
                continueButtonObject.SetActive(true);
            }
        }
        else
        {
            statusText.text = "【生活継続】\n利益が出ました。家族は守られました。";
            if (continueButtonObject != null)
            {
                continueButtonObject.SetActive(true);
            }
        }
    }

    public void OnContinueClicked()
    {
        if (!gameOverFlag)
        {
            SceneManager.LoadScene("InGameScene");
        }
    }

    private void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }
}
