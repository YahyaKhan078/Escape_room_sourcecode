using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Level Info")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI storyText;

    [Header("Truth Table")]
    public Transform truthTableContainer;
    public GameObject truthTableRowPrefab;

    [Header("Top Bar")]
    public TextMeshProUGUI topBarLevelText;
    public Button testButton;
    public Button resetButton;

    [Header("Result Popup")]
    public GameObject resultPopup;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultSubText;
    public Button nextLevelButton;
    public Button retryButton;

    [Header("Gate Palette")]
    public Transform paletteContainer;
    public GameObject paletteButtonPrefab;

    void Awake() { Instance = this; }

    void Start()
    {
        GameManager.Instance.onLevelLoaded += OnLevelLoaded;
        GameManager.Instance.onLevelPassed += OnLevelPassed;
        GameManager.Instance.onLevelFailed += OnLevelFailed;
        GameManager.Instance.onCircuitChanged += RefreshTruthTable;

        testButton.onClick.AddListener(() => GameManager.Instance.TestCircuit());
        resetButton.onClick.AddListener(() => GameManager.Instance.RestartLevel());

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(() =>
            {
                HidePopup();
                GameManager.Instance.NextLevel();
            });

        if (retryButton != null)
            retryButton.onClick.AddListener(() =>
            {
                HidePopup();
                GameManager.Instance.RestartLevel();
            });

        resultPopup.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.onLevelLoaded -= OnLevelLoaded;
        GameManager.Instance.onLevelPassed -= OnLevelPassed;
        GameManager.Instance.onLevelFailed -= OnLevelFailed;
        GameManager.Instance.onCircuitChanged -= RefreshTruthTable;
    }

    // ── Level Loaded ─────────────────────────────────────────
    void OnLevelLoaded()
    {
        HidePopup(); // ADD THIS as first line
        LevelData level = GameManager.Instance.GetCurrentLevel();

        if (levelNameText != null) levelNameText.text = level.levelName;
        if (roomNameText != null) roomNameText.text = level.roomName;
        if (storyText != null) storyText.text = level.storyDescription;
        if (topBarLevelText != null)
            topBarLevelText.text = $"LEVEL {level.levelIndex + 1}  —  " +
                                   level.levelName.ToUpper();

        BuildTruthTable(level);
        BuildPalette(level);
        HidePopup();
    }

    // ── Truth Table ───────────────────────────────────────────
    void BuildTruthTable(LevelData level)
    {
        foreach (Transform child in truthTableContainer)
            Destroy(child.gameObject);

        // Header row
        GameObject header = Instantiate(truthTableRowPrefab, truthTableContainer);
        SetRowTexts(header, level, null, true);

        // Data rows
        foreach (TruthTableRow row in level.truthTable)
        {
            GameObject rowGo = Instantiate(truthTableRowPrefab, truthTableContainer);
            SetRowTexts(rowGo, level, row, false);
        }
    }

    void SetRowTexts(GameObject rowGo, LevelData level,
                     TruthTableRow row, bool isHeader)
    {
        TextMeshProUGUI[] texts = rowGo.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length == 0) return;

        int inputCount = level.inputNames.Length;
        // inputs + output + result = inputCount + 2 cells
        List<string> cells = new List<string>();

        if (isHeader)
        {
            foreach (string n in level.inputNames) cells.Add(n);
            cells.Add(level.outputName ?? "OUT");
            cells.Add("OK");
        }
        else
        {
            for (int i = 0; i < row.inputs.Length; i++)
                cells.Add(row.inputs[i] ? "1" : "0");
            cells.Add(row.expectedOutput ? "1" : "0");
            cells.Add("?");  // filled in by RefreshTruthTable
        }

        for (int i = 0; i < texts.Length && i < cells.Count; i++)
            texts[i].text = cells[i];
    }

    public void RefreshTruthTable()
    {
        if (truthTableContainer == null) return;
        LevelData level = GameManager.Instance.GetCurrentLevel();
        if (level == null) return;

        // Save current input states
        bool[] savedInputs = new bool[level.inputNames.Length];
        for (int i = 0; i < level.inputNames.Length; i++)
            savedInputs[i] = GateLogic.Instance.GetOutput("INPUT_" + i);

        int rowIndex = 0;
        int dataRowIndex = 0;

        foreach (Transform child in truthTableContainer)
        {
            if (rowIndex == 0) { rowIndex++; continue; }
            if (dataRowIndex >= level.truthTable.Count) break;

            TruthTableRow row = level.truthTable[dataRowIndex];

            for (int i = 0; i < level.inputNames.Length; i++)
                GateLogic.Instance.SetInputValue("INPUT_" + i, row.inputs[i]);

            GateLogic.Instance.PropagateAll();
            bool actual = GateLogic.Instance.GetOutput("OUTPUT_0");
            bool correct = actual == row.expectedOutput;

            TextMeshProUGUI[] texts =
                child.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length > 0)
            {
                TextMeshProUGUI okCell = texts[texts.Length - 1];
                okCell.text = correct ? "OK" : "X";
                okCell.color = correct
                    ? new Color(0f, 0.85f, 0.4f)
                    : new Color(0.9f, 0.2f, 0.2f);
            }

            rowIndex++;
            dataRowIndex++;
        }

        // Restore actual input states
        for (int i = 0; i < level.inputNames.Length; i++)
            GateLogic.Instance.SetInputValue("INPUT_" + i, savedInputs[i]);

        GateLogic.Instance.PropagateAll();
    }

    // ── Gate Palette ──────────────────────────────────────────
    void BuildPalette(LevelData level)
    {
        foreach (Transform child in paletteContainer)
            Destroy(child.gameObject);

        foreach (GateType type in level.availableGates)
        {
            if (type == GateType.INPUT || type == GateType.OUTPUT) continue;

            GameObject btn = Instantiate(paletteButtonPrefab, paletteContainer);
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = "+ " + type.ToString();
                txt.fontStyle = FontStyles.Bold;
            }

            // Color each gate type differently
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                switch (type)
                {
                    case GateType.AND:
                        btnImg.color = new Color(0.1f, 0.3f, 0.6f, 0.9f);
                        break;
                    case GateType.OR:
                        btnImg.color = new Color(0.3f, 0.1f, 0.6f, 0.9f);
                        break;
                    case GateType.NOT:
                        btnImg.color = new Color(0.6f, 0.1f, 0.1f, 0.9f);
                        break;
                    case GateType.XOR:
                        btnImg.color = new Color(0.6f, 0.4f, 0.0f, 0.9f);
                        break;
                    case GateType.NAND:
                        btnImg.color = new Color(0.1f, 0.5f, 0.3f, 0.9f);
                        break;
                }
            }

            GateType captured = type;
            btn.GetComponent<Button>().onClick.AddListener(() =>
                CircuitManager.Instance.SpawnGate(captured,
                    new Vector2(Random.Range(-150f, 150f),
                                Random.Range(-80f, 80f))));
        }
    }

    // ── Popups ────────────────────────────────────────────────
    void OnLevelPassed()
    {
        resultPopup.SetActive(true);
        resultTitleText.text = "ACCESS GRANTED";
        resultTitleText.color = new Color(0f, 0.85f, 0.4f);

        int current = GameManager.Instance.currentLevelIndex;
        int total = GameManager.Instance.levelDatabase.TotalLevels;
        bool hasNext = current < total - 1;

        nextLevelButton.gameObject.SetActive(hasNext);
        retryButton.gameObject.SetActive(false);

        AudioManager.Instance?.PlayLevelPass();

        if (hasNext)
            resultSubText.text = "Circuit verified. Door unlocked.";
        else
            resultSubText.text = "All levels complete! You escaped!";
    }

    void OnLevelFailed()
    {
        resultPopup.SetActive(true);
        resultTitleText.text = "LOGIC ERROR";
        resultTitleText.color = new Color(0.9f, 0.2f, 0.2f);
        resultSubText.text = "Circuit mismatch. Check failing rows.";

        nextLevelButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        AudioManager.Instance?.PlayLevelFail();
    }

    void HidePopup() => resultPopup.SetActive(false);
    public void ShowGameComplete()
    {
        resultPopup.SetActive(true);
        resultTitleText.text = "ESCAPED!";
        resultTitleText.color = new Color(0f, 1f, 0.5f);
        resultSubText.text = "All levels cleared. You are a DLD master!";
        nextLevelButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }
}

