using UnityEngine;
using CHARACTERS;
using TMPro;

namespace DIALOGUE
{
    [CreateAssetMenu(fileName = "Dialogue System Configuration", menuName ="Dialogue System/Dialogue Configuration Asset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public CharacterConfigurationSO CharacterConfigurationAsset;
        
        public Color defaultTextColor = Color.white;
        public TMP_FontAsset defaultFont;


        public float defaultNameFontSize = 65f;
        public float defaultDialogueFontSize = 45f;
        public float dialogueFontScale = 1f;

    }
}