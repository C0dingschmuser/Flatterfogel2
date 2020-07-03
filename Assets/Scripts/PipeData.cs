using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

using Random = UnityEngine.Random;

public class PipeData : MonoBehaviour
{

    public bool isChecked = false, beginDestruction = false, isTop = false, destructionComplete = false,
        highscorePipe = false, destructable = false, nextSpawned = false, emptyPipe = false;

    public int renderDeactivated = 2;

    public GameObject middleObj = null;
    public Pipe thisPipe = null;

    [SerializeField]
    private UnityEngine.Experimental.Rendering.Universal.Light2D lightObj = null;
    
    public float colorTime, abstand;
    public Transform particleParent;

    private float fadeTime = 0.4f, lightTime = 1f, yDiffToWall = 0, divConst = 1;
    private bool destructionStarted = false, fullDestructionStarted = false, physicsCalculated = false;

    private int count = 0, max = 0, realMax = 0, pipeExpId = 0, frameWait = 1;
    private List<GameObject> destroyedParts = new List<GameObject>();
    private ObjectPooler objectPooler;

    private Vector3 globalScale = new Vector3(1, 1, 1);
    private bool destroyFull = false;

    [SerializeField]
    private Sprite squareSprite = null;

#pragma warning disable 0649
    [SerializeField] private Color currentColor;
#pragma warning restore 0649

    public bool IsDestructionStarted(bool full = false)
    {
        if(!full)
        {
            return destructionStarted;
        } else
        {
            return fullDestructionStarted;
        }
    }

    public void ResetPipe(bool isTop = false, bool highscorePipe = false, bool emptyPipe = false)
    {
        physicsCalculated = false;
        isChecked = false;
        beginDestruction = false;
        destructable = false;
        destroyFull = false;
        destructionComplete = false;
        this.highscorePipe = highscorePipe;
        destructionStarted = false;
        fullDestructionStarted = false;
        nextSpawned = false;
        this.emptyPipe = emptyPipe;
        renderDeactivated = 2;
        this.isTop = isTop;

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<SpriteRenderer>().color = currentColor;

        transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = currentColor;

        lightObj.intensity = 0f;
        colorTime = 0f;
    }

    public void SetLightIntensity(float i)
    {
        lightObj.intensity = i;
    }

    public void AddDestroyedPart(GameObject part)
    {
        destroyedParts.Add(part);
    }

    public void StartDestruction(float fadeTime, int id, bool destroyFull = false)
    {
        if (destructionStarted && !destroyFull) return;
        if (fullDestructionStarted) return;

        this.destroyFull = destroyFull;

        pipeExpId = id;

        this.fadeTime = fadeTime;
        destructionStarted = true;

        if(destroyFull)
        {
            fullDestructionStarted = true;
            beginDestruction = false;
        }

        if(!isTop)
        {
            frameWait = 1;
        } else
        {
            frameWait = 0;
        }

        if(!destroyFull)
        {
            Color c = GetComponent<SpriteRenderer>().color;

            GetComponent<SpriteRenderer>().color = Color.white;
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;

            GetComponent<SpriteRenderer>().DOColor(c, fadeTime);
            transform.GetChild(0).GetComponent<SpriteRenderer>().
                GetComponent<SpriteRenderer>().DOColor(c, fadeTime);
        }

        colorTime = fadeTime;
    }

    public void StartFlash(float lightTime = 1f)
    {
        this.lightTime = lightTime;
        lightObj.intensity = 1f;
    }

    private void Awake()
    {
        objectPooler = ObjectPooler.Instance;
    }

