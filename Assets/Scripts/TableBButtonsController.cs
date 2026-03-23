using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TableBButtonsController : MonoBehaviour
{
    [Header("Light Controls")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private bool lightsOn = true;

    [Header("Tip Display")]
    [SerializeField] private GameObject tipTextObject;
    [SerializeField] private float tipSeconds = 2f;

    public void ResetScene()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene("MenuScene");
    }

    public void ToggleLights()
    {
        lightsOn = !lightsOn;

        if (!directionalLight)
            directionalLight = FindDirectionalLight();

        if (directionalLight)
            directionalLight.enabled = lightsOn;
    }

    public void ShowTip()
    {
        if (!tipTextObject) return;

        StopAllCoroutines();
        StartCoroutine(TipRoutine());
    }

    IEnumerator TipRoutine()
    {
        tipTextObject.SetActive(true);
        yield return new WaitForSeconds(tipSeconds);
        tipTextObject.SetActive(false);
    }

    Light FindDirectionalLight()
    {
        foreach (var l in FindObjectsOfType<Light>())
            if (l.type == LightType.Directional) return l;
        return null;
    }
}
