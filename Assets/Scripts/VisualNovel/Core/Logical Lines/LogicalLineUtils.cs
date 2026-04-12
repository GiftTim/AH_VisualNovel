using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    /*
     * LogicalLineUtils
     * ─────────────────────────────────────────────────────────────────────
     * 논리 라인 처리에 필요한 세 가지 정적 유틸리티 클래스를 묶은 컨테이너.
     *
     *   Encapsulation : { } 블록(캡슐화 영역)의 줄 목록과 파일 인덱스를 추출
     *   Expressions   : 수식 파싱·계산 및 단일 값 추출
     *   Conditions    : ==, !=, <, >, <=, >=, &&, || 조건 평가
     *
     * 하나의 정적 클래스에 묶는 이유:
     *   - 논리 라인끼리 공유하는 기능들을 한 곳에 모아 중복 방지
     *   - 네임스페이스처럼 기능별로 분리해 using static으로 선택적 임포트 가능
     * ─────────────────────────────────────────────────────────────────────
     */
    public static class LogicalLineUtils
    {
        /*
         * ════════════════════════════════════════════════════════════════
         * Encapsulation
         * { } 블록의 내용(줄 목록)과 파일 기준 절대 인덱스를 추출하는 유틸리티.
         * choice, if/else 등에서 블록 내 줄을 파싱할 때 사용.
         * ════════════════════════════════════════════════════════════════
         */
        public static class Encapsulation
        {
            /*
             * EncapsulatedData 구조체
             * RipEncapsulationData()의 반환 결과를 담는 구조체.
             *   lines         : 추출된 줄 목록 (null이면 데이터 없음 → isNull = true)
             *   startingIndex : 이 블록이 시작하는 파일 기준 절대 인덱스
             *   endingIndex   : 이 블록이 끝나는 파일 기준 절대 인덱스 (닫는 '}' 줄)
             *   isNull        : lines가 null이면 유효하지 않은 데이터임을 나타냄
             */
            public struct EncapsulatedData
            {
                public bool isNull => lines == null; // null 여부로 유효성 판단
                public List<string> lines;
                public int startingIndex;
                public int endingIndex;
            }

            // 블록 시작 문자: '{' 로 시작하는 줄이 블록의 시작
            private const char ENCAPSULATION_START = '{';
            // 블록 종료 문자: '}' 로 시작하는 줄이 블록의 종료
            private const char ENCAPSULATION_END = '}';

            /*
             * RipEncapsulationData
             * conversation에서 startingIndex부터 { } 블록을 찾아 내용을 추출한다.
             *
             * 매개변수:
             *   conversation           : 전체 대화 줄 목록
             *   startingIndex          : 탐색을 시작할 줄 번호 (로컬 인덱스, Conversation 내부 기준)
             *   ripHeaderAndEncapsulators : true이면 헤더 줄(choice/if 라인)과 { } 줄 자체도 포함
             *                              false이면 { } 안의 내용만 추출
             *   parentStartingIndex    : Conversation의 파일 기준 시작 오프셋.
             *                           startingIndex + parentStartingIndex = 파일 기준 절대 인덱스
             *
             * encapsulationDepth 카운터:
             *   { 를 만날 때마다 ++, } 를 만날 때마다 --.
             *   0이 되면 현재 블록이 끝났음을 의미.
             *   중첩 블록(블록 안의 블록)을 올바르게 처리하기 위해 필요.
             */
            public static EncapsulatedData RipEncapsulationData(Conversation conversation, int startingIndex, bool ripHeaderAndEncapsulators = false, int parentStartingIndex = 0)
            {
                int encapsulationDepth = 0;
                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startingIndex = (startingIndex + parentStartingIndex), endingIndex = 0 };

                for (int i = startingIndex; i < conversation.Count; i++)
                {
                    string line = conversation.GetLines()[i];

                    // ripHeaderAndEncapsulators=true 이거나, 블록 안(depth>0)이면서 닫는 '}'가 아닌 줄만 추가
                    if (ripHeaderAndEncapsulators || (encapsulationDepth > 0 && !IsEncapsulationEnd(line)))
                    {
                        data.lines.Add(line);
                    }

                    if (IsEncapsulationStart(line))
                    {
                        encapsulationDepth++; // 블록 깊이 증가
                        continue;
                    }

                    if (IsEncapsulationEnd(line))
                    {
                        encapsulationDepth--; // 블록 깊이 감소
                        if (encapsulationDepth == 0)
                        {
                            // 블록이 완전히 닫혔으면 절대 인덱스를 기록하고 탐색 종료
                            data.endingIndex = (i + parentStartingIndex);
                            break;
                        }
                    }
                }

                return data;
            }

            // '{' 로 시작하는 줄인지 확인. Trim()으로 앞뒤 공백 제거 후 비교.
            public static bool IsEncapsulationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START);
            // '}' 로 시작하는 줄인지 확인. Trim()으로 앞뒤 공백 제거 후 비교.
            public static bool IsEncapsulationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END);
        }

        /*
         * ════════════════════════════════════════════════════════════════
         * Expressions
         * 수식 문자열을 파싱·계산하고, 단일 값을 추출하는 유틸리티.
         * LL_Operator에서 변수 연산($score += 5 등)에 사용.
         * ════════════════════════════════════════════════════════════════
         */
        public static class Expressions
        {
            // 지원하는 산술/대입 연산자 목록
            public static HashSet<string> OPERATORS = new HashSet<string>()
            { "-", "-=", "+", "+=", "*", "*=", "/", "/=", "=" };

            // 산술 연산자(+, -, *, /, =, +=, -=, *=, /=)를 구분자로 Split하는 정규식
            // Regex.Split에 사용하면 연산자도 결과 배열에 포함됨 → [피연산자, 연산자, 피연산자, ...] 형태
            public static readonly string REGEX_ARITHMATIC = @"([-+*/=]=?)";

            // $변수명 연산자 형태의 라인을 감지하는 정규식
            // LL_Operator.Matches()에서 이 패턴으로 해당 라인이 변수 연산인지 판별
            public static readonly string REGEX_OPERATOR_LINE = @"^\$\w+\s*(=|\+=|-=|\*=/=|)\s*";

            /*
             * CalculateValue
             * expressionParts 배열로부터 최종 계산 결과를 반환한다.
             *
             * 계산 흐름 (연산자 우선순위 적용):
             *   1) expressionParts에서 연산자 문자열과 피연산자 문자열을 분리
             *   2) ExtractValue()로 각 피연산자 문자열을 실제 값(object)으로 변환
             *   3) 곱셈/나눗셈 먼저 처리 (CalculateValue_DivisionAndMultiplication)
             *   4) 덧셈/뺄셈 나중 처리 (CalculateValue_AdditionAndSubtraction)
             *   5) 최종적으로 operands[0]에 결과가 남음
             */
            public static object CalculateValue(string[] expressionParts)
            {
                List<string> operandStrings = new List<string>();
                List<string> operatorStrings = new List<string>();
                List<object> operands = new List<object>();

                for (int i = 0; i < expressionParts.Length; i++)
                {
                    string part = expressionParts[i].Trim();

                    if (part == string.Empty)
                    {
                        continue; // 빈 문자열은 무시 (Split 결과에 빈 항목이 포함될 수 있음)
                    }

                    if (OPERATORS.Contains(part))
                    {
                        operatorStrings.Add(part); // 연산자 분류
                    }
                    else
                    {
                        operandStrings.Add(part); // 피연산자 분류
                    }
                }

                // 각 피연산자 문자열을 실제 값으로 변환 (변수 조회, 숫자 파싱, 문자열 처리 등)
                foreach (string operandString in operandStrings)
                {
                    operands.Add(ExtractValue(operandString));
                }

                // 연산자 우선순위: 곱셈/나눗셈 먼저
                CalculateValue_DivisionAndMultiplication(operatorStrings, operands);

                // 연산자 우선순위: 덧셈/뺄셈 나중
                CalculateValue_AdditionAndSubtraction(operatorStrings, operands);

                return operands[0]; // 모든 계산 후 남은 최종 결과
            }

            /*
             * CalculateValue_DivisionAndMultiplication
             * operatorStrings와 operands를 순회하며 곱셈(*) 과 나눗셈(/) 을 처리한다.
             * in-place 교체: 계산 결과를 operands[i]에 저장하고 operands[i+1]을 제거.
             * i-- 이유: 요소 제거 후 인덱스가 앞으로 당겨지므로 다음 반복에서 같은 위치 재확인 필요.
             *
             * [주의] 나눗셈 버그:
             *   else 블록 안의 if(rightOperand == 0) 조건이 중첩되어 있어
             *   operands[i] = leftOperand / rightOperand 코드가 절대 실행되지 않음.
             *   외부 if(rightOperand == 0)가 true → 내부 if(rightOperand == 0)도 항상 true
             *   → Debug.LogError 후 return → 나눗셈 결과 저장 줄에 도달 불가.
             *   코드 로직을 변경하지 않으므로 현재 상태 그대로 유지.
             *
             * [추가 주의] operands.Remove(i + 1):
             *   List<object>.Remove()는 인수로 전달된 값(value)을 찾아 제거함.
             *   여기서 i+1은 int이므로, 리스트에서 값이 (int)(i+1)인 요소를 찾아 제거.
             *   의도한 동작은 RemoveAt(i+1)일 가능성이 높으나 코드 변경 없이 유지.
             */
            private static void CalculateValue_DivisionAndMultiplication(List<string> operatorStrings, List<object> operands)
            {
                for (int i = 0; i < operatorStrings.Count; i++)
                {
                    string operatorString = operatorStrings[i];

                    if (operatorString == "*" || operatorString == "/")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (operatorString == "*")
                        {
                            operands[i] = leftOperand * rightOperand;
                        }
                        else
                        {
                            if (rightOperand == 0) // [주의] 이 조건이 true이면 아래 if도 항상 true
                            {
                                if (rightOperand == 0) // 0으로 나눌 수 없음
                                {
                                    Debug.LogError("0으로 나눌 수 없습니다!");
                                    return;
                                }
                                operands[i] = leftOperand / rightOperand; // [버그] 이 줄은 절대 실행되지 않음
                            }
                        }

                        operands.Remove(i + 1);          // [주의] Remove(value)이므로 의도와 다를 수 있음
                        operatorStrings.RemoveAt(i);
                        i--; // 요소 제거로 인한 인덱스 보정
                    }
                }
            }

            /*
             * CalculateValue_AdditionAndSubtraction
             * operatorStrings와 operands를 순회하며 덧셈(+) 과 뺄셈(-) 을 처리한다.
             * in-place 교체 + i-- 패턴은 위의 곱셈/나눗셈과 동일한 이유.
             */
            private static void CalculateValue_AdditionAndSubtraction(List<string> operatorStrings, List<object> operands)
            {
                for (int i = 0; i < operatorStrings.Count; i++)
                {
                    string operatorString = operatorStrings[i];

                    if (operatorString == "+" || operatorString == "-")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (operatorString == "+")
                        {
                            operands[i] = leftOperand + rightOperand;
                        }
                        else
                        {
                            operands[i] = leftOperand - rightOperand;
                        }
                        operands.Remove(i + 1);          // [주의] Remove(value)이므로 의도와 다를 수 있음
                        operatorStrings.RemoveAt(i);
                        i--; // 요소 제거로 인한 인덱스 보정
                    }
                }
            }

            /*
             * ExtractValue
             * 단일 문자열 값을 실제 타입의 object로 변환한다.
             *
             * 4가지 분기:
             *   1) '$' 시작         → VariableStore에서 변수 값 조회
             *   2) '"' 시작 + '"' 끝 → 문자열 리터럴 (TagManager 치환 후 따옴표 제거)
             *   3) 숫자(int/float)   → TryParse (int 먼저, float 나중)
             *   4) bool              → TryParse (negate 처리 포함)
             *   5) 기타              → TagManager.Inject 후 문자열로 반환
             *
             * '!' 접두사(negate) 처리:
             *   값 앞에 '!'가 있으면 bool 변수를 반전해서 반환.
             *   예) !$isHappy → !true = false
             */
            public static object ExtractValue(string value)
            {
                bool negate = false;

                // '!' 접두사 감지: bool 변수를 부정(반전)해서 반환할 플래그
                if (value.StartsWith('!'))
                {
                    negate = true;
                    value = value.Substring(1); // '!' 제거
                }

                if (value.StartsWith(VariableStore.VARIABLE_ID)) // '$' 시작 → 변수 조회
                {
                    string variableName = value.TrimStart(VariableStore.VARIABLE_ID); // '$' 제거
                    if (!VariableStore.HasVariable(variableName))
                    {
                        Debug.LogError($"Variable {variableName} does not exist.");
                        return null;
                    }

                    VariableStore.TryGetValue(variableName, out object val);

                    // bool 변수에 '!' 접두사가 있었으면 반전
                    if (val is bool boolVal && negate)
                    {
                        return !boolVal;
                    }

                    return val;
                }
                else if (value.StartsWith('\"') && value.EndsWith('\"')) // 문자열 리터럴
                {
                    // TagManager로 태그·변수 치환 후 따옴표 제거
                    value = TagManager.Inject(value, injectTags: true, injectVariable: true);
                    return value.Trim('"');
                }
                else
                {
                    // 숫자(int) 파싱 시도
                    if (int.TryParse(value, out int intvalue))
                    {
                        return intvalue;
                    }
                    // 숫자(float) 파싱 시도 (int 실패 후)
                    else if (float.TryParse(value, out float floatValue))
                    {
                        return floatValue;
                    }
                    // bool 파싱 시도 (negate 처리 포함)
                    else if (bool.TryParse(value, out bool boolValue))
                    {
                        return negate ? !boolValue : boolValue;
                    }
                    else
                    {
                        // 위 타입 모두 실패 → TagManager 치환 후 문자열로 처리
                        value = TagManager.Inject(value, injectTags: true, injectVariable: true);
                        return value;
                    }

                }
            }
        }

        /*
         * ════════════════════════════════════════════════════════════════
         * Conditions
         * 조건 문자열을 평가해 bool 결과를 반환하는 유틸리티.
         * LL_Condition에서 if(조건) 평가에 사용.
         * ════════════════════════════════════════════════════════════════
         */
        public static class Conditions
        {
            // 비교·논리 연산자를 구분자로 Split하는 정규식
            // 예) "$score >= 10" → ["$score ", ">=", " 10"]
            public static readonly string REGEX_CONDITION_LINE = @"(==|!=|<=|>=|<|>|&&|\|\|)";

            /*
             * EvaluateCondition
             * 조건 문자열을 평가해 true/false를 반환한다.
             *
             * 처리 흐름:
             *   1) TagManager.Inject()로 태그·변수를 먼저 치환 ($score → "10" 등)
             *   2) REGEX_CONDITION_LINE 정규식으로 연산자를 구분자 삼아 Split
             *   3) 따옴표 제거 (문자열 리터럴의 양쪽 따옴표 처리)
             *   4) parts.Length == 1 → 단일 bool 값 파싱 (예: "true", "false")
             *      parts.Length == 3 → EvaluateExpression으로 좌/연산자/우 평가
             *      그 외             → 지원하지 않는 형식으로 오류 출력
             */
            public static bool EvaluateCondition(string condition)
            {
                // 먼저 변수·태그를 실제 값으로 치환
                condition = TagManager.Inject(condition, injectTags: true, injectVariable: true);

                // 연산자를 구분자로 Split (연산자 자체도 결과에 포함됨)
                string[] parts = Regex.Split(condition, REGEX_CONDITION_LINE)
                    .Select(p => p.Trim()).ToArray();

                // 문자열 리터럴의 따옴표 제거
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("\"") && parts[i].EndsWith("\""))
                    {
                        parts[i] = parts[i].Substring(1, parts[i].Length - 2);
                    }
                }

                if (parts.Length == 1)
                {
                    // 단일 값: bool로 직접 파싱 (예: if(true), if($isVisible))
                    if (bool.TryParse(parts[0], out bool result))
                    {
                        return result;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse condition : {condition}");
                        return false;
                    }
                }
                else if (parts.Length == 3)
                {
                    // 3항 형식: 좌변 / 연산자 / 우변 (가장 일반적인 형태)
                    return EvaluateExpression(parts[0], parts[1], parts[2]);
                }
                else
                {
                    Debug.LogError($"Unsupported condition format: {condition}");
                    return false;
                }
            }

            // 타입별 연산자 함수를 저장하는 delegate 타입
            // T 타입의 두 값을 받아 bool을 반환하는 함수 시그니처
            private delegate bool OperatorFunc<T>(T left, T right);

            /*
             * boolOperators / floatOperators / intOperators
             * 타입별로 지원하는 연산자와 해당 람다 함수를 딕셔너리로 관리.
             * 설계 이유: switch/if 대신 딕셔너리 조회로 연산자 처리 → 코드 간결화.
             *   bool  : &&, ||, ==, != 만 지원 (크기 비교 불가)
             *   float : 모든 비교 연산자 지원
             *   int   : 모든 비교 연산자 지원
             */
            private static Dictionary<string, OperatorFunc<bool>> boolOperators = new Dictionary<string, OperatorFunc<bool>>()
            {
                { "&&", (left, right) => left && right },
                { "||", (left, right) => left || right },
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right }
            };

            private static Dictionary<string, OperatorFunc<float>> floatOperators = new Dictionary<string, OperatorFunc<float>>()
            {
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
                { "<",  (left, right) => left <  right },
                { ">",  (left, right) => left >  right },
                { "<=", (left, right) => left <= right },
                { ">=", (left, right) => left >= right }
            };

            private static Dictionary<string, OperatorFunc<int>> intOperators = new Dictionary<string, OperatorFunc<int>>()
            {
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
                { "<",  (left, right) => left <  right },
                { ">",  (left, right) => left >  right },
                { "<=", (left, right) => left <= right },
                { ">=", (left, right) => left >= right }
            };

            /*
             * EvaluateExpression
             * 좌변·연산자·우변 문자열로 비교 결과를 반환한다.
             *
             * 타입 시도 순서: bool → float → int → string
             *   - 둘 다 bool로 파싱되면 boolOperators 사용
             *   - 둘 다 float로 파싱되면 floatOperators 사용
             *   - 둘 다 int로 파싱되면 intOperators 사용
             *   - 모두 실패하면 문자열 비교 (== / != 만 지원)
             *
             * 문자열 비교에서 <, > 등을 지원하지 않는 이유:
             *   사전순 비교가 의도치 않은 결과를 낼 수 있어 명시적으로 제한.
             */
            private static bool EvaluateExpression(string left, string op, string right)
            {
                // bool 타입 비교
                if(bool.TryParse(left, out bool leftBool) && bool.TryParse(right, out bool rightBool))
                {
                    return boolOperators[op](leftBool, rightBool);
                }

                // float 타입 비교
                if(float.TryParse(left, out float leftFloat) && float.TryParse(right, out float rightFloat))
                {
                    return floatOperators[op](leftFloat, rightFloat);
                }

                // int 타입 비교
                if (int.TryParse(left, out int leftInt) && int.TryParse(right, out int rightInt))
                {
                    return intOperators[op](leftInt, rightInt);
                }

                // 문자열 비교 (== / != 만 지원)
                switch(op)
                {
                    case "==":
                        return left == right;
                    case "!=":
                        return left != right;
                    default:
                        throw new InvalidOperationException($"Unsupported Operation: {op}");
                }

            }
        }
    }
}
