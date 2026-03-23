using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Рычаг-рубильник.
/// Толкнул до порога → сработал спавн.
/// Никакого возврата — стоит где поставили.
/// Дёрни обратно за resetAngle → флаг сбросится (можно снова).
/// </summary>
public class LeverTrigger : MonoBehaviour
{
    [Header("HingeJoint")]
    public HingeJoint hinge;

    [Header("Spawn")]
    public GameObject prefabToSpawn;
    public Transform  spawnPoint;

    [Header("Angles")]
    [Tooltip("Угол срабатывания (° от нейтрали)")]
    public float activateAngle = 35f;
    [Tooltip("Угол возврата флага — чтобы можно было снова дёрнуть")]
    public float resetAngle    = 10f;

    [Header("Events")]
    public UnityEvent OnActivated;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   activateClip;

    bool _activated;

    void Reset()
    {
        hinge       = GetComponent<HingeJoint>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!hinge) return;
        float a = hinge.angle;

        if (!_activated && Mathf.Abs(a) >= activateAngle)
        {
            _activated = true;

            // Спавн
            if (prefabToSpawn && spawnPoint)
                Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

            // Звук
            if (audioSource && activateClip)
                audioSource.PlayOneShot(activateClip);

            OnActivated?.Invoke();
        }

        // Сброс флага когда рычаг вернули к нейтрали
        if (_activated && Mathf.Abs(a) < resetAngle)
            _activated = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!spawnPoint) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.08f);
        Gizmos.DrawLine(transform.position, spawnPoint.position);
    }
}
