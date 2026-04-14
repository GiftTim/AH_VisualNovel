using UnityEngine;

/// <summary>
/// 싱글턴 패턴 기반 클래스.
/// 씬에 딱 하나만 존재해야 하는 Manager 계열 오브젝트에 상속시켜 사용한다.
/// 예) GameManager : Singleton<GameManager>
/// </summary>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    // 씬에서 유일한 인스턴스를 저장하는 정적 변수
    // public이라 외부에서 직접 할당도 가능하지만, 보통 Instance 프로퍼티를 통해 접근한다
    public static T _Instance;

    /// <summary>
    /// 어디서든 Singleton<T>.Instance 로 접근하면 씬의 해당 컴포넌트를 반환한다.
    /// 아직 캐싱되지 않았으면 씬에서 자동으로 찾아 저장한다.
    /// </summary>
    public static T Instance
    {
        get
        {
            // _Instance가 비어 있을 때만 씬에서 탐색 (매 프레임 탐색 방지)
            if (_Instance == null)
                _Instance = FindFirstObjectByType<T>(); // Unity 6 호환 버전 (구버전: FindObjectOfType)

            return _Instance;
        }
    }
}
