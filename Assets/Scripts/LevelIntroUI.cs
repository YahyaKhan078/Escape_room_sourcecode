using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelIntroUI : MonoBehaviour
{
    public static LevelIntroUI Instance;

    [Header("References")]
    public GameObject introPanel;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI levelNameText;
    public CanvasGroup canvasGroup;

    void Awake() { Instance = this; }

    void Start()
    {
        GameManager.Instance.onLevelLoaded += OnLevelLoaded;
        introPanel.SetActive(false);
    }

    void OnLevelLoaded()
    {
        LevelData level = GameManager.Instance.GetCurrentLevel();
        StartCoroutine(PlayIntro(level));
    }

    IEnumerator PlayIntro(LevelData level)
    {
        // Setup text
        levelNumberText.text = $"LEVEL {level.levelIndex + 1}";
        levelNameText.text = level.levelName.ToUpper();

        introPanel.SetActive(true);
        canvasGroup.alpha = 0f;

        // Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(1.5f);

        // Slide up and fade out
        RectTransform rt = introPanel.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            canvasGroup.alpha = 1f - t;
            rt.anchoredPosition = Vector2.Lerp(
                startPos,
                startPos + new Vector2(0, 200f),
                t);
            yield return null;
        }

        introPanel.SetActive(false);
        rt.anchoredPosition = startPos;
    }
}