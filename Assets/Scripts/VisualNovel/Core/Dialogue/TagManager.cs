using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TagManager
{
    // 태그 이름과 그에 대응하는 값을 반환하는 함수를 저장하는 딕셔너리
    private readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>();
    // <태그명> 형태를 찾기 위한 정규식 패턴
    private readonly Regex tagRegex = new Regex("<\\w+>");

    public TagManager()
    {
        InitializeTags();
    }  
    
    private void InitializeTags()
    {
        // 정적인 값 또는 동적인 함수를 등록
        tags["<mainChar>"] = () => "Avira";
        tags["<time>"] = () => DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        tags["<playerLevel>"] = () => "15";
        tags["<tempVal1>"] = () => "42";
    }

    // 4. 입력받은 텍스트 내의 모든 태그를 실제 데이터로 치환
    public string Inject(string text)
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
