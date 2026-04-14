using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// 파일 읽기 / 쓰기 / 저장 / 불러오기를 담당하는 정적 유틸리티 클래스.
/// VisualNovel 시스템 전반에서 파일 I/O가 필요한 곳이면 어디서든 이 클래스를 사용한다.
///
/// 제공 기능:
///   - 텍스트 파일 읽기  (ReadTextFile  : 디스크 파일)
///   - TextAsset 읽기   (ReadTextAsset : Resources 폴더 내 에셋)
///   - 세이브 파일 저장  (Save          : JSON 문자열을 파일에 기록, 암호화 옵션 있음)
///   - 세이브 파일 불러오기 (Load        : 파일을 읽어 JSON 역직렬화, 암호화 옵션 있음)
///   - 디렉토리 자동 생성 (TryCreateDirectoryFromPath)
/// </summary>
public class FileManager
{
    // XOR 암호화에 사용할 키 문자열.
    // Save / Load 의 encrypt 옵션을 true로 설정할 때 이 키로 바이트 단위 XOR 처리를 한다.
    private const string KEY = "SECRETKEY";

    // ─── 읽기 ──────────────────────────────────────────────────────

    /// <summary>
    /// 디스크에 있는 텍스트 파일을 한 줄씩 읽어 List로 반환한다.
    ///
    /// 경로 규칙:
    ///   - '/'로 시작하면 절대 경로로 그대로 사용한다.
    ///   - '/'로 시작하지 않으면 FilePaths.gameDatas 를 앞에 붙여 상대 경로로 취급한다.
    ///
    /// 예) "saveData.txt" → FilePaths.gameDatas + "saveData.txt" 경로에서 읽음
    /// </summary>
    /// <param name="filePath">읽을 파일 경로</param>
    /// <param name="includeBlackLines">true면 빈 줄도 포함, false면 빈 줄 제외</param>
    /// <returns>각 줄을 담은 문자열 리스트. 파일이 없으면 빈 리스트 반환.</returns>
    public static List<string> ReadTextFile(string filePath, bool includeBlackLines = true)
    {
        // 절대 경로('/')가 아니면 gameDatas 기본 경로를 앞에 붙인다
        if (!filePath.StartsWith('/'))
            filePath = FilePaths.gameDatas + filePath;

        List<string> lines = new List<string>();

        try
        {
            // StreamReader: 파일을 열어 한 줄씩 읽는 표준 C# 방식
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    // includeBlackLines가 false이면 공백만 있는 줄은 건너뜀
                    if (includeBlackLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"File not found: {ex.FileName}");
        }

        return lines;
    }

    /// <summary>
    /// Resources 폴더 안에 있는 TextAsset을 경로 문자열로 찾아 한 줄씩 읽어 List로 반환한다.
    ///
    /// TextAsset: Unity에서 .txt / .csv 등 텍스트 파일을 에셋으로 임포트한 것.
    ///            Resources.Load로 런타임에 불러올 수 있다.
    ///
    /// 예) filePath = "Dialogue Files/chapter1" → Resources/Dialogue Files/chapter1.txt
    /// </summary>
    /// <param name="filePath">Resources 기준 경로 (확장자 불필요)</param>
    /// <param name="includeBlackLines">true면 빈 줄도 포함</param>
    /// <returns>각 줄을 담은 문자열 리스트. 에셋이 없으면 null 반환.</returns>
    public static List<string> ReadTextAsset(string filePath, bool includeBlackLines = true)
    {
        // Resources.Load: Unity가 빌드에 포함시킨 에셋을 런타임에 불러오는 API
        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if (asset == null)
        {
            Debug.LogError($"TextAsset not found: '{filePath}'");
            return null;
        }

        // 실제 읽기는 TextAsset 오버로드에 위임
        return ReadTextAsset(asset, includeBlackLines);
    }

