using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/*
 * VariableStore
 * ─────────────────────────────────────────────────────────────────────
 * 게임 전체의 변수를 관리하는 정적 저장소.
 * 두 층 구조로 이루어진다:
 *   1) Database  : 변수를 묶는 네임스페이스 (그룹). 기본 데이터베이스는 "Default".
 *   2) Variable<T>: 실제 값을 저장하는 제네릭 변수. getter/setter 커스텀 가능.
 *
 * 변수 접근 방법:
 *   $변수명           → Default 데이터베이스에서 조회
 *   $데이터베이스명.변수명 → 지정한 데이터베이스에서 조회
 *
 * 사용 예:
 *   VariableStore.CreateVariable("score", 0);        // Default DB에 score 생성
 *   VariableStore.TrySetValue("score", 100);         // score = 100
 *   VariableStore.TryGetValue("score", out object v);// v = 100
 *   VariableStore.CreateVariable("save.chapter", 1); // save DB에 chapter 생성
 * ─────────────────────────────────────────────────────────────────────
 */
public class VariableStore
{
    // 기본 데이터베이스의 이름 상수. DB명을 지정하지 않으면 이 이름의 DB를 사용.
    private const string DEFAULT_DATABASE_NAME = "Default";

    // 변수 이름에서 DB명과 변수명을 구분하는 문자 ('.')
    // 예) "save.chapter" → DB="save", 변수명="chapter"
    public const char DATABASE_VARIABLE_RELATIONAL_ID = '.';

    // 변수 식별자 정규식: '!' 부정 접두사 선택적, '$' 필수, 알파벳·숫자·_·'.' 허용
    // 예) "$score", "!$isHappy", "$save.chapter"
    public static readonly string REGEX_VARIABLE_IDS = @"[!]?\$[a-zA-Z0-9_.]+";

    // 스크립트에서 변수를 나타내는 접두사 문자 ('$')
    public const char VARIABLE_ID = '$';

    /*
     * Database
     * 변수를 하나의 그룹(네임스페이스)으로 묶는 내부 클래스.
     * 같은 이름의 변수라도 다른 DB에 있으면 독립적으로 존재한다.
     */
    public class Database
    {
        public Database(string name)
        {
            this.name = name;
            variables = new Dictionary<string, Variable>();
        }

        // 이 데이터베이스의 이름
        public string name;
        // 변수명 → Variable 인스턴스 매핑
        public Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
    }

    /*
     * Variable (추상 클래스)
     * 타입에 관계없이 변수를 다룰 수 있도록 Get/Set을 추상화한 기반 클래스.
     * 딕셔너리에 Variable<T>를 저장할 때 타입 파라미터 없이 참조하기 위해 사용.
     */
    public abstract class Variable
    {
        public abstract object Get();
        public abstract void Set(object value);
    }

    /*
     * Variable<T>
     * 실제 값을 저장하는 제네릭 변수 클래스.
     *
     * getter/setter 커스텀 설계 이유:
     *   - getter=null이면 내부 value 필드를 반환 (일반 변수)
     *   - getter를 직접 지정하면 외부 소스(예: 다른 컴포넌트의 프로퍼티)와 연동 가능
     *   - setter=null이면 내부 value 필드에 저장 (일반 변수)
     *   - setter를 직접 지정하면 값 변경 시 외부 동작(이벤트 발생 등) 연결 가능
     */
    public class Variable<T> : Variable
    {
        private T value; // 내부 저장 값

        private Func<T> getter;   // 값을 읽는 함수 (null이면 내부 value 반환)
        private Action<T> setter; // 값을 쓰는 함수 (null이면 내부 value 설정)

        public Variable(T defaultValue = default, Func<T> getter = null, Action<T> setter = null)
        {
            value = defaultValue;

            // getter가 없으면 내부 value를 반환하는 기본 람다 사용
            if (getter == null)
            {
                this.getter = () => value;
            }
            else
            {
                this.getter = getter;
            }

            // setter가 없으면 내부 value를 갱신하는 기본 람다 사용
            if(setter == null)
            {
                this.setter = newValue => value = newValue;
            }
            else
            {
                this.setter = setter;
            }
        }

        // getter 함수를 호출해 현재 값을 object로 반환
        public override object Get() => getter();

        // setter 함수를 호출해 값을 T 타입으로 캐스팅 후 설정
        public override void Set(object newValue)
        {
            setter((T)newValue);
        }
    }

    // 데이터베이스 이름 → Database 인스턴스 매핑.
    // 초기화 시 Default DB를 미리 생성해 항상 접근 가능하도록 함.
    public static Dictionary<string, Database> databases = new Dictionary<string, Database>() { { DEFAULT_DATABASE_NAME, new Database(DEFAULT_DATABASE_NAME)} };

    // Default 데이터베이스 단축 접근 프로퍼티
    private static Database defaultDatabase => databases[DEFAULT_DATABASE_NAME];

    /*
     * CreateDatabase
     * 새 데이터베이스를 생성한다.
     * 같은 이름이 이미 있으면 경고를 출력하고 false 반환 (중복 방지).
     */
    public static bool CreateDatabase(string name)
    {
        if (!databases.ContainsKey(name))
        {
            databases[name] = new Database(name);
            return true;
        }

        Debug.LogWarning($"이름이 {name}인 데이터베이스가 이미 존재합니다");
        return false;
    }

