using System.Collections.Generic;
using UnityEngine;

public enum GateType { AND, OR, NOT, XOR, NAND, INPUT, OUTPUT }

[System.Serializable]
public class Gate
{
    public string id;
    public GateType type;
    public List<string> inputIds = new List<string>();
    public bool outputValue = false;

    public Gate(string id, GateType type)
    {
        this.id = id;
        this.type = type;
    }

    public bool Evaluate(List<bool> inputs)
    {
        switch (type)
        {
            case GateType.AND: return inputs.Count >= 2 && inputs[0] && inputs[1];
            case GateType.OR: return inputs.Count >= 2 && (inputs[0] || inputs[1]);
            case GateType.NOT: return inputs.Count >= 1 && !inputs[0];
            case GateType.XOR: return inputs.Count >= 2 && (inputs[0] != inputs[1]);
            case GateType.NAND: return inputs.Count >= 2 && !(inputs[0] && inputs[1]);
            case GateType.INPUT: return inputs.Count >= 1 && inputs[0];
            case GateType.OUTPUT: return inputs.Count >= 1 && inputs[0];
            default: return false;
        }
    }
}

public class GateLogic : MonoBehaviour
{
    public static GateLogic Instance;

    private Dictionary<string, Gate> gates = new Dictionary<string, Gate>();
    private int idCounter = 0;

    void Awake()
    {
        Instance = this;
    }

    public Gate CreateGate(GateType type)
    {
        string id = type.ToString() + "_" + idCounter++;
        Gate g = new Gate(id, type);
        gates[id] = g;
        return g;
    }
    public Gate CreateGateWithId(string id, GateType type)
    {
        Gate g = new Gate(id, type);
        gates[id] = g;
        return g;
    }
    public void Connect(string fromId, string toId)
    {
        if (!gates.ContainsKey(toId)) return;
        Gate target = gates[toId];
        if (!target.inputIds.Contains(fromId))
            target.inputIds.Add(fromId);
    }

    public void Disconnect(string fromId, string toId)
    {
        if (!gates.ContainsKey(toId)) return;
        gates[toId].inputIds.Remove(fromId);
    }

    public void RemoveGate(string id)
    {
        if (!gates.ContainsKey(id)) return;
        gates.Remove(id);
        foreach (var g in gates.Values)
            g.inputIds.Remove(id);
    }

    public void SetInputValue(string id, bool value)
    {
        if (!gates.ContainsKey(id)) return;
        gates[id].outputValue = value;
    }

    public void PropagateAll()
    {
        // Multiple passes to handle chain reactions
        for (int pass = 0; pass < 10; pass++)
        {
            foreach (var gate in gates.Values)
            {
                if (gate.type == GateType.INPUT) continue;

                List<bool> inputs = new List<bool>();
                foreach (string inputId in gate.inputIds)
                {
                    if (gates.ContainsKey(inputId))
                        inputs.Add(gates[inputId].outputValue);
                }
                gate.outputValue = gate.Evaluate(inputs);
            }
        }
    }

    public bool GetOutput(string id)
    {
        if (!gates.ContainsKey(id)) return false;
        return gates[id].outputValue;
    }

    public Gate GetGate(string id)
    {
        if (!gates.ContainsKey(id)) return null;
        return gates[id];
    }

    public void ClearAll()
    {
        gates.Clear();
        idCounter = 0;
    }
}