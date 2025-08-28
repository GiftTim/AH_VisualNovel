using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [Header("커서/이름바 UI")]
    [SerializeField] private GameObject cursorRoot; // 커서 UI 루트

    [SerializeField] private GameObject nomalCursor;
    [SerializeField] private GameObject interactiveCursor_NPC;
    [SerializeField] private GameObject interactiveCursor_Item;

    [SerializeField] private GameObject go_TargetNameBar;
    [SerializeField] private TextMeshProUGUI txt_TargetName;

    [Header("시각 이펙트")]
    [SerializeField] private GameObject interactionEffect;       // 원형 이펙트 루트
    [SerializeField] private Image img_Interaction_CircleEffect; // 퍼지는 원 이미지

    [Header("클릭 홀드 게이지 (표시 + 속도 소유)")]
    [SerializeField] private Image gaugeImage;
    [SerializeField] private GameObject gaugeUI;

    // UI 내부 전용(애니메이션 핸들)
    private Coroutine pulseCo;
    private bool pulseActive;

    // ===== 커서/이름바 =====
    public void ShowInteractionCursor(RaycastHit hitInfo, string currentTag)
    {
        if (!cursorRoot.activeSelf) cursorRoot.SetActive(true);

        nomalCursor.SetActive(false);
        go_TargetNameBar.SetActive(true);

        interactiveCursor_NPC.SetActive(currentTag == "Interaction_NPC");
        interactiveCursor_Item.SetActive(currentTag == "Interaction_Item");

        var it = hitInfo.transform.GetComponent<InteractionType>();
        txt_TargetName.text = it ? it.GetName() : "";
        if (currentTag != "Interaction_NPC" && currentTag != "Interaction_Item")
        {
            go_TargetNameBar.SetActive(false);
            txt_TargetName.text = "";
        }
    }

    public void ShowNomalCursor()
    {
        txt_TargetName.text = "";
        nomalCursor.SetActive(true);
        interactiveCursor_NPC.SetActive(false);
        interactiveCursor_Item.SetActive(false);
        go_TargetNameBar.SetActive(false);
    }

    public void HideAllCursors()
    {
        cursorRoot.SetActive(false);
        interactionEffect.SetActive(false);
        SetPulse(false);
        HideGauge();
    }

    public void ShowAllCursors()
    {
        cursorRoot.SetActive(true);
    }


    // ===== 펄스 이펙트(순수 표시) =====
    public void SetPulse(bool on)
    {
        if (on)
        {
            if (pulseActive) return;
            pulseActive = true;
            pulseCo = StartCoroutine(PulseEffect());
        }
        else
        {
            interactionEffect.SetActive(false);
            if (!pulseActive) return;
            pulseActive = false;
            if (pulseCo != null) StopCoroutine(pulseCo);
        }
    }

    private IEnumerator PulseEffect()
    {
        interactionEffect.SetActive(true);
        while (pulseActive)
        {
            var color = img_Interaction_CircleEffect.color;
            color.a = 0.5f;
            img_Interaction_CircleEffect.color = color;

            img_Interaction_CircleEffect.transform.localScale = Vector3.one;
            var scale = Vector3.one;

            while (pulseActive && color.a > 0f)
            {
                color.a -= 0.01f;
                img_Interaction_CircleEffect.color = color;
                scale += Vector3.one * Time.deltaTime;
                img_Interaction_CircleEffect.transform.localScale = scale;
                yield return null;
            }
            yield return null;
        }
    }

    // ===== 게이지(상태 없이 표시만) =====
    public void ShowGauge()
    {
        if (gaugeImage) gaugeImage.fillAmount = 0f;
        gaugeUI.SetActive(true);
    }

    public void SetGauge01(float value01)
    {
        if (!gaugeUI.activeSelf) gaugeUI.SetActive(true);
        if (gaugeImage) gaugeImage.fillAmount = Mathf.Clamp01(value01);
    }

    public void HideGauge()
    {
        if (gaugeImage) gaugeImage.fillAmount = 0f;
        if (gaugeUI.activeSelf) gaugeUI.SetActive(false);
    }
}