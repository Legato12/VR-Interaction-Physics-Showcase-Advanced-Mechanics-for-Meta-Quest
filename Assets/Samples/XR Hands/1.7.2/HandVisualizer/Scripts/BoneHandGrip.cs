using UnityEngine;
using UnityEngine.InputSystem;

public class BoneHandGripAction : MonoBehaviour
{
    [Header("Input (set in Inspector)")]
    public InputActionProperty gripAction;

    [Header("INDEX")]
    public Transform indexMeta;
    public Transform indexProx;
    public Transform indexInter;
    public Transform indexDist;

    [Header("MIDDLE")]
    public Transform middleMeta;
    public Transform middleProx;
    public Transform middleInter;
    public Transform middleDist;

    [Header("RING")]
    public Transform ringMeta;
    public Transform ringProx;
    public Transform ringInter;
    public Transform ringDist;

    [Header("PINKY/LITTLE")]
    public Transform pinkyMeta;
    public Transform pinkyProx;
    public Transform pinkyInter;
    public Transform pinkyDist;

    [Header("THUMB")]
    public Transform thumbMeta;
    public Transform thumbProx;
    public Transform thumbDist;

    [Header("Curl Angles (fist pose)")]
    public float indexMetaAngle = 10f;
    public float indexProxAngle = 65f;
    public float indexInterAngle = 85f;
    public float indexDistAngle = 55f;

    public float middleMetaAngle = 12f;
    public float middleProxAngle = 80f;
    public float middleInterAngle = 95f;
    public float middleDistAngle = 65f;

    public float ringMetaAngle = 15f;
    public float ringProxAngle = 85f;
    public float ringInterAngle = 95f;
    public float ringDistAngle = 70f;

    public float pinkyMetaAngle = 22f;
    public float pinkyProxAngle = 75f;
    public float pinkyInterAngle = 85f;
    public float pinkyDistAngle = 70f;

    [Header("Thumb Angles (fist pose)")]
    public float thumbMetaAngle = 20f;
    public float thumbProxAngle = 55f;
    public float thumbDistAngle = 40f;

    [Header("Smoothing")]
    public float speed = 10f;

    // Open rotations (saved)
    Quaternion indexMeta0, indexProx0, indexInter0, indexDist0;
    Quaternion middleMeta0, middleProx0, middleInter0, middleDist0;
    Quaternion ringMeta0, ringProx0, ringInter0, ringDist0;
    Quaternion pinkyMeta0, pinkyProx0, pinkyInter0, pinkyDist0;
    Quaternion thumbMeta0, thumbProx0, thumbDist0;

    float currentGrip = 0f;

    void OnEnable()
    {
        if (gripAction.action != null)
            gripAction.action.Enable();
    }

    void OnDisable()
    {
        if (gripAction.action != null)
            gripAction.action.Disable();
    }

    void Start()
    {
        // Save "open hand" pose (whatever is in prefab right now)
        Save0(indexMeta, ref indexMeta0);
        Save0(indexProx, ref indexProx0);
        Save0(indexInter, ref indexInter0);
        Save0(indexDist, ref indexDist0);

        Save0(middleMeta, ref middleMeta0);
        Save0(middleProx, ref middleProx0);
        Save0(middleInter, ref middleInter0);
        Save0(middleDist, ref middleDist0);

        Save0(ringMeta, ref ringMeta0);
        Save0(ringProx, ref ringProx0);
        Save0(ringInter, ref ringInter0);
        Save0(ringDist, ref ringDist0);

        Save0(pinkyMeta, ref pinkyMeta0);
        Save0(pinkyProx, ref pinkyProx0);
        Save0(pinkyInter, ref pinkyInter0);
        Save0(pinkyDist, ref pinkyDist0);

        Save0(thumbMeta, ref thumbMeta0);
        Save0(thumbProx, ref thumbProx0);
        Save0(thumbDist, ref thumbDist0);
    }

    void Update()
    {
        float targetGrip = 0f;

        if (gripAction.action != null)
            targetGrip = gripAction.action.ReadValue<float>();

        // clamp 0..1
        targetGrip = Mathf.Clamp01(targetGrip);

        currentGrip = Mathf.MoveTowards(currentGrip, targetGrip, Time.deltaTime * speed);

        ApplyFist(currentGrip);
    }

    void Save0(Transform t, ref Quaternion q)
    {
        if (t != null) q = t.localRotation;
    }

    Quaternion Curl(Quaternion openRot, float angleDeg, float t)
    {
        // IMPORTANT: fist = bend DOWN around local X
        // so we rotate by NEGATIVE angle.
        return openRot * Quaternion.Euler(+angleDeg * t, 0f, 0f);
    }

    void ApplyFinger(Transform meta, Transform prox, Transform inter, Transform dist,
                     Quaternion meta0, Quaternion prox0, Quaternion inter0, Quaternion dist0,
                     float t,
                     float metaAngle, float proxAngle, float interAngle, float distAngle)
    {
        if (meta != null) meta.localRotation = Curl(meta0, metaAngle, t);
        if (prox != null) prox.localRotation = Curl(prox0, proxAngle, t);
        if (inter != null) inter.localRotation = Curl(inter0, interAngle, t);
        if (dist != null) dist.localRotation = Curl(dist0, distAngle, t);
    }

    void ApplyThumb(Transform meta, Transform prox, Transform dist,
                    Quaternion meta0, Quaternion prox0, Quaternion dist0,
                    float t,
                    float metaAngle, float proxAngle, float distAngle)
    {
        if (meta != null) meta.localRotation = Curl(meta0, metaAngle, t);
        if (prox != null) prox.localRotation = Curl(prox0, proxAngle, t);
        if (dist != null) dist.localRotation = Curl(dist0, distAngle, t);
    }

    void ApplyFist(float t)
    {
        ApplyFinger(indexMeta, indexProx, indexInter, indexDist,
            indexMeta0, indexProx0, indexInter0, indexDist0,
            t,
            indexMetaAngle, indexProxAngle, indexInterAngle, indexDistAngle);

        ApplyFinger(middleMeta, middleProx, middleInter, middleDist,
            middleMeta0, middleProx0, middleInter0, middleDist0,
            t,
            middleMetaAngle, middleProxAngle, middleInterAngle, middleDistAngle);

        ApplyFinger(ringMeta, ringProx, ringInter, ringDist,
            ringMeta0, ringProx0, ringInter0, ringDist0,
            t,
            ringMetaAngle, ringProxAngle, ringInterAngle, ringDistAngle);

        ApplyFinger(pinkyMeta, pinkyProx, pinkyInter, pinkyDist,
            pinkyMeta0, pinkyProx0, pinkyInter0, pinkyDist0,
            t,
            pinkyMetaAngle, pinkyProxAngle, pinkyInterAngle, pinkyDistAngle);

        ApplyThumb(thumbMeta, thumbProx, thumbDist,
            thumbMeta0, thumbProx0, thumbDist0,
            t,
            thumbMetaAngle, thumbProxAngle, thumbDistAngle);
    }
}
