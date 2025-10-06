using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapePortal : MonoBehaviour
{
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = GameManager.instance;
        if (gameManager == null)
        {
            Debug.LogError("EscapePortal: GameManagerが見つかりません！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("プレイヤーが帰還ポータルに到達しました。街へ戻ります。");
            if (gameManager != null)
            {
                // LoadWinSceneの代わりに、ReturnToTownを呼び出す
                gameManager.ReturnToTown();
            }
        }
    }
}
