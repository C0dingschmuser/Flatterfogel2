using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Dissolver : MonoBehaviour
{
    [SerializeField]
    private Material material = null;
    [SerializeField]
    private float startDissolveAmount = 1;
    [SerializeField]
    private float dissolveTime = 1;
    private float dissolveAmount = 1;
    [SerializeField]
    private float endDissolveAmount = 0;
    private bool isDissolving = false;

    private Tween dissolveTween = null;

    public bool runOnStartup = false;

    // Start is called before the first frame update
    void Start()
    {
        if (runOnStartup)
        {
            StartDissolve(new Color32(53, 0, 162, 255));
        }
    }

    public void StartDissolve(Color32 color, float time = 1f)
    {
        if (dissolveTween != null)
        {
            dissolveTween.Kill();
        }

        material.SetFloat("_DissolveAmount", startDissolveAmount);

        dissolveAmount = startDissolveAmount;
        time = dissolveTime;

        dissolveTween = DOTween.To(() => dissolveAmount, x => dissolveAmount = x, endDissolveAmount, time);
        dissolveTween.OnUpdate(() =>
        {
            material.SetFloat("_DissolveAmount", dissolveAmount);
        });

        material.SetFloat("_DissolveAmount", dissolveAmount);
        material.SetColor("DissolveColor", color);

        dissolveTime = time;

        isDissolving = true;
    }

    public void ResetDissolve()
    {
        if(dissolveTween != null)
        {
            dissolveTween.Kill();
        }

        material.SetFloat("_DissolveAmount", 1);
        isDissolving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isDissolving)
        {
            //material.SetFloat("_DissolveAmount", dissolveAmount);

            if(dissolveAmount.Equals(0))
            {
                isDissolving = false;
            }
        }
    }
}
