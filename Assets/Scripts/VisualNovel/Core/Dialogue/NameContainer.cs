using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /*
     * NameContainer
     * ─────────────────────────────────────────────────────────────────────
     * 화자 이름을 표시하는 UI 박스 클래스.
     * DialogueContainer가 소유하며, 화자가 바뀔 때마다 이름과 스타일을 갱신한다.
     *
     * 구성 요소:
     *   dialogueNameBox : 이름 박스 전체 GameObject (활성/비활성으로 표시/숨김)
     *   nameText        : 실제 이름 텍스트 컴포넌트 (TextMeshProUGUI)
     * ─────────────────────────────────────────────────────────────────────
     */
    [System.Serializable]
    public class NameContainer
    {
        // 이름 박스 전체 오브젝트 (SetActive로 표시/숨김 전환)
        [SerializeField] private GameObject dialogueNameBox;
        // 이름 텍스트 컴포넌트 (public getter, private setter)
        [field: SerializeField] public TextMeshProUGUI nameText { get; private set; }

        /*
         * Show
         * 이름 박스를 활성화(표시)한다.
         * nameToShow가 빈 문자열이 아니면 텍스트도 함께 변경.
         * 빈 문자열이면 텍스트는 변경하지 않고 활성화만 한다.
         * (이전 이름을 유지하면서 박스를 다시 보여줄 때 사용)
         */
        public void Show(string nameToShow = "")
        {
            dialogueNameBox.SetActive(true);

            if (nameToShow != string.Empty)
                nameText.text = nameToShow;
        }

        /*
         * Hide
         * 이름 박스를 비활성화(숨김)한다.
         * narrator 처리 시 또는 화자가 없는 대사 시 호출된다.
         */
        public void Hide()
        {
            dialogueNameBox.SetActive(false);
        }

        // 화자별 이름 텍스트 스타일 변경 메서드 (캐릭터 설정에 따라 호출)
        public void SetNameColor(Color color) => nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
        public void SetNameFontSize(float size) => nameText.fontSize = size;
    }
}
