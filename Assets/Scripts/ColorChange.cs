using UnityEngine;

public class ColorChange : MonoBehaviour
{
    private Renderer _rend;
    private Color _defaultColor;
    void Start()
    {
        _rend = GetComponent<Renderer>();
        if (_rend) _defaultColor = _rend.material.color;
    }

    public void OnGrab()
    {
        if (_rend) _rend.material.color = Color.green;
    }

    public void OnRelease()
    {
        if (_rend) _rend.material.color = _defaultColor;
    }
}
