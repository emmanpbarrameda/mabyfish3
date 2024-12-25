using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject getReady;
    [SerializeField] private AudioSource scoreSound;
    [SerializeField] private AudioSource gameOverSound;
    [SerializeField] private AudioSource gameStartSound;
    [SerializeField] private AudioSource thresholdSound;
    [SerializeField] private AudioSource backgroundSound;
    [SerializeField] private AudioSource highScoreSound;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button creditsButton;

    public int score { get; private set; } = 0;
    private int highScore = 0;
    private const string HIGH_SCORE_KEY = "HighScore";

    private bool isGamePaused = false;
    private bool isPlaying = false;
    private bool isQuitting = false;
    private bool hasPlayedHighScoreSound = false;

    // sound plays only depends on reached net
    private bool threshold8Reached = false;
    private bool threshold20Reached = false;
    private bool threshold30Reached = false;

    // app launched
    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
        
        LoadHighScore();
    }

    // on close of app
    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }


    // start
    private void Start()
    {
        // Set initial player position
        if (player != null)
        {
            player.transform.position = new Vector3(-0.002f, 0.008f, -2f);
        }

        // about button
        if (aboutButton != null) {
            aboutButton.onClick.AddListener(ShowAboutDialog);
        }

        // credits button
        if (creditsButton != null) {
            creditsButton.onClick.AddListener(ShowCreditsDialog);
        }
        
        getReady.SetActive(true);
        gameOver.SetActive(false);
        UpdateScoreText();
        Pause();
    }


    //about btn
    private void ShowAboutDialog()
    {
    #if PLATFORM_ANDROID
        using (var dialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", GetCurrentActivity()))
        {
            // dialog title
            dialogBuilder.Call<AndroidJavaObject>("setTitle", "About the Game");

            // content
            string message = 
                "MABY ENDLESS QUEST\n\n" +
                "Version: 1.0.0\n" +
                "\n" +
                " Developers \n" +
                "- Andrew Benitez\n" +
                "- Daruel James Mirando\n" +
                "- Emman Barrameda\n" +
                "- Jhon Lester Delgado\n" +
                "- Nicole Shane Argana\n" +
                "\n" +
                "Enjoy the game and have fun!";

            dialogBuilder.Call<AndroidJavaObject>("setMessage", message);

            // ok
            dialogBuilder.Call<AndroidJavaObject>("setPositiveButton", "OK", new DialogInterfaceOnClickListener(() => { /* nothing happens */ }));

            // show dialog
            dialogBuilder.Call<AndroidJavaObject>("show");
        }
    #else
        Debug.Log("About dialog only supported on Android!");
    #endif
    }


    //credits btn
    private void ShowCreditsDialog()
    {
    #if PLATFORM_ANDROID
        using (var dialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", GetCurrentActivity()))
        {
            // dialog title
            dialogBuilder.Call<AndroidJavaObject>("setTitle", "Credits");

            // content
            string message = 
                "MABY ENDLESS QUEST\n\n" +
                "Version: 1.0.0\n" +
                "\n" +
                " Credits \n" +
                " Spirtes: https://www.canva.com/ \n" +
                " Sounds: https://pixabay.com/sound-effects/ \n" +
                "\n" +
                "Enjoy the game and have fun!";

            dialogBuilder.Call<AndroidJavaObject>("setMessage", message);

            // ok
            dialogBuilder.Call<AndroidJavaObject>("setPositiveButton", "OK", new DialogInterfaceOnClickListener(() => { /* nothing happens */ }));

            // show dialog
            dialogBuilder.Call<AndroidJavaObject>("show");
        }
    #else
        Debug.Log("About dialog only supported on Android!");
    #endif
    }


    // handle back button
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isQuitting)
        {
            HandleBackButton();
        }
    }
    // show back confirmation dialog
    private void HandleBackButton()
    {
        #if PLATFORM_ANDROID
            isQuitting = true;
            PauseGame();
            
            using (var dialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", GetCurrentActivity()))
            {
                dialogBuilder.Call<AndroidJavaObject>("setTitle", "Quit Game?");
                dialogBuilder.Call<AndroidJavaObject>("setMessage", "Do you want to quit the game?");
                
                // yes
                dialogBuilder.Call<AndroidJavaObject>("setPositiveButton", "Yes", new DialogInterfaceOnClickListener(() => {
                    Application.Quit();
                }));
                
                // no
                dialogBuilder.Call<AndroidJavaObject>("setNegativeButton", "No", new DialogInterfaceOnClickListener(() => {
                    isQuitting = false;
                    if (isPlaying && !gameOver.activeSelf)
                    {
                        ResumeGame();
                    }
                }));
                
                // Show dialog
                dialogBuilder.Call<AndroidJavaObject>("show");
            }
        #else
            Application.Quit();
        #endif
    }
    private AndroidJavaObject GetCurrentActivity()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
    private class DialogInterfaceOnClickListener : AndroidJavaProxy
    {
        private System.Action action;

        public DialogInterfaceOnClickListener(System.Action action) 
            : base("android.content.DialogInterface$OnClickListener")
        {
            this.action = action;
        }

        public void onClick(AndroidJavaObject dialog, int which)
        {
            action?.Invoke();
            dialog.Call("dismiss");
        }
    }
    // on app pause
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isQuitting)
        {
            PauseGame();
        }
    }
    // pause game
    private void PauseGame()
    {
        if (isPlaying && !gameOver.activeSelf)
        {
            isGamePaused = true;
            Time.timeScale = 0f;
            player.enabled = false;
            
            if (backgroundSound != null && backgroundSound.isPlaying)
            {
                backgroundSound.Pause();
            }
        }
    }


    // resume
    private void ResumeGame()
    {
        if (isGamePaused && isPlaying && !gameOver.activeSelf)
        {
            isGamePaused = false;
            Time.timeScale = 1f;
            player.enabled = true;
            
            if (backgroundSound != null && !backgroundSound.isPlaying)
            {
                backgroundSound.UnPause();
            }
        }
    }

    // pause game
    public void Pause()
    {
        Time.timeScale = 0f;
        player.enabled = false;
    }

    // play
    public void Play()
    {
        isPlaying = true;
        isGamePaused = false;
        hasPlayedHighScoreSound = false;
        
        if (gameStartSound != null) {
            gameStartSound.Play();
        }

        if (backgroundSound != null) {
            backgroundSound.Play();
        }

        score = 0;
        UpdateScoreText();

        playButton.SetActive(false);
        gameOver.SetActive(false);
        getReady.SetActive(false);

        // about button
        if (aboutButton != null) {
            aboutButton.gameObject.SetActive(false);
        }

        // credits button
        if (creditsButton != null) {
            creditsButton.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
        player.enabled = true;

        // player position
        player.transform.position = new Vector3(-0.002f, 0.008f, -2f);

        player.UpdateStats(3f, -4f, 3.5f);

        Pipes[] pipes = FindObjectsOfType<Pipes>();
        for (int i = 0; i < pipes.Length; i++) {
            Destroy(pipes[i].gameObject);
        }

        threshold8Reached = false;
        threshold20Reached = false;
        threshold30Reached = false;
    }

    // game over functions
    public void GameOver()
    {
        isPlaying = false;
        isGamePaused = false;
        
        if (gameOverSound != null) {
            gameOverSound.Play();
        }

        if (backgroundSound != null) {
            backgroundSound.Stop();
        }

        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
        }

        playButton.SetActive(true);
        gameOver.SetActive(true);
        getReady.SetActive(false);
        
        // about button
        if (aboutButton != null) {
            aboutButton.gameObject.SetActive(true);
        }

        // credits button
        if (creditsButton != null) {
            creditsButton.gameObject.SetActive(true);
        }

        UpdateScoreText();
        Pause();
    }

    // get high score
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    // save high score
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    // update score
    private void UpdateScoreText()
    {
        if (gameOver.activeSelf || getReady.activeSelf)
        {
            scoreText.text = $"{score} | High: {highScore}";
        }
        else
        {
            scoreText.text = score.ToString();
        }
    }

    // score detector
    public void IncreaseScore()
    {
        score++;
        UpdateScoreText();

        if (scoreSound != null) {
            scoreSound.Play();
        }

        // Check for new high score during active gameplay
        if (isPlaying && score > highScore) {
            highScore = score;

            // Play highScoreSound only once
            if (!hasPlayedHighScoreSound && highScoreSound != null) {
                highScoreSound.Play();
                hasPlayedHighScoreSound = true;
            }

            // Save new high score
            SaveHighScore();
        }

        UpdatePlayerStats();
    }
    public void ResetHighScoreFlag() {
        hasPlayedHighScoreSound = false;
    }
    

    // update gameplay, depends on score
    private void UpdatePlayerStats() {
        if (score >= 8 && score < 20 && !threshold8Reached)
        {
            player.UpdateStats(6f, -10f, 6f);
            PlayThresholdSound(1.1f);
            threshold8Reached = true;
        }
        else if (score >= 20 && score < 30 && !threshold20Reached)
        {
            player.UpdateStats(7f, -11f, 7f);
            PlayThresholdSound(1.2f);
            threshold20Reached = true;
        }
        else if (score >= 30 && !threshold30Reached)
        {
            player.UpdateStats(8f, -12f, 8f);
            PlayThresholdSound(1.3f);
            threshold30Reached = true;
        }
    }

    // gameplay level sound
    private void PlayThresholdSound(float pitch)
    {
        if (thresholdSound != null)
        {
            thresholdSound.pitch = pitch;
            thresholdSound.Play();
        }
    }
}