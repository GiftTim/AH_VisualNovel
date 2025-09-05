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

    public void AddCommand(string commandName, Delegate command)
    {
        if (!database.ContainsKey(commandName))
        {
            database.Add(commandName, command);
        }
        else
            Debug.Log($"Command already exists in the database '{commandName}'");
    }

    public Delegate GetCommand(string commandName)
    {
        if (!database.ContainsKey(commandName))
        {
            Debug.Log($"Command '{commandName}' does not exist in the database!");
            return null;
        }

        return database[commandName];
            
    }
}
