using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 3D 다이스 프리팹을 RenderTexture로 캡처해 2D 스프라이트(시트)로 저장하는 에디터 툴
/// 메뉴: Tools > Dice Sprite Capture
/// </summary>
public class DiceSpriteCapture : EditorWindow
{
    // ── 설정 ──────────────────────────────────────────────
    GameObject   _prefab;
    Material     _material;
    int          _frameCount   = 24;
    int          _resolution   = 256;
    RotationMode _rotationMode = RotationMode.YAxis;
    bool         _spriteSheet  = true;
    string       _outputFolder = "Assets/Resources/Graphics/Dice";

    // 카메라 설정
    float _cameraDistance = 2.5f;
    float _cameraFOV      = 35f;
    Color _bgColor        = new Color(0, 0, 0, 0);

    // 조명 설정
    Color _lightColor     = Color.white;
    float _lightIntensity = 1.2f;

    // 메쉬에서 직접 추출한 면 법선 캐시
    Vector3[]  _meshFaceNormals;
    GameObject _cachedPrefabForNormals;

    // Rolling 모드 파라미터
    float _rollSpeed      = 3.5f;   // 주 회전 속도 (바퀴/루프)
    float _precession     = 0.6f;   // 세차 강도 (0=직선 회전, 1=크게 흔들림)
    float _tiltBias       = 20f;    // 회전축 기울기 (도)

    // 미리보기
    Texture2D _preview;
    int       _previewFrame;

    enum RotationMode { YAxis, Rolling, MeshFaces }

    int ActualFrameCount
    {
        get
        {
            if (_rotationMode != RotationMode.MeshFaces) return _frameCount;
            EnsureFaceNormals();
            return _meshFaceNormals != null ? _meshFaceNormals.Length : 0;
        }
    }

    // ── 윈도우 열기 ────────────────────────────────────────
    [MenuItem("Tools/Dice Sprite Capture")]
    static void Open() => GetWindow<DiceSpriteCapture>("Dice Sprite Capture");

    // ── GUI ───────────────────────────────────────────────
    void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("[ 소스 ]", EditorStyles.boldLabel);

        var prevPrefab = _prefab;
        _prefab   = (GameObject)EditorGUILayout.ObjectField("다이스 프리팹", _prefab, typeof(GameObject), false);
        _material = (Material)EditorGUILayout.ObjectField("머티리얼 (선택)", _material, typeof(Material), false);

        // 프리팹이 바뀌면 캐시 초기화
        if (_prefab != prevPrefab)
            _meshFaceNormals = null;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("[ 캡처 설정 ]", EditorStyles.boldLabel);
        _rotationMode = (RotationMode)EditorGUILayout.EnumPopup("회전 모드", _rotationMode);

        bool isMeshFaces = _rotationMode == RotationMode.MeshFaces;

        GUI.enabled = !isMeshFaces;
        _frameCount = EditorGUILayout.IntSlider("프레임 수", _frameCount, 4, 64);
        GUI.enabled = true;

        // Rolling 파라미터 UI
        if (_rotationMode == RotationMode.Rolling)
        {
            EditorGUILayout.Space(2);
            _rollSpeed  = EditorGUILayout.Slider("회전 속도 (바퀴/루프)", _rollSpeed, 1f, 8f);
            _precession = EditorGUILayout.Slider("세차 강도", _precession, 0f, 1f);
            _tiltBias   = EditorGUILayout.Slider("축 기울기 (도)", _tiltBias, 0f, 60f);
        }

        // MeshFaces 감지 UI
        if (isMeshFaces)
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = _prefab != null;
                if (GUILayout.Button("메쉬에서 면 감지", GUILayout.Width(140)))
                {
                    _meshFaceNormals = null;
                    EnsureFaceNormals();
                }
                GUI.enabled = true;

