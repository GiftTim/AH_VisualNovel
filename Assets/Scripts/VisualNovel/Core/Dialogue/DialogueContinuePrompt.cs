using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    public class DialogueContinuePrompt : MonoBehaviour
    {
        private RectTransform root;

        [SerializeField] private Animator anim;
        [SerializeField] private TextMeshProUGUI tmpro;
        [SerializeField] private float yOffset = 10f;

        public bool isShowing => anim.gameObject.activeSelf;

        // Start is called before the first frame update
        void Start()
        {
            root = GetComponent<RectTransform>();
        }

        public void Show()
        {
            if (tmpro.text == string.Empty)
            {
                if (isShowing)
                {
                    Hide();
                }
                return;
            }

            tmpro.ForceMeshUpdate();

            anim.gameObject.SetActive(true);
            root.transform.SetParent(tmpro.transform);
            int lastCharIndex = tmpro.textInfo.characterCount - 1;

            while (lastCharIndex > 0 &&
                  (tmpro.textInfo.characterInfo[lastCharIndex].character == ' ' ||
                   tmpro.textInfo.characterInfo[lastCharIndex].character == '\n'))
            {
                lastCharIndex--;
            }

            TMP_CharacterInfo finalCharacter = tmpro.textInfo.characterInfo[lastCharIndex];
            Vector3 targetPos = finalCharacter.bottomRight;
            float characterWidth = finalCharacter.pointSize * 0.5f;

            targetPos = new Vector3(targetPos.x + characterWidth, targetPos.y + yOffset, 0f);

            root.localPosition = targetPos;
        }

        public void Hide()
        {
            anim.gameObject.SetActive(false);
        }

    }
}


