using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기존 클래스에 편의 기능을 추가하는 확장 메서드 모음.
/// "this 타입" 파라미터를 통해 마치 해당 클래스의 메서드인 것처럼 호출할 수 있다.
/// 예) myList.GetRandomItem() / myTransform.ClearChilds()
/// </summary>
public static class ClassExtentions
{
    // Destroy된 오브젝트를 즉시 목록에서 제외하기 위해 이름을 이 값으로 변경해 표시
    private const string DELETED_OBJECT_NAME = "DELETED";

    /// <summary>Vector2의 X값만 교체한 새 Vector2를 반환한다.</summary>
    public static Vector2 SetX(this Vector2 vector, float x)
    {
        return new Vector2(x, vector.y);
    }

    /// <summary>Vector2의 Y값만 교체한 새 Vector2를 반환한다.</summary>
    public static Vector2 SetY(this Vector2 vector, float y)
    {
        return new Vector2(vector.x, y);
    }

    /// <summary>
    /// 해당 GameObject에서 T 컴포넌트를 가져오고, 없으면 추가해서 반환한다.
    /// GetComponent + AddComponent를 한 번에 처리하는 편의 메서드.
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : MonoBehaviour
    {
        var component = gameObject.GetComponent<T>();
        if (component == null) component = gameObject.AddComponent<T>();
        return component;
    }

    /// <summary>해당 GameObject에 T 컴포넌트가 붙어 있는지 확인한다.</summary>
    public static bool HasComponent<T>(this GameObject gameObject) where T : MonoBehaviour
    {
        return gameObject.GetComponent<T>() != null;
    }

    /// <summary>리스트에서 무작위 요소 하나를 반환한다.</summary>
    public static T GetRandomItem<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Transform의 모든 자식 오브젝트를 Destroy한다.
    /// Destroy는 비동기라 실제 제거 전까지 이름을 DELETED로 변경해 GetChilds에서 제외되도록 한다.
    /// </summary>
    public static void ClearChilds(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            child.name = DELETED_OBJECT_NAME; // 즉시 제외 표시
            GameObject.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Transform의 자식 오브젝트 목록을 반환한다.
    /// ClearChilds로 삭제 예약된 오브젝트(이름이 DELETED인 것)는 제외한다.
    /// </summary>
    public static List<GameObject> GetChilds(this Transform transform)
    {
        var childs = new List<GameObject>();

        foreach (Transform child in transform)
        {
            if (child.name == DELETED_OBJECT_NAME) continue; // 삭제 예약된 오브젝트 건너뜀

            childs.Add(child.gameObject);
        }

        return childs;
    }

    /// <summary>
    /// 리스트를 무작위 순서로 섞는다 (Fisher-Yates Shuffle 알고리즘).
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1); // 0 ~ n 중 무작위 인덱스
            T value = list[k];
            list[k] = list[n]; // 현재 위치와 무작위 위치 교환
            list[n] = value;
        }
    }
}
