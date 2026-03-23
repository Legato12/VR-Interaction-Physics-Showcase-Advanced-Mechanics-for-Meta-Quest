using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleHeadMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject menuButtons;  // Menu buttons object (Resume/Exit)
    [SerializeField] private CanvasGroup blackPanel;  // Black fade panel

    [Header("Settings")]
    [SerializeField] private float fadeTime = 2.0f;   // Fade out duration

    private void Start()
    {
        // Start with fully faded in and menu buttons visible
        if (blackPanel != null) blackPanel.alpha = 1f;
        if (menuButtons != null) menuButtons.SetActive(true);
    }

    /// <summary>
    /// Called by the START button to begin the game
    /// </summary>
    public void StartGame()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        // 1. Hide menu buttons so they don't interfere
        if (menuButtons != null) menuButtons.SetActive(false);

        // 2. Gradually fade out the black overlay
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            // Lerp from 1 (black) to 0 (transparent)
            if (blackPanel != null)
                blackPanel.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);

            yield return null;
        }

        // 3. Completely disable the panel for optimization
        if (blackPanel != null)
        {
            blackPanel.alpha = 0f;
            blackPanel.gameObject.SetActive(false);
        }

        // Optional: Disable entire Canvas if no longer needed
        // gameObject.SetActive(false);
    }
}
