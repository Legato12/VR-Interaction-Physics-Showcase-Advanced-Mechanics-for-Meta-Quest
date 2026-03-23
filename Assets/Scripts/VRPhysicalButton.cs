using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Кнопка для Table_B.
/// Срабатывает двумя способами:
///   1. Луч (hover) — как раньше
///   2. Физическое нажатие — любым объектом с Rigidbody сверху,
///      включая grabbed-предметы, контроллер и меш руки.
///
/// Физика: над кнопкой создаётся тонкий trigger-слой.
/// Когда объект входит в него И у него есть достаточная
/// скорость вниз → кнопка нажимается.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(AudioSource))]
public class VRPhysicalButton : MonoBehaviour
{
    [Header("📢 Logic & Sound")]
    public UnityEvent OnPressed;
    public AudioClip clickSound;

    [Header("🎨 Visual Feedback")]
    [SerializeField] Renderer  buttonRenderer;
    [SerializeField] Transform movingPart;
    [SerializeField] Color normalColor  = Color.cyan;
    [SerializeField] Color pressedColor = Color.white;

    [Header("⚙️ Settings")]
    [SerializeField] float pressDistance = 0.015f;  // глубина хода кнопки
    [SerializeField] float returnSpeed   = 0.1f;    // задержка возврата (сек)

    [Header("🖐️ Physical Press")]
    [Tooltip("Минимальная скорость объекта ВНИЗ для срабатывания (м/с). 0 = любое касание сверху.")]
    [SerializeField] float minPressVelocity = 0.05f;

    [Tooltip("Высота триггер-зоны над кнопкой (м). Обычно 1-3 см.")]
    [SerializeField] float triggerZoneHeight = 0.02f;

    [Tooltip("Небольшой множитель размера триггер-зоны по X/Z (1.0 = точно по размеру кнопки).")]
    [SerializeField] float triggerZoneScale = 0.9f;

    // ── State ─────────────────────────────────────────────────────
    AudioSource           _audio;
    XRSimpleInteractable  _interactable;
    Vector3               _startPos;
    bool                  _isPressed;

    // ── Init ──────────────────────────────────────────────────────
    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _audio        = GetComponent<AudioSource>();

        if (!movingPart)     movingPart     = transform;
        if (!buttonRenderer) buttonRenderer = movingPart.GetComponent<Renderer>();

        _startPos = movingPart.localPosition;
        if (buttonRenderer) buttonRenderer.material.color = normalColor;

        // ── Создать trigger-зону над кнопкой ──────────────────────
        // Размер кнопки в локальных единицах (BoxCollider size=1 × localScale)
        var bc = GetComponent<BoxCollider>();
        var btnScale = transform.localScale;

        // Размер trigg-зоны: ширина/глубина как у кнопки * factor, высота = triggerZoneHeight
        float zoneX = (bc ? bc.size.x : 1f) * btnScale.x * triggerZoneScale;
        float zoneY = triggerZoneHeight;
        float zoneZ = (bc ? bc.size.z : 1f) * btnScale.z * triggerZoneScale;

        // Центр trigger-зоны: чуть выше верхней грани кнопки
        // Верхняя грань в локальных координатах: startPos.y + (colliderSize.y * scale.y)/2
        float btnHalfHeight = (bc ? bc.size.y : 1f) * btnScale.y * 0.5f;
        float trigCenterY   = _startPos.y + btnHalfHeight + zoneY * 0.5f;

        // Создать дочерний GO с trigger-коллайдером
        var trigGO = new GameObject("[PhysBtn] TriggerZone");
        trigGO.transform.SetParent(transform.parent, false); // sibling, не дочерний (scale не наследуется)
        trigGO.transform.localPosition = new Vector3(
            transform.localPosition.x,
            trigCenterY,
            transform.localPosition.z);
        trigGO.transform.localRotation = transform.localRotation;
        trigGO.layer = gameObject.layer;

        var trigBC = trigGO.AddComponent<BoxCollider>();
        trigBC.isTrigger = true;
        trigBC.size      = new Vector3(zoneX, zoneY, zoneZ);

        var det = trigGO.AddComponent<ButtonTriggerDetector>();
        det.button = this;
    }

    void OnEnable()
    {
        _interactable.hoverEntered.AddListener(PressFromRay);
    }

    void OnDisable()
    {
        _interactable.hoverEntered.RemoveListener(PressFromRay);
    }

    // ── Press API (вызывается как из луча, так и из физики) ───────
    public void Press(bool fromPhysics = false)
    {
        if (_isPressed) return;
        _isPressed = true;

        OnPressed?.Invoke();

        if (clickSound) _audio.PlayOneShot(clickSound);

        if (buttonRenderer) buttonRenderer.material.color = pressedColor;

        // Анимация хода кнопки
        movingPart.localPosition = _startPos - Vector3.up * pressDistance;

        Invoke(nameof(Release), returnSpeed);
    }

    void PressFromRay(HoverEnterEventArgs _) => Press(false);

    // Публичный метод для ButtonTriggerDetector
    public void TryPhysicalPress(float downwardVelocity)
    {
        if (downwardVelocity >= minPressVelocity)
            Press(true);
    }

    void Release()
    {
        _isPressed = false;
        if (buttonRenderer) buttonRenderer.material.color = normalColor;
        movingPart.localPosition = _startPos;
    }

    // ── Вспомогательный класс (создаётся в Awake, не нужно ставить вручную) ──
    // Живёт на отдельном GO рядом с кнопкой
    class ButtonTriggerDetector : MonoBehaviour
    {
        public VRPhysicalButton button;

        // Кэш velocity для определения скорости нажатия
        readonly System.Collections.Generic.Dictionary<Collider, Vector3> _prevPos
            = new System.Collections.Generic.Dictionary<Collider, Vector3>();

        void OnTriggerEnter(Collider other)
        {
            // Игнорировать другие кнопки, триггеры XRI, UI
            if (other.isTrigger) return;
            if (other.GetComponent<VRPhysicalButton>()) return;

            // Начать трекинг позиции
            _prevPos[other] = other.transform.position;
        }

        void OnTriggerStay(Collider other)
        {
            if (other.isTrigger) return;
            if (!_prevPos.ContainsKey(other)) { _prevPos[other] = other.transform.position; return; }

            // Скорость объекта (вниз = отрицательный Y в мировых координатах)
            var delta = other.transform.position - _prevPos[other];
            float downward = -delta.y / Time.deltaTime; // положительное = движение вниз

            _prevPos[other] = other.transform.position;

            // Дополнительно: пробуем из Rigidbody (точнее)
            var rb = other.attachedRigidbody;
            if (rb && !rb.isKinematic)
                downward = Mathf.Max(downward, -rb.velocity.y);

            button.TryPhysicalPress(downward);
        }

        void OnTriggerExit(Collider other)
        {
            _prevPos.Remove(other);
        }
    }
}
