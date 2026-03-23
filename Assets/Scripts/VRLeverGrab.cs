using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Правильный граб для рычага с HingeJoint.
///
/// XRGrabInteractable не работает с HingeJoint — он пытается
/// переместить объект к руке, что конфликтует с джойнтом.
///
/// Этот скрипт:
/// 1. Использует XRSimpleInteractable (hover + select без движения)
/// 2. Пока триггер зажат — вычисляет дельту движения контроллера
///    и применяет AddTorque на рычаг вдоль оси шарнира
/// 3. HingeJoint полностью контролирует физику — никакого конфликта
///
/// УСТАНОВКА: добавь на LeverArm вместо XRGrabInteractable
/// (XRGrabInteractable удали если есть)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class VRLeverGrab : MonoBehaviour
{
    [Header("Grab Feel")]
    [Tooltip("Сила с которой рука крутит рычаг")]
    [SerializeField] float torqueMultiplier = 12f;
    [Tooltip("Насколько движение руки вдоль оси шарнира влияет на вращение")]
    [SerializeField] float lateralSensitivity = 8f;
    [Tooltip("Максимальный торк за кадр (защита от взрыва)")]
    [SerializeField] float maxTorque = 25f;

    // ── State ────────────────────────────────────────────────────────
    Rigidbody           _rb;
    HingeJoint          _hinge;
    XRSimpleInteractable _interactable;
    XRHoverGlow         _glow;

    // Текущий хват
    IXRSelectInteractor  _grabInteractor;
    Vector3              _prevControllerPos;
    bool                 _isGrabbed;

    void Awake()
    {
        _rb           = GetComponent<Rigidbody>();
        _hinge        = GetComponent<HingeJoint>();
        _interactable = GetComponent<XRSimpleInteractable>();
        _glow         = GetComponent<XRHoverGlow>();
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(OnGrabbed);
        _interactable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(OnGrabbed);
        _interactable.selectExited.RemoveListener(OnReleased);
    }

    // ── Grab Events ──────────────────────────────────────────────────
    void OnGrabbed(SelectEnterEventArgs args)
    {
        _grabInteractor  = args.interactorObject;
        _prevControllerPos = GetInteractorPos();
        _isGrabbed       = true;
        Debug.Log($"[Lever] ✓ Grabbed by {_grabInteractor}");
    }

    void OnReleased(SelectExitEventArgs args)
    {
        _grabInteractor = null;
        _isGrabbed      = false;
        Debug.Log("[Lever] Released");
    }

    // ── Physics ──────────────────────────────────────────────────────
    void FixedUpdate()
    {
        if (!_isGrabbed || _grabInteractor == null) return;

        Vector3 currentPos = GetInteractorPos();
        Vector3 delta      = currentPos - _prevControllerPos;
        _prevControllerPos = currentPos;

        if (delta.sqrMagnitude < 0.000001f) return;

        // Ось шарнира в мировом пространстве
        Vector3 worldAxis = transform.TransformDirection(_hinge.axis);

        // Перпендикуляр к оси — направление "нажатия" на рычаг
        // Вектор от пивота к контроллеру
        Vector3 toCtrl = (currentPos - transform.position).normalized;
        Vector3 lever  = Vector3.Cross(worldAxis, toCtrl);

        // Проекция движения контроллера на lever-перпендикуляр
        float push = Vector3.Dot(delta, lever);

        // Торк = push * multiplier, ограниченный maxTorque
        float torque = Mathf.Clamp(push * torqueMultiplier * 100f, -maxTorque, maxTorque);

        _rb.AddTorque(worldAxis * torque, ForceMode.Force);
    }

    Vector3 GetInteractorPos()
    {
        if (_grabInteractor is MonoBehaviour mb)
            return mb.transform.position;
        return transform.position;
    }

    // Gizmo — показать ось шарнира
    void OnDrawGizmosSelected()
    {
        if (!_hinge) return;
        Gizmos.color = Color.cyan;
        Vector3 axis = transform.TransformDirection(_hinge.axis);
        Gizmos.DrawRay(transform.position, axis * 0.3f);
        Gizmos.DrawRay(transform.position, -axis * 0.3f);
    }
}
