using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Localization;
using DG.Tweening;
using TMPro;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum MineralType
{
    Fliesium = 0,
    Lilium = 1,
    Altium = 5,
    Modium = 7,
    Admium = 8,
    Legium = 9,
    Blussizin_L = 10,
    Blussizin_M = 11,
    Blussizin_S = 12,
    Fliesizin = 13,
    Coin = 14,
    Lava = 15,
    AchCoin = 16,
}

public class MineHandler : MonoBehaviour
{
    public GameObject player, mineStonePrefab, mainCamera, controlCanvas,
        timeUI, lineObj, minerCollider, movement, fliesizinEffect, mineBombEffect,
        mineBackground;

    public MinerCollisionHandler[] colliders = new MinerCollisionHandler[3];
    public Sprite[] minedSprites = new Sprite[3];

    public GameObject propoll;

    public Sprite boneSprite;
    public SwipeDetector swipeDetector;

    public FlatterFogelHandler ffHandler;
    public BackgroundHandler bgHandler;
    ObjectPooler objPooler;
    int nextMinePos = 0;

    public static bool cameraOK = false;

    public bool minedSpriteCheck = false;
    public bool miningActive = false;
    public bool isMoving = false, isShaking = false, lastCamPosAssigned = false;

    [SerializeField]
    private LocalizedString[] mineralName = null;
    [SerializeField]
    private string[] mineralString = null;

    private bool backgroundSet = false;
    private GameObject[] mineObjs = new GameObject[4096]; //aktuelle mineobjs die sichtbar sind
    private Tween playerMoveTween = null;

    private Vector3 lastCamPos;

    public HeatShield currentHeatShield = null;

    int mineObjsCounter = 0, migrations = 0;

    public class BoneHolder
    {
        public ulong score = 0;
        public string name = "";
        public int xPos = 0;
        public bool shown = false;
    }

    private List<BoneHolder> boneData = new List<BoneHolder>();

    public Item[] rawMinerals = new Item[2];
    public Item[] meltedMinerals = new Item[2];
    public Sprite[] mineSprites = new Sprite[2];
    public Sprite[] otherMineSprites;
    public static MineHandler Instance;

    public Vector3 leftPos, leftGrossPos, rightPos, rightGrossPos;

    public static Rect leftKreuz, upKreuz, rightKreuz, downKreuz;

    // Start is called before the first frame update
    void Awake()
    {
        objPooler = ObjectPooler.Instance;

        Instance = this;

        for(int i = 0; i < mineObjs.Length; i++)
        {
            mineObjs[i] = null;
        }
    }

