using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

public static class FirebaseHandler
{
    //Firebase calls hierüber ausführen für Check ob Initialized

    public static void SetUserProperty(string name, string property)
    {
        if(FirebaseAnalyticsInitialize.firebaseReady)
        {
            FirebaseAnalytics.SetUserProperty(name, property);
        }
    }

    public static void LogEvent(string eventName)
    {
        if(FirebaseAnalyticsInitialize.firebaseReady)
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
    }

    public static void LogEvent(string eventName, string parameterName, long parameterValue)
    {
        if (FirebaseAnalyticsInitialize.firebaseReady)
        {
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }
    }

    public static void SetCurrentScreen(string screenName, string screenClass)
    {
        if(FirebaseAnalyticsInitialize.firebaseReady)
        {
            FirebaseAnalytics.SetCurrentScreen(screenName, screenClass);
        }
    }
}
