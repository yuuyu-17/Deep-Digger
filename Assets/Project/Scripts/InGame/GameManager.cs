using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // プレイヤーが掘る必要のある宝石の数
    public int gemsToCollect = 5;

    // 現在の宝石の数
    private int collectedGems = 0;

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
    }

    public void AddGem()
    {
        collectedGems++;
        Debug.Log("Collected Gems: " + collectedGems + " / " + gemsToCollect);

        if (collectedGems >= gemsToCollect)
        {
            Debug.Log("ゲームクリア！");
            // ここでゲームクリア処理を呼び出す
            LoadWinScene();
        }
    }

    private void LoadWinScene()
    {
        // 勝利画面のシーンに遷移
        //SceneManager.LoadScene("WinScene");
    }
}
