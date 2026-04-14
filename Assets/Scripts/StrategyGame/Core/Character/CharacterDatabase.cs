using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 씬에 존재하는 CharacterSO 싱글턴 데이터베이스.
/// Awake 시점에 Resources 폴더에서 모든 CharacterSO를 로드하고 ID로 딕셔너리에 저장한다.
/// FactoryCharacterUnit이 불러오기 시 캐릭터 SO를 ID로 검색할 때 사용한다.
/// </summary>
public class CharacterDatabase : Singleton<CharacterDatabase>
{
    /// <summary>
    /// CharacterSO 인스턴스들이 위치한 Resources 기준 상대 경로.
    /// 예) "StrategyGame/Characters" → Resources/StrategyGame/Characters/ 폴더
    /// Inspector에서 설정한다.
    /// </summary>
    [SerializeField] private string _charactersFolder;

    // ID(string) → CharacterSO 매핑 딕셔너리. 빠른 검색을 위해 사용
    private Dictionary<string, CharacterSO> _characterSOs;

    /// <summary>
    /// 게임 시작 시 Resources에서 모든 CharacterSO를 로드해 딕셔너리에 등록한다.
    /// Awake는 Start보다 먼저 실행되므로 다른 스크립트가 Awake/Start에서 DB를 사용해도 안전하다.
    /// </summary>
    private void Awake()
    {
        _characterSOs = new Dictionary<string, CharacterSO>();

        // Resources.LoadAll: 해당 폴더의 모든 CharacterSO 에셋을 배열로 가져옴
        var characterList = Resources.LoadAll<CharacterSO>(_charactersFolder).ToList();

        characterList.ForEach(c =>
        {
            _characterSOs.Add(c.ID, c); // ID를 키로 등록
        });
    }

    /// <summary>
    /// ID로 CharacterSO를 검색해 반환한다.
    /// 없는 ID라면 null을 반환한다.
    /// </summary>
    public CharacterSO GetCharacterSO(string characterID)
    {
        if (_characterSOs.ContainsKey(characterID)) return _characterSOs[characterID];

        return null;
    }
}
