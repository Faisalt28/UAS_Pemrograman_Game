using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject titleScreenPanel;
    public GameObject gameOverScreen;
    public GameObject uiControls;

    [Header("Buttons & Text")]
    public Button playButton;
    public Button volumeButton;
    public TextMeshProUGUI difficultyText;
    public Button newbieButton;
    public Button proButton;

    public TextMeshProUGUI gameOverText;
    public Button retryButton;
    public Button mainMenuButton;

    private string selectedDifficulty = "";
    private bool showDifficultyButtons = true;
    private bool isGameOver = false;

    public int playerDamage { get; private set; }

    private int destroyedAreaCount = 0;
    private int totalAreasToDestroy = 3;

    private int defeatedEnemyCount = 0;
    private int totalEnemiesToDefeat = 12;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        bool showMenu = PlayerPrefs.GetInt("ShowMainMenu", 0) == 1;
        bool isRestart = PlayerPrefs.GetInt("IsRestart", 0) == 1;

        PlayerPrefs.DeleteKey("ShowMainMenu");
        PlayerPrefs.DeleteKey("IsRestart");

        gameOverScreen?.SetActive(false);
        gameOverText?.gameObject.SetActive(false);
        uiControls?.SetActive(false); // Hide controls initially

        if (showMenu)
        {
            Time.timeScale = 0f;
            titleScreenPanel?.SetActive(true);
        }
        else if (isRestart)
        {
            StartGame();
        }
        else
        {
            Time.timeScale = 0f;
            titleScreenPanel?.SetActive(true);
        }

        playButton.interactable = false;
        playButton.onClick.AddListener(StartGame);
        volumeButton?.onClick.AddListener(() => Debug.Log("Volume clicked"));

        newbieButton?.onClick.AddListener(() => SelectDifficulty("Newbie"));
        proButton?.onClick.AddListener(() => SelectDifficulty("Pro"));
        difficultyText?.GetComponent<Button>()?.onClick.AddListener(ToggleDifficultyButtons);

        retryButton?.onClick.AddListener(RestartGame);
        mainMenuButton?.onClick.AddListener(ReturnToMainMenu);

        UpdateDifficultyUI();
    }

    void SelectDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        showDifficultyButtons = false;
        playerDamage = difficulty == "Newbie" ? 10 : 2;
        playButton.interactable = true;
        UpdateDifficultyUI();
        Debug.Log($"Difficulty: {selectedDifficulty}, Damage: {playerDamage}");
    }

    void ToggleDifficultyButtons()
    {
        selectedDifficulty = "";
        showDifficultyButtons = true;
        playButton.interactable = false;
        UpdateDifficultyUI();
    }

    void UpdateDifficultyUI()
    {
        difficultyText.text = string.IsNullOrEmpty(selectedDifficulty)
            ? "Select Difficulty"
            : $"Difficulty: {selectedDifficulty} (Click to change)";

        bool show = string.IsNullOrEmpty(selectedDifficulty) || showDifficultyButtons;
        newbieButton?.gameObject.SetActive(show && selectedDifficulty != "Newbie");
        proButton?.gameObject.SetActive(show && selectedDifficulty != "Pro");
    }

    public void StartGame()
    {
        titleScreenPanel?.SetActive(false);
        gameOverScreen?.SetActive(false);
        gameOverText?.gameObject.SetActive(false);
        uiControls?.SetActive(true); // Tampilkan kontrol saat game dimulai

        Time.timeScale = 1f;
        Debug.Log("Game started");
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        gameOverText.text = "GAME OVER";
        gameOverText.gameObject.SetActive(true);
        gameOverScreen?.SetActive(true);
        uiControls?.SetActive(false); // Sembunyikan kontrol saat game over
    }

    public void GameComplete()
    {
        if (isGameOver) return;
        isGameOver = true;

        gameOverText.text = "YOU WIN!";
        gameOverText.gameObject.SetActive(true);
        gameOverScreen?.SetActive(true);
        uiControls?.SetActive(false); // Sembunyikan kontrol saat menang
    }

    public void AreaDestroyed()
    {
        destroyedAreaCount++;
        Debug.Log($"Area destroyed: {destroyedAreaCount}/{totalAreasToDestroy}");

        if (destroyedAreaCount >= totalAreasToDestroy)
        {
            GameOver();
        }
    }

    public void EnemyDefeated()
    {
        defeatedEnemyCount++;
        Debug.Log($"Enemy defeated: {defeatedEnemyCount}/{totalEnemiesToDefeat}");

        if (defeatedEnemyCount >= totalEnemiesToDefeat)
        {
            GameComplete();
        }
    }

    public void RestartGame()
    {
        PlayerPrefs.SetInt("IsRestart", 1);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.SetInt("ShowMainMenu", 1);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
