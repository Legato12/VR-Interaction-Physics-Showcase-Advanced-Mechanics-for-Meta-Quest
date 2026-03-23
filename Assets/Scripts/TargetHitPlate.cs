using System.Collections;
using UnityEngine;

/// <summary>
/// Target hit plate that reacts to collisions with physics objects
/// </summary>
public class TargetHitPlate : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color idleColor = new Color(1f, 0.9f, 0.2f); // Yellow
    [SerializeField] private Color hitColor = Color.white;

    [Header("Timing")]
    [SerializeField] private float resetSeconds = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip;

    [Header("Camera Background Effect")]
    [SerializeField] private bool changeCameraBackground = true;
    [SerializeField] private Color bgHitColor = new Color(1f, 0f, 1f); // Magenta

    private Camera cam;
    private Color bgDefault;
    private bool isCoolingDown = false;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        cam = Camera.main;
        if (cam != null) bgDefault = cam.backgroundColor;

        SetIdle();
    }

    void OnCollisionEnter(Collision col)
    {
        // фильтр: реагируем только если объект с Rigidbody (чтобы не триггерилось от стены/руки)
        if (col.rigidbody == null) return;

        // чтобы не спамило при множественных контактах
        if (isCoolingDown) return;

        StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        isCoolingDown = true;

        // цвет мишени
        if (targetRenderer)
            targetRenderer.material.color = hitColor;

        // звук
        if (audioSource && hitClip)
        {
            audioSource.spatialBlend = 1f;
            audioSource.PlayOneShot(hitClip, 1f);
        }

        // фон камеры
        if (changeCameraBackground && cam != null)
            cam.backgroundColor = bgHitColor;

        yield return new WaitForSeconds(resetSeconds);

        // вернуть назад
        SetIdle();

        if (changeCameraBackground && cam != null)
            cam.backgroundColor = bgDefault;

        isCoolingDown = false;
    }

    void SetIdle()
    {
        if (targetRenderer)
            targetRenderer.material.color = idleColor;
    }
}
