using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GateUI : MonoBehaviour, IDragHandler, IBeginDragHandler,
                      IEndDragHandler, IPointerClickHandler
{
    [Header("References")]
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI symbolText;
    public Image background;
    public Button deleteButton;
    public RectTransform outputPin;
    public RectTransform inputPin;

    [Header("Colors")]
    public Color activeColor = new Color(0f, 0.78f, 1f, 0.25f);
    public Color inactiveColor = new Color(0.05f, 0.12f, 0.2f, 1f);
    public Color inputOnColor = new Color(0f, 0.72f, 0.42f, 0.3f);

    [HideInInspector] public string gateId;
    [HideInInspector] public GateType gateType;
    [HideInInspector] public bool inputValue = false;

    private RectTransform rt;
    private Canvas rootCanvas;

    // ── Init ─────────────────────────────────────────────────
    public void Init(GateType type, string label)
    {
        gateType = type;
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (labelText != null) labelText.text = label;
        if (symbolText != null) symbolText.text = GetSymbol(type);

        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(
                type != GateType.INPUT && type != GateType.OUTPUT);
            deleteButton.onClick.AddListener(
                () => CircuitManager.Instance.RemoveGate(this));
        }

        SetSignal(false);
        // Make NOT symbol fit better
        if (type == GateType.NOT && symbolText != null)
            symbolText.fontSize = 14;
    }

    // ── Signal visual ─────────────────────────────────────────
    public void SetSignal(bool on)
    {
        if (background == null) return;

        if (gateType == GateType.INPUT)
            background.color = inputValue
                ? new Color(0f, 0.45f, 0.25f, 0.9f)
                : new Color(0.05f, 0.12f, 0.25f, 0.9f);
        else
            background.color = on
                ? new Color(0f, 0.35f, 0.55f, 0.9f)
                : new Color(0.05f, 0.12f, 0.25f, 0.9f);

        if (labelText != null) labelText.color = Color.white;
        if (symbolText != null) symbolText.color = Color.white;
    }

    // ── Drag ──────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData e)
    {
        if (CircuitManager.Instance.IsWiring()) return;
    }

    public void OnDrag(PointerEventData e)
    {
        if (CircuitManager.Instance.IsWiring()) return;
        if (gateType == GateType.INPUT || gateType == GateType.OUTPUT) return;
        if (rootCanvas == null) return;
        rt.anchoredPosition += e.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData e) { }

    // ── Click ─────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData e)
    {
        if (gateType == GateType.INPUT)
        {
            CircuitManager.Instance.ToggleInput(this);
            return;
        }

        if (CircuitManager.Instance.IsWiring())
        {
            CircuitManager.Instance.CompleteWiring(this);
            return;
        }
    }

    // ── Output pin click ──────────────────────────────────────
    public void OnOutputPinClick()
    {
        if (gateType == GateType.OUTPUT) return;
        CircuitManager.Instance.StartWiring(this);
    }

    // ── Pin world positions ───────────────────────────────────
    public Vector3 GetOutputPinWorld()
    {
        // For Screen Space Overlay, .position IS screen position
        if (outputPin != null) return outputPin.position;
        // Fallback: right edge of gate
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        return (corners[2] + corners[3]) * 0.5f;
    }

    public Vector3 GetInputPinWorld()
    {
        if (inputPin != null) return inputPin.position;
        // Fallback: left edge of gate
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        return (corners[0] + corners[1]) * 0.5f;
    }
    // ── Helpers ───────────────────────────────────────────────
    string GetSymbol(GateType t)
    {
        switch (t)
        {
            case GateType.AND: return "&";
            case GateType.OR: return "≥1";
            case GateType.NOT: return "!A"; // cleaner than ¬
            case GateType.XOR: return "=1";
            case GateType.NAND: return "↑";
            case GateType.INPUT: return "IN";
            case GateType.OUTPUT: return "OUT";
            default: return "?";
        }
    }
}