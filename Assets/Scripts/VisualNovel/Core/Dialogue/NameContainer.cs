using System.Collections;
using System.Collections.Generic;
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
    }
}