using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainHandler : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sRenderer = null;

    [SerializeField]
    private Train[] allTrains = null;
    [SerializeField]
    private Train currentTrain = null;

    private float speed = 100f;
    private bool night = false;

    private void Awake()
    {
        sRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator SetNightDelayed(float delay, bool night)
    {
        yield return new WaitForSeconds(delay);

        SetNight(night);
    }

    private void SetNight(bool night)
    {
        this.night = night;

        if(night)
        {
            sRenderer.sprite = currentTrain.nightSprite;
        } else
        {
            sRenderer.sprite = currentTrain.sprite;
        }
    }

    public void UpdateTrain(float scrollSpeed)
    {
        transform.Translate(Vector3.left * (speed  + scrollSpeed) * Time.deltaTime);

        if(transform.position.x < 475 + currentTrain.length)
        {
            if(transform.position.x < -789 - currentTrain.length)
            {
                if(sRenderer.enabled)
                {
                    sRenderer.enabled = false;
                    transform.Translate(Vector3.right * Random.Range(2000, 3500));
                    speed = Random.Range(100, 250);

                    currentTrain = allTrains[Random.Range(0, allTrains.Length)];

                    if(night)
                    {
                        sRenderer.sprite = currentTrain.nightSprite;
                    } else
                    {
                        sRenderer.sprite = currentTrain.sprite;
                    }
                }
            } else
            {
                if (!sRenderer.enabled)
                {
                    sRenderer.enabled = true;
                }
            }
        }
    }
}
