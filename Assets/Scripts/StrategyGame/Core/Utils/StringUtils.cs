using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 문자열 처리 유틸리티 클래스.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// 문자열에서 정규식 패턴에 맞는 부분을 모두 추출해 리스트로 반환한다.
    /// 주로 대화 파일이나 설정 텍스트에서 특정 태그 안의 값을 파싱할 때 사용한다.
    /// 예) input = "<Hero>용사</Hero>", pattern = "<Hero>(.*?)</Hero>" → ["용사"]
    /// </summary>
    /// <param name="input">검색 대상 문자열</param>
    /// <param name="pattern">정규식 패턴 (캡처 그룹 1개 이상 포함)</param>
    public static List<string> ExtractStringsInAngleBrackets(string input, string pattern)
    {
        List<string> extractedStrings = new List<string>();

        MatchCollection matches = Regex.Matches(input, pattern); // 패턴에 맞는 모든 매치 탐색

        foreach (Match match in matches)
        {
            // Groups[0] = 전체 매치, Groups[1] = 첫 번째 캡처 그룹 값
            extractedStrings.Add(match.Groups[1].Value);
        }

        return extractedStrings;
    }
}