    private void CreateDestroyedParts(bool full = false)
    {
        if (beginDestruction) return;

        /*Algo auf 32Px Sprites ausgelegt
            -> divConst korrigiert Größe bei anderer Auflösung*/
        divConst = 1;//32f / thisPipe.sprite[0].pixelsPerUnit;

        if (pipeExpId == 1)
        { //wenn letzte pipe die zu blus assigned
            SoundManager.Instance.PlaySound(SoundManager.Sound.BlusExplosion);
            FlatterFogelHandler.Instance.StartCameraShake();
        }

        //sprite renderer von endstück deaktivieren
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        beginDestruction = true;

        Vector3 center = transform.parent.position; //mitte

        if (isTop)
        {
            center.y += abstand;

            if (OptionHandler.normalAspect)
            {
                yDiffToWall = 1434 - center.y;
            }
            else
            {
                yDiffToWall =
                    (OptionHandler.defaultCameraPos.y + OptionHandler.cameraBounds.extents.y) - center.y;
            }

        }
        else
        {
            center.y -= abstand;
            yDiffToWall = center.y - 225;
        }

        yDiffToWall += 15;

        int max = (int)Math.Ceiling((double)yDiffToWall / 18.75f);
        count = max;
        this.max = max;

        float simScale = GetComponent<SpriteRenderer>().size.y * 75;

        realMax = (int)Math.Ceiling((double)simScale / 18.75f);

        float endDiff = 63.225f;//93.65f;

        Vector3 endPos = transform.GetChild(0).position;
        Vector3 startPos = new Vector3(endPos.x - 35.18f, endPos.y + 28.2f, endPos.z);

        float xAdd = 23.38f, yAdd = -18.69f; //nach rechts | nach unten

        GameObject dPart = null;

        startPos.y -= 18.69f;

        if (isTop)
        {
            yAdd = 18.69f;
            startPos.y -= 37.38f;
        }
        else
        {
            startPos.y += 18.225f;
        }

        Sprite[,] endSprites = FF_PlayerData.Instance.pipeEndSprite;
        Color pipeColor = ShopHandler.Instance.pipeColor;

        SpriteRenderer tmpRenderer = null;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                dPart = objectPooler.SpawnFromPool("DestroyedPipePart",
                    new Vector3(startPos.x + (x * xAdd), startPos.y + (y * yAdd)),
                    Quaternion.identity, true, true);

                ResetDestroyedPart(dPart, true, true);

                tmpRenderer = dPart.GetComponent<SpriteRenderer>();

                if (!isTop)
                {
                    //if (x == 0 && y == 0)
                    //{
                    tmpRenderer.sprite =
                        endSprites[3 - y, x];
                    //}
                }
                else
                {
                    dPart.transform.Rotate(new Vector3(180, 0, 0));
                    tmpRenderer.sprite =
                        endSprites[3 - y, x];
                }

                tmpRenderer.drawMode = SpriteDrawMode.Sliced;

                tmpRenderer.size = new Vector2(4.1f * divConst, 5.12f * divConst);

                tmpRenderer.color = pipeColor;
                    
                if(full)
                {
                    dPart.GetComponent<Rigidbody2D>().isKinematic = false;
                } else
                {
                    dPart.GetComponent<Rigidbody2D>().isKinematic = true;
                }

                dPart.GetComponent<BoxCollider2D>().autoTiling = false;

                destroyedParts.Add(dPart);
            }
        }

        //Destruction objs von rest von pipe erstellen

        if (isTop)
        { //nach oben / unten verschieben da dort ende-objs benutzt werden
            transform.position =
                new Vector3(transform.position.x, transform.position.y + endDiff);
        }
        else
        {
            transform.position =
                new Vector3(transform.position.x, transform.position.y - endDiff);
        }

        count -= 4;

        if(!full)
        {
            if (count > 4) count = 4;
        }

        Sprite[,] pipeSprites = FF_PlayerData.Instance.pipeSprite;

        int yCount = 0;

        for (int a = count; a > 0; a--)
        {

            float newX = transform.position.x - 18.75f - (18.675f / 2);

            if (!isTop)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - 18.675f);

                for (int i = 0; i < 4; i++)
                {
                    dPart = objectPooler.SpawnFromPool("DestroyedPipePart",
                                new Vector3(newX + 18.675f * i, transform.position.y + ((realMax / 2) * 18.675f)),
                                Quaternion.identity, true, true);

                    ResetDestroyedPart(dPart, true);

                    tmpRenderer = dPart.GetComponent<SpriteRenderer>();

                    tmpRenderer.color = pipeColor;
                        
                    if (full)
                    {
                        dPart.GetComponent<Rigidbody2D>().isKinematic = false;
                    }
                    else
                    {
                        dPart.GetComponent<Rigidbody2D>().isKinematic = true;
                    }

                    tmpRenderer.sprite =
                        pipeSprites[3 - yCount, i];

                    tmpRenderer.drawMode =
                        SpriteDrawMode.Sliced;

                    tmpRenderer.size = new Vector2(5.15f * divConst, 5.15f * divConst);

                    dPart.GetComponent<BoxCollider2D>().autoTiling = false;

                    //dPart.GetComponent<Rigidbody2D>().AddForce(
                    //    new Vector2(Random.Range(-12500, 10000), Random.Range(-1000, 2500)));

                    //dPart.transform.SetParent(transform.parent);
                    destroyedParts.Add(dPart);
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 18.675f);

                for (int i = 0; i < 4; i++)
                {
                    dPart = objectPooler.SpawnFromPool("DestroyedPipePart",
                                new Vector3(newX + 18.675f * i, transform.position.y - ((realMax / 2) * 18.675f)),
                                Quaternion.identity, true, true);

                    ResetDestroyedPart(dPart, true);

                    tmpRenderer = dPart.GetComponent<SpriteRenderer>();

                    tmpRenderer.color = pipeColor;

                    if (full)
                    {
                        dPart.GetComponent<Rigidbody2D>().isKinematic = false;
                    }
                    else
                    {
                        dPart.GetComponent<Rigidbody2D>().isKinematic = true;
                    }

                    dPart.transform.Rotate(new Vector3(180, 0, 0));
                    tmpRenderer.sprite =
                        pipeSprites[3 - yCount, i];

                    tmpRenderer.drawMode =
                        SpriteDrawMode.Sliced;

                    tmpRenderer.size = new Vector2(5.15f * divConst, 5.15f * divConst);

                    dPart.GetComponent<BoxCollider2D>().autoTiling = false;

                    //dPart.GetComponent<Rigidbody2D>().AddForce(
                    //    new Vector2(Random.Range(-7500, 5000), Random.Range(-2500, 1000)));

                    //dPart.transform.SetParent(transform.parent);
                    destroyedParts.Add(dPart);
                }
            }

            colorTime = 0.03f;
            yCount++;
            if (yCount > 3)
            {
                yCount = 0;
            }
        }

        count = 0;
        destructionComplete = true;

        //pipes "verstecken"
        if(full)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        } else
        {
            GetComponent<BoxCollider2D>().enabled = true;
        }

        transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;

        frameWait = 1;

        Invoke("ScaleDown", 5f);
    }

    private void Update()
    {
        /*if(!scaleDone)
        {
            for (int i = 0; i < destroyedParts.Count; i++)
            {
                destroyedParts[i].transform.localScale = globalScale;
            }
        }*/

        if(isChecked && !emptyPipe)
        {
            if (lightObj.intensity > 0)
            {
                float newI = lightObj.intensity - (lightTime * Time.deltaTime);
                lightObj.intensity = Mathf.Clamp(newI, 0, 2);
            } else if(colorTime > 0)
            {
                colorTime -= 1 * Time.deltaTime;

                if(frameWait > 0)
                {
                    frameWait--;
                    return;
                }

                if(colorTime <= 0)
                {
                    CreateDestroyedParts(destroyFull);

                    if (frameWait > 0)
                    {
                        frameWait--;
                        return;
                    }

                    if(!physicsCalculated)
                    {
                        physicsCalculated = true;
                        int len = destroyedParts.Count;
                        for (int i = 0; i < len; i++)
                        {
                            destroyedParts[i].GetComponent<BoxCollider2D>().autoTiling = true;
                        }
                    }
                    /*count--;

                    if(count > 0)
                    {
                        
                    } else
                    {
                        if(!destructionComplete)
                        {

                        }
                    }*/
                }
            } //25 ist abstand zwischen destroy objs
        }
    }

    public void ScaleDown()
    {
        CancelInvoke("ResetAll");

        //scaleDone = false;
        globalScale = new Vector3(29.25f, 29.25f, 29.25f);
        //DOTween.To(() => globalScale, x => globalScale = x, Vector3.zero, 0.25f);

        for (int i = 0; i < destroyedParts.Count; i++)
        {
            destroyedParts[i].transform.DOScale(0, Random.Range(0.05f, 0.5f));
        }

        Invoke("ScaleDone", 0.51f);
        Invoke("ResetAll", 0.51f);
    }

    private void ScaleDone()
    {
        //scaleDone = true;
    }

    public void ResetAll()
    {
        for (int i = 0; i < destroyedParts.Count; i++)
        {
            ResetDestroyedPart(destroyedParts[i]);
        }
        destroyedParts.Clear();
    }

    public void ResetDestroyedPart(GameObject part, bool active = false, bool end = false)
    {
        part.SetActive(active);

        SpriteRenderer tmpR = part.GetComponent<SpriteRenderer>();

        tmpR.sprite = squareSprite;
        tmpR.color = Color.black;

        part.transform.localScale = new Vector3(29.25f, 29.25f, 29.25f);
        tmpR.drawMode = SpriteDrawMode.Sliced;
        tmpR.size = new Vector2(0.64f, 0.64f);

        part.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

        if(!end)
        {
            part.transform.localScale = new Vector3(3.65f, 3.65f, 29.25f);
            part.GetComponent<PipeDestructionPart>().isPipeEnd = false;
        } else
        {
            part.transform.localScale = new Vector3(5.75f, 4.38f, 29.25f);
            part.GetComponent<PipeDestructionPart>().isPipeEnd = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            collision.gameObject.SetActive(false);
        }
    }

    public void UpdateDestroyedParts()
    {
        if (destroyedParts.Count > 0 /*&& isChecked*/)
        {
            float scrollSpeed = FlatterFogelHandler.scrollSpeed;
            Vector3 pos;

            List<GameObject> delList = new List<GameObject>();
            for (int i = 0; i < destroyedParts.Count; i++)
            {
                pos = destroyedParts[i].transform.position;
                pos.x -= scrollSpeed * Time.deltaTime;

                destroyedParts[i].transform.position = pos;

                if(pos.x < -764 || pos.y < 131 || pos.x > 182)
                { //out of bounds
                    delList.Add(destroyedParts[i]);
                }
            }

            for(int i = 0; i < delList.Count; i++)
            {
                destroyedParts.Remove(delList[i]);
                ResetDestroyedPart(delList[i]);
            }
        }
    }

}