using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TagManager
{
    private readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>();
    private readonly Regex tagRegex = new Regex("<\\w+>");

    public TagManager()
    {
        InitializeTags();
    }  
    
    private void InitializeTags()
    {
        tags["<mainChar"] = () => "Avira";
        
    }

}
