using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupForward : MonoBehaviour
{
    public StartupHandler sHandler;
    public Material startupMat;

    public void StartDissolve()
    {
        sHandler.StartDissolve();
    }

    public void ResetMat()
    {
        startupMat.SetFloat("_DissolveAmount", 1);
    }
}
