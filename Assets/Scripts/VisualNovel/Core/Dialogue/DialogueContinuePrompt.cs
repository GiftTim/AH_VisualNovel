using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /*
     * DialogueContinuePrompt
     * ─────────────────────────────────────────────────────────────────────
     * 대화 계속 아이콘(▶)의 위치를 계산하고 표시/숨김을 담당하는 클래스.
     * 텍스트의 마지막 문자 오른쪽 하단에 위치하도록 매번 좌표를 계산한다.
     *
     * 구성 요소:
     *   root   : 이 컴포넌트의 RectTransform (위치 설정에 사용)
     *   anim   : ▶ 아이콘 GameObject의 Animator (활성/비활성으로 표시/숨김)
     *   tmpro  : 대사 텍스트 컴포넌트 (마지막 문자 위치 계산 기준)
     *   yOffset: 마지막 문자 bottomRight에서 추가로 올릴 Y 오프셋
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DialogueContinuePrompt : MonoBehaviour
    {
        private RectTransform root;

        [SerializeField] private Animator anim;
        [SerializeField] private TextMeshProUGUI tmpro;
        [SerializeField] private float yOffset = 10f;

        // ▶ 아이콘이 현재 표시 중인지 여부
        public bool isShowing => anim.gameObject.activeSelf;

        // Start is called before the first frame update
        void Start()
        {
            root = GetComponent<RectTransform>();
        }

        /*
         * Show
         * ▶ 아이콘을 텍스트의 마지막 문자 오른쪽에 위치시키고 활성화한다.
         *
         * ForceMeshUpdate() 필요 이유:
         *   TMP는 텍스트가 변경되어도 즉시 textInfo를 업데이트하지 않는다.
         *   ForceMeshUpdate()를 호출해야 characterInfo가 최신 상태로 갱신된다.
         *
         * 역방향 탐색 이유:
         *   텍스트 끝에 공백이나 줄바꿈이 있으면 해당 문자들을 건너뛰어야 한다.
         *   실제 마지막 보이는 문자를 찾기 위해 역방향으로 탐색.
         *
         * 위치 계산:
         *   마지막 문자의 bottomRight + 문자 너비(pointSize * 0.5f) + yOffset
         */
        public void Show()
        {
            // 텍스트가 비어있으면 이미 표시 중이면 숨기고 종료
            if (tmpro.text == string.Empty)
            {
                if (isShowing)
                {
                    Hide();
                }
                return;
            }

            // textInfo를 최신 상태로 강제 갱신 (Show 호출 시점에 즉시 적용하기 위함)
            tmpro.ForceMeshUpdate();

            anim.gameObject.SetActive(true);
            root.transform.SetParent(tmpro.transform);
            int lastCharIndex = tmpro.textInfo.characterCount - 1;

            // 뒤에서부터 공백과 줄바꿈을 건너뛰어 실제 마지막 보이는 문자를 찾음
            while (lastCharIndex > 0 &&
                  (tmpro.textInfo.characterInfo[lastCharIndex].character == ' ' ||
                   tmpro.textInfo.characterInfo[lastCharIndex].character == '\n'))
            {
                lastCharIndex--;
            }

            TMP_CharacterInfo finalCharacter = tmpro.textInfo.characterInfo[lastCharIndex];
            Vector3 targetPos = finalCharacter.bottomRight;
            // 문자 너비만큼 오른쪽으로 이동 (포인트 크기의 절반을 너비로 근사)
            float characterWidth = finalCharacter.pointSize * 0.5f;

            targetPos = new Vector3(targetPos.x + characterWidth, targetPos.y + yOffset, 0f);

            root.localPosition = targetPos;
        }

        /*
         * Hide
         * ▶ 아이콘을 즉시 비활성화(숨김)한다.
         * 페이드 없이 SetActive(false)로 즉시 처리.
         */
        public void Hide()
        {
            anim.gameObject.SetActive(false);
        }

    }
}



