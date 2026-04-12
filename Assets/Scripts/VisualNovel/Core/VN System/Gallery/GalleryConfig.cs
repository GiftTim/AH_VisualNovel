using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * ============================================================
 * GalleryConfig (Gallery Configuration / Unlock Data)
 * ============================================================
 * 갤러리(CG 열람) 시스템의 "잠금 해제된 이미지 목록"을
 * 저장하고 불러오는 데이터 클래스.
 *
 * [역할]
 * - 플레이어가 게임 중에 unlockCG 명령어로 열람한 이미지의 이름을 기억한다.
 * - 한 번 열람한 CG는 게임을 다시 시작해도 갤러리에서 볼 수 있어야 하므로
 *   별도 파일(gallery.vng)에 저장한다. (세이브 슬롯과 독립적)
 *
 * [저장 형식]
 * - JSON 직렬화, 파일 확장자 .vng (Visual Novel Gallery).
 * - FilePaths.gameDatas 경로에 "gallery.vng" 파일로 저장.
 * - ENCRYPT = false → 평문 JSON.
 *
 * [static activeConfig]
 * - 현재 로드된 갤러리 데이터의 인스턴스.
 * - null이면 아직 로드되지 않은 상태.
 * - UnlockImage, ImageIsUnlocked에서 null이면 자동으로 Load()를 호출한다.
 *
 * [세이브 슬롯과의 관계]
 * - 세이브 슬롯(VNGameSave)과 별도로 저장된다.
 * - 어떤 슬롯으로 플레이하든 갤러리는 공유된다.
 * ============================================================
 */
[System.Serializable]
public class GalleryConfig
{
    // ─────────────────────────────────────────────────────────
    // activeConfig: 현재 메모리에 로드된 GalleryConfig 인스턴스.
    //               null이면 아직 Load()가 호출되지 않은 상태.
    // ENCRYPT     : 파일 암호화 여부. false = 평문 JSON.
    // filePath    : 갤러리 데이터 파일 경로. 예) "GameData/gallery.vng"
    // ─────────────────────────────────────────────────────────
    public static GalleryConfig activeConfig;
    public const bool ENCRYPT = false;
    public static string filePath => $"{FilePaths.gameDatas}gallery.vng";

    // ─────────────────────────────────────────────────────────
    // unlockedImages: 플레이어가 열람한 이미지의 이름(Texture.name) 목록.
    //   GalleryMenu는 이 목록에 이름이 있는 이미지만 표시한다.
    //   없는 이미지는 검은 박스(잠금 상태)로 표시된다.
    // ─────────────────────────────────────────────────────────
    public List<string> unlockedImages = new List<string>();

    /*
     * Load()  [정적 메서드]
     * ────────────────────────────────────────────────────────
     * 갤러리 데이터 파일을 읽어 activeConfig에 로드한다.
     * 파일이 없으면 (첫 실행 시) 빈 GalleryConfig 인스턴스를 생성한다.
     * ────────────────────────────────────────────────────────
     */
    public static void Load()
    {
        if (File.Exists(filePath))
        {
            // 파일이 존재하면 JSON 역직렬화하여 로드.
            activeConfig = FileManager.Load<GalleryConfig>(filePath, encrypt: ENCRYPT);
        }
        else
        {
            // 파일이 없으면 (최초 실행) 빈 데이터로 시작.
            activeConfig = new GalleryConfig();
        }
    }

    /*
     * Save()  [정적 메서드]
     * ────────────────────────────────────────────────────────
     * 현재 activeConfig를 JSON 파일로 저장한다.
     * UnlockImage()에서 새 이미지가 해제될 때 자동 호출된다.
     * ────────────────────────────────────────────────────────
     */
    public static void Save() => FileManager.Save(filePath, JsonUtility.ToJson(activeConfig), encrypt: ENCRYPT);

    /*
     * Erase()  [정적 메서드]
     * ────────────────────────────────────────────────────────
     * 갤러리 잠금 해제 데이터를 전부 초기화한다.
     * unlockedImages 목록을 비우고 파일을 덮어쓴다.
     * (게임 초기화 기능 등에서 사용)
     * ────────────────────────────────────────────────────────
     */
    public static void Erase()
    {
        // activeConfig가 null이면 빈 인스턴스를 새로 생성.
        if (activeConfig == null)
        {
            activeConfig = new GalleryConfig();
        }

        // 열람 목록을 완전히 비운다.
        activeConfig.unlockedImages = new List<string>();

        // 빈 상태를 파일에 저장.
        Save();
    }

    /*
     * UnlockImage(imageName)  [정적 메서드]
     * ────────────────────────────────────────────────────────
     * 지정한 이름의 이미지를 갤러리에서 열람 가능하도록 잠금 해제한다.
     * CMD_DatabaseExtension_Gallery의 unlockCG 명령어에서 호출된다.
     *
     * [중복 방지]
     * - 이미 목록에 있는 이미지는 다시 추가하지 않는다.
     * - 새로 추가된 경우에만 Save()를 호출하여 불필요한 파일 쓰기를 방지.
     *
     * [매개변수]
     *  imageName: Resources/Gallery 폴더 내 Texture 에셋의 이름(파일명).
     * ────────────────────────────────────────────────────────
     */
    public static void UnlockImage(string imageName)
    {
        // activeConfig가 로드되지 않았으면 먼저 로드.
        if (activeConfig == null)
        {
            Load();
        }

        // 중복 방지: 아직 목록에 없는 경우에만 추가하고 저장.
        if (!activeConfig.unlockedImages.Contains(imageName))
        {
            activeConfig.unlockedImages.Add(imageName);
            Save(); // 변경 사항 즉시 파일에 반영.
        }
    }

    /*
     * ImageIsUnlocked(imageName)  [정적 메서드]
     * ────────────────────────────────────────────────────────
     * 지정한 이름의 이미지가 갤러리에서 열람 가능한지 여부를 반환한다.
     * GalleryMenu.LoadPage()에서 각 이미지 버튼을 표시할 때 호출된다.
     *
     * [매개변수]
     *  imageName: 확인할 이미지의 이름.
     *
     * [반환값]
     *  true  → 열람 가능 (이미 잠금 해제됨)
     *  false → 열람 불가 (아직 잠금 중, 검은 박스로 표시)
     * ────────────────────────────────────────────────────────
     */
    public static bool ImageIsUnlocked(string imageName)
    {
        // activeConfig가 로드되지 않았으면 먼저 로드.
        if (activeConfig == null)
        {
            Load();
        }

        return activeConfig.unlockedImages.Contains(imageName);
    }
}
