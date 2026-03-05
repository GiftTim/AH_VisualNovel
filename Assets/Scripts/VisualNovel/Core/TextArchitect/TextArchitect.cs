using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System.Linq;
using System;

/// <summary>
/// 여러 설정된 방식 중 하나로 화면에 텍스트를 생성하는 역할을 합니다. 
/// 대화 생성 과정을 간소화하기 위해 설계되었습니다.
/// </summary>
public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;

    /// <summary>
    /// 이 아키텍트에 할당된 TextMeshPro 컴포넌트입니다. (UI 또는 월드 공간용)
    /// </summary>
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    /// <summary>
    /// 현재 아키텍트에 의해 화면에 표시되고 있는 텍스트입니다.
    /// </summary>
    public string currentText => tmpro.text;

    /// <summary>
    /// 아키텍트가 현재 만들려고 하는 목표 텍스트입니다. 
    /// 이어쓰기(Append) 시 추가되는 이전 텍스트(preText)는 제외한 순수 목표 문자열입니다.
    /// </summary>
    public string targetText { get; private set; } = "";

    /// <summary>
    /// 아키텍트가 최종적으로 표시하려는 전체 텍스트입니다.
    /// 이어쓰기 시 이전 텍스트와 현재 목표 텍스트를 합친 결과물입니다.
    /// </summary>
    public string fullTargetText => preText + targetText;

    /// <summary>
    /// 이어쓰기(Append) 빌드 이전에 이미 존재해야 하는 텍스트입니다.
    /// </summary>
    public string preText { get; private set; } = "";

    /// <summary>
    /// 현재 TextMeshPro 컴포넌트에서 렌더링 중인 텍스트의 색상입니다.
    /// </summary>
    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

    /// <summary>
    /// 텍스트가 생성되는 속도를 결정합니다.
    /// </summary>
    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    private const float baseSpeed = 1;
    private float speedMultiplier = 1;

    /// <summary>
    /// 한 프레임(또는 사이클)당 생성될 글자 수입니다. 
    /// 페이드(Fade) 기법을 사용할 때는 이 값이 속도 배수 역할을 합니다.
    /// </summary>
    public int charactersPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; } }
    private int characterMultiplier = 1;

    /// <summary>
    /// true일 경우, 텍스트를 평소보다 훨씬 빠르게 표시합니다. (유저의 클릭/스킵 시 사용)
    /// </summary>
    public bool hurryUp = false;

    /// <summary>
    /// 아키텍트에서 사용할 수 있는 모든 TABuilder 클래스들의 목록입니다. 
    /// TABuilder는 선택된 방식에 따라 고유한 방법으로 문자열을 화면에 그립니다.
    /// </summary>
    private Dictionary<string, Type> builders = new Dictionary<string, Type>();

    /// <summary>
    /// 실제 텍스트 생성을 수행하는 빌더 객체입니다. 설정된 빌드 방식에 따라 교체됩니다.
    /// </summary>
    private TABuilder builder = null;
    private TABuilder.BuilderTypes _builderType;

    /// <summary>
    /// 현재 아키텍트가 텍스트를 나타내기 위해 사용 중인 빌더의 타입입니다.
    /// </summary>
    public TABuilder.BuilderTypes builderType => _builderType;

    private Coroutine buildProcess = null;

    /// <summary>
    /// 현재 아키텍트가 실시간으로 텍스트를 빌드(출력)하고 있는 중인지 여부입니다.
    /// </summary>
    public bool isBuilding => buildProcess != null;

    /// <summary>
    /// UI용 TextMeshProUGUI 객체를 사용하여 텍스트 아키텍트를 생성합니다.
    /// </summary>
    public TextArchitect(TextMeshProUGUI uiTextObject, TABuilder.BuilderTypes builderType = TABuilder.BuilderTypes.Instant)
    {
        tmpro_ui = uiTextObject;
        AddBuilderTypes();
        SetBuilderType(builderType);
    }

    /// <summary>
    /// 월드 공간용 TextMeshPro 객체를 사용하여 텍스트 아키텍트를 생성합니다.
    /// </summary>
    public TextArchitect(TextMeshPro worldTextObject, TABuilder.BuilderTypes builderType = TABuilder.BuilderTypes.Instant)
    {
        tmpro_world = worldTextObject;
        AddBuilderTypes();
        SetBuilderType(builderType);
    }

    /// <summary>
    /// 프로젝트 내의 모든 TABuilder 하위 클래스를 찾아 딕셔너리에 등록합니다. (리플렉션 사용)
    /// </summary>
    private void AddBuilderTypes()
    {
        builders = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(TABuilder)))
            .ToDictionary(t => t.Name, t => t);
    }

    /// <summary>
    /// 지정된 빌드 타입에 맞는 올바른 TABuilder 객체로 업데이트합니다.
    /// </summary>
    public void SetBuilderType(TABuilder.BuilderTypes builderType)
    {
        string name = TABuilder.CLASS_NAME_PREFIX + builderType.ToString();
        Type classType = builders[name];

        // 동적으로 클래스 인스턴스 생성
        builder = Activator.CreateInstance(classType) as TABuilder;
        builder.architect = this;
        builder.onComplete += OnComplete; // 완료 이벤트 연결

        _builderType = builderType;
    }

    /// <summary>
    /// 전달받은 텍스트를 처음부터 새로 빌드하여 표시합니다.
    /// </summary>
    public Coroutine Build(string text)
    {
        preText = "";
        targetText = text;

        Stop(); // 진행 중인 작업 중단

        buildProcess = builder.Build();
        return buildProcess;
    }

    /// <summary>
    /// 현재 표시된 텍스트 뒤에 새로운 텍스트를 이어서 빌드합니다.
    /// </summary>
    public Coroutine Append(string text)
    {
        preText = currentText;
        targetText = text;

        Stop(); // 진행 중인 작업 중단

        buildProcess = builder.Build();
        return buildProcess;
    }

    /// <summary>
    /// 연출 없이 텍스트를 즉시 오브젝트에 적용합니다.
    /// </summary>
    public void SetText(string text)
    {
        preText = "";
        targetText = text;

        Stop();

        tmpro.text = targetText;
        builder.ForceComplete();
    }

    /// <summary>
    /// 텍스트 빌드 과정을 중단합니다. 텍스트를 완성시키지 않고 그 자리에서 즉시 멈춥니다.
    /// </summary>
    public void Stop()
    {
        if (isBuilding)
            tmpro.StopCoroutine(buildProcess);

        buildProcess = null;
    }

    /// <summary>
    /// 활성화된 빌드 과정을 즉시 중단하고 텍스트를 끝까지 완성시킵니다.
    /// </summary>
    public void ForceComplete()
    {
        if (isBuilding)
            builder.ForceComplete();

        Stop();

        OnComplete();
    }

    /// <summary>
    /// 텍스트 출력이 완전히 끝났을 때 호출되는 내부 메서드입니다.
    /// </summary>
    private void OnComplete()
    {
        hurryUp = false;
        buildProcess = null;
    }
}
