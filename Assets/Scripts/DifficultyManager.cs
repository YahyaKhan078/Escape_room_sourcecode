using UnityEngine;

public enum Difficulty { Easy, Medium, Hard }

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public Difficulty currentDifficulty = Difficulty.Easy;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this);
    }

    public int GetMaxLevels()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return 3;
            case Difficulty.Medium: return 6;
            case Difficulty.Hard: return 10;
            default: return 3;
        }
    }

    public void SetDifficulty(int d)
    {
        currentDifficulty = (Difficulty)d;
        Debug.Log($"Difficulty set to: {currentDifficulty}");
    }
}