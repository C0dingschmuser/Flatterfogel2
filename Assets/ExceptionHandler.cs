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
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);

        Application.logMessageReceived += LogCallback;

        exceptionString = ObscuredPrefs.GetString("ExceptionString", "");
    }

    private void Start()
    {
        FirebaseAnalyticsInitialize.CheckIfReady();
    }

    //Called when there is an exception
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        string data = "C: " + condition + " ST: " + stackTrace;

        if (type == LogType.Assert ||
            type == LogType.Exception ||
            type == LogType.Error ||
            (type == LogType.Warning && !data.Contains("VideoPlayer")))
        {
            string hasError = "";

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
