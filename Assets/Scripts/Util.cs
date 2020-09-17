using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;

public static class Util
{
    public static IEnumerator<float> _EmulateUpdate(System.Action func, MonoBehaviour scr)
    {
        yield return Timing.WaitForOneFrame;
        while (scr.gameObject != null)
        {
            if (scr.gameObject.activeInHierarchy && scr.enabled)
                func();

            yield return Timing.WaitForOneFrame;
        }
    }
}