                if (_meshFaceNormals != null)
                    EditorGUILayout.HelpBox($"감지된 면: {_meshFaceNormals.Length}개", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("프리팹 연결 후 면 감지를 눌러주세요.", MessageType.Warning);
            }
        }

        _resolution  = EditorGUILayout.IntPopup("해상도", _resolution,
                           new[] { "128", "256", "512", "1024" },
                           new[] {  128,   256,   512,  1024  });
        _spriteSheet = EditorGUILayout.Toggle("스프라이트 시트로 저장", _spriteSheet);
        _outputFolder = EditorGUILayout.TextField("출력 폴더", _outputFolder);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("[ 카메라 ]", EditorStyles.boldLabel);
        _cameraDistance = EditorGUILayout.Slider("카메라 거리", _cameraDistance, 1f, 6f);
        _cameraFOV      = EditorGUILayout.Slider("FOV", _cameraFOV, 10f, 60f);
        _bgColor        = EditorGUILayout.ColorField("배경색 (알파=투명)", _bgColor);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("[ 조명 ]", EditorStyles.boldLabel);
        _lightColor     = EditorGUILayout.ColorField("조명 색상", _lightColor);
        _lightIntensity = EditorGUILayout.Slider("조명 강도", _lightIntensity, 0.1f, 3f);

        EditorGUILayout.Space(10);

        // ── 미리보기 ──
        using (new EditorGUILayout.HorizontalScope())
        {
            bool canPreview = _prefab != null && (!isMeshFaces || (_meshFaceNormals != null && _meshFaceNormals.Length > 0));
            GUI.enabled = canPreview;
            if (GUILayout.Button("미리보기", GUILayout.Width(120)))
                UpdatePreview();
            GUI.enabled = true;

            if (_preview != null && ActualFrameCount > 0)
            {
                int maxFrame = ActualFrameCount - 1;
                _previewFrame = Mathf.Clamp(_previewFrame, 0, maxFrame);
                _previewFrame = EditorGUILayout.IntSlider(_previewFrame, 0, maxFrame);
                if (GUILayout.Button("◀", GUILayout.Width(30))) { _previewFrame = Mathf.Max(0, _previewFrame - 1); UpdatePreview(); }
                if (GUILayout.Button("▶", GUILayout.Width(30))) { _previewFrame = Mathf.Min(maxFrame, _previewFrame + 1); UpdatePreview(); }
            }
        }

        if (_preview != null)
        {
            float size = Mathf.Min(position.width, 200);
            Rect rect = GUILayoutUtility.GetRect(size, size);
            GUI.DrawTexture(rect, _preview, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.Space(10);

        bool canCapture = _prefab != null && (!isMeshFaces || (_meshFaceNormals != null && _meshFaceNormals.Length > 0));
        GUI.enabled = canCapture;
        if (GUILayout.Button("캡처 시작", GUILayout.Height(36)))
            Capture();
        GUI.enabled = true;

        if (_prefab == null)
            EditorGUILayout.HelpBox("다이스 프리팹을 먼저 연결해주세요.", MessageType.Info);
    }

    // ── 면 법선 캐시 보장 ─────────────────────────────────
    void EnsureFaceNormals()
    {
        if (_prefab == null) return;
        if (_meshFaceNormals != null && _cachedPrefabForNormals == _prefab) return;

        _meshFaceNormals = ExtractFaceNormalsFromMesh(_prefab);
        _cachedPrefabForNormals = _prefab;

        if (_meshFaceNormals != null)
            Debug.Log($"[DiceSpriteCapture] 메쉬에서 {_meshFaceNormals.Length}개 면 감지 완료");
        else
            Debug.LogError("[DiceSpriteCapture] 메쉬에서 MeshFilter를 찾을 수 없습니다.");
    }

    // ── 실제 메쉬 삼각형에서 면 법선 추출 ────────────────
    // UV 분할 등으로 삼각형이 많더라도 동일 방향끼리 그룹화해 고유 면만 반환
    static Vector3[] ExtractFaceNormalsFromMesh(GameObject prefab)
    {
        var mf = prefab.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return null;

        Mesh     mesh      = mf.sharedMesh;
        Vector3[] verts    = mesh.vertices;
        int[]     tris     = mesh.triangles;

        var uniqueNormals = new List<Vector3>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 a = verts[tris[i]];
            Vector3 b = verts[tris[i + 1]];
            Vector3 c = verts[tris[i + 2]];

            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

            // 볼록 메쉬: 면 중심이 법선과 같은 방향인지 확인 (안쪽을 향하면 반전)
            Vector3 center = (a + b + c) / 3f;
            if (Vector3.Dot(normal, center) < 0f)
                normal = -normal;

            // 이미 등록된 법선과 5° 이내면 중복으로 간주
            bool isDuplicate = false;
            foreach (var n in uniqueNormals)
            {
                if (Vector3.Angle(n, normal) < 5f) { isDuplicate = true; break; }
            }

            if (!isDuplicate)
                uniqueNormals.Add(normal);
        }

        return uniqueNormals.ToArray();
    }

    // ── 미리보기 (단일 프레임) ─────────────────────────────
    void UpdatePreview()
    {
        if (_prefab == null) return;
        _preview = CaptureFrame(_previewFrame, ActualFrameCount, _resolution);
        Repaint();
    }

    // ── 전체 캡처 ─────────────────────────────────────────
    void Capture()
    {
        if (!Directory.Exists(_outputFolder))
            Directory.CreateDirectory(_outputFolder);

        int count = ActualFrameCount;
        var frames = new Texture2D[count];
        for (int i = 0; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("다이스 캡처 중...", $"프레임 {i + 1}/{count}", (float)i / count);
            frames[i] = CaptureFrame(i, count, _resolution);
        }
        EditorUtility.ClearProgressBar();

        if (_spriteSheet) SaveAsSpriteSheet(frames);
        else              SaveAsIndividualPNGs(frames);

        AssetDatabase.Refresh();
        Debug.Log($"[DiceSpriteCapture] 완료 → {_outputFolder}");
    }

    // ── 프레임 1장 캡처 ───────────────────────────────────
    Texture2D CaptureFrame(int frameIndex, int totalFrames, int res)
    {
        const int LAYER = 30;

        // ① 다이스 인스턴스
        GameObject diceGO = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
        foreach (Transform t in diceGO.GetComponentsInChildren<Transform>())
            t.gameObject.layer = LAYER;
        if (_material != null)
            foreach (var mr in diceGO.GetComponentsInChildren<MeshRenderer>())
                mr.sharedMaterial = _material;

        diceGO.transform.rotation = GetRotation(frameIndex, totalFrames);

        // ② 카메라
        var camGO = new GameObject("_CaptureCamera");
        var cam   = camGO.AddComponent<Camera>();
        cam.cullingMask     = 1 << LAYER;
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = _bgColor;
        cam.fieldOfView     = _cameraFOV;
        cam.nearClipPlane   = 0.1f;
        cam.farClipPlane    = 100f;
        camGO.transform.position = new Vector3(0, 0, -_cameraDistance);
        camGO.transform.LookAt(Vector3.zero);

        // ③ 조명 (메인 + 필)
        var lightGO = new GameObject("_CaptureLight");
        var light   = lightGO.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.color     = _lightColor;
        light.intensity = _lightIntensity;
        lightGO.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        var fillGO  = new GameObject("_CaptureFill");
        var fill    = fillGO.AddComponent<Light>();
        fill.type      = LightType.Directional;
        fill.color     = new Color(0.6f, 0.7f, 1f);
        fill.intensity = 0.4f;
        fillGO.transform.rotation = Quaternion.Euler(-20f, 150f, 0f);

        // ④ 캡처
        var rt = new RenderTexture(res, res, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing   = 4;
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, res, res), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        // ⑤ 정리
        cam.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(camGO);
        DestroyImmediate(lightGO);
        DestroyImmediate(fillGO);
        DestroyImmediate(diceGO);

        return tex;
    }

    // ── 회전 계산 ─────────────────────────────────────────
    Quaternion GetRotation(int frameIndex, int totalFrames)
    {
        float t = (float)frameIndex / totalFrames;
        switch (_rotationMode)
        {
            case RotationMode.YAxis:
                return Quaternion.Euler(20f, t * 360f, 0f);

            case RotationMode.Rolling:
                return GetRollingRotation(t);

            case RotationMode.MeshFaces:
            default:
                return GetMeshFaceRotation(frameIndex);
        }
    }

    // ── 세차운동 굴러가는 모션 ────────────────────────────
    // 회전축 자체가 sin/cos로 느리게 회전(세차)하며 굴러가는 느낌 생성
    Quaternion GetRollingRotation(float t)
    {
        // 세차 각도: 1루프에 한 바퀴 돌며 축이 흔들림
        float precAngle = t * Mathf.PI * 2f;

        // 기울어진 기본 축 + 세차에 의한 흔들림
        Vector3 axis = new Vector3(
            Mathf.Sin(precAngle) * _precession,
            1f,
            Mathf.Cos(precAngle) * _precession * 0.6f
        ).normalized;

        // 기울기 바이어스 적용 (축을 살짝 앞으로 기울임)
        Quaternion tilt = Quaternion.AngleAxis(_tiltBias, Vector3.right);
        axis = tilt * axis;

        // 주 회전: t * 360 * speed
        float mainAngle = t * 360f * _rollSpeed;
        return Quaternion.AngleAxis(mainAngle, axis);
    }

    // ── 메쉬 면 회전: 해당 면 법선이 카메라(-Z)를 향하도록 ──
    Quaternion GetMeshFaceRotation(int faceIndex)
    {
        if (_meshFaceNormals == null || faceIndex >= _meshFaceNormals.Length)
            return Quaternion.identity;

        return Quaternion.FromToRotation(_meshFaceNormals[faceIndex], Vector3.back);
    }

    // ── 스프라이트 시트 저장 ──────────────────────────────
    void SaveAsSpriteSheet(Texture2D[] frames)
    {
        int cols  = Mathf.CeilToInt(Mathf.Sqrt(frames.Length));
        int rows  = Mathf.CeilToInt((float)frames.Length / cols);
        int res   = _resolution;
        var sheet = new Texture2D(cols * res, rows * res, TextureFormat.RGBA32, false);

        var clear = new Color[cols * res * rows * res];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        sheet.SetPixels(clear);

        for (int i = 0; i < frames.Length; i++)
        {
            int col = i % cols;
            int row = rows - 1 - (i / cols);
            sheet.SetPixels(col * res, row * res, res, res, frames[i].GetPixels());
            DestroyImmediate(frames[i]);
        }
        sheet.Apply();

        string path = Path.Combine(_outputFolder, "Dice_SpriteSheet.png");
        File.WriteAllBytes(path, sheet.EncodeToPNG());
        DestroyImmediate(sheet);
        Debug.Log($"스프라이트 시트 저장: {path}  ({cols}×{rows} = {frames.Length}프레임)");
    }

    // ── 개별 PNG 저장 ─────────────────────────────────────
    void SaveAsIndividualPNGs(Texture2D[] frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            string path = Path.Combine(_outputFolder, $"Dice_Frame_{i:D3}.png");
            File.WriteAllBytes(path, frames[i].EncodeToPNG());
            DestroyImmediate(frames[i]);
        }
        Debug.Log($"개별 PNG {frames.Length}장 저장: {_outputFolder}");
    }
}
