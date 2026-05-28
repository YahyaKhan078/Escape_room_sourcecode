using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Setup")]
    public LevelDatabase levelDatabase;
    public int currentLevelIndex = 0;

    [Header("State")]
    public bool levelComplete = false;
    public bool levelFailed = false;

    public System.Action onLevelLoaded;
    public System.Action onLevelPassed;
    public System.Action onLevelFailed;
    public System.Action onCircuitChanged;

    void Awake()
    {
        // NO DontDestroyOnLoad — GameManager is scene-specific
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitAfterFrame());
    }

    IEnumerator InitAfterFrame()
    {
        yield return null;
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        currentLevelIndex = index;
        levelComplete = false;
        levelFailed = false;

        GateLogic.Instance.ClearAll();
        onLevelLoaded?.Invoke();

        Debug.Log($"Level loaded: {GetCurrentLevel().levelName}");
    }

    public LevelData GetCurrentLevel()
    {
        return levelDatabase.GetLevel(currentLevelIndex);
    }

    public void NotifyCircuitChanged()
    {
        onCircuitChanged?.Invoke();
    }

    public void TestCircuit()
    {
        LevelData level = GetCurrentLevel();
        if (level == null) return;

        bool allPassed = true;

        for (int r = 0; r < level.truthTable.Count; r++)
        {
            TruthTableRow row = level.truthTable[r];

            if (row.inputs == null ||
                row.inputs.Length < level.inputNames.Length)
            {
                Debug.LogWarning($"Row {r} has missing inputs — skipping");
                continue;
            }

            for (int i = 0; i < level.inputNames.Length; i++)
                GateLogic.Instance.SetInputValue("INPUT_" + i, row.inputs[i]);

            GateLogic.Instance.PropagateAll();

            bool actual = GateLogic.Instance.GetOutput("OUTPUT_0");

            if (actual != row.expectedOutput)
            {
                allPassed = false;
                break;
            }
        }

        if (allPassed)
        {
            levelComplete = true;
            onLevelPassed?.Invoke();
            Debug.Log("LEVEL PASSED!");
        }
        else
        {
            levelFailed = true;
            onLevelFailed?.Invoke();
            Debug.Log("LEVEL FAILED — logic mismatch");
        }
    }

    public void NextLevel()
    {
        int maxLevels = DifficultyManager.Instance != null
                        ? DifficultyManager.Instance.GetMaxLevels()
                        : levelDatabase.TotalLevels;

        if (currentLevelIndex + 1 < maxLevels)
            LoadLevel(currentLevelIndex + 1);
        else
        {
            Debug.Log("All levels complete!");
            UIManager.Instance?.ShowGameComplete();
        }
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }
}