    /// <summary>
    /// 이미 로드된 TextAsset 오브젝트를 한 줄씩 읽어 List로 반환한다.
    /// ReadTextAsset(string) 에서 내부적으로 호출하거나, 직접 TextAsset을 가지고 있을 때 사용한다.
    /// </summary>
    /// <param name="asset">읽을 TextAsset 오브젝트</param>
    /// <param name="includeBlackLines">true면 빈 줄도 포함</param>
    /// <returns>각 줄을 담은 문자열 리스트</returns>
    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlackLines = true)
    {
        List<string> lines = new List<string>();

        // StringReader: 디스크 파일 대신 메모리 안의 문자열을 스트림처럼 읽는 클래스
        // asset.text 는 TextAsset의 전체 텍스트 내용
        using (StringReader sr = new StringReader(asset.text))
        {
            // Peek() > -1 : 읽을 내용이 남아 있는지 확인 (EndOfStream 대신 StringReader에서 사용)
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if (includeBlackLines || !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }
        }
        return lines;
    }

    // ─── 디렉토리 ──────────────────────────────────────────────────

    /// <summary>
    /// 주어진 경로에 필요한 디렉토리(폴더)가 없으면 자동으로 생성한다.
    /// Save() 내부에서 파일을 쓰기 전에 먼저 호출된다.
    ///
    /// 동작 흐름:
    ///   1. 경로가 이미 존재하면 바로 true 반환
    ///   2. 경로에 '.'이 포함되어 있으면 파일 경로로 판단 → 부모 폴더만 추출해서 생성
    ///   3. 빈 문자열이면 오류 로그 후 false 반환
    ///   4. 그 외엔 Directory.CreateDirectory로 폴더 생성 시도
    /// </summary>
    /// <param name="path">생성할 디렉토리 경로 또는 파일 경로</param>
    /// <returns>디렉토리가 존재하거나 생성에 성공하면 true, 실패하면 false</returns>
    public static bool TryCreateDirectoryFromPath(string path)
    {
        // 이미 폴더나 파일이 있으면 생성 불필요
        if (Directory.Exists(path) || File.Exists(path))
            return true;

        // '.'이 있으면 파일 경로로 간주 → 부모 디렉토리 경로만 추출
        if (path.Contains("."))
        {
            path = Path.GetDirectoryName(path);
            if (Directory.Exists(path))
                return true;
        }

        if (path == string.Empty)
        {
            Debug.LogError("Cannot create directory from empty path.");
            return false;
        }

        try
        {
            // 중간 폴더가 없어도 한 번에 전체 경로를 생성해 줌
            Directory.CreateDirectory(path);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Could not create directory! {ex}");
            return false;
        }
    }

    // ─── 저장 / 불러오기 ───────────────────────────────────────────

    /// <summary>
    /// 데이터를 JSON 문자열로 받아 지정 경로의 파일에 저장한다.
    ///
    /// encrypt = false (기본): 텍스트 그대로 저장 → 파일을 열면 내용을 읽을 수 있음
    /// encrypt = true        : XOR 암호화 후 바이너리로 저장 → 파일을 열어도 내용을 읽기 어려움
    ///
    /// 저장 전에 TryCreateDirectoryFromPath를 호출해 폴더를 자동 생성한다.
    /// </summary>
    /// <param name="filePath">저장할 파일 경로 (전체 경로)</param>
    /// <param name="JSONData">저장할 JSON 문자열</param>
    /// <param name="encrypt">true면 XOR 암호화 적용</param>
    public static void Save(string filePath, string JSONData, bool encrypt = false)
    {
        // 저장 경로의 폴더가 없으면 먼저 생성
        if (!TryCreateDirectoryFromPath(filePath))
        {
            Debug.LogError($"FAILED TO SAVE FILE '{filePath}' Please see the console for error details.");
            return;
        }

        if (encrypt)
        {
            // 문자열 → UTF-8 바이트 배열 → XOR 암호화 → 바이너리 파일로 저장
            byte[] dataBytes = Encoding.UTF8.GetBytes(JSONData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
            byte[] encryptedBytes = Xor(dataBytes, keyBytes);
            File.WriteAllBytes(filePath, encryptedBytes);
        }
        else
        {
            // 평문 텍스트로 저장
            StreamWriter sw = new StreamWriter(filePath);
            sw.Write(JSONData);
            sw.Close();
        }

        Debug.Log($"Saved data to '{filePath}'");
    }

    /// <summary>
    /// 지정 경로의 파일을 읽어 JSON 역직렬화 후 T 타입 오브젝트로 반환한다.
    ///
    /// encrypt = false (기본): 텍스트 파일의 첫 번째 줄을 JSON으로 파싱
    /// encrypt = true        : 바이너리 파일을 XOR 복호화한 뒤 JSON으로 파싱
    ///
    /// 파일이 없으면 오류 로그를 출력하고 default(T) (null 또는 0) 반환.
    /// </summary>
    /// <typeparam name="T">역직렬화할 대상 타입 (예: SaveData)</typeparam>
    /// <param name="filePath">읽을 파일 경로 (전체 경로)</param>
    /// <param name="encrypt">true면 XOR 복호화 적용</param>
    /// <returns>역직렬화된 T 오브젝트. 파일이 없으면 default(T).</returns>
    public static T Load<T>(string filePath, bool encrypt = false)
    {
        if (File.Exists(filePath))
        {
            if (encrypt)
            {
                // 바이너리 읽기 → XOR 복호화 → UTF-8 문자열 복원 → JSON 파싱
                byte[] encryptedBytes = File.ReadAllBytes(filePath);
                byte[] keyBytes = Encoding.UTF8.GetBytes(KEY);
                byte[] decryptedBytes = Xor(encryptedBytes, keyBytes);
                string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
                return JsonUtility.FromJson<T>(decryptedString);
            }
            else
            {
                // 첫 번째 줄만 읽어 JSON으로 파싱 (Save 시 한 줄로 저장했으므로)
                string JSONData = File.ReadAllLines(filePath)[0];
                return JsonUtility.FromJson<T>(JSONData);
            }
        }
        else
        {
            Debug.LogError($"File not found: '{filePath}'");
            return default(T);
        }
    }

    // ─── 내부 유틸리티 ─────────────────────────────────────────────

    /// <summary>
    /// XOR 암호화 / 복호화 공통 메서드.
    /// XOR은 같은 키로 두 번 적용하면 원래 값으로 돌아오기 때문에
    /// 암호화와 복호화가 완전히 동일한 로직으로 처리된다.
    ///
    /// 키가 데이터보다 짧으면 키를 반복(i % key.Length)해서 사용한다.
    /// </summary>
    /// <param name="input">처리할 바이트 배열</param>
    /// <param name="key">XOR에 사용할 키 바이트 배열</param>
    /// <returns>XOR 처리된 바이트 배열</returns>
    private static byte[] Xor(byte[] input, byte[] key)
    {
        byte[] output = new byte[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            // 각 바이트를 키의 대응 바이트와 XOR
            output[i] = (byte)(input[i] ^ key[i % key.Length]);
        }
        return output;
    }
}
