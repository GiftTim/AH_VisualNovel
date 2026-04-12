namespace VISUALNOVEL
{
    /*
     * ============================================================
     * VN_VariableData (Visual Novel Variable Save Data)
     * ============================================================
     * VariableStore에 등록된 변수 하나를 세이브 파일에 저장하기 위한
     * 직렬화 단위 데이터 컨테이너.
     *
     * [역할]
     * - VariableStore는 런타임 메모리에만 존재하므로,
     *   게임 종료 후에도 변수 값을 유지하려면 파일에 저장해야 한다.
     * - 변수 이름, 값(문자열), 타입 정보를 세트로 저장한다.
     *
     * [이름 형식]
     *  "DB명.변수명" 형식으로 저장된다.
     *  예) "Default.playerGold", "VN.mainCharacterName"
     *  로드 시 VNGameSave.SetVariableData()가 이 이름으로 TrySetValue를 호출.
     *
     * [값 형식]
     *  모든 값은 문자열로 저장된다.
     *  예) true → "True", 42 → "42", 3.14f → "3.14"
     *
     * [타입 형식]
     *  C# 타입의 전체 이름(Type.FullName) 문자열로 저장된다.
     *  - bool   → "System.Boolean"
     *  - int    → "System.Int32"
     *  - float  → "System.Single"  (Single = float의 CLR 이름)
     *  - string → "System.String"
     *  로드 시 이 문자열을 switch로 판별하여 적절한 타입으로 파싱한다.
     *
     * [직렬화]
     * - VNGameSave.GetVariableData()에서 생성.
     * - VNGameSave.SetVariableData()에서 읽어 VariableStore에 복원.
     * ============================================================
     */
    [System.Serializable]
    public class VN_VariableData
    {
        // "DB명.변수명" 형식의 변수 식별자.
        // 예) "Default.hasMetHiro", "VN.mainCharacterName"
        public string name;

        // 변수의 현재 값을 문자열로 변환한 것.
        // 예) "True", "42", "Hello"
        public string value;

        // 값의 C# 타입 전체 이름.
        // 예) "System.Boolean", "System.Int32", "System.Single", "System.String"
        // 로드 시 이 값으로 파싱 방식을 결정한다.
        public string type;
    }
}
