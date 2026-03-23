using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ColorSocket : MonoBehaviour
{
    [Header("Socket Settings")]
    [SerializeField] private SocketColor acceptsColor;
    [SerializeField] private Renderer socketRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successClip;

    private XRSocketInteractor _socket;

    private void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _socket.selectEntered.AddListener(OnInserted);
        _socket.selectExited.AddListener(OnRemoved);
    }

    private void OnInserted(SelectEnterEventArgs args)
    {
        var tag = args.interactableObject.transform.GetComponent<SocketColorTag>();
        if (tag == null) return;

        if (tag.Color == acceptsColor)
        {
            SetGlow(true);
            if (audioSource && successClip) audioSource.PlayOneShot(successClip);
        }
        else
        {
            SetGlow(false);
        }
    }

    private void OnRemoved(SelectExitEventArgs args)
    {
        SetGlow(false);
    }

    private void SetGlow(bool on)
    {
        if (!socketRenderer) return;

        var mat = socketRenderer.material;
        if (on)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white * 2f);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }
    }

    public bool IsCorrectOccupied()
    {
        if (!_socket.hasSelection) return false;
        var tag = _socket.firstInteractableSelected.transform.GetComponent<SocketColorTag>();
        return tag != null && tag.Color == acceptsColor;
    }
}
