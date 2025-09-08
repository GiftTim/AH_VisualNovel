using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

namespace DIALOGUE
{
    [System.Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject dialogueNameBox;
        [SerializeField] private TMPro.TextMeshProUGUI nameText;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Show(string nameToShow = "")
        {
            dialogueNameBox.SetActive(true);

            if (nameToShow != string.Empty)
                nameText.text = nameToShow;
        }

        public void Hide()
        {
            dialogueNameBox.SetActive(false);
        }

        public void SetNameColor(Color color) => nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
    }
}