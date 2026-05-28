using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WireUI : MonoBehaviour, IPointerClickHandler
{
    public GateUI fromGate;
    public GateUI toGate;

    private Canvas rootCanvas;
    private UILineRenderer line;

    public Color highColor = new Color(0f, 0.78f, 1f, 1f);
    public Color lowColor = new Color(0.1f, 0.22f, 0.3f, 1f);

    public void Init(GateUI from, GateUI to)
    {
        fromGate = from;
        toGate = to;

        // Get canvas from parent hierarchy
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null && from != null)
            rootCanvas = from.GetComponentInParent<Canvas>();

        GameObject lineGo = new GameObject("UILine", typeof(CanvasRenderer));
        lineGo.transform.SetParent(transform, false);

        // Make sure RectTransform fills canvas
        RectTransform lrt = lineGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        line = lineGo.AddComponent<UILineRenderer>();
        line.color = lowColor;
        line.thickness = 4f;
        line.raycastTarget = false;
    }

    public void SetSignal(bool value)
    {
        if (line != null)
            line.color = value ? highColor : lowColor;
    }

    void Update()
    {
        if (line == null || fromGate == null || rootCanvas == null) return;

        Vector2 start = ScreenToCanvas(fromGate.GetOutputPinWorld());
        Vector2 end = toGate != null
                        ? ScreenToCanvas(toGate.GetInputPinWorld())
                        : ScreenToCanvas(Input.mousePosition);

        line.SetPoints(start, end);
    }

    // For Screen Space Overlay: RectTransform.position IS screen pos
    Vector2 ScreenToCanvas(Vector3 screenPos)
    {
        // Convert to the WIRE CONTAINER's local space, not canvas space
        // because UILineRenderer draws relative to its parent (wireContainer)
        RectTransform containerRect = transform.parent as RectTransform;
        if (containerRect == null)
            containerRect = rootCanvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect,
            new Vector2(screenPos.x, screenPos.y),
            null,
            out Vector2 local);
        return local;
    }

    // Click wire to delete it
    public void OnPointerClick(PointerEventData e)
    {
        if (toGate != null) // don't delete preview wire
            CircuitManager.Instance.DeleteWire(this);
    }
}