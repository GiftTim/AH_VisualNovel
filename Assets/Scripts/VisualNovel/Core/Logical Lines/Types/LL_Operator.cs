using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace DIALOGUE.LogicalLines
{
    public class LL_Operator : ILogicalLine
    {
        string ILogicalLine.keyword => throw new System.NotImplementedException();

        IEnumerator ILogicalLine.Execute(DIALOGUE_LINE line)
        {
            throw new System.NotImplementedException();
        }

        bool ILogicalLine.Matches(DIALOGUE_LINE line)
        {
            throw new System.NotImplementedException();
        }
    }
}