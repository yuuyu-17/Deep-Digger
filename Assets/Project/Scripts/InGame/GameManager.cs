using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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

    private void Start()
    {
        
    }

    private void Update()
    {
        
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