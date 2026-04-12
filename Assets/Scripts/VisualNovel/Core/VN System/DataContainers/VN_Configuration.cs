using UnityEngine;

/*
 * ============================================================
 * VN_Configuration (Visual Novel Configuration)
 * ============================================================
 * 플레이어가 ConfigMenu(설정 화면)에서 변경할 수 있는
 * 게임 설정 값들을 저장하는 데이터 컨테이너.
 *
 * [저장 항목]
 *  - 화면 해상도
 *  - 대화 텍스트 출력 속도 / 자동 읽기 속도
 *  - 음악 / SFX / 음성 볼륨 및 음소거 여부
 *  - 히스토리 로그 UI 크기 배율
 *
 * [저장 위치]
 * - FilePaths.gameDatas 경로에 "vnconfig.cfg" 파일로 저장.
 * - JSON 형식. ENCRYPT가 false이므로 평문으로 저장.
 *
 * [사용 방법]
 * - 로드: VN_Configuration.activeConfig = FileManager.Load<VN_Configuration>(filePath)
 *         → 그 후 activeConfig.Load()를 호출하면 UI에 값이 반영됨.
 * - 저장: activeConfig.Save()
 *
 * [activeConfig]
 * - 현재 적용 중인 설정의 인스턴스를 가리키는 static 참조.
 * ============================================================
 */
[System.Serializable]
public class VN_Configuration
{
    // ─────────────────────────────────────────────────────────
    // activeConfig: 현재 적용 중인 설정 인스턴스.
    //               ConfigMenu 등에서 VN_Configuration.activeConfig 로 접근.
    // ─────────────────────────────────────────────────────────
    public static VN_Configuration activeConfig;

    // ─────────────────────────────────────────────────────────
    // filePath: 설정 파일이 저장되는 경로.
    //           예) "GameData/vnconfig.cfg"
    // ENCRYPT: 파일 암호화 여부. false = 평문 JSON 저장.
    // ─────────────────────────────────────────────────────────
    public static string filePath => $"{FilePaths.gameDatas}/vnconfig.cfg";
    public const bool ENCRYPT = false;

    // ── 화면 설정 ──────────────────────────────────────────────
    // display_resolution: 현재 선택된 해상도 이름 문자열.
    //   ConfigMenu의 드롭다운 옵션 텍스트와 일치해야 한다.
    //   예) "전체창 (1920 x 1080)"
    public string display_resolution = "전체창 (1920 x 1080)";

    // ── 텍스트 설정 ────────────────────────────────────────────
    // dialogueTextSpeed: 대화 텍스트 출력 속도 배율.
    //   1f = 기본 속도. 값이 클수록 빠르게 출력된다.
    // dialogueAutoReadSpeed: 자동 읽기(Auto) 모드에서 다음 대사로
    //   넘어가는 속도 배율. 1f = 기본 속도.
    public float dialogueTextSpeed = 1f;
    public float dialogueAutoReadSpeed = 1f;

    // ── 오디오 설정 ────────────────────────────────────────────
    // musicVolume   : 배경 음악 볼륨. 0(음소거) ~ 1(최대).
    // muteMusic     : 음악 음소거 여부.
    // sfxVolume     : 효과음 볼륨. 0 ~ 1.
    // muteSFX       : 효과음 음소거 여부.
    // voicesVolume  : 음성 볼륨. 0 ~ 1.
    // muteVoices    : 음성 음소거 여부.
    public float musicVolume = 1f;
    public bool muteMusic = false;
    public float sfxVolume = 1f;
    public bool muteSFX = false;
    public float voicesVolume = 1f;
    public bool muteVoices = false;

    // ── 기타 설정 ──────────────────────────────────────────────
    // historyLogScale: 히스토리 로그 UI의 폰트/항목 크기 배율.
    //   1f = 기본 크기.
    public float historyLogScale = 1f;

    /*
     * Load()
     * ────────────────────────────────────────────────────────
     * 이 인스턴스에 저장된 설정 값들을 ConfigMenu의 UI에 반영한다.
     * 설정 파일을 불러온 직후 호출하여 화면에 적용한다.
     *
     * 처리 순서:
     *  1) 저장된 해상도 이름과 일치하는 드롭다운 항목을 찾아 선택.
     *  2) 텍스트 출력 속도 / 자동 읽기 속도 슬라이더 값 설정.
     *  3) 음악 / 효과음 / 음성 볼륨 슬라이더 값 설정.
     *  4) 각 음소거 버튼의 아이콘 이미지를 현재 상태에 맞게 변경.
     * ────────────────────────────────────────────────────────
     */
    public void Load()
    {
        var ui = ConfigMenu.instance.ui;

        // ── 해상도 드롭다운 설정 ──
        // 저장된 해상도 이름(display_resolution)과 동일한 드롭다운 옵션을 찾는다.
        int res_index = 0;
        for (int i = 0; i < ui.resolutions.options.Count; i++)
        {
            string optionText = ui.resolutions.options[i].text;

            if (optionText == display_resolution)
            {
                res_index = i;
                break; // 일치하는 항목을 찾으면 더 이상 탐색하지 않는다.
            }
        }

        // 찾은 인덱스로 드롭다운 값 설정 후 실제 해상도 변경 메서드 호출.
        ui.resolutions.value = res_index;
        ConfigMenu.instance.SetDisplayResolution();

        // ── 텍스트 속도 슬라이더 설정 ──
        ui.architectSpeed.value   = dialogueTextSpeed;    // 텍스트 출력 속도
        ui.autoReaderSpeed.value  = dialogueAutoReadSpeed; // 자동 읽기 속도

        // ── 오디오 볼륨 슬라이더 설정 ──
        ui.musicVolume.value  = musicVolume;
        ui.sfxVolume.value    = sfxVolume;
        ui.voicesVolume.value = voicesVolume;

        // ── 음소거 버튼 아이콘 설정 ──
        // 음소거 상태(true)이면 음소거 아이콘, 아니면 소리 켜짐 아이콘으로 변경.
        ui.musicMute.sprite  = muteMusic  ? ui.mutedSymbol : ui.unmutedSymbol;
        ui.sfxMute.sprite    = muteSFX    ? ui.mutedSymbol : ui.unmutedSymbol;
        ui.voicesMute.sprite = muteVoices ? ui.mutedSymbol : ui.unmutedSymbol;
    }

    /*
     * Save()
     * ────────────────────────────────────────────────────────
     * 현재 설정 값을 JSON으로 직렬화하여 파일에 저장한다.
     * ConfigMenu에서 설정이 변경될 때마다 호출된다.
     * ────────────────────────────────────────────────────────
     */
    public void Save()
    {
        // 이 인스턴스를 JSON 문자열로 변환하여 파일에 저장.
        FileManager.Save(filePath, JsonUtility.ToJson(this), encrypt: ENCRYPT);
    }
}
