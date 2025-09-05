using System;
using UnityEngine;

public class CMD_DatabaseExtension_Examples : CMD_DatabaseExtension
{
    new public static void Extend(CommandDatabase database)
    {
        // Add Action with no parameters
        database.AddCommand("print", new Action(PrintDefaultMassage));
        database.AddCommand("print_1p", new Action<string>(PrintUserMessage));
        database.AddCommand("print_mp", new Action<string[]>(PrintLines));

        // Add lambda with no parameters
        database.AddCommand("lambda", new Action(() =>
        {
            Debug.Log("Printing a default Message to console from lambda command.");
        }));

        
        // Add coroutine with no parameters
    }

    private static void PrintDefaultMassage()
    {
        Debug.Log("Printing a default Message to console");
    }

    private static void PrintUserMessage(string message)
    {
        Debug.Log($"User Mesage: '{message}'");
    }

    private static void PrintLines(string[] lines)
    {
        int i = 1;
        foreach (string line in lines)
        {
            Debug.Log($"{i++}, '{line}'");
        }
    }
    
}
