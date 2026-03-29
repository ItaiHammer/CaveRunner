using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Two lines: (1) collected / total in level, (2) plain hint for exit vs optional 100%.")]
    public TMP_Text fragmentCounterText;

    [Header("Timer")]
    [Tooltip("Assign a TextMeshPro label for the countdown.")]
    public TMP_Text timerText;
    [Tooltip("If off, timer logic and UI updates are skipped.")]
    public bool useTimer = true;
    [Min(0)]
    public int timerMinutes;
    [Range(0, 59)]
    public int timerSeconds;
    [Range(0, 999)]
    public int timerMilliseconds;
    [Tooltip("Fired once when the countdown hits zero.")]
    public UnityEvent onTimeExpired;

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

    float timeRemainingSeconds;

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
    }

    void Update()
    {
        if (!useTimer || IsTimeExpired || timerText == null)
            return;

        timeRemainingSeconds -= Time.deltaTime;
        if (timeRemainingSeconds <= 0f)
        {
            timeRemainingSeconds = 0f;
            IsTimeExpired = true;
            RefreshTimerUI();
            onTimeExpired?.Invoke();
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
        CoreFragmentsCollected++;
        if (CoreFragmentsCollected > TotalCoreFragmentsInLevel)
            CoreFragmentsCollected = TotalCoreFragmentsInLevel;

        RefreshFragmentCounterUI();
    }

    void RefreshFragmentCounterUI()
    {
        if (fragmentCounterText == null)
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
        if (timerText == null)
            return;

        if (!useTimer)
        {
            timerText.text = string.Empty;
            return;
        }

        float t = Mathf.Max(0f, timeRemainingSeconds);
        int m = (int)(t / 60f);
        float remainder = t - m * 60f;
        int s = Mathf.FloorToInt(remainder);
        int ms = Mathf.Clamp(Mathf.FloorToInt((remainder - s) * 1000f), 0, 999);
        timerText.text = $"{m}:{s:00}.{ms:000}";
    }
}
