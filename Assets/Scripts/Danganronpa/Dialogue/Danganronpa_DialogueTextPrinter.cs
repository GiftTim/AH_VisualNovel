using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Danganronpa_DialogueTextPrinter
{

    public IEnumerator PrintText(TextMeshProUGUI target, string raw, float delay, Action onComplete = null)
    {
        target.text = "";
        bool first = true;

        foreach (char c in raw)
        {
            target.text += c;
            if (first)
            {
                first = false;
            }
            yield return new WaitForSeconds(delay); // ← (로그만 넣음. 필요하면 Realtime로 바꾸자)
        }

        onComplete?.Invoke();
    }

    public void SetTextInstant(TextMeshProUGUI target, string fullText)
    {
        if (target != null) target.text = fullText;
    }

    public void Clear(TextMeshProUGUI target)
    {
        if (target != null) target.text = "";
    }
}