using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("Other Buttons")]
    public Button playButton;
    public Button muteButton;
    public Button quitButton;

    [Header("Visuals")]
    public TextMeshProUGUI muteButtonText;
    public Image easyHighlight;
    public Image mediumHighlight;
    public Image hardHighlight;

    private bool isMuted = false;
    private Difficulty selectedDifficulty = Difficulty.Easy;

    void Start()
    {
        easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        mediumButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Medium));
        hardButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
        playButton.onClick.AddListener(PlayGame);
        muteButton.onClick.AddListener(ToggleMute);
        quitButton.onClick.AddListener(QuitGame);

        SelectDifficulty(Difficulty.Easy);
    }

    void SelectDifficulty(Difficulty d)
    {
        selectedDifficulty = d;

        // Visual feedback — highlight selected
        easyHighlight.color = d == Difficulty.Easy
            ? new Color(0f, 0.8f, 0.3f, 0.4f)
            : new Color(1, 1, 1, 0.1f);
        mediumHighlight.color = d == Difficulty.Medium
            ? new Color(1f, 0.6f, 0f, 0.4f)
            : new Color(1, 1, 1, 0.1f);
        hardHighlight.color = d == Difficulty.Hard
            ? new Color(0.9f, 0.1f, 0.1f, 0.4f)
            : new Color(1, 1, 1, 0.1f);
    }

    void PlayGame()
    {
        // Set difficulty before loading game
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.SetDifficulty((int)selectedDifficulty);

        SceneManager.LoadScene("SampleScene");
    }

    void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        muteButtonText.text = isMuted ? "SOUND OFF" : "SOUND ON";
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}