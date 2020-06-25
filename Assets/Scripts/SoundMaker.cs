using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SoundMaker : MonoBehaviour
{
    private bool running = false;

    private string data = "";
    SoundManager sM = null;

    // Start is called before the first frame update
    void Start()
    {
        sM = GetComponent<SoundManager>();
        StartCoroutine(RecordTrackData());
    }

    IEnumerator RecordTrackData()
    {
        while(true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!running)
                {
                    running = true;
                    data = "";
                    sM.PlayMusic(MusicID.Track2);
                    Debug.Log("start");
                }
                else
                {
                    running = false;
                    //EditorGUIUtility.systemCopyBuffer = data;
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                data += "1;";
                Debug.Log("log");
            }
            else
            {
                data += "0;";
            }

            yield return new WaitForSeconds(0.01f); //100 mal die sekunde
        }
    }
}
