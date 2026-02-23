using DIALOGUE;
using UnityEngine;
using System.Collections;

public class CanvasGroupController
{
    private MonoBehaviour owner;
    private CanvasGroup   rootCG;

    // 대화창 페이드 속도
    private const float DEFAULT_FADE_SPEED = 3f;

    // 코루틴 중복 실행 방지용 참조 관리 패턴
    private Coroutine co_showing = null;
    private Coroutine co_hiding = null;

    //읽기 전용 프로퍼티 (Read-only Property)
    public bool isShowing => co_showing != null;    // 대화창이 보이는 중인지 여부
    public bool isHiding => co_hiding != null;      // 대화창이 숨겨지는 중인지 여부
    public bool isFading => isShowing || isHiding;  // 둘 중 하나라도 실행 중인지 여부
    public bool isVisible => co_showing != null || rootCG.alpha > 0f; // 대화창이 보이는 중이거나 투명도가 0보다 큰 경우

    public CanvasGroupController(MonoBehaviour owner, CanvasGroup rootCG)
    {
        this.owner = owner;
        this.rootCG = rootCG;
    }

    public Coroutine Show(float speed = 1f, bool immediate = false)
    {
        if (isShowing)
        {
            return co_showing;
        }
        else if (isHiding)
        {
            DialogueSystem.instance.StopCoroutine(co_hiding);
            co_hiding = null;
        }

        co_showing = DialogueSystem.instance.StartCoroutine(Fading(1f, speed, immediate));

        return co_showing;
    }

    public Coroutine Hide(float speed = 1f, bool immediate = false)
    {
        if (isHiding)
        {
            return co_hiding;
        }
        else if (isShowing)
        {
            DialogueSystem.instance.StopCoroutine(co_showing);
            co_showing = null;
        }

        co_hiding = DialogueSystem.instance.StartCoroutine(Fading(0f, speed, immediate));

        return co_hiding;
    }

    private IEnumerator Fading(float alpha, float speed, bool immediate)
    {
        CanvasGroup cg = rootCG;

        if(immediate)
        {
            cg.alpha = alpha;
        }

        while (cg.alpha != alpha)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * DEFAULT_FADE_SPEED * speed);
            yield return null;
        }

        co_showing = null;
        co_hiding = null;
    }

}
