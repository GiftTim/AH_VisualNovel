/// <summary>
/// StrategyGame 시스템에서 사용하는 경로를 한 곳에서 관리하는 상수 클래스.
/// FilePaths.cs (VisualNovel)와 동일한 패턴.
/// 문자열을 코드 곳곳에 직접 쓰는 대신 이 클래스의 상수를 참조한다.
/// </summary>
public static class StrategyGamePaths
{
    // ─── Resources 경로 ────────────────────────────────────────────
    // Resources.Load() / Resources.LoadAll() 에 넘기는 경로
    // 기준: Assets/Resources/ 폴더가 루트

    /// <summary>Resources 내 StrategyGame 루트 경로.</summary>
    private const string RESOURCES_ROOT = "StrategyGame/";

    /// <summary>CharacterSO 인스턴스가 위치한 Resources 경로.</summary>
    public const string Characters = RESOURCES_ROOT + "Characters";

    /// <summary>PlayerLevelSO 인스턴스가 위치한 Resources 경로. (필요 시 사용)</summary>
    public const string PlayerLevels = RESOURCES_ROOT + "PlayerLevels";
}
