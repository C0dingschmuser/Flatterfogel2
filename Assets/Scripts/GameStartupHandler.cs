using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using CodeStage.AntiCheat.Storage;

public class GameStartupHandler : MonoBehaviour
{
    private void Awake()
    {
        /*        int installedBundle = 0;
                PlayerPrefs.SetInt("InstalledBundle", installedBundle);

                int downloadedBundle = PlayerPrefs.GetInt("DownloadedBundle", 0);

                if (installedBundle > downloadedBundle)
                { //wenn installierte version höher dann override
                    PlayerPrefs.SetInt("DownloadedBundle", installedBundle);
                }

                Debug.Log(installedBundle + " " + downloadedBundle);

                if (downloadedBundle > installedBundle)
                { //asset bundle heruntergeladen -> laden
                    UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle("mainbundle", (uint)downloadedBundle, 0);
                    www.SendWebRequest();

                    while(!www.isDone)
                    { //waiting
                        if(www.isNetworkError || www.isNetworkError)
                        {
                            Debug.LogWarning("Network Error!");
                            break;
                        }
                    }

        #if UNITY_EDITOR
                    Debug.Log("BundleLoading Placeholder");
        #else
                    if(www.isDone)
                    {
                        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                    }
        #endif
                }

        #if UNITY_EDITOR
                Debug.Log("LoadingScene Placeholder");
        #else
                StartCoroutine(LoadScene());
        #endif*/

        int clear = ObscuredPrefs.GetInt("PreBetaClear", 0);
        if(clear == 0)
        { //Clear Data from previous version(s) if pre beta version
            ObscuredPrefs.DeleteAll();
            PlayerPrefs.DeleteAll();

            ObscuredPrefs.SetInt("PreBetaClear", 1);
        }

        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("FF");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
