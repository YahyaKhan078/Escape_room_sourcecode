using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircuitManager : MonoBehaviour
{
    public static CircuitManager Instance;

    [Header("References")]
    public Transform gateContainer;
    public Transform wireContainer;
    public GameObject gatePrefab;

    [Header("Wire Drawing")]
    public Color wireHighColor = new Color(0f, 0.78f, 1f, 1f);
    public Color wireLowColor = new Color(0.1f, 0.22f, 0.3f, 1f);

    private List<GateUI> spawnedGates = new List<GateUI>();
    private List<WireUI> spawnedWires = new List<WireUI>();
    private GateUI wiringFrom = null;
    private bool isWiring = false;
    private WireUI previewWire = null;

    void Awake() { Instance = this; }

    void Start()
    {
        GameManager.Instance.onLevelLoaded += OnLevelLoaded;
        GameManager.Instance.onCircuitChanged += RefreshAllWires;
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.onLevelLoaded -= OnLevelLoaded;
        GameManager.Instance.onCircuitChanged -= RefreshAllWires;
    }

    // ── Level load ───────────────────────────────────────────
    void OnLevelLoaded()
    {
        ClearAll();
        SpawnIONodes();
    }

    void SpawnIONodes()
    {
        LevelData level = GameManager.Instance.GetCurrentLevel();
        int inputCount = level.inputNames.Length;

        for (int i = 0; i < inputCount; i++)
        {
            float y = GetNodeY(i, inputCount);
            GateUI g = SpawnGate(GateType.INPUT,
                                 new Vector2(-480f, y),
                                 level.inputNames[i]);
            g.gateId = "INPUT_" + i;
            GateLogic.Instance.CreateGateWithId(g.gateId, GateType.INPUT);
        }

        GateUI og = SpawnGate(GateType.OUTPUT,
                              new Vector2(480f, 0f),
                              level.outputName ?? "OUT");
        og.gateId = "OUTPUT_0";
        GateLogic.Instance.CreateGateWithId(og.gateId, GateType.OUTPUT);
    }

    float GetNodeY(int i, int total)
    {
        if (total == 1) return 0f;
        float spacing = 120f;
        float totalH = (total - 1) * spacing;
        return (totalH / 2f) - i * spacing;
    }

    // ── Spawning ─────────────────────────────────────────────
    public GateUI SpawnGate(GateType type, Vector2 pos, string label = "")
    {
        GameObject go = Instantiate(gatePrefab, gateContainer);
        go.GetComponent<RectTransform>().anchoredPosition = pos;

        GateUI gui = go.GetComponent<GateUI>();
        if (label == "") label = type.ToString();
        gui.Init(type, label);

        if (type != GateType.INPUT && type != GateType.OUTPUT)
        {
            Gate g = GateLogic.Instance.CreateGate(type);
            gui.gateId = g.id;
        }

        spawnedGates.Add(gui);
        AudioManager.Instance?.PlayButton();
        GameManager.Instance.NotifyCircuitChanged();
        return gui;
    }

    public void RemoveGate(GateUI gui)
    {
        if (gui.gateType == GateType.INPUT ||
            gui.gateType == GateType.OUTPUT) return;

        spawnedWires.RemoveAll(w =>
        {
            if (w.fromGate == gui || w.toGate == gui)
            {
                GateLogic.Instance.Disconnect(
                    w.fromGate.gateId, w.toGate.gateId);
                Destroy(w.gameObject);
                return true;
            }
            return false;
        });

        GateLogic.Instance.RemoveGate(gui.gateId);
        spawnedGates.Remove(gui);
        Destroy(gui.gameObject);
        AudioManager.Instance?.PlayWireDelete();
        GameManager.Instance.NotifyCircuitChanged();
    }

    // ── Wiring ───────────────────────────────────────────────
    public void StartWiring(GateUI from)
    {
        if (previewWire != null)
        {
            Destroy(previewWire.gameObject);
            previewWire = null;
        }

        wiringFrom = from;
        isWiring = true;

        GameObject wgo = new GameObject("PreviewWire", typeof(RectTransform));
        wgo.transform.SetParent(wireContainer, false);

        // Stretch to fill container
        RectTransform wrt = wgo.GetComponent<RectTransform>();
        wrt.anchorMin = Vector2.zero;
        wrt.anchorMax = Vector2.one;
        wrt.offsetMin = Vector2.zero;
        wrt.offsetMax = Vector2.zero;

        wgo.AddComponent<CanvasRenderer>();
        previewWire = wgo.AddComponent<WireUI>();
        previewWire.Init(from, null);
    }

    public void CompleteWiring(GateUI to)
    {
        if (previewWire != null)
        {
            Destroy(previewWire.gameObject);
            previewWire = null;
        }

        if (!isWiring || wiringFrom == null || to == wiringFrom)
        { CancelWiring(); return; }

        if (to.gateType == GateType.INPUT)
        { CancelWiring(); return; }

        bool exists = spawnedWires.Exists(
            w => w.fromGate == wiringFrom && w.toGate == to);
        if (exists) { CancelWiring(); return; }

        GateLogic.Instance.Connect(wiringFrom.gateId, to.gateId);

        GameObject wgo = new GameObject("Wire", typeof(RectTransform));
        wgo.transform.SetParent(wireContainer, false);

        RectTransform wrt = wgo.GetComponent<RectTransform>();
        wrt.anchorMin = Vector2.zero;
        wrt.anchorMax = Vector2.one;
        wrt.offsetMin = Vector2.zero;
        wrt.offsetMax = Vector2.zero;

        wgo.AddComponent<CanvasRenderer>();
        WireUI wire = wgo.AddComponent<WireUI>();
        wire.Init(wiringFrom, to);
        spawnedWires.Add(wire);
        AudioManager.Instance?.PlayWireConnect();
    }

    public void CancelWiring()
    {
        if (previewWire != null)
        {
            Destroy(previewWire.gameObject);
            previewWire = null;
        }
        wiringFrom = null;
        isWiring = false;
    }

    public bool IsWiring() => isWiring;
    public GateUI WiringFrom() => wiringFrom;

    // ── Escape key cancels wiring ─────────────────────────────
    void Update()
    {
        if (isWiring && Input.GetKeyDown(KeyCode.Escape))
            CancelWiring();

        // Ctrl+Z deletes last wire
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastWire();
        }
    }

    void UndoLastWire()
    {
        if (spawnedWires.Count == 0) return;
        WireUI last = spawnedWires[spawnedWires.Count - 1];
        DeleteWire(last);
        Debug.Log("Wire undone");
    }

    // ── Refresh ──────────────────────────────────────────────
    void RefreshAllWires()
    {
        PropagateInputs();
        GateLogic.Instance.PropagateAll();

        foreach (WireUI w in spawnedWires)
        {
            if (w == null || w.fromGate == null) continue;
            bool sig = GateLogic.Instance.GetOutput(w.fromGate.gateId);
            w.SetSignal(sig);
        }

        foreach (GateUI g in spawnedGates)
        {
            if (g == null) continue;
            bool sig = GateLogic.Instance.GetOutput(g.gateId);
            g.SetSignal(sig);
        }
    }
    public void DeleteWire(WireUI wire)
    {
        if (wire == null) return;
        if (wire.fromGate != null && wire.toGate != null)
            GateLogic.Instance.Disconnect(
                wire.fromGate.gateId, wire.toGate.gateId);

        spawnedWires.Remove(wire);
        Destroy(wire.gameObject);
        AudioManager.Instance?.PlayWireDelete();
        GameManager.Instance.NotifyCircuitChanged();
    }
    void PropagateInputs()
    {
        foreach (GateUI g in spawnedGates)
        {
            if (g.gateType == GateType.INPUT)
                GateLogic.Instance.SetInputValue(g.gateId, g.inputValue);
        }
    }

    public void ToggleInput(GateUI g)
    {
        g.inputValue = !g.inputValue;
        GameManager.Instance.NotifyCircuitChanged();
        AudioManager.Instance?.PlayInputToggle();
    }

    void ClearAll()
    {
        foreach (GateUI g in spawnedGates)
            if (g != null) Destroy(g.gameObject);
        foreach (WireUI w in spawnedWires)
            if (w != null) Destroy(w.gameObject);

        spawnedGates.Clear();
        spawnedWires.Clear();
        GateLogic.Instance.ClearAll();
    }
}