    /*
     * GetDatabase
     * 이름으로 데이터베이스를 가져온다.
     *   - 빈 문자열 → Default DB 반환 (DB명 없이 변수명만 사용하는 경우)
     *   - 없는 이름 → 자동 생성 후 반환 (최초 접근 시 DB가 없을 수 있으므로)
     */
    public static Database GetDatabase(string name)
    {
        if (name == string.Empty)
        {
            return defaultDatabase;
        }

        if(!databases.ContainsKey(name))
        {
            CreateDatabase(name);
        }

        return databases[name];
    }

    /*
     * CreateVariable<T>
     * 지정한 이름으로 새 변수를 생성한다.
     * ExtractInfo()로 DB명과 변수명을 분리해 해당 DB에 변수를 등록.
     * 같은 이름의 변수가 이미 있으면 경고 후 false 반환.
     */
    public static bool CreateVariable<T>(string name, T defaultValue, Func<T> getter = null, Action<T> setter = null)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if (db.variables.ContainsKey(variableName))
        {
            Debug.LogWarning($"이름이 {variableName}인 변수가 이미 존재합니다");
            return false;
        }

        db.variables[variableName] = new Variable<T>(defaultValue, getter, setter);
        return true;

    }

    /*
     * TryGetValue
     * 변수의 현재 값을 가져온다.
     * out 패턴 사용: 존재하면 true + 값 반환, 없으면 false + null 반환.
     */
    public static bool TryGetValue(string name, out object variable)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if(!db.variables.ContainsKey(variableName))
        {
            variable = null;
            return false;
        }

        variable = db.variables[variableName].Get();
        return true;
    }

    /*
     * TrySetValue<T>
     * 변수에 새 값을 설정한다.
     * 변수가 없으면 false 반환 (없는 변수에는 설정 불가, CreateVariable 먼저 호출 필요).
     */
    public static bool TrySetValue<T>(string name, T value)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if(!db.variables.ContainsKey(variableName))
        {
            return false;
        }

        db.variables[variableName].Set(value);
        return true;
    }

    /*
     * ExtractInfo
     * 변수 이름 문자열에서 DB명과 변수명을 분리해 반환하는 내부 유틸리티.
     *
     * 분리 규칙:
     *   "score"        → parts=["score"],        db=Default,      variableName="score"
     *   "save.chapter" → parts=["save","chapter"], db=GetDatabase("save"), variableName="chapter"
     */
    private static (string[], Database, string) ExtractInfo(string name)
    {
        string[] parts = name.Split(DATABASE_VARIABLE_RELATIONAL_ID);
        Database db = parts.Length > 1 ? GetDatabase(parts[0]): defaultDatabase; // '.'이 있으면 앞부분이 DB명
        string variableName = parts.Length > 1 ? parts[1] : parts[0];           // '.'이 있으면 뒷부분이 변수명

        return (parts, db, variableName);
    }

    /*
     * HasVariable
     * 변수가 존재하는지 여부만 확인한다 (값 조회 없이 ContainsKey만 사용).
     * TryGetValue 없이 존재 여부만 빠르게 확인할 때 사용.
     */
    public static bool HasVariable(string name)
    {
        string[] parts = name.Split(DATABASE_VARIABLE_RELATIONAL_ID);
        Database db = parts.Length > 1 ? GetDatabase(parts[0]) : defaultDatabase;
        string variableName = parts.Length > 1 ? parts[1] : parts[0];

        return db.variables.ContainsKey(variableName);

    }

    /*
     * RemoveVariable
     * 지정한 변수를 DB에서 제거한다.
     * 씬 전환이나 세이브 초기화 등 특정 변수만 제거할 때 사용.
     */
    public static void RemoveVariable(string name)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if(db.variables.ContainsKey(variableName))
        {
            db.variables.Remove(variableName);
        }
    }

    /*
     * RemoveAllVariables
     * 모든 DB와 변수를 제거하고 Default DB만 남긴다.
     * 게임 전체 초기화(새 게임 시작 등)에 사용.
     */
    public static void RemoveAllVariables()
    {
        databases.Clear();
        databases[DEFAULT_DATABASE_NAME] = new Database(DEFAULT_DATABASE_NAME);

    }

    /*
     * PrintAllDatabases
     * 현재 존재하는 모든 DB 이름을 Debug.Log로 출력한다.
     * 디버깅 전용 유틸리티.
     */
    public static void PrintAllDatabases()
    {
        foreach(KeyValuePair<string, Database> dbEntry in databases)
        {
            Debug.Log($"Database: '<color=#FFB145>{dbEntry.Key}</color>'");
        }
    }

    /*
     * PrintAllVariables
     * 모든 DB(또는 지정한 DB)의 모든 변수 이름과 값을 Debug.Log로 출력한다.
     * database를 지정하면 해당 DB만, null이면 전체 DB를 출력.
     * 디버깅 전용 유틸리티.
     */
    public static void PrintAllVariables(Database database = null)
    {
        if(database != null)
        {
            PrintAllDatabaseVariables(database);
            return;
        }

        // database가 null이면 모든 DB를 순회해 출력
        foreach (var dbEntry in databases)
        {
            PrintAllDatabaseVariables(dbEntry.Value);
        }
    }

    // 단일 DB의 모든 변수를 출력하는 내부 유틸리티
    private static void PrintAllDatabaseVariables(Database database)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Database: '<color=#F38544>{database.name}</color>'");

        foreach (KeyValuePair<string, Variable> variablePair in database.variables)
        {
            string variableName = variablePair.Key;
            object variableValue = variablePair.Value.Get();
            Debug.Log($"\t<color=#FFB145>Variable [{variableName}]</color> = <color=#FFD22D>{variableValue}</color>");
        }
        Debug.Log(sb.ToString());
    }


}
