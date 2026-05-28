using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "DLD Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public LevelData GetLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return null;
        return levels[index];
    }

    public int TotalLevels => levels.Count;
}