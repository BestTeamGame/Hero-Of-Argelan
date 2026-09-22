using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject exitButton;

    private void Start()
    {
        settingsPanel.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Settings()
    {
        settingsPanel.SetActive(true);

        playButton.SetActive(false);
        settingsButton.SetActive(false);
        exitButton.SetActive(false);
    }

    public void Back()
    {
        settingsPanel.SetActive(false);

        playButton.SetActive(true);
        settingsButton.SetActive(true);
        exitButton.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}