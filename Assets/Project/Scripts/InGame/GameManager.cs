using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("シーン設定")]
    public string townSceneName = "TownScene"; // 例: "TownScene"
    public string mineSceneName = "MineScene"; // 例: "MineScene"

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

    public void GoToMine()
    {
        Debug.Log("街から洞窟へ移動します。");
        SceneManager.LoadScene(mineSceneName);
    }
    
    // ★★★ 洞窟から街へ帰還 ★★★
    public void ReturnToTown()
    {
        Debug.Log("洞窟から街へ帰還します。");
        SceneManager.LoadScene(townSceneName);
        
        // ★ヒント: 帰還時にスコア清算や次回への準備ロジックをここに追加できます★
    }

    public void LoadWinScene()
    {
        // ゲームのクリア処理を実行
        Debug.Log("ゲームクリア！WinSceneに遷移します。");
        
        // 現在のシーンを停止し、WinSceneへ移動
        SceneManager.LoadScene("WinScene");
    }

    public void LoadGameOverScene()
    {
        Debug.Log("燃料切れによりゲームオーバー。GameOverSceneに遷移します。");
        SceneManager.LoadScene("GameOver");
    }
}