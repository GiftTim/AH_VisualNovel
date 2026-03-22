using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using VISUALNOVEL;

public class TagManager
{
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {

        {"<mainChar>",   () => VNGameSave.activeFile.playerName },
        {"<time>",       () => DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture) },
        {"<playerLevel>",() => "15"},
        { "<input>",     () => InputPanel.instance.lastInput },
    };
    

    private static readonly Regex tagRegex = new Regex("<\\w+>");

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


    private static string InjectVariables(string value)
    {
        var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
        var matchesList = matches.Cast<Match>().ToList();

        for(int i = matchesList.Count - 1; i >= 0; i--)
        {
            var match = matchesList[i];
            string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID, '!');
            bool negate = match.Value.StartsWith('!');

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
            
            if(negate && variableValue is bool)
            {
                variableValue = !(bool)variableValue;
            }

            int lengthToBeRemoved = match.Index + match.Length > value.Length ? value.Length - match.Index : match.Length;
            if(endsInIllegalCharacter)
            {
                lengthToBeRemoved -= 1;
            }

            value = value.Remove(match.Index, lengthToBeRemoved);
            value = value.Insert(match.Index, variableValue.ToString());
        }

        return value;

    }
}
