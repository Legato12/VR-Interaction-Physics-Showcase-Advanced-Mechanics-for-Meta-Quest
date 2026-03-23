// Assets/Scripts/LeverDebug.cs
// Добавь на LeverArm — показывает всё в реальном времени в Console + OnGUI
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LeverDebug : MonoBehaviour
{
    HingeJoint          _hinge;
    Rigidbody           _rb;
    LeverTrigger        _trigger;
    XRGrabInteractable  _grab;

    float _maxAngleSeen;
    float _timer;

    void Awake()
    {
        _hinge   = GetComponent<HingeJoint>();
        _rb      = GetComponent<Rigidbody>();
        _trigger = GetComponent<LeverTrigger>();
        _grab    = GetComponent<XRGrabInteractable>();

        // Сразу проверить конфиг
        if (!_hinge)
            Debug.LogError("[Lever] ❌ HingeJoint NOT FOUND на " + name);
        else
        {
            Debug.Log($"[Lever] HingeJoint: connectedBody={(_hinge.connectedBody ? _hinge.connectedBody.name : "WORLD")}" +
                      $" axis={_hinge.axis} limits={_hinge.limits.min}..{_hinge.limits.max}");
        }

        if (!_rb)
            Debug.LogError("[Lever] ❌ Rigidbody NOT FOUND");
        else
            Debug.Log($"[Lever] RB: constraints={_rb.constraints} kinematic={_rb.isKinematic} gravity={_rb.useGravity}");

        if (!_trigger)
            Debug.LogWarning("[Lever] ⚠ LeverTrigger не найден");
        else
        {
            bool hingeOk = _trigger.hinge == _hinge;
            bool spawnOk = _trigger.prefabToSpawn != null;
            bool ptOk    = _trigger.spawnPoint != null;
            Debug.Log($"[Lever] LeverTrigger: hinge={( hingeOk ? "✓ OK" : "❌ WRONG (не тот HingeJoint!)")}" +
                      $" prefab={( spawnOk ? "✓" : "❌ NULL")} spawnPoint={( ptOk ? "✓" : "❌ NULL")}" +
                      $" activateAngle={_trigger.activateAngle}");
        }

        if (!_grab)
            Debug.LogWarning("[Lever] ⚠ XRGrabInteractable не найден — граб контроллером невозможен");
        else
            Debug.Log($"[Lever] XRGrab: movementType={_grab.movementType} throwOnDetach={_grab.throwOnDetach}");
    }

    void Update()
    {
        if (!_hinge) return;

        float angle = _hinge.angle;
        if (Mathf.Abs(angle) > Mathf.Abs(_maxAngleSeen))
            _maxAngleSeen = angle;

        // Лог каждые 0.5 сек или когда угол > 20°
        _timer += Time.deltaTime;
        if (_timer > 0.5f || Mathf.Abs(angle) > 20f)
        {
            _timer = 0f;
            bool nearActivate = Mathf.Abs(angle) >= (_trigger ? _trigger.activateAngle * 0.8f : 28f);
            string status = nearActivate ? "⚡ NEAR ACTIVATE" : "";
            Debug.Log($"[Lever] angle={angle:F1}° maxSeen={_maxAngleSeen:F1}° {status}");
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"[Lever] 💥 Collision with '{col.gameObject.name}'" +
                  $" impulse={col.impulse.magnitude:F2}" +
                  $" | angle at impact={(_hinge ? _hinge.angle.ToString("F1") : "?")}°");
    }

    void OnGUI()
    {
        if (!_hinge) return;
        float a = _hinge.angle;
        float activate = _trigger ? _trigger.activateAngle : 35f;

        GUI.Box(new Rect(10, 60, 220, 95), "");
        GUI.Label(new Rect(15, 65,  210, 20), $"Lever angle: {a:F1}°");
        GUI.Label(new Rect(15, 83,  210, 20), $"Max seen: {_maxAngleSeen:F1}°  (need ±{activate}°)");
        GUI.Label(new Rect(15, 101, 210, 20), $"RB constraints: {(_rb ? _rb.constraints.ToString() : "?")}");
        GUI.Label(new Rect(15, 119, 210, 20), Mathf.Abs(a) >= activate ? "🟢 TRIGGERED!" : "⚪ not triggered");

        // Визуальная полоска угла
        float norm = Mathf.InverseLerp(-55f, 55f, a);
        GUI.Box(new Rect(15, 137, 200, 10), "");
        GUI.Box(new Rect(15 + norm * 180f, 135, 4, 14), "");
    }
}
