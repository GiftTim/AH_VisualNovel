using System;
using UnityEngine;

/// <summary>
/// 저장 시스템에서 식별자(ID)가 필요한 ScriptableObject의 기반 클래스.
/// CharacterSO 등이 이를 상속받아 ID로 저장/불러오기 시 매칭한다.
/// 예) CharacterDatabase.GetCharacterSO(characterID) 처럼 ID로 SO를 검색한다.
/// </summary>
public class SavebleSO : ScriptableObject
{
    /// <summary>
    /// 이 ScriptableObject를 고유하게 식별하는 문자열 ID.
    /// 저장 파일에 이 ID가 기록되며, 불러오기 시 CharacterDatabase에서 이 ID로 검색된다.
    /// Unity Inspector에서 직접 입력해야 하며, 같은 타입 내에서 중복되면 안 된다.
    /// </summary>
    public string ID;
}
