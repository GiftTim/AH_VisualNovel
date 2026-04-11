using UnityEngine;

public class ScreenShotFunction : MonoBehaviour
{
    public static Texture2D CaptureScreenshot(int width, int height, float superSize = 1, string filePath = "") 
        => CaptureScreenshot(Camera.main, width, height, superSize, filePath);
    
    public static Texture2D CaptureScreenshot(Camera cam, int width, int height, float superSize = 1, string filePath = "")
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

        if(filePath != "")
        {
            SaveScreenshotToFile(screenShot, filePath);
        }

        return screenShot;
    }

    public enum ImageType { PNG, JPG }

    public static void SaveScreenshotToFile(Texture2D screenshot, string filePath, ImageType fileType = ImageType.PNG)
    {
        byte[] bytes = new byte[0];
        string extension = "";

        switch (fileType)
        {
            case ImageType.PNG:
                bytes = screenshot.EncodeToPNG();
                break;
            case ImageType.JPG:
                bytes = screenshot.EncodeToJPG();
                break;
        }

        if(!filePath.Contains("."))
        {
            filePath = filePath + extension;
        }

        FileManager.TryCreateDirectoryFromPath(filePath);

        System.IO.File.WriteAllBytes(filePath, bytes);

    }



}
