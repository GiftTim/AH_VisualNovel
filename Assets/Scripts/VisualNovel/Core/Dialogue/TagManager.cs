using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using VISUALNOVEL;

/*
 * TagManager
 * ─────────────────────────────────────────────────────────────────────
 * 대사 텍스트에 포함된 두 종류의 치환을 처리하는 정적 유틸리티 클래스.
 *
 *   1) 태그 치환 (<태그이름>)
 *      - 딕셔너리 'tags'에 등록된 키를 찾아 Func<string>의 반환값으로 교체.
 *      - 예) "<mainChar>" → VNGameSave.activeFile.playerName
 *      - Func<string> 설계로 지연 평가(Lazy Evaluation) 지원:
 *        딕셔너리 등록 시점이 아닌 Inject() 호출 시점에 값을 계산함.
 *
 *   2) 변수 치환 ($변수명)
 *      - VariableStore에 등록된 게임 변수를 문자열로 교체.
 *      - "!" 접두사 사용 시 bool 변수를 부정(negate)해 치환.
 *      - 예) "$playerName" → "KIM", "!$isHappy" → "False"
 * ─────────────────────────────────────────────────────────────────────
 */
public class TagManager
{
    /*
     * tags 딕셔너리
     * 태그 이름 → 값을 반환하는 함수(Func<string>) 매핑.
     * Func<string>을 사용하는 이유: 태그 치환 시점에 최신 값을 반환하기 위함.
     * (예: <time>은 호출할 때마다 현재 시간을 반환해야 한다.)
     */
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {
        {"<mainChar>",   () => VNGameSave.activeFile.playerName },       // 플레이어 이름
        {"<time>",       () => DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture) }, // 현재 시간
        {"<playerLevel>",() => "15"},                                    // 플레이어 레벨 (하드코딩 예시)
        { "<input>",     () => InputPanel.instance.lastInput },          // 마지막 입력 패널 값
    };


    // 태그 패턴 감지용 정규식: <알파벳/숫자로 이루어진 단어>
    private static readonly Regex tagRegex = new Regex("<\\w+>");

    /*
     * Inject
     * 텍스트에 태그 치환과 변수 치환을 순서대로 적용한다.
     * injectTags / injectVariable 매개변수로 각 치환을 선택적으로 활성화할 수 있다.
     * (예: speaker에는 변수만 치환, commands에는 둘 다 치환 등)
     */
    public static string Inject(string text, bool injectTags = true, bool injectVariable = true)
    {
        if (injectTags)
        {
            text = InjectTags(text);
        }

        if (injectVariable)
        {
            text = InjectVariables(text);
        }

        return text;
    }

    /*
     * InjectTags
     * tagRegex로 태그 패턴을 모두 찾아 딕셔너리 값으로 교체한다.
     * 딕셔너리에 없는 태그는 교체하지 않고 그대로 남긴다.
     */
    private static string InjectTags(string value)
    {
        // 텍스트에 태그 패턴(<...>)이 있는지 먼저 확인
        if (tagRegex.IsMatch(value))
        {
            // 발견된 모든 패턴(Match)에 대해 반복
            foreach (Match match in tagRegex.Matches(value))
            {
                // match.Value는 "<mainChar>"와 같이 추출된 문자열임
                // 딕셔너리에 해당 키가 있는지 확인 후 치환
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    // 딕셔너리에 등록된 함수(tagValueRequest)를 실행해 결과를 얻고 Replace 함
                    value = value.Replace(match.Value, tagValueRequest());
                }
            }
        }

        return value;
    }


    /*
     * InjectVariables
     * VariableStore에 등록된 변수를 텍스트에서 찾아 실제 값으로 교체한다.
     *
     * 역방향 순회(i = matchesList.Count - 1 → 0) 이유:
     *   앞에서 뒤로 교체하면 이후 매치의 인덱스가 변경되어 오프셋이 어긋남.
     *   뒤에서 앞으로 교체하면 아직 처리되지 않은 앞 매치의 인덱스가 영향을 받지 않음.
     */
    private static string InjectVariables(string value)
    {
        var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
        var matchesList = matches.Cast<Match>().ToList();

        // 역방향 순회: 뒤에서부터 교체해야 앞 매치의 인덱스가 유지됨
        for(int i = matchesList.Count - 1; i >= 0; i--)
        {
            var match = matchesList[i];
            // '$' 또는 '!' 접두사를 제거해 순수 변수 이름만 추출
            string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID, '!');
            // '!' 접두사가 있으면 bool 변수를 부정(negate)해 치환
            bool negate = match.Value.StartsWith('!');

            // 변수 이름이 '.'으로 끝나면 문장 부호로 판단해 제거 후 처리
            bool endsInIllegalCharacter = variableName.EndsWith(VariableStore.DATABASE_VARIABLE_RELATIONAL_ID);
            if(endsInIllegalCharacter)
            {
                variableName = variableName.Substring(0, variableName.Length - 1);
            }

            if (!VariableStore.TryGetValue(variableName, out object variableValue))
            {
                UnityEngine.Debug.LogError($"문자열 할당(Assignment) 과정에서 변수 '{variableName}'을(를) 찾을 수 없습니다");
                continue;
            }

            // bool 변수에 '!' 접두사가 붙었으면 반전
            if(negate && variableValue is bool)
            {
                variableValue = !(bool)variableValue;
            }

            // '.'으로 끝났던 경우 제거 길이를 1 줄여서 '.'는 교체하지 않음
            int lengthToBeRemoved = match.Index + match.Length > value.Length ? value.Length - match.Index : match.Length;
            if(endsInIllegalCharacter)
            {
                lengthToBeRemoved -= 1;
            }

            // 매치 위치의 문자를 제거하고 변수 값을 삽입
            value = value.Remove(match.Index, lengthToBeRemoved);
            value = value.Insert(match.Index, variableValue.ToString());
        }

        return value;

    }
}
