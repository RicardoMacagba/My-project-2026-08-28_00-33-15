using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterText : MonoBehaviour
{
    public TMP_Text textDisplay;
    public float typingSpeed = 0.04f;

    private string fullText;
    private Coroutine typingCoroutine;

    public bool IsTyping { get; private set; }

    void Awake()
    {
        if (textDisplay == null)
        {
            Debug.LogError("TypewriterText requires a TMP_Text reference.", this);
            enabled = false;
            return;
        }

        fullText = textDisplay.text;
        textDisplay.text = "";
    }

    public void StartTyping()
    {
        if (textDisplay == null)
        {
            Debug.LogError("TypewriterText requires a TMP_Text reference.", this);
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        IsTyping = true;
        textDisplay.text = "";
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void ClearText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = null;
        IsTyping = false;
        textDisplay.text = "";
    }

    IEnumerator TypeText()
    {
        foreach (char letter in fullText)
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
        IsTyping = false;
    }
}