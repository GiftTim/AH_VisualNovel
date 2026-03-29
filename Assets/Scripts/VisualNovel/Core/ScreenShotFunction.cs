using UnityEngine;

public class ScreenShotFunction : MonoBehaviour
{
    public static Texture2D CaptureScreenshot(int width, int height, int superSize = 1, string filePath = "") 
        => CaptureScreenshot(Camera.main, width, height, superSize, filePath);
    
    public static Texture2D CaptureScreenshot(Camera cam, int width, int height, int superSize = 1, string filePath = "")
    {
        if(superSize != 1)
        {
            width = Mathf.RoundToInt(width * superSize);
            height = Mathf.RoundToInt(height * superSize);
        }

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 32);

        cam.targetTexture = rt;

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);

        cam.Render();

        RenderTexture.active = rt;

        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return screenShot;
    }

    public enum ImageType { PNG, JPG }

    public static void SaveScreenshotToFile(Texture2D screenshot, string filePath, string fileType = ".png")
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("File path is null or empty. Cannot save screenshot.");
            return;
        }

        if (!filePath.EndsWith(fileType))
        {
            filePath += fileType;
        }
    {
        byte[] bytes = screenshot.EncodeToJPG();
        System.IO.File.WriteAllBytes(filePath, bytes);
    }
}
