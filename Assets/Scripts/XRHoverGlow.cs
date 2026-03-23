using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class XRHoverGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private float glowBoost = 1.5f;

    private XRGrabInteractable _grab;
    private Renderer[] _renderers;
    private Color[] _baseColors;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _renderers = GetComponentsInChildren<Renderer>(true);

        _baseColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] && _renderers[i].material.HasProperty("_Color"))
                _baseColors[i] = _renderers[i].material.color;
        }
    }

    private void OnEnable()
    {
        _grab.hoverEntered.AddListener(OnHoverEnter);
        _grab.hoverExited.AddListener(OnHoverExit);
    }

    private void OnDisable()
    {
        _grab.hoverEntered.RemoveListener(OnHoverEnter);
        _grab.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs e)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            if (!renderer) continue;

            var baseColor = _baseColors[i];
            var boosted = new Color(baseColor.r * glowBoost, baseColor.g * glowBoost, baseColor.b * glowBoost, baseColor.a);

            renderer.material.color = boosted;

            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", boosted);
            }
        }
    }

    private void OnHoverExit(HoverExitEventArgs e)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var renderer = _renderers[i];
            if (!renderer) continue;

            renderer.material.color = _baseColors[i];

            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", Color.black);
                renderer.material.DisableKeyword("_EMISSION");
            }
        }
    }
}
