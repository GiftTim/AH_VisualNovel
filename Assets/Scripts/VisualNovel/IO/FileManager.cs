using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;

public class FileManager 
{
    public static List<string> ReadTextFile(string filePath, bool includeBlackLines = true)
    {
        if(!filePath.StartsWith('/'))
            filePath = FilePaths.root + filePath;

        List<string> lines = new List<string>();

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (includeBlackLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch(FileNotFoundException ex)
        {
            Debug.LogError($"File not found: {ex.FileName}");
        }

        return lines;
    }

    public static List<string> ReadTextAsset(string filePath, bool includeBlackLines = true)
    {

        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if (asset == null)
        {
            Debug.LogError($"TextAsset not found: '{filePath}'");
            return null;
        }

        return ReadTextAsset(asset, includeBlackLines);
    }

    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlackLines = true)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(asset.text))
        {
            while(sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                if (includeBlackLines || !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }
        }
        return lines;
    }
}
