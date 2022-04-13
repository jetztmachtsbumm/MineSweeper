using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenManager : MonoBehaviour
{
    public void OnRetryBtnClicked()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnMainMenuBtnClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
