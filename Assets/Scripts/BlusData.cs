using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using DG.Tweening;

public class BlusData : MonoBehaviour
{
    public bool isDestroyed = false, endAnimation = false, modeChangeBlus = false, renderDisabled = true,
        isCoin = false, blusActive = false, moveDir = false;
    public float timer = 5f, yDiff = 0, maxYDiff = 50, speed = 10;

    private int anCounter = 0;
    private Vector3 startPos;
    private List<Vector3> oldDestroyedPos = new List<Vector3>();
    private Coroutine animationRoutine = null;

    public UnityEngine.Experimental.Rendering.Universal.Light2D lightObj = null;

    public GameObject[] assignedPipes = new GameObject[2];

    public Color32 defaultLightColor;

    [SerializeField]
    private ParticleSystem effect = null, deathEffect = null;

    [SerializeField]
    private Sprite[] blusAnimation = null;

    [SerializeField] private GameObject destroyedBlusPartPrefab = null;
    [SerializeField] private Transform destroyedBlusParent = null, imageObj = null, blusLight = null;

    public void SpawnCoin(Vector3 newPos, Sprite[] animationSprites)
    {
        ResetBlus();

        imageObj.GetComponent<SpriteRenderer>().sortingOrder = 8;

        blusAnimation = animationSprites;
        isCoin = true;

        transform.position = newPos;

        imageObj.gameObject.SetActive(true);
        imageObj.GetComponent<SpriteRenderer>().sprite = animationSprites[0];

        DestroyBlus(1, 1);
    }

    public void SetSprites(Sprite[] animationSprites)
    {
        blusAnimation = animationSprites;

        animationRoutine = StartCoroutine(HandleAnimation(Random.Range(0, 13)));
    }

    public void ResetBlus(GameObject pipeTop = null, GameObject pipeBot = null, int type = 0)
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        imageObj.GetComponent<BoxCollider2D>().enabled = true;

        imageObj.GetComponent<SpriteRenderer>().sortingOrder = 5;
        imageObj.GetComponent<SpriteRenderer>().color = Color.white;
        imageObj.gameObject.SetActive(false);

        isCoin = false;

        if(pipeTop != null)
        {
            assignedPipes[0] = pipeTop;
        }

        if(pipeBot != null)
        {
            assignedPipes[1] = pipeBot;
        }

        renderDisabled = true;
        isDestroyed = false;
        endAnimation = false;
        modeChangeBlus = false;
        timer = 5f;

        effect.gameObject.SetActive(true);
        deathEffect.gameObject.SetActive(false);

        ParticleSystem.EmissionModule eM = effect.emission;
        eM.rateOverTime = 8;

        Vector3 cPos = transform.position;

        transform.position = startPos;

        destroyedBlusParent.gameObject.SetActive(false);
        for(int i = 0; i < destroyedBlusParent.transform.childCount; i++)
        { //reset destroyed obj position
            Transform obj = destroyedBlusParent.transform.GetChild(i);
            obj.rotation = Quaternion.identity;
            obj.transform.position = oldDestroyedPos[i];
            obj.localScale = new Vector3(1, 1, 1);
        }

        if(OptionHandler.lightEnabled == 0)
        {
            blusLight.gameObject.SetActive(false);
        } else
        {
            blusLight.gameObject.SetActive(true);
        }

       // if(OptionHandler.hardcoreActive)
        //{
        //    lightObj.intensity = 0;
        //    blusLight.gameObject.SetActive(true);
        //} else
        //{
            lightObj.intensity = 1;
        //}

        transform.position = cPos;

