using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace DIALOGUE.LogicalLines
{
    /*
     * LogicalLineManager
     * ─────────────────────────────────────────────────────────────────────
     * ILogicalLine 구현체를 자동으로 탐지·등록하고,
     * ConversationManager의 요청에 따라 적합한 핸들러를 찾아 실행하는 클래스.
     *
     * Reflection 자동 탐지 방식의 장점:
     *   새 논리 라인(LL_XXX)을 추가할 때 이 클래스를 수정할 필요가 없다.
     *   LL_XXX 파일을 만들고 ILogicalLine을 구현하기만 하면 자동으로 등록됨.
     *
     * 사용 흐름:
     *   ConversationManager.RunningConversation()
     *     → TryGetLogic(line, out logic)
     *       → 등록된 핸들러 중 Matches(line) == true인 것을 찾아 코루틴 시작
     *       → logic(Coroutine)을 out으로 반환 → ConversationManager가 yield return
     * ─────────────────────────────────────────────────────────────────────
     */
    public class LogicalLineManager
    {
        // DialogueSystem 싱글톤 단축 접근 프로퍼티
        private DialogueSystem dialogueSystem => DialogueSystem.instance;

        // 등록된 ILogicalLine 구현체 인스턴스 목록
        // Reflection으로 자동 생성되며, TryGetLogic에서 순회해 적합한 핸들러를 찾는다.
        private List<ILogicalLine> logicalLines = new List<ILogicalLine>();


        // 생성자: 인스턴스 생성 즉시 Reflection으로 모든 구현체를 탐지·등록
        public LogicalLineManager() => LoadLogicalLines();

        /*
         * LoadLogicalLines
         * Reflection을 사용해 현재 어셈블리에서 ILogicalLine을 구현하는
         * 모든 클래스를 찾아 인스턴스를 생성하고 logicalLines 목록에 등록한다.
         *
         * Reflection 흐름:
         *   1) Assembly.GetExecutingAssembly()
         *      → 현재 실행 중인 어셈블리(게임 스크립트 전체)를 가져옴
         *   2) assembly.GetTypes().Where(...)
         *      → ILogicalLine을 구현(IsAssignableFrom)하고, 인터페이스 자체는 제외(!t.IsInterface)
         *      → LL_Choice, LL_Condition, LL_Input, LL_Operator 등이 해당됨
         *   3) Activator.CreateInstance(lineType)
         *      → 각 타입의 기본 생성자로 인스턴스 생성 후 목록에 추가
         */
        private void LoadLogicalLines()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] lineTypes = assembly.GetTypes()
                                            .Where(t => typeof(ILogicalLine).IsAssignableFrom(t) && !t.IsInterface) // ILogicalLine 구현체만 필터 (인터페이스 자신 제외)
                                            .ToArray();
            foreach (var lineType in lineTypes)
            {
                ILogicalLine line = (ILogicalLine)Activator.CreateInstance(lineType); // 기본 생성자로 인스턴스 생성
                logicalLines.Add(line);
            }
        }

        /*
         * TryGetLogic
         * 주어진 DIALOGUE_LINE에 맞는 ILogicalLine 핸들러를 찾아 코루틴을 시작한다.
         *
         * 반환:
         *   true  + logic(Coroutine) : 처리 가능한 핸들러를 찾아 코루틴 시작
         *   false + logic(null)      : 처리 가능한 핸들러 없음 (일반 대사·명령어 라인)
         *
         * logic out 매개변수:
         *   ConversationManager가 이 코루틴을 yield return해 완료까지 대기하기 위해 사용.
         *
         * dialogueSystem.StartCoroutine() 사용 이유:
         *   코루틴은 MonoBehaviour에서만 시작 가능하므로, DialogueSystem(MonoBehaviour)에 위임.
         */
        public bool TryGetLogic(DIALOGUE_LINE line, out Coroutine logic)
        {
            foreach (var logicalLine in logicalLines)
            {
                if(logicalLine.Matches(line)) // 이 핸들러가 이 라인을 처리할 수 있으면
                {
                    logic = dialogueSystem.StartCoroutine(logicalLine.Execute(line)); // 코루틴 시작
                    return true;
                }
            }

            // 처리 가능한 핸들러가 없으면 null 반환
            logic = null;
            return false;
        }
    }
}
