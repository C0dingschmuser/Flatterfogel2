using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.Storage;
using System.Runtime.CompilerServices;

public class ExceptionHandler : MonoBehaviour
{
    public static string exceptionString = "";

    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;

        exceptionString = ObscuredPrefs.GetString("ExceptionString", "");
    }

    //Called when there is an exception
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Assert ||
            type == LogType.Exception ||
            type == LogType.Error)
        {
            string hasError = "";

            string data = "C: " + condition + " ST: " + stackTrace;

            if (type == LogType.Assert ||
                type == LogType.Exception ||
                type == LogType.Error)
            {
                hasError = "LogType.Error";
                data = hasError + " " + data;
            }

            exceptionString += data + '\n';

            ObscuredPrefs.SetString("ExceptionString", exceptionString);
        }
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }
}