        DisableEnableLight(false);
    }

    public void DisableEnableLight(bool enable)
    {
        blusLight.gameObject.SetActive(enable);
    }

    public void DisableEnbleSprite(bool enable)
    {
        imageObj.gameObject.SetActive(enable);
    }

    IEnumerator HandleAnimation(int offset)
    {
        anCounter = 0 + offset;
        int dir = 0;

        imageObj.GetComponent<SpriteRenderer>().flipX = false;

        while (true)
        {
            imageObj.GetComponent<SpriteRenderer>().sprite = blusAnimation[anCounter];
            
            if(dir == 0)
            {
                anCounter++;
            } else
            {
                anCounter--;
            }

            if (!isCoin)
            {
                if (anCounter > 18)
                {
                    anCounter = 17;
                    dir = 1;

                    imageObj.GetComponent<SpriteRenderer>().flipX = true;
                }
                else if (anCounter < 0)
                {
                    anCounter = 1;
                    dir = 0;

                    imageObj.GetComponent<SpriteRenderer>().flipX = false;
                }
            }
            else
            {
                if (anCounter > 35)
                {
                    anCounter = 0;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetBlusPipeType(int type)
    {
        Color32 c = defaultLightColor;

        switch(type)
        {
            case 1: //nur moving
                c = new Color32(255, 153, 0, 255);
                break;
            case 2: //destructable
                c = new Color32(0, 143, 255, 255);
                break;
        }

        lightObj.color = c;
    }

    public void UpdateHardcoreLight()
    {
        float abstand = Mathf.Abs(6.4f - transform.position.x);
        float newIntensity = 1 - (abstand / 152.9f);

        newIntensity *= 1.5f;

        if(transform.position.x > 6.4f || 
            transform.position.x < -146.5f)
        {
            newIntensity = 0;
        }

        lightObj.intensity = newIntensity;
    }

    public void DestroyBlus(int effectCode = 3, float time = 1f)
    {
        if(animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        blusActive = false;

        imageObj.GetComponent<BoxCollider2D>().enabled = false;
        //destroyedBlusParent.gameObject.SetActive(true);

        ParticleSystem.EmissionModule eM = effect.emission;
        eM.rateOverTime = 0;

        if(effectCode > 2)
        {
            deathEffect.gameObject.SetActive(true);
        }

        if(isCoin)
        {
            StartCoroutine(DisableBlus(time));
        } else
        {
            imageObj.gameObject.SetActive(false);
            blusLight.gameObject.SetActive(false);
            isDestroyed = true;
        }
    }

    private IEnumerator FinalAnimation(float waitTime)
    {
        int dir = 0;

        if(anCounter > 18)
        { //hochzählen geht schneller
            dir = 1;
        }

        //anzahl an frames bis front ansicht
        int remaining = anCounter;

        if(dir == 1)
        {
            remaining = 35 - anCounter;
        }

        float step = waitTime / remaining;

        while(anCounter != 0)
        {
            if(dir == 0)
            {
                anCounter--;
            } else
            {
                anCounter++;
            }

            if (anCounter < 0 || anCounter > 35)
            {
                anCounter = 0;
            }

            imageObj.GetComponent<SpriteRenderer>().sprite = blusAnimation[anCounter];

            yield return new WaitForSeconds(step);
        }
    }

    private IEnumerator DisableBlus(float waitTime)
    {
        transform.DOMove(new Vector3(-710.3f, 1396, 100), waitTime).SetEase(Ease.InBack);

        StartCoroutine(FinalAnimation(waitTime * 0.5f));
        yield return new WaitForSeconds(waitTime * 0.8f);

        transform.DOScale(0, waitTime * 0.2f);

        yield return new WaitForSeconds(waitTime * 0.2f);

        ShopHandler.Instance.CoinAddEffect();

        imageObj.gameObject.SetActive(false);
        blusLight.gameObject.SetActive(false);
        isDestroyed = true;
    }

    public void DoDownscale()
    {
        effect.gameObject.SetActive(false);

        int len = destroyedBlusParent.childCount;
        for(int i = 0; i < len; i++)
        {
            destroyedBlusParent.GetChild(i).DOScale(0, 0.75f);
        }
    }

    public void StartMove(float speed, float maxDiff)
    {
        blusActive = true;

        yDiff = Random.Range(-30, 30);

        this.speed = speed;
        maxYDiff = maxDiff;
    }

    private void Update()
    {
        if(blusActive && !isDestroyed && TutorialHandler.Instance.mainTut != 0)
        {
            Vector3 pos = transform.position;
            float dist = speed * Time.deltaTime;

            if(!moveDir)
            { //hoch
                pos.y += dist;

                yDiff += dist;
            } else
            { //runter
                pos.y -= dist;

                yDiff -= dist;
            }

            transform.position = pos;

            if(Mathf.Abs(yDiff) >= maxYDiff)
            {
                //yDiff = 0;

                if(!moveDir)
                {
                    moveDir = true;
                } else
                {
                    moveDir = false;
                }
            }
        }
    }

    public void GenerateDestroyedParts(int resolution)
    {
        if(destroyedBlusParent.transform.childCount > 0)
        {
            for(int i = 0; i < destroyedBlusParent.transform.childCount; i++)
            {
                Destroy(destroyedBlusParent.transform.GetChild(i).gameObject);
            }
        }

        oldDestroyedPos.Clear();

        this.startPos = transform.position;

        return; //disabled da 3d effekt
#pragma warning disable CS0162

        Vector3 startPos = transform.position;
        startPos.x -= 50f;
        startPos.y -= 50f;

        float offset = ((100f / resolution) / 2f);
        float width = offset;

        int pixelOffset = 42 / resolution;

        startPos.x += offset;
        startPos.y += offset;

        Texture2D tex = imageObj.GetComponent<SpriteRenderer>().sprite.texture;

        /*Sprite test = Sprite.Create(tex, new Rect(0, 0, 40, 40), new Vector2(0.5f, 0.5f));
        imageObj.GetComponent<SpriteRenderer>().sprite = test;*/

        tex.filterMode = FilterMode.Point;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                Sprite newSprite =
                    Sprite.Create(tex,
                                    new Rect(x * pixelOffset, y * pixelOffset, pixelOffset, pixelOffset),
                                    new Vector2(0.5f, 0.5f));


                float ph = 18.75f;
                if(resolution > 4)
                {
                    ph = 21.875f;
                }

                GameObject newDeathPart = Instantiate(destroyedBlusPartPrefab,
                                                        new Vector3(startPos.x + (width * x) + ph,
                                                                    startPos.y + (width * y) + ph),
                                                        Quaternion.identity, destroyedBlusParent);

                //newDeathPart.GetComponent<RectTransform>().sizeDelta = 
                //    new Vector2(width, width);

                newDeathPart.GetComponent<SpriteRenderer>().sprite = newSprite;
                newDeathPart.AddComponent<PolygonCollider2D>();

                oldDestroyedPos.Add(newDeathPart.transform.position);
                //newDeathPart.GetComponent<BoxCollider2D>().size = new Vector2(width / 58.5f, width / 58.5f);

                //newDeathPart.GetComponent<DeadData>().originalPos = newDeathPart.transform.position;
            }
        }
#pragma warning restore CS0162
    }
}
