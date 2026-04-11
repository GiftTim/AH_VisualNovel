using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    [System.Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject dialogueNameBox;
        [field: SerializeField] public TextMeshProUGUI nameText { get; private set; }
        
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
        public void SetNameFontSize(float size) => nameText.fontSize = size;
    }
}