using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(LayoutElement))]
public class AutoFitTextHeight : MonoBehaviour
{
    public bool FixWidth;
    public bool FixHeight;
    public Vector2 Offset;

    public enum LayoutFormat
    {
        Preferred,
        Min
    }
    public LayoutFormat layoutFormat = LayoutFormat.Preferred;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private LayoutElement   m_Layout;
    
    private RectTransform     m_Rect;

    // track last written values so we only mark dirty on real changes
    private float m_LastWidth  = -1f;
    private float m_LastHeight = -1f;

    private TextMeshProUGUI Text
    {
        get
        {
            if (m_Text == null)
                m_Text = GetComponentInChildren<TextMeshProUGUI>(true);
            return m_Text;
        }
    }

    private void Awake()
    {
        m_Rect   = GetComponent<RectTransform>();
        m_Layout = GetComponent<LayoutElement>();

        if (m_Layout == null)
        {
            m_Layout = gameObject.AddComponent<LayoutElement>();
        }
    }

    private void OnEnable()  => UpdateSizes();
    private void Start()     => UpdateSizes();
    private void Update()    => UpdateSizes();

    private void UpdateSizes()
    {
        if (Text == null) return;

        // Force mesh rebuild so GetRenderedValues is accurate
        Text.ForceMeshUpdate();
        Vector2 rendered = Text.GetRenderedValues(false);
        
        if (FixWidth)
            ApplySize(rendered.x + Offset.x, Axis.Horizontal);

        if (FixHeight)
            ApplySize(rendered.y + Offset.y, Axis.Vertical);
    }

    private enum Axis { Horizontal, Vertical }

    private void ApplySize(float rawSize, Axis axis)
    {
        bool   isHorizontal = axis == Axis.Horizontal;
        float  lastValue    = isHorizontal ? m_LastWidth : m_LastHeight;
        float  newValue     = rawSize;
        // pick which LayoutElement property to set
        if (layoutFormat == LayoutFormat.Preferred)
        {
            if (isHorizontal) m_Layout.preferredWidth  = newValue;
            else              m_Layout.preferredHeight = newValue;
        }
        else // Min
        {
            if (isHorizontal) m_Layout.minWidth  = newValue;
            else              m_Layout.minHeight = newValue;
        }

        // cache & only rebuild if changed
        if (!Mathf.Approximately(newValue, lastValue))
        {
            if (isHorizontal) m_LastWidth  = newValue;
            else              m_LastHeight = newValue;

            LayoutRebuilder.MarkLayoutForRebuild(m_Rect);
        }
    }

#if UNITY_EDITOR
    private void OnValidate() => UpdateSizes();
#endif
}

