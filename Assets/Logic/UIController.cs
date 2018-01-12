using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text TutorialText;

    public void ShowText(string text, float seconds)
    {
        TutorialText.text = text;
        TutorialText.gameObject.SetActive(true);
        if (seconds > 0)
            StartCoroutine(ClearTextAfter(seconds));

    }
    public void ClearText()
    {
        TutorialText.gameObject.SetActive(false);
        TutorialText.text = "";
    }
    public IEnumerator ClearTextAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ClearText();
    }
}
