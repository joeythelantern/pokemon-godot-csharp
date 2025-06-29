using Godot;
using System;

namespace Game.Core;

public static class Logger
{
    public static void Log(LogLevel level, params object[] message)
    {
        var dateTime = DateTime.Now;
        string timeStamp = $"[{dateTime:yyyy-MM-dd HH:mm:ss}]";
        var callingMethod = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod();
        string logMessage = $"{timeStamp} [{level}] [{callingMethod.DeclaringType.Name}] [{callingMethod.Name}] ";
        string color = "CYAN";

        switch (level)
        {
            case LogLevel.DEBUG:
                color = "WHITE";
                break;
            case LogLevel.INFO:
                color = "CYAN";
                break;
            case LogLevel.WARNING:
                color = "YELLOW";
                break;
            case LogLevel.ERROR:
                color = "RED";
                break;
            default:
                break;
        }

        GD.PrintRich([$"[color={color}]{logMessage}[/color]", .. message]);
    }

    public static void Debug(params object[] message)
    {
        Log(LogLevel.DEBUG, message);
    }

    public static void Info(params object[] message)
    {
        Log(LogLevel.INFO, message);
    }

    public static void Warning(params object[] message)
    {
        Log(LogLevel.WARNING, message);
    }

    public static void Error(params object[] message)
    {
        Log(LogLevel.ERROR, message);
    }
}
