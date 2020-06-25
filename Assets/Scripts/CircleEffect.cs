using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CircleEffect : MonoBehaviour
{
    public int segments;
    public float radius;
    LineRenderer line;
    private bool effectRunning = false;
    private float width = 30;

    public Color effectColor;

    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        effectColor = Color.white;

        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        line.startWidth = width;
        line.endWidth = width;

        line.enabled = false;
    }


    public void BeginEffect(Color newColor, Vector3 pos)
    {
        transform.position = pos;
        line.enabled = true;
        effectRunning = true;

        radius = 0.75f;
        effectColor = newColor;
        CreatePoints();

        line.startColor = effectColor;
        line.endColor = effectColor;

        DOTween.To(() => radius, x => radius = x, 10, .5f).SetEase(Ease.Linear);
        DOTween.To(() => width, x => width = x, 0, .5f).SetEase(Ease.Linear);
        Invoke("EndEffect", .51f);
    }

#pragma warning disable IDE0051 // Nicht verwendete private Member entfernen
    void EndEffect()
#pragma warning restore IDE0051 // Nicht verwendete private Member entfernen
    {
        line.enabled = false;
        effectRunning = false;
    }

    void CreatePoints()
    {
        float x;
        float y;
        float z = 0f;

        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle);
            y = Mathf.Cos(Mathf.Deg2Rad * angle);
            line.SetPosition(i, new Vector3(x, y, z) * radius);
            angle += (360f / segments);
        }
    }

    private void Update()
    {
        if(effectRunning)
        {
            if(line.enabled)
            {
                CreatePoints();
                line.startWidth = width;
                line.endWidth = width;
            }
        }
    }
}