using UnityEngine;

public class FilePaths
{
    private const string HOME_DIRECTORY_SYMBOL = "~/";
    public static readonly string root = $"{Application.dataPath}/gameData/";

    // Resources Paths
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_backgroundVideos = $"{resources_graphics}BG Videos/";
    public static readonly string resources_blendTextures = $"{resources_graphics}Transition Effects/";

    public static readonly string resources_audio    = "Audio/";
    public static readonly string resources_sfx      = $"{resources_audio}SFX/";
    public static readonly string resources_voices   = $"{resources_audio}Voices/";
    public static readonly string resources_music    = $"{resources_audio}Music/";
    public static readonly string resources_ambience = $"{resources_audio}Ambience/";

    public static readonly string resources_dialogueFiles = $"Dialogue Files/";

    /// <summary>
    /// 기본 경로 또는 리소스 폴더의 루트를 사용하여 리소스 경로를 반환합니다. 
    /// 기본 경로에서 파일을 찾을 수 없는 경우, 리소스 폴더의 루트에서 파일을 찾으려고 시도합니다.
    /// </summary>
    /// <param name="defaultPath">기본 경로</param>
    /// <param name="resourceName">리소스 이름</param>
    /// <returns>찾은 리소스의 경로</returns>
    public static string GetPathToResource(string defaultPath, string resourceName)
    {
        if (resourceName.StartsWith(HOME_DIRECTORY_SYMBOL))
        {
            return resourceName.Substring(HOME_DIRECTORY_SYMBOL.Length);
        }

        return defaultPath + resourceName;
    }

}

