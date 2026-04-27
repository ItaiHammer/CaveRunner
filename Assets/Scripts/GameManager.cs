using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Elements")]
    [SerializeField]private PlayerMovement playerController;

    [Header("Gameplay HUD")]
    [Tooltip("Optional parent for all in-game HUD (timer, fragments, etc.). Hidden on game over. If null, fragment + timer objects are hidden instead.")]
    public GameObject gameplayHudRoot;
    [Tooltip("Two lines: collected / total and hints.")]
    public TMP_Text fragmentCounterText;
    public TMP_Text timerText;

    [Header("Timer")]
    public bool useTimer = true;
    [Min(0)]
    public int timerMinutes;
    [Range(0, 59)]
    public int timerSeconds;
    [Range(0, 999)]
    public int timerMilliseconds;
    [Tooltip("Shown in the game over reason line when the countdown hits zero.")]
    public string timeUpMessage = "Time's up!";

    [Header("Game Over")]
    public GameObject gameOverMenu;
    [Tooltip("Label for why the run ended (e.g. Time's up, fell, hazard).")]
    public TMP_Text gameOverReasonText;
    [Tooltip("Used when TriggerGameOver() is called with no custom message.")]
    public string defaultGameOverMessage = "Game over.";
    public string deathMessage = "You died!";
    public UnityEvent onGameOver;

    [Header("Victory")]
    public GameObject victoryMenu;
    [Tooltip("Shows remaining countdown time at the moment of victory (same format as the HUD timer).")]
    public TMP_Text victoryTimeRemainingText;
    [Tooltip("If the level has no timer, this text is set to this string instead.")]
    public string victoryNoTimerLabel = "—";
    public UnityEvent onVictory;

    [Header("Music")]
    [Tooltip("Shared background music source used during gameplay and swapped on victory.")]
    public AudioSource backgroundMusicSource;
    [Tooltip("Track to play once the player wins.")]
    public AudioClip victoryMusicClip;
    [Tooltip("Loop victory track while the victory menu is shown.")]
    public bool loopVictoryMusic = true;

    [Header("Level exit")]
    [Tooltip("Empty GameObject with a Collider2D (trigger) for the end zone.")]
    public GameObject levelExitObject;

    [Header("Fragments")]
    [Tooltip("How many core fragments the player must collect to finish the level. Can be less than total in the scene.")]
    public int fragmentsRequiredToFinish = 3;

    public int TotalCoreFragmentsInLevel { get; private set; }

    public int CoreFragmentsCollected { get; private set; }

    public bool HasMetFragmentRequirement =>
        CoreFragmentsCollected >= fragmentsRequiredToFinish;

    public bool HasCollectedAll =>
        TotalCoreFragmentsInLevel > 0 && CoreFragmentsCollected >= TotalCoreFragmentsInLevel;

    public bool IsTimeExpired { get; private set; }

    public bool IsGameOver { get; private set; }

    public bool IsVictory { get; private set; }

    float timeRemainingSeconds;

    Collider2D resolvedExitCollider;
    readonly List<Collider2D> exitOverlapBuffer = new List<Collider2D>();
    PlayerMovement cachedPlayer;

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
        cachedPlayer = FindObjectOfType<PlayerMovement>();

        CountCoreFragmentsInScene();
        if (fragmentsRequiredToFinish > TotalCoreFragmentsInLevel)
        {
            Debug.LogWarning(
                $"GameManager: fragmentsRequiredToFinish ({fragmentsRequiredToFinish}) is greater than core fragments in the scene ({TotalCoreFragmentsInLevel}). The level cannot be completed as configured.");
        }

        RefreshFragmentCounterUI();

        if (useTimer)
        {
            timeRemainingSeconds =
                timerMinutes * 60f
                + Mathf.Clamp(timerSeconds, 0, 59)
                + Mathf.Clamp(timerMilliseconds, 0, 999) / 1000f;
            RefreshTimerUI();
        }

        if (gameOverMenu != null)
            gameOverMenu.SetActive(false);
        if (victoryMenu != null)
            victoryMenu.SetActive(false);

        if (levelExitObject != null)
        {
            resolvedExitCollider = levelExitObject.GetComponent<Collider2D>();
            if (resolvedExitCollider == null)
                Debug.LogWarning("GameManager: Level Exit Object needs a Collider2D on the same GameObject.", levelExitObject);
        }

        if (resolvedExitCollider != null && !resolvedExitCollider.isTrigger)
            Debug.LogWarning(
                "GameManager: level exit collider should have Is Trigger enabled.",
                resolvedExitCollider);
    }

    void FixedUpdate()
    {
        if (resolvedExitCollider == null || IsVictory || IsGameOver)
            return;

        exitOverlapBuffer.Clear();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        int count = resolvedExitCollider.OverlapCollider(filter, exitOverlapBuffer);
        for (int i = 0; i < count; i++)
        {
            Collider2D c = exitOverlapBuffer[i];
            if (c == null || !IsPlayerCollider(c))
                continue;

            TriggerVictory();
            return;
        }
    }

    static bool IsPlayerCollider(Collider2D c)
    {
        if (c.CompareTag("Player"))
            return true;
        return c.GetComponentInParent<PlayerMovement>() != null;
    }

    void Update()
    {
        if (!useTimer || IsGameOver || IsVictory || timerText == null)
            return;

        timeRemainingSeconds -= Time.deltaTime;
        if (timeRemainingSeconds <= 0f)
        {
            timeRemainingSeconds = 0f;
            IsTimeExpired = true;
            RefreshTimerUI();
            TriggerGameOver(timeUpMessage);
            return;
        }
        if(playerController.getDead())
        {
            timeRemainingSeconds = 0f;
            IsTimeExpired = false;
            RefreshTimerUI();
            TriggerGameOver(deathMessage);
            return;
        }
        RefreshTimerUI();
    }

    void CountCoreFragmentsInScene()
    {
        CoreFragment[] fragments = FindObjectsOfType<CoreFragment>();
        TotalCoreFragmentsInLevel = fragments.Length;
    }

    public void RegisterCoreFragmentCollected()
    {
        if (IsGameOver || IsVictory)
            return;

        CoreFragmentsCollected++;
        if (CoreFragmentsCollected > TotalCoreFragmentsInLevel)
            CoreFragmentsCollected = TotalCoreFragmentsInLevel;

        RefreshFragmentCounterUI();
    }

    public void TriggerGameOver()
    {
        TriggerGameOver(defaultGameOverMessage);
    }

    public void TriggerGameOver(string reason)
    {
        if (IsGameOver || IsVictory)
            return;

        PlayPlayerDeathSfx();
        IsGameOver = true;
        Time.timeScale = 0f;

        HideGameplayHud();

        if (gameOverReasonText != null)
            gameOverReasonText.text = string.IsNullOrEmpty(reason) ? defaultGameOverMessage : reason;

        if (gameOverMenu != null)
            gameOverMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        onGameOver?.Invoke();
    }

    public void TriggerVictory()
    {
        if (IsGameOver || IsVictory)
            return;

        SwitchToVictoryMusic();
        IsVictory = true;
        Time.timeScale = 0f;

        HideGameplayHud();

        if (victoryTimeRemainingText != null)
        {
            if (useTimer)
                victoryTimeRemainingText.text = FormatTime(timeRemainingSeconds);
            else
                victoryTimeRemainingText.text = victoryNoTimerLabel;
        }

        if (victoryMenu != null)
            victoryMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        onVictory?.Invoke();
    }

    void SwitchToVictoryMusic()
    {
        if (backgroundMusicSource == null || victoryMusicClip == null)
            return;

        if (backgroundMusicSource.isPlaying)
            backgroundMusicSource.Stop();

        backgroundMusicSource.loop = loopVictoryMusic;
        backgroundMusicSource.clip = victoryMusicClip;
        backgroundMusicSource.Play();
    }

    void PlayPlayerDeathSfx()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<PlayerMovement>();
        if (cachedPlayer != null)
            cachedPlayer.PlayDeathSfx();
    }

    static string FormatTime(float seconds)
    {
        float t = Mathf.Max(0f, seconds);
        int m = (int)(t / 60f);
        float remainder = t - m * 60f;
        int s = Mathf.FloorToInt(remainder);
        int ms = Mathf.Clamp(Mathf.FloorToInt((remainder - s) * 1000f), 0, 999);
        return $"{m}:{s:00}.{ms:000}";
    }

    void HideGameplayHud()
    {
        if (gameplayHudRoot != null)
        {
            gameplayHudRoot.SetActive(false);
            return;
        }

        if (fragmentCounterText != null)
            fragmentCounterText.gameObject.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    void RefreshFragmentCounterUI()
    {
        if (IsGameOver || IsVictory || fragmentCounterText == null)
            return;

        if (TotalCoreFragmentsInLevel <= 0)
        {
            fragmentCounterText.text = "No fragments in this level.";
            return;
        }

        string main = $"{CoreFragmentsCollected} / {TotalCoreFragmentsInLevel} fragments";

        string status;
        if (!HasMetFragmentRequirement)
        {
            int need = Mathf.Max(0, fragmentsRequiredToFinish - CoreFragmentsCollected);
            status = need == 1
                ? "Pick up 1 more to open the exit."
                : $"Pick up {need} more to open the exit.";
        }
        else if (!HasCollectedAll)
        {
            int left = TotalCoreFragmentsInLevel - CoreFragmentsCollected;
            status = left == 1
                ? "Exit is open. 1 left if you want 100%."
                : $"Exit is open. {left} left if you want 100%.";
        }
        else
            status = "Exit is open. You found them all.";

        fragmentCounterText.text = $"{main}\n{status}";
    }

    void RefreshTimerUI()
    {
        if (IsGameOver || IsVictory || timerText == null)
            return;

        if (!useTimer)
        {
            timerText.text = string.Empty;
            return;
        }

        timerText.text = FormatTime(timeRemainingSeconds);
    }
}
