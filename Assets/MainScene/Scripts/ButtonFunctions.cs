using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ButtonFunctions : MonoBehaviour
{
    public Casino casino;
    public Animator animator;
    private string _UILayoutOnScreen;

    public void Start()
    {
        animator.enabled = true;
        animator.Play("StartUI");
        _UILayoutOnScreen = "StartUI";
    }
    public void GameOverUI()
    {
        animator.enabled = true;
        if (_UILayoutOnScreen=="GameUI")
            animator.Play("GameUItoRestartUI");
        if (_UILayoutOnScreen == "CasinoUI")
            animator.Play("CasinoUItoRestartUI");
        _UILayoutOnScreen = "RestartUI";
    }
    public void CasinoUItoGameUI()
    {
        animator.enabled = true;
        animator.Play("CasinoUItoGameUI");
        _UILayoutOnScreen = "GameUI";
    }
    public void OnCasinoUItoGameUIDone()
    {
        casino.SpawnResult();
    }
    public void GameUItoCasinoUI()
    {
        animator.enabled = true;
        animator.Play("GameUItoCasinoUI");
        _UILayoutOnScreen = "CasinoUI";
    }
    /*public void TurnOffAnimator()
    {
        animator.enabled = false;
    }*/
    public void StartUItoGameUI()
    {
        animator.enabled = true;
        animator.Play("StartUItoGameUI");
        _UILayoutOnScreen = "GameUI";
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnStartUItoGameUIDone()
    {
        Main.S.GameStart();
    }
     
    public void OpenShop()
    {
        SceneManager.LoadScene("Shop");
    }
    public void ResetCharacter()
    {
        File.Delete(Path.Combine(Application.persistentDataPath, "XMLPlayerData/BoughtItems.xml"));
        PlayerPrefs.DeleteAll();
        Restart();
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }
}
