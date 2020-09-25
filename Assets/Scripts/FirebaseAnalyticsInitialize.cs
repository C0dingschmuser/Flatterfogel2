using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseAnalyticsInitialize : MonoBehaviour
{

    public static bool firebaseReady = false, loaded = false;

    public static FirebaseAnalyticsInitialize Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CheckIfReady();
        Timing.RunCoroutine(_WaitForFinish());
    }

    IEnumerator<float> _WaitForFinish()
    {
        while(!loaded)
        {
            yield return Timing.WaitForSeconds(0.05f);
        }

        RemoteConfigHandler.Instance.SetGetDefaults();
    }

    public void CheckIfReady()
    {

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {

                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                firebaseReady = true;
                loaded = true;
                Debug.Log("Firebase is ready for use.");

                // Create and hold a reference to your FirebaseApp, i.e.
                //   app = Firebase.FirebaseApp.DefaultInstance;
                // where app is a Firebase.FirebaseApp property of your application class.

                // Set a flag here indicating that Firebase is ready to use by your
                // application.
            }
            else
            {
                firebaseReady = false;
                loaded = false;
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
}
