using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Expressions;

namespace DIALOGUE.LogicalLines
{
    public class LL_Operator : ILogicalLine
    {
        string ILogicalLine.keyword => throw new System.NotImplementedException();

        IEnumerator ILogicalLine.Execute(DIALOGUE_LINE line)
        {
            string trimmedLine = line.rawData.Trim();
            string[] parts = Regex.Split(trimmedLine, REGEX_ARITHMATIC);

            if (parts.Length < 3)
            {
                Debug.LogError($"유효하지 않은 명령어(Invalid command): {trimmedLine}");
                yield break;
            }

            string variable = parts[0].TrimStart(VariableStore.VARIABLE_ID);
            string op = parts[1].Trim();
            string[] remainingParts = new string[parts.Length - 2];
            Array.Copy(parts, 2, remainingParts, 0, parts.Length - 2);

            object value = CalculateValue(remainingParts);

            if (value == null)
            {
                yield break;
            }   

            ProcessOperator(variable, op, value);
        }

        private void ProcessOperator(string variable, string op, object value)
        {
            if(VariableStore.TryGetValue(variable, out object currentValue))
            {
                ProcessOperatorOnVariable(variable, op, value, currentValue);
            }
            else if (op == "=")
            {
                VariableStore.CreateVariable(variable, value);
            }
            else
            {
                Debug.LogWarning($"변수 '{variable}'이 존재하지 않습니다. '=' 연산자만 사용할 수 있습니다. (Variable '{variable}' does not exist. Only '=' operator can be used.)");
            }    
        }

        private void ProcessOperatorOnVariable(string variable, string op, object value, object currentValue)
        {
            switch (op)
            {
                case "=":
                    VariableStore.TrySetValue(variable, value);
                    break;
                case "+=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) + Convert.ToDouble(value));
                    break;
                case "-=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) - Convert.ToDouble(value));
                    break;
                case "*=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) * Convert.ToDouble(value));
                    break;
                case "/=":
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

        bool ILogicalLine.Matches(DIALOGUE_LINE line)
        {
            Match match = Regex.Match(line.rawData.Trim(), REGEX_OPERATOR_LINE);

            return match.Success;
        }
    }
}