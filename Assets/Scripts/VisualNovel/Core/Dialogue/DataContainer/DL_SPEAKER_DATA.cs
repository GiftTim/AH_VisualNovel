using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    /*
     * DL_SPEAKER_DATA
     * ─────────────────────────────────────────────────────────────────────
     * 대화 라인의 화자(Speaker) 정보를 파싱해 저장하는 클래스.
     * DialogueParser가 rawLine에서 분리한 화자 문자열을 입력으로 받는다.
     *
     * 지원하는 화자 문법 예시:
     *   "KIM"                          → 이름만
     *   "enter KIM"                    → 화면에 없으면 캐릭터를 등장(enter)시킴
     *   "KIM as 별명"                  → 대화창에 '별명'으로 표시
     *   "KIM at 0.5"                   → 화면 X 위치 0.5로 이동
     *   "KIM at 0.5:0.2"               → 화면 X, Y 위치로 이동
     *   "KIM [0:happy,1:sad]"          → 레이어 0은 happy, 레이어 1은 sad 표정
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DL_SPEAKER_DATA
    {
        // 파싱 전 원본 화자 문자열
        public string rawData { get; private set; } = string.Empty;

        // name     : 실제 캐릭터 이름 (CharacterManager에서 조회할 때 사용)
        // castName : " as " 뒤에 오는 별칭 (대화창에 표시되는 이름, 없으면 "")
        public string name, castName;

        /// <summary>
        /// This is the name that will display in the dialogueData box to show who is speaking
        /// </summary>
        // castName이 있으면 castName, 없으면 name을 대화창에 표시
        public string displayname => isCastingName ? castName : name;

        // " at " 뒤에 오는 좌표값 (화면상 위치 캐스팅)
        public Vector2 castPosition;

        // " [레이어:표정, ...]" 형태의 표정 캐스팅 목록
        public List<(int layer, string expression)> CastExpressions { get; set; }

        // 별칭 캐스팅 여부 (castName이 비어있지 않으면 true)
        public bool isCastingName => castName != string.Empty;
        // 위치 캐스팅 여부
        public bool isCastingPosition = false;
        // 표정 캐스팅 여부 (목록이 비어있지 않으면 true)
        public bool isCastingExpression => CastExpressions.Count > 0;

        // "enter " 키워드가 있을 때 true → 화면에 없으면 캐릭터를 등장시킴
        public bool makeCharacerEnter = false;

        // Regex 분리에 사용하는 식별자 상수
        private const string NAMECAST_ID       = " as ";  // 별칭 구분자
        private const string POSITIONCAST_ID   = " at ";  // 위치 구분자
        private const string EXPRESSIONCAST_ID = " [";    // 표정 구분자 시작
        private const char AXISDELIMITER_ID            = ':';  // X:Y 축 구분자
        private const char EXPRESSIONLAYER_JOINER      = ',';  // 표정 항목 구분자
        private const char EXPRESSIONLAYER_DELIMITER   = ':';  // 레이어:표정 구분자

        // 화자 문자열 앞에 "enter "가 붙으면 캐릭터를 등장시키는 키워드
        private const string ENTER_KEYWORD = "enter ";

        /*
         * ProcessKeywords
         * rawSpeaker 앞에 "enter " 키워드가 있으면 해당 부분을 제거하고
         * makeCharacerEnter = true 로 설정한다.
         * 실제 이름 파싱 전에 키워드를 먼저 처리해 이름에 섞이지 않게 함.
         */
        private string ProcessKeywords(string rawSpeaker)
        {
            if (rawSpeaker.ToLower().StartsWith(ENTER_KEYWORD))
            {
                rawSpeaker = rawSpeaker.Substring(ENTER_KEYWORD.Length);
                makeCharacerEnter = true;
            }
            return rawSpeaker;
        }

        /*
         * 생성자
         * Regex로 " as " / " at " / " [" 식별자를 감지해 매치 위치를 찾고
         * 각 매치 사이 구간을 잘라내 name / castName / castPosition / CastExpressions 를 채운다.
         * 매치가 0개이면 전체 문자열이 이름이다.
         */
        public DL_SPEAKER_DATA(string rawSpeaker)
        {
            rawData = rawSpeaker;
            rawSpeaker = ProcessKeywords(rawSpeaker);

            // 세 가지 캐스팅 식별자를 동시에 감지하는 패턴
            string pattern = @$"{NAMECAST_ID}|{POSITIONCAST_ID}|{EXPRESSIONCAST_ID.Insert(EXPRESSIONCAST_ID.Length - 1, @"\")}";
            MatchCollection matches = Regex.Matches(rawSpeaker, pattern);

            // Populate this data to avoid null references to values
            // null 참조 방지를 위해 기본값으로 초기화
            castName = "";
            castPosition = Vector2.zero;
            CastExpressions = new List<(int layer, string expression)>();

            // if there are no matches,then this entire line is the speakerData name
            // 캐스팅 식별자가 없으면 전체 문자열이 이름
            if (matches.Count == 0)
            {
                name = rawSpeaker.Trim();
                return;
            }

            // Otherwise, isolate the speakername from the casting data.
            // 첫 번째 식별자 이전 문자열이 실제 캐릭터 이름
            int index = matches[0].Index;
            name = rawSpeaker.Substring(0, index).Trim();

            // 각 매치를 순회하며 해당 캐스팅 데이터를 파싱
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int startIndex = 0, endIndex = 0;

                if (match.Value == NAMECAST_ID)
                {
                    // " as " 뒤부터 다음 식별자(또는 끝)까지가 별칭
                    startIndex = match.Index + NAMECAST_ID.Length;
                    endIndex = i < matches.Count - 1 ? matches[i + 1].Index : rawSpeaker.Length;
                    castName = rawSpeaker.Substring(startIndex, endIndex - startIndex).Trim();
                }
                else if (match.Value == POSITIONCAST_ID)
                {
                    isCastingPosition = true;
                    // " at " 뒤부터 다음 식별자(또는 끝)까지가 좌표 문자열
                    startIndex = match.Index + POSITIONCAST_ID.Length;
                    endIndex = i < matches.Count - 1 ? matches[i + 1].Index : rawSpeaker.Length;
                    string castPos = rawSpeaker.Substring(startIndex, endIndex - startIndex).Trim();

                    // "X" 또는 "X:Y" 형식으로 파싱
                    string[] axis = castPos.Split(AXISDELIMITER_ID, System.StringSplitOptions.RemoveEmptyEntries);

                    float.TryParse(axis[0], out castPosition.x);

                    if (axis.Length > 1)
                        float.TryParse(axis[1], out castPosition.y);
                }
                else if (match.Value == EXPRESSIONCAST_ID)
                {
                    // " [" 뒤부터 닫는 ']' 직전까지가 표정 문자열
                    startIndex = match.Index + EXPRESSIONCAST_ID.Length;
                    endIndex = i < matches.Count - 1 ? matches[i + 1].Index : rawSpeaker.Length;
                    string castExp = rawSpeaker.Substring(startIndex, endIndex - (startIndex+1));

                    // "레이어:표정, 레이어:표정" 형태를 (int layer, string expression) 튜플 목록으로 변환
                    CastExpressions = castExp.Split(EXPRESSIONLAYER_JOINER)
                    .Select(x =>
                    {
                        var parts = x.Trim().Split(EXPRESSIONLAYER_DELIMITER);

                        if(parts.Length == 2)
                            return (int.Parse(parts[0]), parts[1]);
                        else
                            return(0, parts[0]); // 레이어 미지정 시 기본값 0

                    }).ToList();
                }

            }
        }
    }
}
