using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class TestDisto : MonoBehaviour
{
    public PostProcessVolume defaultVolume;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        defaultVolume.profile.TryGetSettings(out LensDistortion lensDisto);

        DOTween.To(() => lensDisto.intensity.value, x => lensDisto.intensity.value = x, -100, 1f);
        DOTween.To(() => lensDisto.scale.value, x => lensDisto.scale.value = x, 0.05f, 1f);
    }

    private void OnDisable()
    {
        defaultVolume.profile.TryGetSettings(out LensDistortion lensDisto);

        DOTween.To(() => lensDisto.intensity.value, x => lensDisto.intensity.value = x, 0, 1f);
        DOTween.To(() => lensDisto.scale.value, x => lensDisto.scale.value = x, 1, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
