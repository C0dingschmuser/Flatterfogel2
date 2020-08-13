using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class BackgroundHandler : MonoBehaviour
{

    public class SpriteDataHolder
    {
        public int currentMaxNumber = 3; //4 schon geladen -> nächste ist 5
        public Sprite[] subSprites = new Sprite[6];
        public Sprite[] subSpritesL = new Sprite[6];
    }

    public class SpriteLayerHolder
    {
        //enthält alle "sprites" eines layers
        //current sprite ist 2 weil ja 4 (3-1)obj schon on screen
        public int currentSprite = 3, rightObjPos = 4, maxSprites = 5;
        public List<SpriteDataHolder> mainSprite = new List<SpriteDataHolder>();
        public GameObject[] layerPartObjs = new GameObject[5];
    }

    public static BackgroundHandler Instance;

    //array von liste die array von sprites enthält
    public SpriteLayerHolder[] spriteData = new SpriteLayerHolder[4]; //weil 4 parralax layer

    public bool mining = false;

    public MineHandler mineHandler;
    public GameObject nonScrollingObj;
    public GameObject[] bgObjs = new GameObject[6];
    public GameObject[] layerParents;

    [SerializeField]
    private new Camera camera = null;
    [SerializeField]
    private FlatterFogelHandler ffHandler = null;
    [SerializeField]
    private bool isScrolling = false;

    public List<Sprite> layer0Sprites = new List<Sprite>();
    public List<Sprite> layer1Sprites = new List<Sprite>();
    public List<Sprite> layer2Sprites = new List<Sprite>();
    public List<Sprite> layer3Sprites = new List<Sprite>();

    public Sprite[] cloudSprites;
    public GameObject cloudObject;

    public List<BackgroundData> bgData = new List<BackgroundData>();
    public Color nightColor, dayColor, nightBackgroundColor, dayBackgroundColor;
    public Color[] bgColor;
    public TextAsset bgDataText;
    public BackgroundData currentBG = new BackgroundData();
    public AnimationCurve dayLight, nightLight;
    public GameObject sunMoon, sunObj, moonObj, sunLightObj, moonLightObj, topExtent;
    public SpriteRenderer topExtentRenderer;

    public UnityEngine.Experimental.Rendering.Universal.Light2D globalLight, moonLight, sunLight;
    public Sprite[] sunMoonSprites = new Sprite[2];

    [SerializeField]
    private Transform lightLayerParent = null;
    [SerializeField]
    private GameObject train = null;

    private Vector3 oldTopExtentPos, oldTopExtentScale;

    private Tween globalLightTween, globalColorTween, moonLightTween, sunLightTween, cameraColorTween;
    private Background currentBackground;
    private float cycleTime = 10;
    public bool night = true, layersEnabled = true;

    [SerializeField]
    private float[] layerSpeeds = new float[4];
    [SerializeField]
    private float[] oldLayerSpeeds = null;

    public class BackgroundData
    { //obsolete
        public int id; //Bild-ID
        public int type; //Typ (Tag/Nacht & co.
        public Color32[] colors = new Color32[3]; //day,sunset,night farbe
        public List<int> sp_Layer2 = new List<int>(); //Unterstützte Layer2s
        public List<int> sp_Layer3 = new List<int>(); //Unterstützte Layer3s
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        //LoadAllBackgrounds();

        //LoadDefaultBackgrounds();

        //int id = Random.Range(0, 2);
        //currentBG = bgData[id];
        //SetNewColor(bgColor[Random.Range(0, bgColor.Length)]);

        SetNewBackground(ShopHandler.Instance.GetCurrentBackground());

        //SpawnCloud(true);
        ScaleLights(true);

        cycleTime = sunMoon.GetComponent<DOTweenPath>().duration;
        StartDayFirst();

        Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
    }

    public void ScaleLights(bool normal)
    {
        if(normal)
        {
            sunLightObj.transform.localScale = new Vector3(0.44f, 1, 1);
            moonLightObj.transform.localScale = sunLightObj.transform.localScale;

            sunLightObj.transform.position = new Vector3(-381, 983.4f, 10);
            moonLightObj.transform.position = sunLightObj.transform.position;
        } else
        {
            sunLightObj.transform.localScale = Vector3.one;
            moonLightObj.transform.localScale = Vector3.one;

            sunLightObj.transform.position = new Vector3(59, 983.4f, 10);
            moonLightObj.transform.position = sunLightObj.transform.position;
        }
    }

    private void LoadAllBackgrounds()
    { //lädt die bgs inkl subsprites ins spriteData-Array
        //OBSOLETE
        for(int spriteLayer = 0; spriteLayer < 3; spriteLayer++)
        {
            SpriteLayerHolder currentLayer = new SpriteLayerHolder();

            int layerSpriteCount = 3;
            switch(spriteLayer)
            {
                case 1:
                    layerSpriteCount = 4;
                    break;
                case 2:
                    layerSpriteCount = 4;
                    break;
            }

            for(int i = 0; i < layerSpriteCount; i++)
            {
                SpriteDataHolder fullLayerSprites = new SpriteDataHolder
                {
                    subSprites =
                    Resources.LoadAll<Sprite>("Sprites/CBG/" + spriteLayer.ToString() + "/" + i.ToString())
                };

                currentLayer.mainSprite.Add(fullLayerSprites);
            }

            int ran = Random.Range(0, layerSpriteCount);

            currentLayer.currentSprite = ran;

            for(int i = 0; i < 5; i++)
            {
                currentLayer.layerPartObjs[i] = transform.GetChild(spriteLayer).GetChild(i).gameObject;
                currentLayer.layerPartObjs[i].GetComponent<SpriteRenderer>().sprite =
                    currentLayer.mainSprite[ran].subSprites[i];
            }

            spriteData[spriteLayer] = currentLayer;
        }
    }

    public void DisableEnableLight(bool enable)
    {
        if (globalLightTween != null) globalLightTween.Kill();
        if (sunLightTween != null) sunLightTween.Kill();
        if (moonLightTween != null) moonLightTween.Kill();

        CancelInvoke("ResetMoonPos");
        CancelInvoke("StartNight");
        CancelInvoke("ResetSunPos");
        CancelInvoke("ResetMoonPos");

        if (enable)
        {
            if(currentBackground.supportsDayNight)
            {
                moonObj.GetComponent<SpriteRenderer>().enabled = true;
                moonLightObj.SetActive(false);

                sunObj.GetComponent<SpriteRenderer>().enabled = true;
                sunLightObj.SetActive(false); //true
            }

            sunObj.GetComponent<DOTweenPath>().DORestart();
            moonObj.GetComponent<DOTweenPath>().DOGoto(0, false);

            StartDayFirst();
        } else
        {
            globalLight.intensity = 0;

            moonObj.GetComponent<SpriteRenderer>().enabled = false;
            moonLightObj.SetActive(false);

            sunObj.GetComponent<SpriteRenderer>().enabled = false;
            sunLightObj.SetActive(false);
        }
    }

    public void SpawnCloud(bool start = false)
    {
        cloudObject.GetComponent<SpriteRenderer>().sprite =
            cloudSprites[Random.Range(0, cloudSprites.Length)];

        float topY = 1434;

        if (!OptionHandler.normalAspect)
        {
            topY = OptionHandler.defaultCameraPos.y + OptionHandler.cameraBounds.extents.y;
        }

        float startX = Random.Range(-318, 435);

        if(!start)
        {
            startX = Random.Range(1003, 1634);
        }

        cloudObject.transform.position = new Vector3(startX, topY - 129);
    }

    public void UpdateTopExtent(Vector3 pos, float ySize)
    {
        float yScale = ySize * 1.64f;

        topExtent.transform.localScale =
            new Vector3(topExtent.transform.localScale.x,
                yScale, 0);

        pos.z = 20;

        oldTopExtentPos = pos;
        oldTopExtentScale = topExtent.transform.localScale;

        topExtent.transform.position = pos;
    }

    public void EnlargeTopExtent(Vector3 pos)
    {
        Vector3 scale = topExtent.transform.localScale;
        scale.x *= 1.512f;
        scale.y *= 1.465f;

        pos.z = 20;

        topExtent.transform.localScale = scale;
        topExtent.transform.position = pos;
    }

    public void ReduceTopExtent()
    {
        topExtent.transform.localScale = oldTopExtentScale;
        topExtent.transform.position = oldTopExtentPos;
    }

    public void SetNewBackground(Background bg)
    {
        if(currentBackground != null)
        {
            if(currentBackground.supportsDayNight && !bg.supportsDayNight)
            { //Tag/Nacht Deaktivieren

                if (globalLightTween != null) globalLightTween.Kill();

            } else if(!currentBackground.supportsDayNight && bg.supportsDayNight)
            { //Tag/Nacht neustart

                if (sunLightTween != null) sunLightTween.Kill();
                if (moonLightTween != null) moonLightTween.Kill();

                CancelInvoke("ResetMoonPos");
                CancelInvoke("StartNight");
                CancelInvoke("ResetSunPos");
                CancelInvoke("ResetMoonPos");

                sunObj.GetComponent<DOTweenPath>().DORestart();
                moonObj.GetComponent<DOTweenPath>().DOGoto(0, false);

                StartDayFirst();
            }
        }

        if(!bg.supportsDayNight)
        {
            globalLight.intensity = 1f;
        }

        currentBackground = bg;

        if(bg.supportsDayNight)
        {
            moonObj.GetComponent<SpriteRenderer>().enabled = true;
            moonObj.SetActive(false);
            moonLightObj.SetActive(false);

            sunObj.GetComponent<SpriteRenderer>().enabled = true;
            sunObj.SetActive(true);
            sunLightObj.SetActive(false); //true
        } else
        {
            moonObj.GetComponent<SpriteRenderer>().enabled = false;
            moonObj.SetActive(false);
            moonLightObj.SetActive(false); //false

            sunObj.GetComponent<SpriteRenderer>().enabled = false;
            sunObj.SetActive(true);
            sunLightObj.SetActive(false);
        }

        if(!bg.scrolling)
        {
            for (int i = 0; i < 4; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            nonScrollingObj.SetActive(true);

            nonScrollingObj.GetComponent<SpriteRenderer>().sprite =
                bg.nonScrollingSprite;

            return;
        } else
        {
            for(int i = 0; i < 4; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            nonScrollingObj.SetActive(false);
        }

        //topExtent.GetComponent<SpriteRenderer>().sprite = bg.topExtentSprite;
        //topExtent.GetComponent<SpriteRenderer>().color = bg.topExtentColor;

        for (int spriteLayer = 0; spriteLayer < 4; spriteLayer++)
        {
            SpriteLayerHolder currentLayer = new SpriteLayerHolder();

            Sprite[] sC = bg.layer0; //normale sprites
            Sprite[] sCL = bg.layer0L; //licht sprites

            switch(spriteLayer)
            {
                case 1:
                    sC = bg.layer1;
                    sCL = bg.layer1L;
                    break;
                case 2:
                    sC = bg.layer2;
                    sCL = bg.layer2L;
                    break;
                case 3:
                    sC = bg.layer3;
                    sCL = bg.layer3L;
                    break;
            }

            currentLayer.maxSprites = sC.Length;

            SpriteDataHolder fullLayerSprites = new SpriteDataHolder
            {
                subSprites = sC,
                subSpritesL = sCL
            };

            currentLayer.mainSprite.Add(fullLayerSprites);

            //currentLayer.currentSprite = 0;

            for (int i = 0; i < 5; i++)
            {
                currentLayer.layerPartObjs[i] = transform.GetChild(spriteLayer).GetChild(i).gameObject;
                currentLayer.layerPartObjs[i].GetComponent<SpriteID>().currentID = i;
                currentLayer.layerPartObjs[i].GetComponent<SpriteRenderer>().sprite =
                    currentLayer.mainSprite[0].subSprites[i];

                Vector3 scale;

                if(spriteLayer < 3)
                {
                    scale = new Vector3(bg.scale[spriteLayer].x, bg.scale[spriteLayer].y, 710f);
                } else
                {
                    scale = new Vector3(bg.scale[bg.lightLayerSpeedLvl].x, 
                        bg.scale[bg.lightLayerSpeedLvl].y, 710f);
                }

                currentLayer.layerPartObjs[i].transform.localScale = scale;
            }

            currentLayer.layerPartObjs[0].transform.position = new Vector3(-560.900f, 789.988f);
            currentLayer.layerPartObjs[0].SetActive(true);

            currentLayer.layerPartObjs[1].transform.position = new Vector3(-200.775f, 789.988f);
            currentLayer.layerPartObjs[1].SetActive(true);

            currentLayer.layerPartObjs[2].transform.position = new Vector3(159.350f, 789.988f);
            currentLayer.layerPartObjs[2].SetActive(false);

            currentLayer.layerPartObjs[3].transform.position = new Vector3(519.475f, 789.988f);
            currentLayer.layerPartObjs[3].SetActive(false);

            currentLayer.layerPartObjs[4].transform.position = new Vector3(879.6f, 789.988f);
            currentLayer.layerPartObjs[4].SetActive(false);

            spriteData[spriteLayer] = currentLayer;
        }

        SetNight(true);

        if (bg.supportsColor)
        {
            Color nC = bg.supportedColors[Random.Range(0, bg.supportedColors.Length)];

            SetNewColor(nC);
        } else
        {
            SetNewColor(Color.white, false);
        }
    }

    public void SetNewColor(Color c, bool supports = true)
    {
        Color original = c;

        for (int layer = 0; layer < 3; layer++)
        {
            switch (layer)
            {
                case 0:
                    c = ChangeColorBrightness(c, +10);
                    if(supports)
                    {
                        topExtent.GetComponent<SpriteRenderer>().color = c;
                    }

                    break;
                case 1:
                    c = ChangeColorBrightness(original, -40);
                    break;
                case 2:
                    c = ChangeColorBrightness(original, -60);
                    break;
            }

            if(!supports)
            {
                c = Color.white;
            }

            for (int i = 0; i < 4; i++)
            {
                spriteData[layer].layerPartObjs[i].GetComponent<SpriteRenderer>().color = c;
            }
        }
    }

    public static Color ChangeColorBrightness(Color32 color, int factor)
    { //negativ = dunkler, positiv = heller
        int r = (color.r + factor > 255) ? 255 : color.r + factor;
        int g = (color.g + factor > 255) ? 255 : color.g + factor;
        int b = (color.b + factor > 255) ? 255 : color.b + factor;

        return new Color32((byte)r, (byte)g, (byte)b, 255);
    }

    #region loadDefaultBackgrounds
    private void LoadDefaultBackgrounds()
    { //OBSOLETE
        string rawBGData = bgDataText.text;
        rawBGData = Regex.Replace(rawBGData, @"\r\n?|\n", "");

        string[] bgList = rawBGData.Split('|');

        for(int i = 1; i < bgList.Length; i++)
        { //startet bei 1 da 0 formatierungserklärung ist
            BackgroundData tempBG = new BackgroundData();

            string[] data = bgList[i].Split(';');
            tempBG.id = Int32.Parse(data[0]);
            tempBG.type = Int32.Parse(data[1]);

            for(int a = 0; a < 3; a++)
            {
                byte r = Byte.Parse(data[2 + a].Split(',')[0]);
                byte b = Byte.Parse(data[2 + a].Split(',')[1]);
                byte g = Byte.Parse(data[2 + a].Split(',')[2]);

                Color32 c = new Color32(r, g, b, 255);
                tempBG.colors[a] = c;
            }

            string[] layer2s = data[5].Split(',');
            string[] layer3s = data[6].Split(',');

            for(int a = 0; a < layer2s.Length; a++)
            {
                tempBG.sp_Layer2.Add(Int32.Parse(layer2s[a]));
            }

            for (int a = 0; a < layer3s.Length; a++)
            {
                tempBG.sp_Layer3.Add(Int32.Parse(layer3s[a]));
            }

            bgData.Add(tempBG);
        }
    }

    public int ReturnRandomLayerBG(int presetID, int layerID)
    { //gibt zufällige imageID für das angegebene layer zurück
        int ran = Random.Range(0, bgData[presetID].sp_Layer2.Count);

        if(layerID == 2)
        {
            ran = Random.Range(0, bgData[presetID].sp_Layer3.Count);
            return bgData[presetID].sp_Layer3[ran];
        }

        return bgData[presetID].sp_Layer2[ran];
    }
    #endregion

    private void StartDayFirst()
    { //lightt
        if(currentBackground.supportsDayNight)
        {
            topExtentRenderer.color = nightBackgroundColor;
            cameraColorTween =
                DOTween.To(() => topExtentRenderer.color, x => topExtentRenderer.color = x, dayBackgroundColor, cycleTime / 6);

            globalColorTween =
                DOTween.To(() => globalLight.color, x => globalLight.color = x, dayColor, cycleTime / 8);

            if (!mining)
            {
                if (OptionHandler.lightEnabled == 1)
                {
                    globalLight.intensity = 0.4f;
                }
                else
                {
                    globalLight.intensity = 0.4f;
                }
            }
        }

        StartDay();

        Invoke("ResetMoonPos", cycleTime / 6);
    }

    public void StartDay()
    {
        //globalLight.GetComponent<UnityEngine.Experimental.Rendering.LWRP.Light2D>().DO

        SetNight(false);
        Timing.RunCoroutine(EndNightLights(cycleTime / 5));

        Invoke("StartNight", cycleTime - (cycleTime / 5));
        //Invoke("SetMoon", cycleTime);
    }

    private IEnumerator<float> EndNightLights(float waitTime)
    {
        yield return Timing.WaitForSeconds(waitTime);
        SetNight(false);
    }

    private void StartNight()
    { //lightt
        if (!mineHandler.miningActive)
        {
            float val = 0.5f;

            if(OptionHandler.lightEnabled != 1)
            {
                val = 0.5f;
            }

            if(currentBackground.supportsDayNight)
            {
                if(!mining)
                {
                    globalLightTween = DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, val, cycleTime / 5);
                }

                cameraColorTween =
                    DOTween.To(() => topExtentRenderer.color, x => topExtentRenderer.color = x, nightBackgroundColor, cycleTime / 6);

                globalColorTween = DOTween.To(() => globalLight.color, x => globalLight.color = x, nightColor, cycleTime / 8);

                SetNight(true);
            }
        }
        

        //Invoke("StartDay", cycleTime + (cycleTime / 5));
    }

    private void SetNight(bool night)
    {
        this.night = night;

        StartCoroutine(train.GetComponent<TrainHandler>().SetNightDelayed(Random.Range(0, 2), night));

        GameObject layerPartObj;
        SpriteLayerHolder currentLayer;
        int currentID;

        for (int layer = 0; layer < 4; layer++)
        {
            currentLayer = spriteData[layer];

            for (int i = 0; i < 5; i++)
            {
                layerPartObj = currentLayer.layerPartObjs[i];

                currentID = layerPartObj.GetComponent<SpriteID>().currentID;

                if(!night)
                {
                    layerPartObj.GetComponent<SpriteRenderer>().sprite =
                        currentLayer.mainSprite[0].subSprites[currentID];
                } else
                {
                    if(currentLayer.mainSprite[0].subSpritesL.Length > 0)
                    {
                        layerPartObj.GetComponent<SpriteRenderer>().sprite =
                            currentLayer.mainSprite[0].subSpritesL[currentID];
                    }
                }
            }
        }
    }

    public void SetMoon()
    {
        night = true;

        sunMoon.GetComponent<Image>().sprite = sunMoonSprites[1];
        sunMoon.transform.GetChild(0).gameObject.SetActive(true);
        sunMoon.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void StartMoonTween()
    { //Tweent Mondlicht hoch und sonnenlicht runter

        moonLightObj.SetActive(false); //true

        if (OptionHandler.lightEnabled == 1 && currentBackground.supportsDayNight)
        {

            //moonLightTween = DOTween.To(() => moonLight.intensity, x => moonLight.intensity = x, 1f, cycleTime / 8);
            //sunLightTween = DOTween.To(() => sunLight.intensity, x => sunLight.intensity = x, 0f, cycleTime / 8);
        }

        Invoke("ResetSunPos", cycleTime / 6);
        StartMoonLoop();
    }

    private void StartMoonLoop()
    {
        sunObj.SetActive(false);
        moonObj.SetActive(true);

        moonObj.GetComponent<DOTweenPath>().DORestart();
    }

    public void StartSunTween()
    { //tweent mondlicht runter und sonnenlicht hoch
        sunLightObj.SetActive(false); //true
        
        if (OptionHandler.lightEnabled == 1 && currentBackground.supportsDayNight)
        {
            topExtentRenderer.color = nightBackgroundColor;
            cameraColorTween =
                DOTween.To(() => topExtentRenderer.color, x => topExtentRenderer.color = x, dayBackgroundColor, cycleTime / 6);

            globalColorTween =
                DOTween.To(() => globalLight.color, x => globalLight.color = x, dayColor, cycleTime / 8);

            //moonLightTween = DOTween.To(() => moonLight.intensity, x => moonLight.intensity = x, 0f, cycleTime / 8);
            //sunLightTween = DOTween.To(() => sunLight.intensity, x => sunLight.intensity = x, 0.75f, cycleTime / 8);
        }

        Invoke("ResetMoonPos", cycleTime / 6);
        StartSunLoop();
    }

    private void StartSunLoop()
    {
        sunObj.SetActive(true);
        moonObj.SetActive(false);

        sunObj.GetComponent<DOTweenPath>().DORestart();
        StartDay();
    }

    private void ResetSunPos()
    {
        sunObj.SetActive(false);
        sunObj.transform.position = new Vector3(-951, 710);
    }

    private void ResetMoonPos()
    { //lightt
        moonObj.SetActive(false);
        moonObj.transform.position = new Vector3(-951, 710);

        //wird erst hier generell hell damit der übergang smoother wirkt
        if (!mineHandler.miningActive)
        {
            if (currentBackground.supportsDayNight)
            {
                globalLightTween =
                        DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, 0.75f, cycleTime / 4);
            }
        }
    }

    public void StartMining()
    { //setzt helligkeit herunter
        if(globalLightTween != null)
        {
            globalLightTween.Kill();
        }

        mining = true;

        float val = 0.5f;
        if (OptionHandler.lightEnabled == 0)
        {
            val = 0.6f;
        }

        globalLightTween =
            DOTween.To(() => globalLight.intensity, x => globalLight.intensity = x, val, 4);

        SetNight(true);

        sunObj.GetComponent<DOTweenPath>().DORestart();
        sunObj.GetComponent<DOTweenPath>().DOPause();

        moonObj.GetComponent<DOTweenPath>().DORestart();
        moonObj.GetComponent<DOTweenPath>().DOPause();

        if (sunLightTween != null) sunLightTween.Kill();
        if (moonLightTween != null) moonLightTween.Kill();

        CancelInvoke("ResetMoonPos");
        CancelInvoke("StartNight");
        CancelInvoke("ResetSunPos");
        CancelInvoke("ResetMoonPos");
    }

    public void EndMining()
    {
        mining = false;

        DisableEnableLight(true);
    }

    public void ResetBG(int pos)
    { //setzt bgObj an pos x zurück
        //OBSOLETE
        int realPos = pos;

        switch(pos)
        {
            case 2:
            case 4:
                realPos = 0;
                break;
            case 3:
            case 5:
                realPos = 1;
                break;
        }

        int ran = Random.Range(0, layer1Sprites.Count);

        if((pos / 2) > 1)
        {
            ran = Random.Range(0, layer2Sprites.Count);
        }

        SetLayerImage(pos, pos / 2, ran);
        bgObjs[pos].SetActive(false);
    }

    public void SetLayerImage(int pos, int layerID, int imageID)
    {
        Sprite newImage = 
            Resources.Load<Sprite>("Sprites/CBG/" + layerID.ToString() + "/" + imageID.ToString());

        bgObjs[pos].GetComponent<SpriteRenderer>().sprite = newImage;
    }

    public void SetScrolling(bool newMode)
    {
        if(OptionHandler.parallaxMode == 0)
        {
            if(newMode)
            {
                newMode = false;
            }
        }

        isScrolling = newMode;
    }

    public bool GetScrolling()
    {
        return isScrolling;
    }

    public void EnableBackground(bool enable)
    {
        if(enable != layersEnabled)
        {
            layersEnabled = enable;

            for (int i = 0; i < layerParents.Length; i++)
            {
                layerParents[i].SetActive(enable);
            }
        }
    }

    public void SpeedUp(bool enable)
    {
        if (enable)
        {
            for (int i = 0; i < layerSpeeds.Length; i++)
            {
                //oldLayerSpeeds[i] = layerSpeeds[i];

                layerSpeeds[i] *= 1.5f;
            }
        }
        else
        {
            for (int i = 0; i < layerSpeeds.Length; i++)
            {
                layerSpeeds[i] = oldLayerSpeeds[i];
            }
        }
    }

    // Update is called once per frame
    void _MainUpdate()
    {
        float realScrollSpeed = 0;

        if (isScrolling)
        {
            realScrollSpeed = layerSpeeds[2];

            Vector3 pos;
            GameObject layerPartObj;
            SpriteLayerHolder currentLayer;
            for(int layer = 0; layer < 4; layer++)
            {
                currentLayer = spriteData[layer];

                for(int i = 0; i < 5; i++)
                {
                    layerPartObj = currentLayer.layerPartObjs[i];
                    pos = layerPartObj.transform.position;

                    if(pos.x <= -921.025f)
                    { //reset
                        layerPartObj.SetActive(false);

                        pos.x = currentLayer.layerPartObjs[currentLayer.rightObjPos].transform.position.x +
                            360.125f;

                        if(currentLayer.maxSprites > 5)
                        { //spritezuweisung nur bei mehr als 5 sprites nötig
                            currentLayer.currentSprite++;
                            if (currentLayer.currentSprite >= currentLayer.maxSprites)
                            {
                                currentLayer.currentSprite = 0;
                            }

                            currentLayer.layerPartObjs[currentLayer.rightObjPos].GetComponent<SpriteID>().currentID =
                                currentLayer.currentSprite;

                            bool nightOK = night;

                            if(nightOK && currentLayer.mainSprite[0].subSpritesL.Length == 0)
                            { //nacht sprite nur zuweisen wenn vorhanden
                                nightOK = false;
                            }

                            if (!nightOK)
                            {
                                currentLayer.layerPartObjs[currentLayer.rightObjPos].GetComponent<SpriteRenderer>().sprite =
                                    currentLayer.mainSprite[0].subSprites[currentLayer.currentSprite];
                            } else
                            {
                                currentLayer.layerPartObjs[currentLayer.rightObjPos].GetComponent<SpriteRenderer>().sprite =
                                    currentLayer.mainSprite[0].subSpritesL[currentLayer.currentSprite];
                            }
                        }

                        currentLayer.rightObjPos = i;
                    } else
                    { //bewegen
                        float lSpeed = layerSpeeds[layer];

                        pos.x -= lSpeed * Time.deltaTime;

                        if(!layerPartObj.activeSelf)
                        {
                            float minX = 159.35f;

                            if(ffHandler.destructionMode)
                            { //destruction mode hat größeres sichtfeld -> früher aktivieren
                                minX = 525;
                            }

                            if(pos.x <= minX)//159.350f)
                            {
                                layerPartObj.SetActive(true);
                            }
                        }
                    }
                    layerPartObj.transform.position = pos;

                }
            }
        }

        if (train.activeSelf)
        {
            train.GetComponent<TrainHandler>().UpdateTrain(realScrollSpeed);
        }

        if(cloudObject.activeSelf)
        {
            float speed = -10;

            if(isScrolling)
            {
                speed = -20;
            }

            cloudObject.transform.Translate(speed * Time.deltaTime, 0, 0);
            if(cloudObject.transform.position.x < -749)
            {
                SpawnCloud(false);
            }
        }
    }
}
