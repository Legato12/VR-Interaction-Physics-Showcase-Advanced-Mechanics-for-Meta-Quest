using UnityEngine;

public enum SocketColor { Red, Green, Blue }

/// <summary>
/// Component that identifies the color of an object for socket interactions
/// </summary>
public class SocketColorTag : MonoBehaviour
{
    [SerializeField] private SocketColor color;

    public SocketColor Color => color;
}
