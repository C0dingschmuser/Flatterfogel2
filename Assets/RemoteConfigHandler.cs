using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.RemoteConfig;

public class RemoteConfigHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetGetDefaults();
    }

    public void SetGetDefaults()
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();

        defaults.Add("config_test_string", "default local string");

        FirebaseRemoteConfig.SetDefaults(defaults);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
