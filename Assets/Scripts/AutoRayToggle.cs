using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Прячет визуал луча (LineRenderer) когда XRGrabInteractable входит в зону руки.
/// НЕ трогает NearFarInteractor — он сам управляет near/far режимом и удерживает граб.
/// </summary>
public class AutoRayToggle : MonoBehaviour
{
    [SerializeField] private GameObject rayObject;
    [SerializeField] private float disableTime = 0.35f;

    private int     _insideCount = 0;
    private float   _timer       = 0f;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        if (rayObject != null)
            _lineRenderer = rayObject.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (_insideCount > 0)
        {
            _timer = disableTime;
            SetRayVisible(false);
        }
        else
        {
            if (_timer > 0f)
                _timer -= Time.deltaTime;
            else
                SetRayVisible(true);
        }
    }

    private void SetRayVisible(bool visible)
    {
        // Только гасим/показываем LineRenderer — не трогаем NearFarInteractor!
        // NearFarInteractor сам переключается между near/far режимами
        // и сохраняет активный граб при переходе. Если отключить компонент — граб обрывается.
        if (_lineRenderer != null)
            _lineRenderer.enabled = visible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<XRGrabInteractable>())
            _insideCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<XRGrabInteractable>())
            _insideCount = Mathf.Max(0, _insideCount - 1);
    }
}
