using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TruthTableRow
{
    public bool[] inputs;
    public bool expectedOutput;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "DLD Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public string roomName;
    public string storyDescription;
    public string hint;

    [Header("Inputs")]
    public string[] inputNames;
    public string outputName = "OUT";

    [Header("Truth Table")]
    public List<TruthTableRow> truthTable = new List<TruthTableRow>();

    [Header("Available Gates")]
    public GateType[] availableGates;

    [Header("Progression")]
    public int levelIndex;
    public int passingScore = 100;
}