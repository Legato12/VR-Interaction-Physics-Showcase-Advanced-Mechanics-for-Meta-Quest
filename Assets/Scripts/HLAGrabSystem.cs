using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Half-Life: Alyx стиль для правой руки.
///
/// ПОМЕСТИТЬ НА: Right Controller
///
/// ── Фазы ──────────────────────────────────────────────────────
/// FAR HOLD:  Схватил лучом → объект следует куда ты ЦЕЛИШЬ,
///            удерживаясь на текущем расстоянии.
///            Правый стик Y: приблизить / отдалить.
///            SnapTurn выключен.
///
/// FLICK:     Резкий рывок кисти к себе → объект летит к руке.
///
/// NEAR GRAB: Объект доплыл к руке → переход в обычный ближний граб.
///            Можно бросить, передать, и т.д.
///            SnapTurn включается обратно.
/// </summary>
public class HLAGrabSystem : MonoBehaviour
{
    // ── References ─────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("NearFarInteractor на RightRay")]
    [SerializeField] XRBaseInteractor nearFarInteractor;

    [Tooltip("SnapTurnProvider на XR Origin")]
    [SerializeField] ActionBasedSnapTurnProvider snapTurnProvider;

    [Tooltip("Трансформ контроллера (Right Controller) для velocity tracking")]
    [SerializeField] Transform controllerTransform;

    // ── Far Hold ────────────────────────────────────────────────────
    [Header("Far Hold (объект следует за лучом)")]
    [Tooltip("Скорость с которой объект следует за движением луча (выше = отзывчивее)")]
    [Range(1f, 30f)]
    [SerializeField] float objectFollowSpeed = 8f;

    [Tooltip("Минимальная дистанция удержания (м)")]
    [SerializeField] float minHoldDistance = 0.5f;

    [Tooltip("Максимальная дистанция удержания (м)")]
    [SerializeField] float maxHoldDistance = 8f;

    [Tooltip("Скорость изменения дистанции стиком Y (м/с)")]
    [SerializeField] float distanceChangeSpeed = 2f;

    // ── Flick ───────────────────────────────────────────────────────
    [Header("Flick (рывок кисти к себе)")]
    [Tooltip("Минимальная скорость кисти к голове для срабатывания (м/с)")]
    [SerializeField] float flickThreshold = 1.8f;

    [Tooltip("Буфер кадров для определения жеста")]
    [SerializeField] int flickWindow = 8;

    [Tooltip("Кулдаун flick после срабатывания (сек)")]
    [SerializeField] float flickCooldown = 0.5f;

    // ── Pull (полёт к руке) ─────────────────────────────────────────
    [Header("Pull (полёт объекта к руке)")]
    [Tooltip("Скорость полёта объекта к руке (м/с). 5-6 = HL:A-like")]
    [SerializeField] float pullSpeed = 5.5f;

    [Tooltip("Расстояние для перехода в near-grab (м)")]
    [SerializeField] float arrivalDistance = 0.15f;

    // ── State ───────────────────────────────────────────────────────
    enum GrabPhase { None, FarHold, Pulling, NearGrab }
    GrabPhase _phase = GrabPhase.None;

    Transform _hoverPoint;          // виртуальная точка — XRI тянет объект к ней
    Transform _originalAttach;      // оригинальный attachTransform интерактора
    Transform _interactorTransform; // трансформ NearFarInteractor'а
    Rigidbody _heldRb;
    XRGrabInteractable _heldGrab;
    float _holdDistance;            // текущая дистанция удержания
    float _flickCooldownTimer;

    // Velocity buffer
    Vector3[] _velBuf;
    int _velIdx;
    Vector3 _prevCtrlPos;

    // ── Init ────────────────────────────────────────────────────────
    void Awake()
    {
        _velBuf = new Vector3[flickWindow];

        if (!nearFarInteractor)
            nearFarInteractor = GetComponentInChildren<XRBaseInteractor>()
                             ?? GetComponent<XRBaseInteractor>();

        if (!controllerTransform)
            controllerTransform = transform;

        // HoverPoint живёт в мировом пространстве (не дочерний никому)
        var hp = new GameObject("[HLA] HoverPoint");
        hp.transform.SetParent(null);
        _hoverPoint = hp.transform;

        if (!snapTurnProvider)
            snapTurnProvider = FindObjectOfType<ActionBasedSnapTurnProvider>();
    }

    void OnEnable()
    {
        if (nearFarInteractor)
        {
            nearFarInteractor.selectEntered.AddListener(OnSelectEntered);
            nearFarInteractor.selectExited.AddListener(OnSelectExited);
        }
    }

    void OnDisable()
    {
        if (nearFarInteractor)
        {
            nearFarInteractor.selectEntered.RemoveListener(OnSelectEntered);
            nearFarInteractor.selectExited.RemoveListener(OnSelectExited);
        }
        CleanUp();
    }

    // ── Grab events ─────────────────────────────────────────────────
    void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Работаем только с Rigidbody объектами
        _heldGrab = args.interactableObject as XRGrabInteractable;
        if (!_heldGrab) return;
        _heldRb = _heldGrab.GetComponent<Rigidbody>();
        if (!_heldRb) return;

        _interactorTransform = nearFarInteractor.transform;

        // Запомнить дистанцию от контроллера до объекта
        _holdDistance = Vector3.Distance(_interactorTransform.position, _heldRb.position);
        _holdDistance = Mathf.Clamp(_holdDistance, minHoldDistance, maxHoldDistance);

