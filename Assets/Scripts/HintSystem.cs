using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HintSystem : MonoBehaviour
{
    [Header("References")]
    public GameObject hintOverlay;
    public TextMeshProUGUI hintText;
    public Button showHintButton;
    public Button closeHintButton;
    public CanvasGroup hintCanvasGroup;

    void Start()
    {
        hintOverlay.SetActive(false);

        showHintButton.onClick.AddListener(ShowHint);
        closeHintButton.onClick.AddListener(HideHint);
        GameManager.Instance.onLevelLoaded += UpdateHint;

        UpdateHint();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onLevelLoaded -= UpdateHint;
    }

    void UpdateHint()
    {
        LevelData level = GameManager.Instance.GetCurrentLevel();
        if (hintText != null && level != null)
            hintText.text = level.hint;
    }

    public void ShowHint()
    {
        UpdateHint();
        hintOverlay.SetActive(true);
        AudioManager.Instance?.PlayButton();
        StopAllCoroutines();
        StartCoroutine(FadeCanvas(0f, 1f, 0.3f));
    }

    public void HideHint()
    {
        AudioManager.Instance?.PlayButton();
        StopAllCoroutines();
        StartCoroutine(FadeOutAndHide());
    }

    IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float t = 0f;
        hintCanvasGroup.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            hintCanvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        hintCanvasGroup.alpha = to;
    }

    IEnumerator FadeOutAndHide()
    {
        yield return StartCoroutine(FadeCanvas(1f, 0f, 0.2f));
        hintOverlay.SetActive(false);
    }
}