using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject startNewGameMenu;
    [SerializeField] private InputField widthInput;
    [SerializeField] private InputField heightInput;
    [SerializeField] private InputField mineCountInput;

    public void OnStartNewGameBtnClicked()
    {
        mainMenu.SetActive(false);
        startNewGameMenu.SetActive(true);
    }

    public void OnQuitBtnClicked()
    {
        Application.Quit();
    }

    public void OnBackBtnClicked()
    {
        startNewGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OnStartGameConfirmBtnClicked()
    {
        try
        {
            int width = int.Parse(widthInput.text);
            int height = int.Parse(heightInput.text);
            int mineCount = int.Parse(mineCountInput.text);

            Game.width = width;
            Game.height = height;
            Game.mineCount = mineCount;

            SceneManager.LoadScene("Game");
        }
        catch (FormatException)
        {

        }
    }

}
