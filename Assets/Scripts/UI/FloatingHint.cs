using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingHint : MonoBehaviour
{
    public static FloatingHint Instance; // Singleton für einfachen Zugriff

    [Header("Text Settings")]
    public TextMeshPro textMesh;
    public float fadeDuration = 0.6f; // 0,6
    public float visibleDuration = 8f; // 8
    public Vector3 offset = new Vector3(0, 0.5f, 0);

    private Coroutine currentRoutine;

    void Awake()
    {
        Instance = this;

        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }

        textMesh.alpha = 0f; 
    }

    public void ShowHint(string message, Transform anchor)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(HintRoutine(message, anchor));
        textMesh.faceColor = new Color32(255, 255, 255, 255);
    }

    private IEnumerator HintRoutine(string message, Transform anchor)
    {
        textMesh.text = message;

        transform.position = anchor.position + offset;
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); 
    
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            textMesh.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        textMesh.alpha = 1f;

        yield return new WaitForSeconds(visibleDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            textMesh.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        textMesh.alpha = 0f;
    }
}
