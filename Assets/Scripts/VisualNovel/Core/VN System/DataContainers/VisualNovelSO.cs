using UnityEngine;

/*
 * ============================================================
 * VisualNovelSO (Visual Novel ScriptableObject)
 * ============================================================
 * 비주얼 노벨 전체 설정을 담는 ScriptableObject.
 *
 * [ScriptableObject란?]
 * - MonoBehaviour처럼 씬 오브젝트에 붙이지 않아도 된다.
 * - 에디터에서 에셋 파일로 생성하여 인스펙터에서 설정할 수 있다.
 * - 씬이 달라져도 값이 유지된다. (씬에 종속되지 않음)
 *
 * [사용 위치]
 * - VNManager 인스펙터의 config 필드에 이 에셋을 드래그하여 할당한다.
 * - VNManager.LoadGame()에서 config.startingFile을 읽어 첫 대화를 시작한다.
 *
 * [에셋 생성 방법]
 * - Unity 메뉴: Assets → Create → Dialogue System → Visual Novel Configuration Asset
 * ============================================================
 */
[CreateAssetMenu(fileName = "Visual Novel Configuration", menuName = "Dialogue System/Visual Novel Configuration Asset")]
public class VisualNovelSO : ScriptableObject
{
    // ─────────────────────────────────────────────────────────
    // startingFile: 게임 맨 처음에 재생될 대화 파일 (TextAsset).
    //
    // TextAsset이란? Unity가 .txt, .csv 등 텍스트 파일을
    // 에셋으로 인식한 것. Resources 폴더에 있어야 런타임에 읽을 수 있다.
    //
    // 이 파일의 각 줄이 대화 스크립트 형식으로 파싱된다.
    // 예) "Hiro \"안녕하세요.\" [show -n Hiro]"
    // ─────────────────────────────────────────────────────────
    public TextAsset startingFile;
}
