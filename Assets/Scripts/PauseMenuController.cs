using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform menuRoot;     // Pause menu canvas root
    [SerializeField] private Transform followTarget; // Main camera transform
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Settings")]
    [SerializeField] private float distance = 1.2f;
    [SerializeField] private Vector3 offset = new Vector3(0f, -0.15f, 0f);
    [SerializeField] private float followSpeed = 12f;

    private bool _isPaused;

    void Start()
    {
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
    }

    void Update()
    {
        // быстрый тест в редакторе
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        // если пауза включена — держим меню перед камерой
        if (_isPaused && pauseMenuRoot && followTarget)
        {
            pauseMenuRoot.transform.position = followTarget.position + followTarget.forward * distance;
            pauseMenuRoot.transform.rotation = Quaternion.LookRotation(pauseMenuRoot.transform.position - followTarget.position);
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;

        if (pauseMenuRoot) pauseMenuRoot.SetActive(_isPaused);

        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void Resume()
    {
        _isPaused = false;
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    void LateUpdate()
    {
        if (!menuRoot || !followTarget) return;

        // позиция перед камерой
        Vector3 targetPos = followTarget.position + followTarget.forward * distance;

        // небольшой оффсет вниз (чтобы не закрывало центр)
        targetPos += followTarget.up * offset.y + followTarget.right * offset.x;

        menuRoot.position = Vector3.Lerp(menuRoot.position, targetPos, Time.deltaTime * followSpeed);

        // поворот в сторону камеры (только по Y чтобы не наклонялось)
        Vector3 lookDir = menuRoot.position - followTarget.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(lookDir);
            menuRoot.rotation = Quaternion.Slerp(menuRoot.rotation, rot, Time.deltaTime * followSpeed);
        }
    }
}
