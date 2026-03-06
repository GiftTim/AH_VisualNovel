using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TagManager
{
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {

        {"<mainChar>",   () => "Avira" },
        {"<time>",       () => DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture) },
        {"<playerLevel>",() => "15"},
        { "<input>",     () => InputPanel.instance.lastInput },
        {"<tempVal1>",   () => "42"}
    };
    

    private static readonly Regex tagRegex = new Regex("<\\w+>");

    public static string Inject(string text)
    {
        // 텍스트에 태그 패턴(<...>)이 있는지 먼저 확인
        if (tagRegex.IsMatch(text))
        {
            // 발견된 모든 패턴(Match)에 대해 반복
            foreach (Match match in tagRegex.Matches(text))
            {
                // match.Value는 "<mainChar>"와 같이 추출된 문자열임
                // 딕셔너리에 해당 키가 있는지 확인 후 치환
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    // 딕셔너리에 등록된 함수(tagValueRequest)를 실행해 결과를 얻고 Replace 함
                    text = text.Replace(match.Value, tagValueRequest());
                }
            }
        }

        return text;
    }

}
