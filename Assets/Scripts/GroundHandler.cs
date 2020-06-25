using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using SRF;

public class GroundHandler : MonoBehaviour
{
    public GameObject grassPrefab, cornerPrefab, platformerInfo, player,
                landingEffectObj, groundCover, rightSteepPrefab;
    public List<GameObject> groundObjs = new List<GameObject>();
    public Sprite[] groundSprites = new Sprite[10];
    public FlatterFogelHandler ffHandler;
    public Sprite[] mineSprites;

    public GameObject minedGroundObj = null, rightSteepObj = null;
    public bool groundSpawnLocked = false;
    public static GroundHandler Instance;

    [SerializeField] private BoxCollider2D groundCollider = null;

    private ObjectPooler objectPooler;
    private int lastSpawnType = 0, mineResetGroundPos = 0;
    private List<GameObject> delList = new List<GameObject>();
    private bool sharpMiningEdge = false;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        objectPooler = ObjectPooler.Instance;

        GenerateStartGroundObjs();
    }

    public void GenerateStartGroundObjs()
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 newPos = new Vector3(-696 + (i * 90), 195f);
            GameObject nG =
                objectPooler.SpawnFromPool("DefaultGround", newPos, Quaternion.identity);

            nG.transform.SetParent(transform);
            nG.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(0, 2)];
            nG.GetComponent<PlatformData>().height = 0;
            nG.GetComponent<PolygonCollider2D>().enabled = false;
            nG.GetComponent<BoxCollider2D>().enabled = false;

            groundObjs.Add(nG);
        }

        groundCollider.enabled = true;
        groundSpawnLocked = false;
    }

    public void StartMiningGround()
    { //wandelt alles in einzelne kollisionsboxen um
        /*groundCollider.enabled = false;

        for(int i = 0; i < groundObjs.Count; i++)
        {
            groundObjs[i].GetComponent<BoxCollider2D>().enabled = true;
        }*/

        //obsolete weil bei movetocorrectpos gehandelt
    }

    public void StartGround()
    {
        groundCollider.enabled = false;

        Vector3 lastPos = groundObjs[groundObjs.Count - 1].transform.position;
        int lastHeight = groundObjs[groundObjs.Count - 1].GetComponent<PlatformData>().height;

        for (int i = 0; i < 5; i++)
        {
            Vector3 newPos =
                   new Vector3(lastPos.x + (90f * i), lastPos.y);

            GameObject nG2 =
                objectPooler.SpawnFromPool("DefaultGround", newPos, Quaternion.identity);
            nG2.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(0, 2)];

            nG2.GetComponent<BoxCollider2D>().enabled = false;
            nG2.GetComponent<PolygonCollider2D>().enabled = false;

            nG2.GetComponent<PlatformData>().height = lastHeight;
            nG2.transform.SetParent(transform);

            groundObjs.Add(nG2);
        }

        groundSpawnLocked = false;

        //letztes groundObj durch eckstück ersetzen
        GameObject nG = groundObjs[groundObjs.Count - 1];

        nG.SetActive(false);
        Vector3 oldPos = nG.transform.position;
        Transform oldParent = nG.transform.parent;

        nG = objectPooler.SpawnFromPool("RightCorner", oldPos, Quaternion.identity);
        nG.transform.SetParent(oldParent);

        nG.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(3, 5)];

        BoxCollider2D collider = nG.GetComponent<BoxCollider2D>();

        Vector2 offset = collider.offset;
        Vector2 size = collider.size;

        int length = groundObjs.Count - 1;

        float[] data = CalcRectData(length);

        offset.x = data[0];
        size.x = data[1];

        collider.offset = offset;
        collider.size = size;
        collider.enabled = true;

        groundObjs[groundObjs.Count - 1] = nG;
    }

    public void GroundUpdate(int gameState, float scrollSpeed)
    {
        if(groundObjs.Count < 2 || gameState == 2)
        {
            return;
        }

        delList.Clear();

        GameObject cGO;
        Vector3 pos;

        int len = groundObjs.Count;
        for (int i = 0; i < len; i++)
        {
            cGO = groundObjs[i];
            pos = cGO.transform.position;

            if(pos.x > -786)
            { //noch nicht an links grenze -> weiter bewegen
                pos.x -= scrollSpeed * Time.deltaTime;

                cGO.transform.position = pos;

                if(!cGO.activeSelf)
                {
                    if (pos.x < 480)
                    {
                        PlatformData pD = cGO.GetComponent<PlatformData>();

                        if(pD.type != 2)
                        {
                            if(pD.type == 3)
                            { //wenn spike früher aktivieren
                                cGO.SetActive(true);
                            }
                            else if (pos.x < 480) //24
                            {
                                cGO.SetActive(true);
                            }
                        }
                    }
                }
            } else
            { //an grenze angekommen -> disabled & neues obj spawnen
                cGO.SetActive(false);
                delList.Add(cGO);
            }
        }

        len = delList.Count;
        for(int i = 0; i < len; i++)
        {
            groundObjs.Remove(delList[i]);
        }

        if(groundObjs[groundObjs.Count - 1].transform.position.x < 280f)
        { //neues obj spawnen
            SpawnObj(gameState);
        }
    }

    public void EnableGroundCover(bool enable)
    {
        groundCover.SetActive(enable);
    }

    public void MoveGroundToCorrectPos()
    {
        sharpMiningEdge = false;

        Vector3 pos = new Vector3(1000, 105);

        for(int i = 0; i < groundObjs.Count; i++)
        { //kleinstes obj finden (xpos)
            Vector3 tempPos = groundObjs[i].transform.position;
            if(tempPos.x < pos.x)
            {
                pos = tempPos;
                break;
            }
        }

        float offset = -786 - pos.x;

        Vector3 playerPos = player.transform.position;
        float distance = 9999;

        int nearest = -1;

        groundCollider.enabled = false;

        for (int i = 0; i < groundObjs.Count; i++)
        { //alle groundobjs nach links bewegen so dass perfekt auf grid aligned +
            //nächstes finden (nach x coord)
            Vector3 newPos = groundObjs[i].transform.position;
            newPos.y = playerPos.y;
            newPos.x += offset;

            groundObjs[i].transform.DOMoveX
                (groundObjs[i].transform.position.x + offset, 0.2f);

            float newDist = Vector3.Distance(newPos, playerPos);

            if (newDist < distance)
            {
                distance = newDist;
                nearest = i;
            }

            groundObjs[i].GetComponent<BoxCollider2D>().enabled = true;
        }

        groundObjs[nearest].GetComponent<SpriteRenderer>().sprite = null;
        groundObjs[nearest].GetComponent<BoxCollider2D>().enabled = false;

        //hole alle pipe positionen mit angepasster y-koordinate weil nur x wichtig ist
        Vector3[] pipePositions = 
            ffHandler.GetAllPipePositions(groundObjs[nearest].transform.position.y);

        bool leftOk = false; //false = keine pipe in nähe (links)

        for(int i = 0; i < pipePositions.Length; i++)
        { 
            distance = Vector3.Distance(groundObjs[nearest - 1].transform.position, pipePositions[i]);

            if(distance < 90)
            {
                leftOk = true;
                break;
            }
        }

        leftOk = false; //override da keine pipes mehr

        if(!leftOk)
        { //links keine pipe in nähe
            groundObjs[nearest - 1].GetComponent<SpriteRenderer>().sprite = groundSprites[7];
            groundObjs[nearest - 1].GetComponent<PolygonCollider2D>().enabled = true;
            groundObjs[nearest - 1].GetComponent<BoxCollider2D>().enabled = false;
        }


        bool rightOk = false; //false = keine pipe in nähe (rechts)

        for(int i = 0; i < pipePositions.Length; i++)
        {
            distance = Vector3.Distance(groundObjs[nearest + 1].transform.position,
                pipePositions[i]);

            if(distance < 90)
            {
                rightOk = true;
                break;
            }
        }

        rightOk = false; //override da keine pipes mehr

        if(!rightOk)
        { //rechts keine pipe in nähe
            /*groundObjs[nearest + 1].GetComponent<SpriteRenderer>().sprite = groundSprites[7];
            groundObjs[nearest + 1].GetComponent<PolygonCollider2D>().enabled = true;
            groundObjs[nearest + 1].GetComponent<BoxCollider2D>().enabled = false;
            groundObjs[nearest + 1].transform.Rotate(0, 180, 0);*/

            Vector3 rPos = groundObjs[nearest + 1].transform.position;
            rPos.z = -16.5f;

            groundObjs[nearest + 1].SetActive(false);

            GameObject steep = Instantiate(rightSteepPrefab, rPos, Quaternion.identity, transform);
            groundObjs[nearest + 1] = steep;
            steep.SetActive(true);

            steep.transform.DOMoveX(rPos.x + offset, 0.2f);

            rightSteepObj = steep;
        }

        if(leftOk)
        {
            sharpMiningEdge = true;
            groundObjs[nearest].GetComponent<SpriteRenderer>().sprite = mineSprites[0];
        } else if(rightOk)
        {
            sharpMiningEdge = true;
            groundObjs[nearest].GetComponent<SpriteRenderer>().sprite = mineSprites[0];
            groundObjs[nearest].transform.Rotate(new Vector3(0, 180, 0));
        } else if(!leftOk && !rightOk)
        {
            groundObjs[nearest].GetComponent<SpriteRenderer>().sprite = mineSprites[1];
        }

        minedGroundObj = groundObjs[nearest];

        Vector3 gPos = groundObjs[nearest].transform.position;

        gPos.y -= 45;

        gPos.z = 0;

        landingEffectObj.transform.position = gPos;
        landingEffectObj.SetActive(false);
        landingEffectObj.SetActive(true);
    }

    public void UpdateMinedGroundObj()
    {
        if(minedGroundObj != null)
        {
            if(sharpMiningEdge) {
                minedGroundObj.GetComponent<SpriteRenderer>().sprite = mineSprites[2];
            } else
            {
                minedGroundObj.GetComponent<SpriteRenderer>().sprite = null;
            }
        }
    }

    public bool MineGround(GameObject player, Sprite mineSprite)
    {
        bool result = true;

        Vector3 pos = player.transform.position;
        float minDistance = 9999;

        int groundPos = -1;

        for(int i = 0; i < groundObjs.Count; i++) 
        {
            float newDist = Vector3.Distance(pos, groundObjs[i].transform.position);
            if(newDist < minDistance)
            {
                minDistance = newDist;
                groundPos = i;
            }
        }

        float time = 1f;

        //this.mineSprite = mineSprite;
        mineResetGroundPos = groundPos;
        minedGroundObj = groundObjs[groundPos];
        //MineHandler.Instance.MinePlayerTo(groundObjs[groundPos].transform.position, time, 2);
        Invoke("ResetMineGround", time);

        return result;
    }

    private void ResetMineGround()
    {
        //groundObjs[mineResetGroundPos].SetActive(false);

        player.GetComponent<PlayerMiner>().StartStopMine(0);

        groundObjs[mineResetGroundPos].GetComponent<BoxCollider2D>().enabled = false;
        //groundObjs[mineResetGroundPos].GetComponent<SpriteRenderer>().sprite = mineSprite;
    }

    public void ResetMine()
    {

        for (int i = 0; i < groundObjs.Count; i++)
        {
            groundObjs[i].GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(0, 2)];
            groundObjs[i].GetComponent<BoxCollider2D>().enabled = false;
            groundObjs[i].GetComponent<PolygonCollider2D>().enabled = false;
            groundObjs[i].SetActive(false);
        }

        landingEffectObj.SetActive(false);

        //GenerateStartGroundObjs();
    }

    public void DissolveGround(int gameState)
    {
        if(gameState == 1 ||
            gameState == 2)
        {
            GetComponent<Dissolver>().StartDissolve(Color.white, 0.6f);
        }
    }

    public void ResetDissolve(int gameState)
    {
        GetComponent<Dissolver>().ResetDissolve();

        if(gameState == 1 || gameState == 2)
        {
            GenerateStartGroundObjs();
            groundCollider.enabled = true;
            //platformerInfo.SetActive(false);
        }
    }

    public void DestroyGround(int gameState = 0)
    {
        if(gameState == 0 || gameState == 3)
        { //ground nur löschen wenn nicht im pipe- oder bossmodus(0 / 3)
            return;
        }

        for(int i = 0; i < groundObjs.Count; i++)
        {
            groundObjs[i].SetActive(false);
        }

        groundObjs.Clear();

        if(rightSteepObj != null)
        {
            Destroy(rightSteepObj);
            rightSteepObj = null;
        }
    }

    public Vector3 GetLastObjPos()
    {
        return groundObjs[groundObjs.Count - 1].transform.position;
    }

    private float[] CalcRectData(int length, float width = 0.2083333f, float abs = 0.0394238f)
    { //berechnet boxcollider maße für x groundobjects inkl abstand
        float[] data = new float[2];

        float nWidth = length * (width + abs); //länge von l blöcken
        nWidth -= abs * 2; //abzug des leeren platzes links und rechts

        float nOffset = -(nWidth / 2) + (abs * 2);

        data[0] = nOffset;
        data[1] = nWidth;

        return data;
    }

    private void SpawnObj(int gameState)
    {
        int type = 0;

        if(gameState == 1)
        {
            type = 1;
        }

        Vector3 lastPos = groundObjs[groundObjs.Count - 1].transform.position;
        int lastHeight = groundObjs[groundObjs.Count - 1].GetComponent<PlatformData>().height;

        int maxUp = 8 - lastHeight; //da 11 max höhe

        int maxDown = lastHeight;
        
        if(lastHeight - maxDown < 0)
        {
            maxDown = 0;
        }

        if(groundSpawnLocked)
        {
            return;
        }

        switch (type)
        {
            case 0:
                Vector3 newPos =
                    new Vector3(lastPos.x + 90f, 195f);

                GameObject nG = 
                    objectPooler.SpawnFromPool("DefaultGround", newPos, Quaternion.identity);
                nG.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(0, 2)];

                nG.GetComponent<BoxCollider2D>().enabled = false;
                nG.GetComponent<PolygonCollider2D>().enabled = false;

                nG.GetComponent<PlatformData>().height = lastHeight;
                nG.transform.SetParent(transform);

                groundObjs.Add(nG);

                break;
            case 1:

                Vector3 oldLastPos = lastPos;
                int space = Random.Range(2, 4); //abstand zu vorherigem
                lastPos.x += 90 * space;

                int length = Random.Range(3, 10);

                int heightDiff = 0;
                int dir = 0;

                float lastSpikeX = -9999;

                if (dir == 0) //todo
                { //andere höhe (+/-2 max)
                    if(Random.Range(0, 10) > 4)
                    {
                        if(maxUp > 0)
                        {
                            dir = 1;
                        }
                    }

                    if(dir == 0 && maxDown == 0)
                    { //richtung umkehren wenn nach unten nicht möglich
                        dir = 1;
                    }

                    if(Random.Range(0, 10) > 4 && 
                        ((dir == 0 && maxDown >= 2) || (dir == 1 && maxUp >= 2)))
                    {
                        if (space <= 2)
                        {
                            heightDiff = 2;
                        } else
                        {
                            heightDiff = 1;
                        }
                    } else
                    {
                        heightDiff = 1;
                    }
                }

                //Debug.Log(maxUp + " " + maxDown + " " + dir + " " + heightDiff);

                if(dir == 0)
                { //runter
                    lastHeight -= heightDiff;
                    lastPos.y -= (90 * heightDiff);

                    if(lastPos.y < 195f)
                    {
                        lastPos.y = 195f;
                        lastHeight = 0;
                    }
                } else
                { //hoch
                    lastHeight += heightDiff;
                    lastPos.y += (90 * heightDiff);
                }

                bool modeChange = ffHandler.SpawnBlus(new Vector3(
                    (Mathf.Abs(lastPos.x - oldLastPos.x) / 2) + oldLastPos.x,
                    oldLastPos.y + (((lastPos.y - oldLastPos.y) / 2) + 270)));

                if(modeChange)
                {
                    groundSpawnLocked = true;

                    return;
                }

                for (int i = 0; i < length; i++)
                {
                    newPos =
                        new Vector3(lastPos.x + (90f * (i + 1)), 
                            lastPos.y);

                    /*if(dir == 0)
                    { //runter
                        newPos.y -= (90 * heightDiff);
                    } else
                    { //hoch
                        newPos.y += (90 * heightDiff);
                    }*/

                    nG = null;

                    bool spike = false;

                    if (i == 0 || i == length - 1)
                    {
                        if (i == length - 1)
                        {
                            nG = objectPooler.SpawnFromPool("RightCorner", newPos, Quaternion.identity);
                        } else
                        {
                            nG = objectPooler.SpawnFromPool("LeftCorner", newPos, Quaternion.identity, false);
                        }
                    }
                    else
                    {
                        if (length >= 5)
                        {
                            if (i > 2 && i < length - 2 && 
                                (Mathf.Abs(newPos.x - lastSpikeX) > 360))
                            { //spike-abstand zu anfang, ende und letzer spike damit man noch drauf landen kann
                                if(Random.Range(0, 4) == 0)
                                { //25%
                                    nG = objectPooler.SpawnFromPool("DefaultSpike", newPos, Quaternion.identity);
                                    spike = true;
                                    lastSpikeX = newPos.x;
                                    nG.GetComponent<PlatformData>().type = 3;
                                }
                            }
                        }

                        if(!spike)
                        {
                            nG = objectPooler.SpawnFromPool("DefaultGround", newPos, Quaternion.identity);
                        }
                    }

                    if (!spike)
                    {
                        nG.GetComponent<BoxCollider2D>().enabled = false;
                        //collider nur beim letzten eckstück aktiv
                    }

                    bool rightCorner = false;

                    if (i == 0)
                    { //links kante
                        nG.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(3, 5)];
                        nG.GetComponent<PlatformData>().type = 1;
                    }
                    else if (i == length - 1)
                    { //rechts kante
                        nG.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(3, 5)];
                        nG.GetComponent<PlatformData>().type = 2;

                        BoxCollider2D collider = nG.GetComponent<BoxCollider2D>();

                        Vector2 offset = collider.offset;
                        Vector2 size = collider.size;

                        float[] data = CalcRectData(length);

                        offset.x = data[0];
                        size.x = data[1];

                        collider.offset = offset;
                        collider.size = size;
                        collider.enabled = true;

                        rightCorner = true;
                    }
                    else
                    {
                        if (!spike)
                        {
                            nG.GetComponent<SpriteRenderer>().sprite = groundSprites[Random.Range(0, 2)];
                        } else
                        {
                            nG.GetComponent<SpriteRenderer>().sprite = groundSprites[6];
                        }
                    }

                    nG.GetComponent<PlatformData>().height = lastHeight;
                    nG.transform.SetParent(transform);

                    if(!rightCorner)
                    {
                        nG.SetActive(false);
                    }

                    groundObjs.Add(nG);
                }

                /*platformerInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = space.ToString();
                if (!platformerInfo.activeSelf)
                {
                    platformerInfo.SetActive(true);
                    platformerInfo.transform.position = new Vector2(-63, 195 + (90 * lastHeight));
                } else
                {
                    platformerInfo.transform.DOMoveY(195 + (90 * lastHeight), 0.1f);
                }*/

                break;
        }

        lastSpawnType = type;
    }
}
