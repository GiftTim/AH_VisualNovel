using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 저장/불러오기를 담당하는 정적(static) 클래스.
/// MonoBehaviour가 아니라 static이므로 씬에 배치할 필요 없이 어디서든 호출 가능하다.
/// JSON 형식으로 직렬화해 Application.persistentDataPath (앱 데이터 폴더)에 저장한다.
///
/// 저장 경로 예시 (Windows):
///   C:/Users/[이름]/AppData/LocalLow/[회사명]/[게임명]/[파일명].json
/// </summary>
public static class SaveSystem
{
    /// <summary>저장 폴더 경로. Unity가 플랫폼별로 적절한 경로를 제공한다.</summary>
    private static string SaveFolder => Application.persistentDataPath;

    /// <summary>
    /// 게임 상태를 JSON으로 직렬화해 파일에 저장한다.
    /// data.SaveFile 이름으로 저장 폴더에 파일이 생성된다.
    /// </summary>
    public static void Save(GameStateData data)
    {
        try
        {
            string saveFilePath = Path.Combine(SaveFolder, data.SaveFile); // 전체 경로 조합
            string json = JsonUtility.ToJson(data, true);                  // 들여쓰기 있는 JSON

            File.WriteAllText(saveFilePath, json); // 파일에 쓰기

            Debug.Log($"[SaveSystem] 저장 완료: {saveFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveSystem] 저장 실패: {ex}");
        }
    }

    /// <summary>
    /// 저장 파일을 읽어 GameStateData로 역직렬화해 반환한다.
    /// 파일이 없으면 GameNotFoundException 발생.
    /// 형식 오류 시 BadFormatGameException 발생.
    /// GameManager.LoadData()에서 try-catch로 처리한다.
    /// </summary>
    public static GameStateData Load(string saveFile)
    {
        string saveFilePath = Path.Combine(SaveFolder, saveFile);

        if (!File.Exists(saveFilePath))
        {
            throw new GameNotFoundException(); // 파일 없음 예외
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);        // 파일 읽기
            var data = JsonUtility.FromJson<GameStateData>(json); // JSON → 오브젝트

            Debug.Log("[SaveSystem] 불러오기 완료.");
            return data;
        }
        catch (System.Exception ex)
        {
            throw new BadFormatGameException(ex.Message); // 형식 오류 예외로 감싸서 던짐
        }
    }

    /// <summary>
    /// 저장 폴더에 있는 모든 .json 파일 이름 목록을 반환한다.
    /// 세이브 선택 UI에서 목록을 표시할 때 GameManager.GetSaves()를 통해 호출한다.
    /// </summary>
    public static List<string> GetAllSaveFileNames()
    {
        string savesDirectory = SaveFolder;
        if (!Directory.Exists(savesDirectory))
            return new List<string>();

        var files = Directory.GetFiles(savesDirectory, "*.json"); // .json 파일만 검색

        // 전체 경로 → 파일명만 추출 (예: "C:/..../Guild.json" → "Guild.json")
        for (int i = 0; i < files.Length; i++) files[i] = Path.GetFileName(files[i]);

        return files.ToList();
    }

    /// <summary>저장 파일을 삭제한다. 존재하지 않는 파일은 무시한다.</summary>
    public static void DeleteSave(string saveFile)
    {
        var path = Path.Combine(SaveFolder, saveFile);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