    public void StartLoadLocalozation()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        for(int i = 0; i < mineralName.Length; i++)
        {
            yield return handle = mineralName[i].GetLocalizedString();

            rawMinerals[i].itemName = (string)handle.Result;
        }
    }

    public void StartMining()
    {
        OptionHandler.Instance.MiningZoomIn();

        mineBackground.SetActive(true);
        mineBackground.transform.position = new Vector3(-381, -774, 50);
        mineBackground.transform.SetParent(transform);
        backgroundSet = false;

        migrations = 0;

        boneData.Clear();
        for (int i = 0; i < ScoreHandler.Instance.scoreData.Count; i++)
        {
            BoneHolder bN = new BoneHolder
            {
                score = ScoreHandler.Instance.scoreData[i].score,
                name = ScoreHandler.Instance.scoreData[i].username,
                xPos = Random.Range(0, 8)
            };
            boneData.Add(bN);
        }

        GenerateStartMines();
        miningActive = true;
        //controlCanvas.SetActive(true);

        //movement.SetActive(true);

        bool swDetector;

        switch(OptionHandler.mineMode)
        {
            case 1: //gesten
                //okk = false;
                swDetector = true;
                break;
            case 2: //kreuz
                //okk = true;
                swDetector = false;
                break;
            default: //halten
                //okk = false;
                swDetector = false;
                break;
        }

        movement.SetActive(false);

        swipeDetector.enabled = swDetector;

        minerCollider.SetActive(true);

        //timeUI.SetActive(true);
        //timeUI.GetComponent<TextMeshProUGUI>().text = time.ToString();

        player.GetComponent<FF_PlayerData>().EnableDisableWings(false);
        player.GetComponent<PlayerMiner>().InitializeMiner(); //setzt ausgewählte miner-textur & co.

        InvokeRepeating(nameof(CheckBounds), 0f, 0.25f);
    }

    public void UpdateMovementSizePos()
    {
        Vector3 pos;

        if(OptionHandler.kreuzPos == 0)
        { //kreuz ist links
            if(OptionHandler.kreuzSize == 0)
            {
                pos = leftPos;
            } else
            {
                pos = leftGrossPos;
            }
        } else
        { //kreuz ist rechts
            if (OptionHandler.kreuzSize == 0)
            {
                pos = leftPos;
                pos.x += 450;
            }
            else
            {
                pos = leftGrossPos;
                pos.x += 300;
            }
        }

        if(OptionHandler.kreuzSize == 0)
        {
            movement.transform.localScale = new Vector3(1f, 1f, 1f);
        } else
        {
            movement.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        }

        pos.z = 300;
        movement.transform.position = pos;

        leftKreuz = new Rect(pos.x - 150, pos.y - 50, pos.x - 50, pos.y + 50);
        upKreuz = new Rect(pos.x - 50, pos.y + 50, pos.x + 50, pos.y + 150);
        rightKreuz = new Rect(pos.x + 50, pos.y - 50, pos.x + 150, pos.y + 50);
        downKreuz = new Rect(pos.x - 50, pos.y - 150, pos.x + 50, pos.y - 50);
    }

    public void GenerateStartMines()
    { //breite ist 9 * 15 Felder 
      // wird aufgerufen bei launch von mine-modus

        GroundHandler.Instance.MoveGroundToCorrectPos();

        Vector3 startPos = new Vector3(-696, 105); //links "oben"

        Vector3 newPos = new Vector3();

        for (int y = 0; y < 9; y++)
        { //erstellt objs von tiefe 0 bis limit unten
            for (int x = 0; x < 8; x++)
            {
                newPos.x = startPos.x + 90 * x;
                newPos.y = startPos.y - 90 * y;

                CreateNewMineObj(newPos, mineObjsCounter, x, y);
            }
        }
    }

    private void CreateNewMineObj(Vector3 newPos, int pos, int xPos, int blockDepth)
    {
        GameObject newMineObj = 
            Instantiate(mineStonePrefab, newPos, Quaternion.identity, transform);

        newMineObj.GetComponent<MineData>().ResetMineData();

        ulong currentScore = ffHandler.GetScore();
        int depth = player.GetComponent<FF_PlayerData>().GetPlayerDepth();

        newMineObj.transform.SetParent(transform);

        int id = 0;

        //bool mineral = false;
        //anhand von score wahrscheinlichkeit ob mineral berechnen
        // höher = mehr

        Sprite s = mineSprites[Random.Range(0, 2)];

        #region oldMinerals

        /*
        if (currentScore <= 10)
        {
            if(Random.Range(0, 17) == 0)
            {
                mineral = true;
            }
        } else if(currentScore <= 25)
        {
            if(Random.Range(0, 15) == 0)
            {
                mineral = true;
            }
        } else if(currentScore <= 40)
        {
            if(Random.Range(0, 13) == 0)
            {
                mineral = true;
            }
        } else if(currentScore <= 75)
        {
            if(Random.Range(0, 10) == 0)
            {
                mineral = true;
            }
        } else if(currentScore <= 150)
        {
            if(Random.Range(0, 8) == 0)
            {
                mineral = true;
            }
        } else if(currentScore >= 200)
        {
            if(Random.Range(0, 6) == 0)
            {
                mineral = true;
            }
        }

        if (mineral)
        { //anhand von tiefe wahrscheinlichkeit für mineral berechnen
            //max tiefe ist 512

            if(depth <= 10)
            {
                id = 1;
            } else if(depth <= 20)
            {
                id = Random.Range(1, 3);
            } else if(depth <= 30)
            {
                id = Random.Range(1, 4);
            } else if(depth <= 50)
            {
                id = Random.Range(1, 5);
            } else if(depth <= 100)
            {
                id = Random.Range(1, 6);
            } else if(depth <= 150)
            {
                id = Random.Range(2, 7);
            } else if(depth <= 200)
            {
                id = Random.Range(4, 8);
            } else if(depth <= 325)
            {
                id = Random.Range(5, 9);
            } else if(depth <= 400)
            {
                id = Random.Range(5, 10);
            }
        }*/

        #endregion

        for (int i = 0; i < boneData.Count; i++)
        {
            if (boneData[i].xPos == xPos &&
                boneData[i].score == (ulong)blockDepth &&
                Random.Range(0, 2) == 0)
            {
                //scale auf 140.65 + box coll ändern
                newMineObj.transform.localScale = 
                    new Vector3(140.65f, 140.65f, 140.65f);

                newMineObj.GetComponent<BoxCollider2D>().size = 
                    new Vector2(0.64f, 0.64f);

                newMineObj.GetComponent<MineData>().deathName =
                    boneData[i].name;

                id = -2;
                s = boneSprite;
                break;
            }
        }

        bool bigstone = false;

        if(id != -2)
        {
            bool bC = false;

            if (Random.Range(0, 25) == 0)
            { //1er blussizin
                id = 13;
                bC = true;
            }
            
            if(!bC)
            { //3er blussizin
                if (Random.Range(0, 40) == 0)
                {
                    id = 12;
                    bC = true;
                }
            }
            
            if(!bC)
            { //5er blussizin
                if (Random.Range(0, 65) == 0)
                {
                    id = 11;
                    bC = true;
                }
            }

            /*if(!bC)
            { //fliesizin
                if (Random.Range(0, 35) == 0)
                {
                    id = 14;
                    bC = true;
                }
            }*/

            if(!bC)
            {
                if (Random.Range(0, 14) == 0)
                {
                    bigstone = true;
                    bC = true;
                }
            }

            if(!bC)
            { //mineral generation
                int mineralID = 0;

                if(currentScore < 75)
                {
                    if(Random.Range(0, 4) > 0)
                    {
                        mineralID = 0;
                    } else
                    {
                        mineralID = 1;
                    }

                } else if(currentScore < 150)
                {
                    if (Random.Range(0, 4) > 0)
                    {
                        mineralID = 1;
                    }
                    else
                    {
                        mineralID = 5;
                    }
                } else if(currentScore < 300)
                {
                    if (Random.Range(0, 4) > 0)
                    {
                        mineralID = 5;
                    }
                    else
                    {
                        mineralID = 7;
                    }
                } else if(currentScore < 450)
                {
                    if (Random.Range(0, 4) > 0)
                    {
                        mineralID = 7;
                    }
                    else
                    {
                        mineralID = 8;
                    }
                } else if(currentScore < 666)
                {
                    if (Random.Range(0, 4) > 0)
                    {
                        mineralID = 8;
                    }
                    else
                    {
                        mineralID = 9;
                    }
                } else if(currentScore > 666)
                {
                    if (Random.Range(0, 4) > 0)
                    {
                        mineralID = 9;
                    }
                    else
                    {
                        mineralID = 8;
                    }
                }

                bool ok = true;

                /*if(currentScore > 100)
                {
                    if(Random.Range(0, 120) == 111)
                    {
                        id = 15;
                        ok = false;
                    }
                }*/

                if(ok)
                {
                    if (Random.Range(0, 35) == 0)
                    { //mineral zuweisung
                        if(Random.Range(0, 5) < 4)
                        { //normales mineral
                            id = mineralID + 1;
                        }
                        else
                        { //mineral für item
                            mineralID = Random.Range(2, 5); //2, 3, 4
                            id = mineralID + 1;
                        }
                    }
                }
            }
        }

        id -= 1;

        if(id >= 0 && blockDepth > 0)
        { //mineral zuweisen bei tiefe größer 0
            s = rawMinerals[id].sprite;
            newMineObj.GetComponent<MineData>().mineral = rawMinerals[id];
        }

        if(id == 10 || id == 11 || id == 12 || id == 13)
        {
            newMineObj.GetComponent<MineData>().StartBlussizinLight(id - 10);
        }

        if(bigstone && blockDepth > 0)
        {
            if(Random.Range(0, 3) > 0)
            {
                s = mineSprites[1]; //otherMineSprites[0];

                newMineObj.GetComponent<MineData>().unmineable = true;
                newMineObj.GetComponent<MineData>().bigstone = true;

                newMineObj.transform.GetChild(1).gameObject.SetActive(true);
            } else
            {
                newMineObj.GetComponent<MineData>().StartLava();
                newMineObj.GetComponent<MineData>().mineral = rawMinerals[(int)MineralType.Lava];
                s = rawMinerals[(int)MineralType.Lava].sprite;
            }
        }

        newMineObj.GetComponent<SpriteRenderer>().sprite = s;
        newMineObj.GetComponent<MineData>().arrayPos = pos;
        newMineObj.GetComponent<MineData>().xPos = xPos;
        newMineObj.GetComponent<MineData>().depth = blockDepth;

        newMineObj.transform.Rotate(new Vector3(0, 0, 90 * Random.Range(0, 3)));

        mineObjs[pos] = newMineObj;
        mineObjsCounter++;
    }

    public void ResetMines()
    {
        for(int i = 0; i < mineObjsCounter; i++)
        {
            if(mineObjs[i] != null)
            {
                Destroy(mineObjs[i]);
            }
        }
        mineObjsCounter = 0;
    }

    public void RegenerateMines(bool down = true)
    { //breite ist 9 * 15 Felder 
      // wird aufgerufen wenn spieler sich nach unten gräbt

        Vector3 startPos = new Vector3(-696, 105); //links "oben"
        int newDepth = player.GetComponent<FF_PlayerData>().GetPlayerDepth();
        //8 nach unten 7 nach oben

        Vector3 newPos = new Vector3();
        for(int i = 0; i < 8; i++)
        { //neue objs unten erstellen

            int realY = newDepth + 7;

            int arrayPos = realY * 8 + i;

            int arrayTopPos = arrayPos - (16 * 8);

            if(down)
            {
                if (arrayPos < mineObjsCounter)
                { //bereits hier gewesen -> nur erneut aktivieren
                    if(!mineObjs[arrayPos].GetComponent<MineData>().mined)
                    {
                        //mineObjs[arrayPos].SetActive(true);
                    }
                }
                else
                {

                    newPos.x = startPos.x + 90 * i;
                    newPos.y = startPos.y - 90 * realY;

                    CreateNewMineObj(newPos, arrayPos, i, realY);
                }
            } else
            { //up
                arrayTopPos += 8;
                if (arrayTopPos >= 0)
                { // bereits hier gewesen -> nur erneut aktivieren
                    if (!mineObjs[arrayTopPos].GetComponent<MineData>().mined)
                    {
                        //mineObjs[arrayTopPos].SetActive(true);
                    }
                }

                arrayPos += 8;
                if (arrayPos < mineObjsCounter)
                { 
                    //mineObjs[arrayPos].SetActive(false);
                }
            }
        }

    }

    private void CheckBounds()
    {
        Vector3 playerPos = player.transform.position;
        float borderTop = playerPos.y + 900;
        float borderBottom = playerPos.y - 1300;

        //int yPos = ((105 - (int)playerPos.y) / 90); //+ 2;

        float y = 0;

        if (!minedSpriteCheck)
        {
            for (int i = 0; i < mineObjsCounter; i++)
            {
                y = mineObjs[i].transform.position.y;

                if (y > borderTop || y < borderBottom)
                {
                    if (mineObjs[i].activeSelf)
                    {
                        mineObjs[i].SetActive(false); //debug
                    }
                }
                else
                {
                    if (!mineObjs[i].activeSelf)
                    {
                        //if(!mineObjs[i].GetComponent<MineData>().mined)
                        //{
                        mineObjs[i].SetActive(true);
                        //}
                    }
                }
            }
        }

        if(minedSpriteCheck)
        {
            minedSpriteCheck = false;

            for(int i = 0; i < mineObjsCounter; i++)
            {
                if(mineObjs[i].activeSelf)
                {
                    if(mineObjs[i].GetComponent<MineData>().mined)
                    {
                        bool left = false, right = false, up = false, down = false;

                        int yPos = i / 8;
                        int xPos = i - (yPos * 8);

                        if (xPos - 1 >= 0)
                        {
                            if (mineObjs[i - 1].GetComponent<MineData>().mined)
                            {
                                left = true;
                            }
                        }

                        if (xPos + 1 <= 7 && i + 1 < mineObjsCounter)
                        {
                            if (mineObjs[i + 1].GetComponent<MineData>().mined)
                            {
                                right = true;
                            }
                        }

                        if (yPos - 1 >= 0)
                        {
                            if (mineObjs[i - 8].GetComponent<MineData>().mined)
                            {
                                up = true;
                            }
                        } else 
                        {
                            GameObject minedGroundObj = GroundHandler.Instance.minedGroundObj;

                            if(minedGroundObj != null)
                            {

                                float dist = Vector3.Distance(mineObjs[i].transform.position, minedGroundObj.transform.position);
                                if (dist < 100)
                                {
                                    up = true;
                                }
                            }
                        }

                        if (yPos + 1 < 512 && (i + 8 < mineObjsCounter))
                        {
                            if (mineObjs[i + 8].GetComponent<MineData>().mined)
                            {
                                down = true;
                            }
                        }

                        bool ok = false;

                        mineObjs[i].transform.rotation = Quaternion.identity;

                        if (!left && !right && up && down)
                        { //tunnel von oben nach unten oder umgedreht
                            ok = true;
                            mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[1];
                        }

                        if (!ok)
                        {
                            if (left && right && !up && !down)
                            { //tunnel von rechts nach links oder umgedreht
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[1];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 90);
                            }
                        }

                        if(!ok)
                        {
                            if(!left && !right && up && !down)
                            { //endstück nach unten
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[5];
                            }
                        }

                        if(!ok)
                        {
                            if(!left && !right && !up && down)
                            { //endstück nach unten
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[5];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 180);
                            }
                        }

                        if(!ok)
                        {
                            if(left && !right && !up && !down)
                            { //endstück nach rechts
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[5];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 90);
                            }
                        }

                        if(!ok)
                        {
                            if(!left && right && !up && !down)
                            { //endstück nach links
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[5];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, -90);
                            }
                        }

                        if (!ok)
                        {
                            if(up && right && !left && !down)
                            { //kante von oben nach rechts
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[2];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, -90);
                            }
                        }

                        if(!ok)
                        {
                            if(up && left && !right && !down)
                            { //kante von oben nach links
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[2];
                            }
                        }

                        if(!ok)
                        {
                            if(!up && right && !left && down)
                            { //kante von unten nach rechts
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[2];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 180);
                            }
                        }

                        if(!ok)
                        {
                            if(!up && left && !right && down)
                            { //kante von unten nach links
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[2];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 90);
                            }
                        }

                        if(!ok)
                        {
                            if(left && right && up && !down)
                            { //umgedrehtes T-Stück
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[3];
                            }
                        }

                        if(!ok)
                        {
                            if(left && right && !up && down)
                            { //T-Stück
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[3];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 180);
                            }
                        }

                        if(!ok)
                        {
                            if(!left && up && down && right)
                            { //T-Stück nach rechts
                                ok = true;

                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[3];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, -90);
                            }
                        }

                        if(!ok)
                        {
                            if(left && up && down && !right)
                            { //T-Stück nach links
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[3];
                                mineObjs[i].transform.rotation = Quaternion.Euler(0, 0, 90);
                            }
                        }

                        if(!ok)
                        {
                            if(!left && !right && !up && !down)
                            {
                                ok = true;
                                mineObjs[i].GetComponent<SpriteRenderer>().sprite = minedSprites[4];
                            }
                        }

                        if (!ok)
                        {
                            mineObjs[i].GetComponent<SpriteRenderer>().sprite = null;
                        }
                    }
                }
            }
        }
    }

    private void StartBackgroundMine()
    {
        /*if(player.GetComponent<FF_PlayerData>().playerDepth == 1)
        {
            lineObj.SetActive(true);
        }*/
        bgHandler.StartMining();
    }

    public void ActivateLineObj()
    {
        lineObj.SetActive(true);
    }

    public bool HandleDir(int dir)
    {
        if(isMoving)
        {
            return false;
        }

        bool result = false;

        float mineTime = PlayerMiner.currentMiner.mineTime / 1000f;

        Vector3 playerPos = player.transform.position;

        if (dir == 0)
        { //oben
            /*int yPos = ((105 - (int)playerPos.y) / 90) + 2;
            player.GetComponent<FF_PlayerData>().playerDepth = yPos;
            RegenerateMines();*/

        } else if (dir == 1 || dir == 3)
        { //rechts / links

            /*int cDir = 1;

            if(dir == 3)
            {
                cDir = 0;
            }*/

            GameObject coll = null;//colliders[cDir].GetComponent<MinerCollisionHandler>().Mine();

            float newXpos = player.transform.position.x + 696;
            int arrayXPos = (int)Math.Round(newXpos / 90f);
            int arrayYPos = player.GetComponent<FF_PlayerData>().GetPlayerDepth() * 8;
            int realPos = (arrayXPos + arrayYPos) - 16;

            if(dir == 1)
            {
                if(arrayXPos + 1 < 8)
                { //nicht über grenze
                    realPos += 1;
                }
            } else if(dir == 3)
            {
                if(arrayXPos - 1 >= 0)
                {
                    realPos -= 1;
                }
            }

            if (realPos >= 0 &&
                realPos < mineObjsCounter)
            {
                coll = mineObjs[realPos];
            }

            bool ok = true;

            if (coll != null)
            {
                if(coll.GetComponent<MineData>().IsOkayToMine())
                {
                    float dist = Vector3.Distance(coll.transform.position, playerPos);

                    if ((coll.transform.position.x > playerPos.x && dir == 1) ||
                        (coll.transform.position.x < playerPos.x && dir == 3))
                    {
                        if (dist < 95)
                        {
                            nextMinePos = realPos;

                            MinePlayerTo(mineObjs[realPos].transform.position, 
                                mineObjs[realPos].GetComponent<MineData>().mineral, mineTime, dir);
                            result = true;
                        }
                        else
                        {
                            ok = false;
                        }
                    }
                    else
                    {
                        ok = false;
                    }
                } else
                {
                    ok = false;
                }
            }
            else
            {
                ok = false;
            }

            if(!ok) 
            { //block bereits gemined
                float xVel = 400;

                if(dir == 3)
                {
                    xVel = -400;
                }

                Vector2 vel = player.GetComponent<Rigidbody2D>().velocity;

                vel.x = xVel;

                player.GetComponent<Rigidbody2D>().velocity = vel;
            }
        } else if (dir == 2)
        { //unten
            int playerDepth = player.GetComponent<FF_PlayerData>().GetPlayerDepth();

            bool ok = false;

            if (playerDepth == 0)
            { //übergabe an ground obj da top layer dort gehandelt wird (obsolete)
                result = GroundHandler.Instance.MineGround(player, minedSprites[0]);

                player.GetComponent<FF_PlayerData>().SetPlayerDepth(1, true);

                //Invoke("StartBackgroundMine", 1f);
                ok = true;
            }
            else
            {
                GameObject coll = null; //colliders[dir].GetComponent<MinerCollisionHandler>().Mine();

                float newXpos = player.transform.position.x + 696;
                int arrayXPos = (int)Math.Round(newXpos / 90f);
                int arrayYPos = playerDepth * 8;
                int realPos = (arrayXPos + arrayYPos) - 8;

                if(realPos < mineObjsCounter)
                {
                    coll = mineObjs[realPos];
                }

                if (coll != null)
                {
                    if(coll.GetComponent<MineData>().IsOkayToMine())
                    {
                        float dist = Vector3.Distance(coll.transform.position, playerPos);

                        if (coll.transform.position.y < playerPos.y && dist <= 95)
                        {

                            nextMinePos = realPos;

                            MinePlayerTo(mineObjs[realPos].transform.position, 
                                mineObjs[realPos].GetComponent<MineData>().mineral, mineTime, dir);

                            player.GetComponent<FF_PlayerData>().SetPlayerDepth(1, true);

                            FlatterFogelHandler.Instance.SetScore(1, 1);

                            if (player.GetComponent<FF_PlayerData>().GetPlayerDepth() == 2)
                            {
                                GroundHandler.Instance.UpdateMinedGroundObj();
                            }

                            result = true;
                            ok = true;
                        }
                    }
                }
            }

            if(ok)
            { //mine beginn erfolgreich
                RegenerateMines(true);
            }
        }

        return result;
    }

    private IEnumerator<float> WaitMineObj(int nextMinePos, float waitTime, bool playerMine = true, bool update = true)
    {
        yield return Timing.WaitForSeconds(waitTime);
        MineObj(nextMinePos, playerMine, update);
    }

    private void MineObj(int nextMinePos, bool playerMine = true, bool update = true, bool force = false)
    {
        mineObjs[nextMinePos].GetComponent<MineData>().mined = true;

        MineData mD = mineObjs[nextMinePos].GetComponent<MineData>();
        Item mineral = mD.mineral;

        if (playerMine)
        {
            player.GetComponent<PlayerMiner>().StartStopMine(0);

            if (mD.deathName.Length > 0)
            {
                GameObject infoText =
                    objPooler.SpawnFromPool("InfoText", mineObjs[nextMinePos].transform.position, Quaternion.identity);

                infoText.GetComponent<InfoText>().StartFlashing("RIP " + mD.deathName);
            }

            if (mineral != null)
            {
                string name = mineral.itemName;

                GameObject infoText =
                    objPooler.SpawnFromPool("InfoText", mineObjs[nextMinePos].transform.position, Quaternion.identity);

                /*bool ok = player.GetComponent<Inventory>().TryAddCargo(mineral);

                if(!ok)
                { //cargo bay full
                    name = "Fogeltasche ist voll!";
                }*/

                int fuelAmount = mineral.fuelAmount;

                int xp = 0, mineralID = (int)mineral.id;

                switch (mineralID)
                {
                    case 0:
                    case 1:
                    case 5:
                    case 7:
                    case 8:
                    case 9:
                        xp = 6;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        xp = 10;
                        break;
                }

                if (xp > 0)
                {
                    LevelHandler.Instance.AddNewXP(xp);
                }

                if (fuelAmount < 0)
                {
                    fliesizinEffect.SetActive(false);
                    fliesizinEffect.SetActive(true);
                }

                if(mineralID == (int)MineralType.Lava)
                {
                    FF_PlayerData.Instance.Die(DeathCause.Burnt);
                } else
                {
                    if (mD.blussizin)
                    {
                        mD.StopBlussizinLights();
                    }
                    else
                    {
                        if (mineral.id != MineralType.Coin)
                        {
                            Inventory.Instance.SetMineralAmount(mineral.id, 1, 1);
                        }
                        else
                        {
                            //ScoreHandler.Instance.AddCoin();
                            ShopHandler.Instance.UpdateBlus(1, 1);
                        }
                    }

                    player.GetComponent<FF_PlayerData>().AddFuel(fuelAmount);
                }

                infoText.GetComponent<InfoText>().StartFlashing(name);
            }
        } else
        {
            if(force)
            {
                mineObjs[nextMinePos].transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        mineObjs[nextMinePos].transform.localScale = new Vector3(360, 360, 360);
        mineObjs[nextMinePos].GetComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.25f);

        if(update) {
            minedSpriteCheck = true;
            CheckBounds();
        }

        int abovePos = nextMinePos - 8;
        if(abovePos >= 0)
        {
            MineData aboveMD = mineObjs[abovePos].GetComponent<MineData>();
            if(aboveMD.bigstone && !aboveMD.mined)
            {
                if(playerMine)
                {
                    Timing.RunCoroutine(DestroyAbove(abovePos, 0.75f));
                }
            }
        }

        mineObjs[nextMinePos].GetComponent<BoxCollider2D>().enabled = false;

        int depth = player.GetComponent<FF_PlayerData>().GetPlayerDepth();

        if(depth >= 9 && !bgHandler.mining)
        {
            StartBackgroundMine();
        }

        //Debug.Log(player.GetComponent<FF_PlayerData>().GetPlayerDepth());
        if (player.GetComponent<FF_PlayerData>().GetPlayerDepth() == 100)
        {
            Invoke(nameof(Migrate), 0.01f);
        }

       // mineObjs[nextMinePos].SetActive(false);
    }

    IEnumerator<float> DestroyAbove(int pos, float waitTime)
    {
        mineObjs[pos].GetComponent<MineData>().StartVibrate();
        yield return Timing.WaitForSeconds(waitTime);
        MineObj(pos, false);
        mineObjs[pos].GetComponent<MineData>().EnableFall();
    }

    public void MineItemClicked(int id)
    {

        if(!ShopHandler.Instance.GetMineItem(id))
        { //wenn item nicht mehr vorhanden return
            //(zieht auch schon automatisch resourcen ab)
            //return;
        }

        if(isMoving)
        {
            return;
        }

        switch(id)
        {
            case 0: //Bombe
                Timing.RunCoroutine(ExplodeRadius(100, 0.15f));

                GameObject effect = objPooler.SpawnFromPool("MineBombEffect", player.transform.position, Quaternion.identity);
                Timing.RunCoroutine(DeactivateDelayed(effect, 4));
                break;
            case 1: //Kanister (35% fuel wiederherstellen)
                float t = player.GetComponent<FF_PlayerData>().GetMaxFuel() * 0.35f;
                player.GetComponent<FF_PlayerData>().AddFuel((int)t);
                break;
        }
    }

    IEnumerator<float> ExplodeRadius(float radius, float time)
    { //Zerstört alle Blöcke im Radius
        yield return Timing.WaitForSeconds(time);


        /*int startPos = nextMinePos;

        bool ok = false;

        if(startPos >= 0 && startPos < mineObjsCounter)
        {
            if(mineObjs[startPos] != null)
            {
                ok = true;
            }
        }*/

        Collider2D[] collObjs = 
            Physics2D.OverlapCircleAll(player.transform.position,
                radius);

        List<GameObject> allBlocks = new List<GameObject>();

        for(int i = 0; i < collObjs.Length; i++)
        {
            if(collObjs[i].gameObject.CompareTag("MineGround"))
            {
                //if(collObjs[i].gameObject != mineObjs[startPos])
                //{
                    allBlocks.Add(collObjs[i].gameObject);
                //}
            }
        }

        GameObject deepest = null;

        if(allBlocks.Count > 0)
        {
            deepest = allBlocks[0];
        }

        for(int i = 0; i < allBlocks.Count; i++)
        {
            if(!allBlocks[i].GetComponent<MineData>().mined)
            {
                objPooler.SpawnFromPool("MineDissolveEffect", allBlocks[i].transform.position, Quaternion.identity);
                MineObj(allBlocks[i].GetComponent<MineData>().arrayPos, false, false, true);

                if(allBlocks[i].transform.position.y < deepest.transform.position.y)
                { //tiefster block der gefunden wurde
                    deepest = allBlocks[i];
                }
            }
        }

        if(deepest != null)
        {
            int newDepth = deepest.GetComponent<MineData>().depth + 1;
            int currentDepth = player.GetComponent<FF_PlayerData>().GetPlayerDepth() - 1;

            if(newDepth > 99)
            {
                newDepth -= 89;
            }

            //ulong currentScore = FlatterFogelHandler.Instance.GetScore();

            //Debug.Log(newDepth + " " + currentDepth);

            if (newDepth > currentDepth)
            {
                int diff = newDepth - currentDepth;

                player.GetComponent<FF_PlayerData>().SetPlayerDepth(diff, true);
                FlatterFogelHandler.Instance.SetScore(diff, 1);
            }
        }

        minedSpriteCheck = true;
        CheckBounds();

        RegenerateMines();

        if (player.GetComponent<FF_PlayerData>().GetPlayerDepth() >= 100)
        {
            Invoke(nameof(Migrate), 0.01f);
        }
    }

    IEnumerator<float> DeactivateDelayed(GameObject obj, float time)
    {
        yield return Timing.WaitForSeconds(time);
        obj.SetActive(false);
    }

    private void Migrate()
    {
        //Debug.Break();
        Vector3 playerPos = player.transform.position;
        Vector3 cameraPos = mainCamera.transform.position;

        migrations++;

        int range = 152;

        GameObject[] tempArray = new GameObject[range]; //16*8 -> die letzten 120 blöcke klonen

        Vector3 startPos = new Vector3(-696, 105);
        Vector3 newPos = new Vector3();

        float oldY, oldPlayerY = playerPos.y;

        float cameraDiff = Mathf.Abs(cameraPos.y - oldPlayerY);

        if(mineObjs[mineObjsCounter - 1] == null)
        {
            for(int i = 2; i < 50; i++)
            {
                if(mineObjs[mineObjsCounter - i] == null)
                {
                    Debug.Log("NULL: " + i);
                } else
                {
                    Debug.Log("VALID: " + i);

                    mineObjsCounter = (mineObjsCounter - i) + 1;
                    break;
                }
            }
        }

        oldY = mineObjs[mineObjsCounter - 1].transform.position.y;

        int y = 0;
        int x = 0;
        for(int i = 0; i < range; i++)
        { //kopiert die letzten [range] blöcke
            tempArray[i] = mineObjs[(mineObjsCounter - range) + i];

            newPos.x = startPos.x + 90 * x;
            newPos.y = startPos.y - 90 * y;

            tempArray[i].transform.position = newPos;

            x++;
            if(x == 8)
            {
                x = 0;
                y++;
            }
        }

        for(int i = 0; i < mineObjsCounter - range; i++)
        { //löscht alle bis auf die letzten [range]
            Destroy(mineObjs[i]);
            mineObjs[i] = null;
        }

        mineObjsCounter = range;

        MineData tmp = null;
        for(int i = 0; i < mineObjsCounter; i++)
        {
            mineObjs[i] = tempArray[i];
            mineObjs[i].SetActive(true);

            tmp = mineObjs[i].GetComponent<MineData>();
            tmp.arrayPos = i;
            //tmp.depth = i / 8;
        }

        player.GetComponent<FF_PlayerData>().SetPlayerDepth(11);

        float newDiff = oldY + Mathf.Abs(mineObjs[mineObjsCounter - 1].transform.position.y);

        playerPos.y += Mathf.Abs(newDiff);

        if(playerMoveTween != null)
        {
            if (playerMoveTween.active)
            {
                playerMoveTween.Kill();
            }
        }

        player.transform.position = playerPos;
        player.GetComponent<CircleCollider2D>().enabled = true;

        float oldX = mainCamera.transform.position.x;

        mainCamera.transform.position = new Vector3(oldX, playerPos.y + cameraDiff, -400);
    }

    public void MinePlayerTo(Vector3 pos, Item m = null, float time = 1f, int dir = 0)
    { //nextMinePos = arraypos von obj was gemined wird
        isMoving = true;

        player.GetComponent<PlayerMiner>().StartStopMine(dir);

        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<CircleCollider2D>().enabled = false;

        time = PlayerMiner.currentMiner.mineTime / 1000f;

        Item mineral = m;

        float baseMP = 1f; //Hitzemultiplikator
        float timeMP = 1f; //1

        if (mineral != null)
        {
            switch (mineral.id)
            {
                case MineralType.Blussizin_S:
                    baseMP = 1.05f;
                    timeMP = 1.5f;
                    break;
                case MineralType.Blussizin_M:
                    baseMP = 1.10f;
                    timeMP = 1.6f;
                    break;
                case MineralType.Blussizin_L:
                    baseMP = 1.15f;
                    timeMP = 1.7f;
                    break;
                case MineralType.Fliesium:
                case MineralType.Lava:
                    baseMP = 1.5f;
                    timeMP = 2f;
                    break;
                case MineralType.Lilium:
                    baseMP = 1.65f;
                    timeMP = 2.2f;
                    break;
                case MineralType.Altium:
                    baseMP = 1.85f;
                    timeMP = 2.3f;
                    break;
                case MineralType.Modium:
                    baseMP = 1.95f;
                    timeMP = 2.75f;
                    break;
                case MineralType.Admium:
                    baseMP = 2f;
                    timeMP = 3.25f;
                    break;
                case MineralType.Legium:
                    baseMP = 2.2f;
                    timeMP = 4f;
                    break;
            }
        }

        baseMP *= currentHeatShield.heatMultiplier;

        bool lavaNear = false;

        for(int x = -1; x < 2 && !lavaNear; x++)
        {
            for(int y = -1; y < 2; y++)
            {
                int nextPos = nextMinePos + x + (y * 8);
                
                if(nextPos >= 0 && nextPos != nextMinePos)
                {
                    if(mineObjs[nextPos] != null)
                    {
                        MineData mD = mineObjs[nextPos].GetComponent<MineData>();
                        
                        if(mD.mineral != null)
                        {
                            if(mD.mineral.id == MineralType.Lava)
                            {
                                lavaNear = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if(lavaNear)
        { //20% höhere hitze wenn lava in nähe
            baseMP *= 1.2f;
        }

        if(mineral == null)
        {
            baseMP = 0;
            time = 0.5f;
        } else
        {
            time *= timeMP;
        }

#if UNITY_EDITOR
        //time = 0.1f;
        baseMP = 0;
        //player.GetComponent<FF_PlayerData>().AddFuel(100);
#endif

        if (baseMP > 0)
        {
            FF_PlayerData.Instance.UpdatePlayerHeat(baseMP, time);
        }

        playerMoveTween = player.transform.DOMove(pos, time);
        Invoke(nameof(RestorePlayerMine), time);

        StartShake(time, 1.5f);

        Timing.RunCoroutine(WaitMineObj(nextMinePos, time + 0.01f));
    }

    private void StartShake(float time, float amount)
    {
        isShaking = true;

        Invoke(nameof(EndShake), time);
        //Camera.main.transform.DOShakePosition(time, 1, 10, 90);
    }

    private void EndShake()
    {
        isShaking = false;
    }

    private void RestorePlayerMine()
    {
        player.GetComponent<Rigidbody2D>().simulated = true;
        player.GetComponent<CircleCollider2D>().enabled = true;
        isMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(miningActive)
        {
            if(player.transform.position.y > 214 && lineObj.activeSelf)
            { //über linie -> minen beenden
                EndMine();

                ffHandler.ChangeMode();
            }
        }
    }

    public void EndMine()
    {
        lineObj.SetActive(false);
        //controlCanvas.SetActive(false);
        //timeUI.SetActive(false);
        movement.SetActive(false);
        miningActive = false;
        lastCamPosAssigned = false;

        fliesizinEffect.SetActive(false);

        minerCollider.SetActive(false);

        player.GetComponent<PlayerMiner>().EndMining();
        player.GetComponent<FF_PlayerData>().EndMine();
        bgHandler.EndMining();

        CancelInvoke("CheckBounds");
        CancelInvoke("HandleTime");
    }

    public void DisableBackground()
    {
        mineBackground.SetActive(false);
    }

    private void LateUpdate()
    {
        if(miningActive && !FlatterFogelHandler.gamePaused)
        {
            Vector3 newPos = mainCamera.transform.position;

            if(lastCamPosAssigned)
            {
                newPos = lastCamPos;
            }

            Vector3 desiredPosition = player.transform.position;
            Vector3 smoothedPosition =
                Vector3.Lerp(newPos, desiredPosition, 5f * Time.deltaTime);

            lastCamPos = smoothedPosition;

            if(isShaking)
            {
                smoothedPosition += Random.insideUnitSphere * 3f;
            }

            if(smoothedPosition.x < -470)
            {
                smoothedPosition.x = -470;
            } else if(smoothedPosition.x > -292)
            {
                smoothedPosition.x = -292;
            }

            smoothedPosition.z = -400;

            mainCamera.transform.position = smoothedPosition;

            if(!backgroundSet)
            {
                if(smoothedPosition.y < -773)
                {
                    backgroundSet = true;
                    mineBackground.transform.SetParent(mainCamera.transform);
                }
            }
        }

        if(backgroundSet)
        {
            Vector3 pos = mainCamera.transform.position;
            if(pos.y > -774)
            {
                mineBackground.transform.SetParent(transform);
                backgroundSet = false;
            }
        }
    }
}
