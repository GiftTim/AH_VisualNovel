using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DIALOGUE
{
    /*
     * DL_DIALOGUE_DATA
     * ─────────────────────────────────────────────────────────────────────
     * 대사 텍스트를 하나 이상의 DIALOGUE_SEGMENT로 분리해 저장하는 클래스.
     *
     * 세그먼트(Segment)란?
     *   하나의 대사 라인 안에 {c}, {a}, {wa 1.5} 등의 구분자가 있으면
     *   텍스트를 여러 부분으로 나눠 각 부분을 순서대로 출력할 수 있다.
     *   예) "안녕!{c}반가워{wa 2}오랜만이야"
     *       → 세그먼트 1: "안녕!" (NONE - 즉시 시작)
     *       → 세그먼트 2: "반가워" (C - 클릭 후 텍스트 초기화)
     *       → 세그먼트 3: "오랜만이야" (WA 2.0 - 2초 후 이어붙이기)
     * ─────────────────────────────────────────────────────────────────────
     */
    public class DL_DIALOGUE_DATA
    {
        // 파싱 전 원본 대사 텍스트
        public string rawData { get; private set; } = string.Empty;

        // 분리된 세그먼트 목록. 구분자가 없으면 원본 전체가 1개의 세그먼트.
        public List<DIALOGUE_SEGMENT> segments;

        /*
         * segmentIdentifierPattern
         * 세그먼트 구분자를 감지하는 정규식:
         *   {c}          → 클릭 후 텍스트 초기화(Clear)
         *   {a}          → 클릭 후 이어붙이기(Append)
         *   {n}          → 클릭 후 줄바꿈 이어붙이기(Newline)
         *   {wa 숫자}    → n초 대기 후 이어붙이기
         *   {wc 숫자}    → n초 대기 후 텍스트 초기화
         */
        private const string segmentIdentifierPattern = @"\{[can]\}|\{w[ca]\s\d*\.?\d*\}";

        public DL_DIALOGUE_DATA(string rawDialogue)
        {
            this.rawData = rawDialogue;
            segments = RipSegments(rawDialogue);
        }

        /*
         * RipSegments
         * rawDialogue에서 구분자 패턴을 정규식으로 찾아 세그먼트 목록을 생성한다.
         *
         * 흐름:
         *   1) 첫 번째 구분자 이전(또는 전체)을 StartSignal.NONE 세그먼트로 추가
         *   2) 이후 각 구분자를 순회하며 구분자 종류(StartSignal)와 딜레이,
         *      그리고 다음 구분자까지의 텍스트를 추출해 세그먼트로 추가
         */
        public List<DIALOGUE_SEGMENT> RipSegments(string rawDialogue)
        {
            List<DIALOGUE_SEGMENT> segments = new List<DIALOGUE_SEGMENT>();
            MatchCollection matches = Regex.Matches(rawDialogue, segmentIdentifierPattern);

            int lastIndex = 0;

            // Find the FIrst or only Segment in the file
            // 구분자 이전 텍스트(첫 세그먼트)를 StartSignal.NONE으로 추가
            DIALOGUE_SEGMENT segment = new DIALOGUE_SEGMENT();
            segment.dialogue = matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index);
            segment.startSignal = DIALOGUE_SEGMENT.StartSignal.NONE;
            segment.signalDelay = 0f;
            segments.Add(segment);

            // 구분자가 없으면 전체가 첫 세그먼트이므로 바로 반환
            if (matches.Count == 0)
                return segments;
            else
                lastIndex = matches[0].Index;

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                segment = new DIALOGUE_SEGMENT();

                //Get the start sgnal for te segment
                // 구분자 문자열 "{wa 1.5}" 에서 중괄호를 제거해 "wa 1.5" 추출
                string signalMatch = match.Value;//{A}
                signalMatch = signalMatch.Substring(1, match.Length - 2);
                string[] signalSplit = signalMatch.Split(' ');

                // 첫 번째 토큰("wa", "wc", "c", "a", "n")을 StartSignal 열거형으로 변환
                segment.startSignal = (DIALOGUE_SEGMENT.StartSignal)Enum.Parse(typeof(DIALOGUE_SEGMENT.StartSignal), signalSplit[0].ToUpper());

                //Get the signal delay
                // 두 번째 토큰이 있으면 대기 시간(초)으로 파싱
                if (signalSplit.Length > 1)
                    float.TryParse(signalSplit[1], out segment.signalDelay);

                //Get the dialogueData for the segment.
                // 이 구분자 끝부터 다음 구분자 시작까지가 이 세그먼트의 텍스트
                int nextIndex = i + 1 < matches.Count ? matches[i + 1].Index : rawDialogue.Length;
                segment.dialogue = rawDialogue.Substring(lastIndex + match.Length, nextIndex - (lastIndex + match.Length));
                lastIndex = nextIndex;

                segments.Add(segment);
            }

            return segments;

        }

        /*
         * DIALOGUE_SEGMENT
         * 세그먼트 하나의 데이터를 담는 구조체.
         *   - dialogue     : 이 세그먼트에서 출력할 텍스트
         *   - startSignal  : 이전 세그먼트가 끝난 후 어떻게 시작할지
         *   - signalDelay  : WA/WC의 대기 시간(초)
         */
        public struct DIALOGUE_SEGMENT
        {
            public string dialogue;
            public StartSignal startSignal;
            public float signalDelay;


            // N(Linebreak)을 추가했습니다.
            /*
             * StartSignal 열거형
             *   NONE : 즉시 시작 (첫 번째 세그먼트에 사용)
             *   C    : 사용자 클릭 후 텍스트를 초기화(Clear)하고 시작
             *   A    : 사용자 클릭 후 이전 텍스트에 이어붙여(Append) 시작
             *   WA   : signalDelay 초 대기 후 이어붙여(Append) 시작
             *   WC   : signalDelay 초 대기 후 텍스트를 초기화(Clear)하고 시작
             *   N    : 사용자 클릭 후 줄바꿈(\n)을 앞에 붙여 이어붙여(Append) 시작
             */
            public enum StartSignal { NONE, C, A, WA, WC, N }


            // 줄바꿈도 텍스트를 초기화하지 않고 이어 붙이는(Append) 동작이므로 N을 추가했습니다.
            // A, WA, N은 모두 이전 텍스트에 이어붙이는 동작 → appendText = true
            public bool appendText => startSignal == StartSignal.A || startSignal == StartSignal.WA || startSignal == StartSignal.N;
        }
    }
}
