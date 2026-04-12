using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Expressions;

namespace DIALOGUE.LogicalLines
{
    /*
     * LL_Operator
     * ─────────────────────────────────────────────────────────────────────
     * 대화 스크립트에서 변수 대입 및 산술 연산을 처리하는 논리 라인 구현체.
     * keyword 대신 정규식(REGEX_OPERATOR_LINE)으로 해당 라인을 감지한다.
     *
     * 지원 문법:
     *   $score = 10       → score 변수에 10 대입 (없으면 새로 생성)
     *   $hp += 5          → hp += 5
     *   $name = "KIM"     → name에 문자열 대입
     *   $total = $a + $b  → 수식 계산 후 대입
     *   $hp -= 10         → hp -= 10
     *   $rate *= 2        → rate *= 2
     *   $hp /= 2          → hp /= 2
     * ─────────────────────────────────────────────────────────────────────
     */
    public class LL_Operator : ILogicalLine
    {
        /*
         * keyword
         * LL_Operator는 키워드 대신 정규식으로 라인을 감지하므로 keyword가 필요 없다.
         * ILogicalLine 인터페이스 구현을 위해 선언하지만, 실제로 호출되면 예외 발생.
         * LogicalLineManager는 TryGetLogic에서 Matches()만 호출하고
         * keyword를 직접 참조하지 않으므로 런타임 에러는 발생하지 않는다.
         */
        string ILogicalLine.keyword => throw new System.NotImplementedException();

        /*
         * Execute
         * 변수 연산 라인을 파싱해 VariableStore에 값을 적용한다.
         *
         * 파싱 흐름:
         *   1) Regex.Split(REGEX_ARITHMATIC)으로 trimmedLine을 분리
         *      예) "$score += 5" → ["$score ", "+=", " 5"]
         *   2) parts[0] = 변수명 ($제거), parts[1] = 연산자, parts[2~] = 우변 수식
         *   3) CalculateValue()로 우변 수식 계산
         *   4) ProcessOperator()로 변수에 연산 결과 적용
         *
         * parts.Length < 3이면:
         *   유효한 연산자가 없다는 의미 → 오류 출력 후 yield break로 조기 종료
         */
        IEnumerator ILogicalLine.Execute(DIALOGUE_LINE line)
        {
            string trimmedLine = line.rawData.Trim();
            // 산술 연산자를 구분자로 분리 (연산자 자체도 결과에 포함됨)
            string[] parts = Regex.Split(trimmedLine, REGEX_ARITHMATIC);

            if (parts.Length < 3)
            {
                Debug.LogError($"유효하지 않은 명령어(Invalid command): {trimmedLine}");
                yield break; // 잘못된 형식이면 코루틴 즉시 종료
            }

            // '$' 제거해 순수 변수명 추출
            string variable = parts[0].Trim().TrimStart(VariableStore.VARIABLE_ID);
            string op = parts[1].Trim(); // 연산자 (=, +=, -=, *=, /=)

            // parts[2] 이후가 우변 수식 → 배열로 복사해 CalculateValue에 전달
            string[] remainingParts = new string[parts.Length - 2];
            Array.Copy(parts, 2, remainingParts, 0, parts.Length - 2);

            // 우변 수식 계산 (변수 조회, 사칙연산 적용 등)
            object value = CalculateValue(remainingParts);

            if (value == null)
            {
                yield break; // 계산 실패 시 종료 (ExtractValue에서 오류 출력됨)
            }

            ProcessOperator(variable, op, value);
        }

        /*
         * ProcessOperator
         * 변수 존재 여부에 따라 대입/연산 또는 새 변수 생성을 결정한다.
         *
         * 분기:
         *   - 변수가 있으면: ProcessOperatorOnVariable로 연산 적용
         *   - 변수가 없고 op == "=": CreateVariable로 새 변수 생성 (초기 대입)
         *   - 변수가 없고 op != "=": 없는 변수에 +=, -= 등은 불가 → 경고 출력
         */
        private void ProcessOperator(string variable, string op, object value)
        {
            if(VariableStore.TryGetValue(variable, out object currentValue))
            {
                ProcessOperatorOnVariable(variable, op, value, currentValue);
            }
            else if (op == "=")
            {
                // 변수가 없을 때 "=" 연산이면 새 변수를 생성
                VariableStore.CreateVariable(variable, value);
            }
            else
            {
                // 변수가 없는데 += 등을 사용하면 경고 출력 (존재하지 않는 변수에는 연산 불가)
                Debug.LogWarning($"변수 '{variable}'이 존재하지 않습니다. '=' 연산자만 사용할 수 있습니다. (Variable '{variable}' does not exist. Only '=' operator can be used.)");
            }
        }

        /*
         * ProcessOperatorOnVariable
         * 이미 존재하는 변수에 연산자를 적용한다.
         *
         * 각 연산자 동작:
         *   "="  : 단순 대입 (기존 값 덮어쓰기)
         *   "+=" : ConcatenateOrAdd → 문자열이면 연결, 숫자면 덧셈
         *   "-=" : double 변환 후 뺄셈
         *   "*=" : double 변환 후 곱셈
         *   "/=" : double 변환 후 나눗셈 (0 나누기 방지 포함)
         */
        private void ProcessOperatorOnVariable(string variable, string op, object value, object currentValue)
        {
            switch (op)
            {
                case "=": // 단순 대입
                    VariableStore.TrySetValue(variable, value);
                    break;
                case "+=": // 문자열이면 연결, 숫자면 덧셈
                    VariableStore.TrySetValue(variable, ConcatenateOrAdd(value, currentValue));
                    break;
                case "-=": // 뺄셈 (double 변환 후 처리)
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) - Convert.ToDouble(value));
                    break;
                case "*=": // 곱셈 (double 변환 후 처리)
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) * Convert.ToDouble(value));
                    break;
                case "/=": // 나눗셈 (0으로 나누기 방지)
                    if (Convert.ToDouble(value) == 0)
                    {
                        Debug.LogError("0으로 나눌 수 없습니다!");
                        return;
                    }
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) / Convert.ToDouble(value));
                    break;
                default:
                    Debug.LogError($"알 수 없는 연산자(Unknown operator): {op}");
                    break;
            }
        }

        /*
         * ConcatenateOrAdd
         * "+=" 연산 시 타입에 따라 동작을 결정한다.
         *   value가 string이면: currentValue.ToString() + value (문자열 연결)
         *   그 외             : double 변환 후 덧셈 (숫자 연산)
         *
         * 분기 이유: $name += "KIM" (문자열 연결)과 $score += 5 (숫자 덧셈)를 모두 지원하기 위함.
         */
        private object ConcatenateOrAdd(object value, object currentValue)
        {
            if (value is string)
                return currentValue.ToString() + value; // 문자열 연결

            return Convert.ToDouble(currentValue) + Convert.ToDouble(value); // 숫자 덧셈
        }

        /*
         * Matches
         * REGEX_OPERATOR_LINE 패턴으로 $변수명 연산자 형태인지 감지한다.
         * 키워드 비교가 아닌 regex 매칭으로 판별하는 이유:
         *   변수명은 임의의 문자열이므로 고정 키워드로는 감지 불가.
         *   정규식 패턴: ^ $변수명 (=|+=|-=|*=/=|) 형태
         */
        bool ILogicalLine.Matches(DIALOGUE_LINE line)
        {
            Match match = Regex.Match(line.rawData.Trim(), REGEX_OPERATOR_LINE);

            return match.Success; // 패턴에 맞으면 이 핸들러가 처리
        }
    }
}