        // HoverPoint — стартует на объекте
        _hoverPoint.position = _heldRb.position;
        _hoverPoint.rotation = _heldRb.rotation;

        // Подменить attachTransform → объект будет тянуться к hoverPoint
        _originalAttach = nearFarInteractor.attachTransform;
        nearFarInteractor.attachTransform = _hoverPoint;

        // Отключить ForceGrab — хотим плавное следование, не телепорт
        SetForceGrab(false);

        _phase = GrabPhase.FarHold;
        _flickCooldownTimer = 0f;

        if (snapTurnProvider) snapTurnProvider.enabled = false;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        // Если выходим не из NearGrab — чистим
        CleanUp();
    }

    void CleanUp()
    {
        if (nearFarInteractor && _originalAttach != null)
            nearFarInteractor.attachTransform = _originalAttach;

        SetForceGrab(true);

        _heldRb   = null;
        _heldGrab = null;
        _originalAttach = null;
        _interactorTransform = null;
        _phase = GrabPhase.None;

        if (snapTurnProvider) snapTurnProvider.enabled = true;
    }

    // ── Update ───────────────────────────────────────────────────────
    void Update()
    {
        // Всегда трекаем velocity контроллера (для flick)
        if (controllerTransform)
        {
            var vel = (controllerTransform.position - _prevCtrlPos) / Time.deltaTime;
            _velBuf[_velIdx % flickWindow] = vel;
            _velIdx++;
            _prevCtrlPos = controllerTransform.position;
        }

        if (_flickCooldownTimer > 0f) _flickCooldownTimer -= Time.deltaTime;

        if (_phase == GrabPhase.None || !_heldRb) return;

        switch (_phase)
        {
            case GrabPhase.FarHold:
                UpdateFarHold();
                break;

            case GrabPhase.Pulling:
                UpdatePulling();
                break;

            case GrabPhase.NearGrab:
                // В ближнем грабе мы не вмешиваемся — NearFarInteractor управляет сам
                break;
        }
    }

    // ── FAR HOLD ────────────────────────────────────────────────────
    void UpdateFarHold()
    {
        // ── Стик Y: изменить дистанцию ──
        float stickY = GetRightStick().y;
        if (Mathf.Abs(stickY) > 0.2f)
            _holdDistance = Mathf.Clamp(
                _holdDistance + stickY * distanceChangeSpeed * Time.deltaTime,
                minHoldDistance, maxHoldDistance);

        // ── HoverPoint следует за лучом ──
        // Целевая позиция = позиция интерактора + его forward × дистанция
        Vector3 targetPos = _interactorTransform.position
                          + _interactorTransform.forward * _holdDistance;

        _hoverPoint.position = Vector3.Lerp(
            _hoverPoint.position, targetPos, objectFollowSpeed * Time.deltaTime);

        // ── Flick detection ──
        if (_flickCooldownTimer <= 0f && DetectFlick())
            BeginPull();
    }

    // ── PULL ─────────────────────────────────────────────────────────
    void UpdatePulling()
    {
        // Цель — позиция интерактора (рука)
        Vector3 handPos = _interactorTransform.position;

        // Двигаем hoverPoint к руке
        _hoverPoint.position = Vector3.MoveTowards(
            _hoverPoint.position, handPos, pullSpeed * Time.deltaTime);

        // Объект прилетел — переходим в Near Grab
        if (Vector3.Distance(_hoverPoint.position, handPos) < arrivalDistance)
            TransitionToNearGrab();
    }

    // ── Transitions ───────────────────────────────────────────────────
    void BeginPull()
    {
        _phase = GrabPhase.Pulling;
        _flickCooldownTimer = flickCooldown;

        // Начальный импульс чтобы объект "дёрнулся"
        if (_heldRb)
        {
            var dir = (_interactorTransform.position - _heldRb.position).normalized;
            _heldRb.velocity = dir * pullSpeed;
        }
    }

    void TransitionToNearGrab()
    {
        _phase = GrabPhase.NearGrab;

        // Восстанавливаем оригинальный attach (обычно null = позиция интерактора)
        // NearFarInteractor теперь тянет объект к руке напрямую
        nearFarInteractor.attachTransform = _originalAttach;

        // Включаем ForceGrab — объект снэпается к руке мгновенно
        SetForceGrab(true);

        // Включаем SnapTurn — правый стик освобождается для поворота
        if (snapTurnProvider) snapTurnProvider.enabled = true;

        // Обнуляем velocity чтобы объект не улетел при снэпе
        if (_heldRb)
        {
            _heldRb.velocity        = Vector3.zero;
            _heldRb.angularVelocity = Vector3.zero;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────
    bool DetectFlick()
    {
        var cam = Camera.main;
        if (!cam) return false;

        var toHead = (cam.transform.position - controllerTransform.position).normalized;
        float maxProj = 0f;
        foreach (var v in _velBuf)
        {
            float p = Vector3.Dot(v, toHead);
            if (p > maxProj) maxProj = p;
        }
        return maxProj > flickThreshold;
    }

    Vector2 GetRightStick()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
            UnityEngine.XR.InputDeviceCharacteristics.Right |
            UnityEngine.XR.InputDeviceCharacteristics.Controller, devices);

        foreach (var dev in devices)
            if (dev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out var axis))
                return axis;
        return Vector2.zero;
    }

    void SetForceGrab(bool value)
    {
        if (nearFarInteractor == null) return;
        var f = nearFarInteractor.GetType().GetField(
            "m_UseForceGrab",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        f?.SetValue(nearFarInteractor, value);
    }
}
