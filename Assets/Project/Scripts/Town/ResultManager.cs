using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
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
    private int taxAmount;

    private void Start()
    {
        if (GameStateManager.instance == null)
        {
            Debug.LogError("GameStateManagerが見つかりません。");
            SceneManager.LoadScene("GameOver");
            return;
        }

        if (FamilyStatusManager.instance == null)
        {
            Debug.LogError("FamilyStatusManagerが見つかりません。");
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
        taxAmount = GameStateManager.instance.totalShiftCost;

        CalculateAndDisplayResults();
    }

    private void CalculateAndDisplayResults()
    {
        int netProfitAfterTax = shiftEarnings - taxAmount;

        // UI表示の更新
        earningsText.text = "採掘スコア: " + shiftEarnings.ToString();
        costText.text = "固定費用 (納税): -" + taxAmount.ToString();

        // 清算後の総資産を計算し、GameStateManagerを更新（総資産の引き継ぎ）
        GameStateManager.instance.UpdateAssets(netProfitAfterTax);

        profitText.text = "清算後残額: " + netProfitAfterTax.ToString();

        // 総資産の最終値をUIに表示
        if (totalAssetsText != null)
        {
            totalAssetsText.text = "現在の総資産: " + GameStateManager.instance.TotalAssets.ToString();
        }

        FamilyStatusManager.instance.ConsumeFood();

        // 追放判定とUIの表示制御
        if (GameStateManager.instance.TotalAssets < 0)
        {
            gameOverFlag = true;
            statusText.text = "【追放確定】\n総資産が尽き、家族を維持できなくなりました。";
            if (continueButtonObject != null)
            {
                continueButtonObject.SetActive(false);
            }
            Invoke("LoadGameOverScene", 5f);
        }
        else if (netProfitAfterTax < 0)
        {
            statusText.text = "【負債発生】\n納税は貯金で補填されました。食料を購入できますが危険です。";
            if (continueButtonObject != null)
            {
                continueButtonObject.SetActive(true);
            }
        }
        else
        {
            statusText.text = "【生活継続】\n納税を済ませました。次のシフトに備えて食料を購入してください。";
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
