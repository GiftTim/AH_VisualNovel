using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandDatabase
{
    private Dictionary<string, Delegate> database = new Dictionary<string, Delegate>();

    public bool HasCommand(string commandName)
    {
        return database.ContainsKey(commandName);
    }
}
