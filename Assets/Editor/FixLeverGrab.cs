// Assets/Editor/FixLeverGrab.cs
// Tools ▶ VR Fix ▶ Fix Lever Grab
//
// Заменяет XRGrabInteractable на правильную пару:
// XRSimpleInteractable + VRLeverGrab
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public static class FixLeverGrab
{
    [MenuItem("Tools/VR Fix/Fix Lever Grab")]
    public static void Fix()
    {
        var armGO = GameObject.Find("LeverArm");
        if (!armGO) { Debug.LogError("[LeverGrab] LeverArm не найден!"); return; }

        // ── 1. Убрать XRGrabInteractable ──────────────────────────────
        var oldGrab = armGO.GetComponent<XRGrabInteractable>();
        if (oldGrab)
        {
            Object.DestroyImmediate(oldGrab);
            Debug.Log("[LeverGrab] ✓ XRGrabInteractable удалён");
        }

        // ── 2. Добавить XRSimpleInteractable если нет ─────────────────
        var simple = armGO.GetComponent<XRSimpleInteractable>();
        if (!simple)
        {
            simple = armGO.AddComponent<XRSimpleInteractable>();
            Debug.Log("[LeverGrab] ✓ XRSimpleInteractable добавлен");
        }

        // Прописать BoxCollider в XRSimpleInteractable
        var col   = armGO.GetComponent<BoxCollider>();
        var so    = new SerializedObject(simple);
        var colsP = so.FindProperty("m_Colliders");
        if (colsP != null && col)
        {
            colsP.arraySize = 1;
            colsP.GetArrayElementAtIndex(0).objectReferenceValue = col;
            so.ApplyModifiedProperties();
            Debug.Log("[LeverGrab] ✓ BoxCollider → XRSimpleInteractable.Colliders");
        }

        // ── 3. Добавить VRLeverGrab ───────────────────────────────────
        var leverGrab = armGO.GetComponent<VRLeverGrab>();
        if (!leverGrab)
        {
            leverGrab = armGO.AddComponent<VRLeverGrab>();
            Debug.Log("[LeverGrab] ✓ VRLeverGrab добавлен");
        }

        // ── 4. XRHoverGlow — оставить (подсветка при наведении) ───────
        if (!armGO.GetComponent<XRHoverGlow>())
        {
            armGO.AddComponent<XRHoverGlow>();
            Debug.Log("[LeverGrab] ✓ XRHoverGlow добавлен");
        }

        // ── 5. Rigidbody: убрать constraints, правильный damping ──────
        var rb = armGO.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.constraints            = RigidbodyConstraints.None;
            rb.angularDrag            = 8f;
            rb.drag                   = 2f;
            rb.mass                   = 0.15f;
            rb.useGravity             = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation          = RigidbodyInterpolation.Interpolate;
            rb.maxAngularVelocity     = 5f;
            Debug.Log("[LeverGrab] ✓ Rigidbody: constraints=None, angDrag=8, maxAngVel=5");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("<color=lime>[LeverGrab] ✓ Готово! Сохрани сцену и тестируй.</color>");
        Debug.Log("  Луч на рычаг → подсветка");
        Debug.Log("  Нажми и удержи Trigger → граб");
        Debug.Log("  Двигай контроллер — рычаг крутится");
        Debug.Log("  HingeJoint держит лимиты ±50°");
    }
}
#endif
