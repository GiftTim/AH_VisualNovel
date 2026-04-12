using UnityEngine;

namespace VISUALNOVEL
{
    /*
     * ============================================================
     * VNDatabaseLinkSetup (Visual Novel Database Link Setup)
     * ============================================================
     * VariableStore(전역 변수 저장소)에 "외부 데이터와 연결된 특수 변수"를
     * 등록하는 클래스.
     *
     * [일반 변수 vs 링크 변수]
     * - 일반 변수: VariableStore에 값이 직접 저장됨.
     * - 링크 변수: 실제 값은 다른 객체(예: VNGameSave)에 저장되고,
     *             VariableStore는 getter/setter 람다를 통해 그 값을 읽고 씀.
     *             즉, 변수 이름 ↔ 실제 데이터를 "다리처럼" 연결한다.
     *
     * [사용 예시]
     * 대화 스크립트에서 $VN.mainCharacterName 을 쓰면,
     * VNGameSave.activeFile.playerName 의 실제 값이 반환된다.
     * 반대로 input 명령어로 플레이어가 이름을 입력하면,
     * VNGameSave.activeFile.playerName 에 자동으로 저장된다.
     *
     * [호출 시점]
     * VNManager.Awake() 에서 SetupExtrnalLinks()가 호출된다.
     * 모든 대화가 시작되기 전에 반드시 완료되어야 한다.
     * ============================================================
     */
    public class VNDatabaseLinkSetup : MonoBehaviour
    {
        /*
         * SetupExtrnalLinks()
         * ────────────────────────────────────────────────────────
         * 외부 데이터와 연결된 특수 변수들을 VariableStore에 등록한다.
         * 새로운 링크 변수가 필요할 때 이 메서드 안에 추가하면 된다.
         *
         * VariableStore.CreateVariable(이름, 기본값, getter, setter)
         *  - 이름    : "DB명.변수명" 형식 → "VN.mainCharacterName"
         *              대화 스크립트에서는 $VN.mainCharacterName 으로 참조
         *  - 기본값  : 변수가 처음 만들어질 때의 초기값 (여기서는 빈 문자열)
         *  - getter  : 변수 값을 읽을 때 실제로 어디서 읽어올지 지정 (람다)
         *  - setter  : 변수 값을 쓸 때 실제로 어디에 저장할지 지정 (람다)
         * ────────────────────────────────────────────────────────
         */
        public void SetupExtrnalLinks()
        {
            // "VN.mainCharacterName" 변수를 VNGameSave.activeFile.playerName 에 연결.
            // - 읽기(getter): VNGameSave.activeFile.playerName 반환
            // - 쓰기(setter): VNGameSave.activeFile.playerName 에 값 저장
            // → 대화 스크립트에서 $VN.mainCharacterName 을 쓰면
            //   플레이어가 입력한 이름이 자동으로 표시된다.
            VariableStore.CreateVariable("VN.mainCharacterName", "",
                                            () => VNGameSave.activeFile.playerName,        // getter: 읽을 때
                                            value => VNGameSave.activeFile.playerName = value); // setter: 쓸 때
        }
    }
}
