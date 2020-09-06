using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using DG.Tweening;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using Firebase.Analytics;

public enum DeathCause
{
    Collision,
    OutOfWorld,
    Minus,
    Ground,
    FuelEmpty,
    Heatshield,
    MieserMinus,
    MieserLaser,
    Crushed,
    Burnt,
    Suicide,
}

[System.Serializable]
public class PipeSpriteData
{
    public bool isEmpty = false;
    public Sprite sprite;
}

public class FF_PlayerData : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D playerLightObj, playerHardcoreLightObj;

    public TutorialHandler tutHandler;

    public GameObject deadParent, playerLight, playerCollider2, deadPlayerPart,
                wing, minerTool, staminaText, itemHolder, fuelParent, mineItemParent,
                hatObj;
    public Camera mainCamera;
    public bool dead = false, isJumping = false, isGrounded = false, heatPaused = false, inShop = false,
        animationRunning = false, hatAnimationRunning = false, inHighscores = false;
    private bool goLocked = false, landing = false;
    public List<GameObject> deadChilds = new List<GameObject>();

    public Transform bottomPlane;

    public PipeSpriteData[,] pipeSprite = new PipeSpriteData[4,4]; //2D 4x4 Array a 8x8Px Sprites
    public PipeSpriteData[,] pipeEndSprite = new PipeSpriteData[4, 4]; //2D 4x4 Array a 10x10 Sprites

    private float jumpTime = 0f;

    [SerializeField]
    private int playerDepth = 0;

    public SpriteRenderer sRenderer, wingRenderer, hatRenderer;

    public Vector3 lastBlusPosition;

    [SerializeField] private Transform deadPlayerParent = null;
    [SerializeField] private Transform mineMovementPrent = null;
    [SerializeField] private Slider fuelSlider = null, heatSlider = null;
    [SerializeField] private TextMeshProUGUI fuelText = null, heatText = null;
    [SerializeField] private Image fuelColorImage = null, heatColorImage = null;

    private Skin currentSkin = null;
    public Wing currentWing = null;
    private Hat currentHat = null;

    //private int lastKey = -1;
    private int groundHeight = 0; //höhe der aktuellen groundmodus-plattform
    private int currentFuel = 200, maxFuel = 200; //fuel für mining modus
    private float currentHeat = 0, maxHeat = 10;
    private Sprite[] anSprites = new Sprite[3];

    private Coroutine heatRoutine = null;
    private bool physicsRes = false, staminaFlashing = false;

    public static FF_PlayerData Instance;
    public const int defaultGravityScale = 175;

    public TextMeshProUGUI deathText;

    [SerializeField]
    FlatterFogelHandler ffHandler = null;
    [SerializeField]
    PlayerMiner playerMiner = null;
    [SerializeField]
    ZigZagHandler zigZagHandler = null;
    [SerializeField]
    ParticleSystem heatParticles = null;
    [SerializeField]
    SpriteRenderer minerSprite = null;

    public LocalizedString collision, outOfWorld, minus, groundCollision, fuelEmpty,
        overheated, vaporized, crushed, burnt, suicide;

    private string collisionString, outOfWorldString, minusString, groundCollisionString,
        fuelEmptyString, overheatedString, vaporizedString, crushedString, burntString, suicideString;

    private void Awake()
    {
        Instance = this;

#if UNITY_EDITOR
        //maxFuel = 10000;
#endif

        for(int i = 0; i < 4; i++)
        {
            for(int a = 0; a < 4; a++)
            {
                pipeSprite[i, a] = new PipeSpriteData();
                pipeEndSprite[i, a] = new PipeSpriteData();
            }
        }

        SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
        //LoadPlayerSkin(0, 0);
    }

    private void Start()
    {
        Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
    }

    public void StartLoadLocalization()
    {
        StartCoroutine(LoadLocalization());
    }

    private IEnumerator LoadLocalization()
    {
        AsyncOperationHandle handle;

        yield return handle = collision.GetLocalizedString();

        collisionString = (string)handle.Result;

        yield return handle = outOfWorld.GetLocalizedString();

        outOfWorldString = (string)handle.Result;

        yield return handle = minus.GetLocalizedString();

        minusString = (string)handle.Result;

        yield return handle = groundCollision.GetLocalizedString();

        groundCollisionString = (string)handle.Result;

        yield return handle = fuelEmpty.GetLocalizedString();

        fuelEmptyString = (string)handle.Result;

        yield return handle = overheated.GetLocalizedString();

        overheatedString = (string)handle.Result;

        yield return handle = vaporized.GetLocalizedString();

        vaporizedString = (string)handle.Result;

        yield return handle = crushed.GetLocalizedString();

        crushedString = (string)handle.Result;

        yield return handle = burnt.GetLocalizedString();

        burntString = (string)handle.Result;

        yield return handle = suicide.GetLocalizedString();

        suicideString = (string)handle.Result;
    }

    public int GetMaxFuel()
    {
        return maxFuel;
    }

    public int GetPlayerDepth()
    {
        return playerDepth;
    }

    public void UpdatePlayerHeat(float add, float time)
    {
        float newHeat = currentHeat + add;

        newHeat = Mathf.Clamp(newHeat, 0, maxHeat);

        heatPaused = true;
        DOTween.To(() => currentHeat, x => currentHeat = x, newHeat, time);
        Invoke(nameof(ResetHeatPause), time + 0.01f);

        UpdateHeatSlider();
    }

    private void ResetHeatPause()
    {
        heatPaused = false;
    }

    IEnumerator HandleHeat()
    {
        ParticleSystem.EmissionModule main = heatParticles.emission;

        float percent = 1f;
        Color32 c;

        while(true)
        { //25 updates pro sekunde (1 * 0.04)
            if(!heatPaused)
            {
                currentHeat = Mathf.Clamp(currentHeat - 0.04f, 0, maxHeat);
            } else
            {
                if(currentHeat >= maxHeat)
                {
                    StopCoroutine(heatRoutine);
                }
            }

            percent = (currentHeat / maxHeat);
            c = minerSprite.color;

            main.rateOverTime = percent * 10;

            c.b = (byte)((1 - percent) * 255);
            c.g = (byte)((1 - percent) * 255);

            minerSprite.color = c;

            UpdateHeatSlider();
            yield return new WaitForSeconds(0.04f);
        }
    }

    public void SetPlayerDepth(int newDepth, bool add = false)
    {
        if(!add)
        {
            playerDepth = newDepth;
        } else
        {
            playerDepth += newDepth;

            if(!dead)
            {
                if (currentHeat >= maxHeat)
                {
                    Die(DeathCause.Heatshield);
                }
            }
        }

        if(playerDepth >= 11)
        {
            BackgroundHandler.Instance.EnableBackground(false);
        }
    }

    public bool IsStaminaLow()
    {
        return staminaFlashing;
    }

    public void RegeneratePhysics(bool high)
    {
        physicsRes = high;
        LoadPlayerSkin(currentSkin, currentWing, high);
    }

    public void LoadPlayerSkin(Skin newSkin, Wing newWing)
    {

        if(currentSkin != null && newSkin != null)
        {
            if (currentSkin.itemID == newSkin.itemID
                    && currentWing.itemID == newWing.itemID)
            { //skin bereits ausgewählt
                return;
            }
        } else
        {
            if(newSkin == null)
            {
                Debug.LogWarning("Trying to Load NULL Skin!");
            } else if(newWing == null)
            {
                Debug.LogWarning("Trying to Load NULL Wing!");
            }
        }

        if(animationRunning)
        {
            animationRunning = false;
        }

        currentSkin = newSkin;
        currentWing = newWing;

        float yDiff = currentWing.xDist;

        Vector3 wPos = transform.position;
        wPos.y += currentSkin.wingStart + yDiff;

        wing.transform.position = wPos;

        LoadPlayerSkin(newSkin, newWing, physicsRes);
    }

    public void LoadHat(Hat newHat)
    {
        currentHat = newHat;

        if(currentHat.animated)
        {
            hatAnimationRunning = true;
        } else
        {
            hatAnimationRunning = false;
        }

        if(newHat.itemID == 0)
        { //kein hut
            hatObj.SetActive(false);
        } else
        {
            hatObj.SetActive(true);

            float yDiff = currentHat.yDist;

            Vector3 pos = transform.position;
            pos.y += currentSkin.hatStart + yDiff;

            hatObj.transform.position = pos;
        }

        hatObj.GetComponent<SpriteRenderer>().sprite = newHat.sprite;
    }

    private void HandleAnimation()
    {
        if (inShop || inHighscores) return;

        currentSkin.shopTime += Time.deltaTime;

        if (currentSkin.shopTime >= currentSkin.animationSpeed)
        { //sprite update
            currentSkin.shopTime = 0;

            currentSkin.shopStep++;
            if (currentSkin.shopStep >= currentSkin.animatedSprites.Length)
            {
                currentSkin.shopStep = 0;
            }

            sRenderer.sprite = currentSkin.animatedSprites[currentSkin.shopStep];
        }
    }

    private void HandleHatAnimation()
    {
        if (inShop || inHighscores) return;

        currentHat.shopTime += Time.deltaTime;

        if (currentHat.shopTime >= currentHat.animationSpeed)
        {
            currentHat.shopTime = 0;

            currentHat.shopStep++;
            if (currentHat.shopStep >= currentHat.animatedSprites.Length)
            {
                currentHat.shopStep = 0;
            }

            hatRenderer.sprite = currentHat.animatedSprites[currentHat.shopStep];
        }
    }

    public void OverrideSprite(Sprite newSprite)
    {
        sRenderer.sprite = newSprite;
    }

    public void OverrideHatSprite(Sprite newSprite)
    {
        hatRenderer.sprite = newSprite;
    }

    public void LoadPlayerSkin(Skin newSkin, Wing newWing, bool physicsResolution = false)
    {
        Vector3 deathPos = transform.position;

        transform.position = ffHandler.playerStartPos;
        deadParent.transform.position = ffHandler.playerStartPos;

        //Debug.Log(deadChilds.Count + " " + deadChildTransform.Length);

        for (int i = 0; i < deadChilds.Count; i++)
        {
            Destroy(deadChilds[i]);
        }
        deadChilds.Clear();

        //Info:
        //Supported Body: 128x128Px
        //Supported Animation: n Step (Source (n * 128) * 128Px) 
        Texture2D sourceBody = newSkin.sprite.texture;//Resources.Load<Texture2D>("Sprites/Flatterfogel/player/" + id.ToString());
        Sprite bodySprite = Sprite.Create(sourceBody, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

        sRenderer.sprite = bodySprite;
        sRenderer.color = Color.white;

        Sprite[] animationSprites; //new Sprite[3]; //Resources.LoadAll<Sprite>("Sprites/Flatterfogel/player/Wings/" + wingID.ToString());
        
        if(newSkin.overrideWing != null)
        {
            currentWing = newSkin.overrideWing;
            newWing = newSkin.overrideWing;
        }

        animationSprites = newWing.sprite;
        
        anSprites = animationSprites;

        sourceBody.filterMode = FilterMode.Point;

        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = animationSprites[1];

        //Schritt 2:
        //Erstelle Death-Sprite mit mittlerer Animation-step

        Texture2D deathTexture = 
            ImageHelpers.AlphaBlend(sourceBody, 
            ImageHelpers.CroppedTextureFromSprite(animationSprites[newWing.middleID]));

        if(!newSkin.wingSupport)
        {
            deathTexture = sourceBody;
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        } else
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        }

        deathTexture.filterMode = FilterMode.Point;

        //Schritt 3:
        //Erstellt alle Spriteparts von Deathtexture und weist sie den death-objs zu

        int resolution = 4; //4*4 = 16 Parts

        if(physicsResolution)
        {
            resolution = 8; //8*8 = 64 Parts
        }

        Vector3 startPos = transform.position;
        startPos.x -= 37.5f; //37.5 weil hälfte von 75 (breite + höhe) = nullpunkt links unten
        startPos.y -= 37.5f;

        float offset = ((75f / resolution) / 2f);
        float width = offset * 2f;

        int pixelOffset = 128 / resolution;

        startPos.x += offset; //75 / 8 Parts = 9.375f / 2 weil mitte = 4.6875f
        startPos.y += offset;

        for(int y = 0; y < resolution; y++)
        {
            for(int x = 0; x < resolution; x++)
            {
                Sprite newSprite = 
                    Sprite.Create(deathTexture, 
                                    new Rect(x * pixelOffset, y * pixelOffset, pixelOffset, pixelOffset), 
                                    new Vector2(0.5f, 0.5f));

                //hole textur von sprite des deadchildobjs
                Texture2D sPart = ImageHelpers.CroppedTextureFromSprite(newSprite);

                if(!ImageHelpers.IsTransparent(sPart))
                { //Wenn nicht transparent -> deadchild erstellen

                    GameObject newDeathPart = Instantiate(deadPlayerPart,
                                    new Vector3(startPos.x + (width * x),
                                                startPos.y + (width * y)),
                                    Quaternion.identity, deadPlayerParent);

                    newDeathPart.GetComponent<SpriteRenderer>().sprite = newSprite;
                    newDeathPart.GetComponent<BoxCollider2D>().size = new Vector2(width / 58.5f, width / 58.5f);

                    newDeathPart.GetComponent<DeadData>().originalPos = newDeathPart.transform.position;

                    deadChilds.Add(newDeathPart);
                }
            }
        }

        if(dead)
        { //wenn tot physik aktivieren & position setzen
            transform.position = deathPos;
            deadParent.transform.position = deathPos;
            for (int i = 0; i < deadChilds.Count; i++)
            {
                deadChilds[i].GetComponent<Rigidbody2D>().simulated = true;
            }
        }

        if(currentSkin.animated)
        {
            animationRunning = true;
        }
    }

    public void LoadPipe()
    {
        Pipe currentPipe = ShopHandler.Instance.GetCurrentPipe();

        Sprite fullDefault = 
            currentPipe.sprite[Random.Range(0, currentPipe.sprite.Length)];
        Sprite endSprite = 
            currentPipe.endSprite[Random.Range(0, currentPipe.endSprite.Length)];

        Texture2D fDT = fullDefault.texture;
        Texture2D fDT_End = endSprite.texture;

        //Sprite w + h MUSS durch 4 teilbar sein
        int width = fDT.width / 4;
        int height = fDT.height / 4;

        int widthEnd = fDT_End.width / 4;
        int heightEnd = fDT_End.height / 4;

        for(int y = 0; y < 4; y++)
        {
            for(int x = 0; x < 4; x++)
            {
                pipeSprite[y,x].sprite = 
                    Sprite.Create(fDT, new Rect(0 + (x * width), 0 + (y * height), width, height), 
                    new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect);

                if(ImageHelpers.IsTransparent(pipeSprite[y,x].sprite.texture)) {
                    pipeSprite[y, x].isEmpty = true;
                    Debug.Log(true + " " + y + " " + x);
                } else
                {
                    pipeSprite[y, x].isEmpty = false;
                }

                pipeEndSprite[y,x].sprite =
                    Sprite.Create(fDT_End, new Rect(0 + (x * widthEnd), 0 + (y * heightEnd), widthEnd, heightEnd),
                    new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect);

                if (ImageHelpers.IsTransparent(pipeEndSprite[y, x].sprite.texture))
                {
                    pipeEndSprite[y, x].isEmpty = true;
                }
                else
                {
                    pipeEndSprite[y, x].isEmpty = false;
                }
            }
        }
    }

    public void ResetRotation()
    {
        GetComponent<Rigidbody2D>().rotation = 0;
        transform.rotation = Quaternion.identity;
        transform.GetChild(0).rotation = Quaternion.identity;
        EnableDisableWings(true);
    }

    private void StartStaminaFlash(string text)
    {
        staminaFlashing = true;
        staminaText.GetComponent<TextMeshProUGUI>().text = text;
        InvokeRepeating(nameof(StaminaFlash), 0f, 0.5f);
    }

    private void StaminaFlash()
    {
        if(staminaText.activeSelf)
        {
            staminaText.SetActive(false);
        } else
        {
            staminaText.SetActive(true);
        }
    }

    private void StopStaminaFlash()
    {
        staminaFlashing = false;
        staminaText.SetActive(false);
        CancelInvoke(nameof(StaminaFlash));
    }

    public void SetWingAnimation(int step)
    {
        if (dead) return;

        if ((FlatterFogelHandler.gameState == 0 && FlatterFogelHandler.state == 1) || 
            (FlatterFogelHandler.gameState == 1 && !isGrounded))
        {
            wingRenderer.sprite = anSprites[step];
        }

        if(FlatterFogelHandler.gameState == 2 &&
            !landing &&
            MineHandler.Instance.miningActive)
        { //mine-modus
            if (currentFuel > 0)
            {
                currentFuel -= 1;

                if (currentFuel < 0) currentFuel = 0;

                float val = UpdateFuelSlider();

                if (val <= 0.25f && !staminaFlashing)
                {
                    //StartStaminaFlash("Landen!");
                }
            }
            else
            { //stamina alle
                Die(DeathCause.FuelEmpty);
            }
        }
    }

    public void EndMine()
    {
        
        if(heatRoutine != null)
        {
            StopCoroutine(heatRoutine);
        }

        EnableDisableWings(true);
        StopStaminaFlash();
    }

    public void ResetMine(bool show = false)
    {
        currentFuel = maxFuel;
        currentHeat = 0;

        UpdateFuelSlider();
        UpdateHeatSlider();

        //fuelSlider.gameObject.SetActive(show);
        fuelParent.SetActive(show);
        mineItemParent.SetActive(show);
    }

    public void AddFuel(int amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0, maxFuel);
    }

    private float UpdateHeatSlider()
    {
        float max = maxHeat;

        float val = (float)currentHeat / max;
        heatSlider.value = val;

        if (val < 0.2f)
        { //dunkelgrün
            heatColorImage.color = new Color32(0, 127, 0, 255);
        }
        else if (val < 0.4f)
        { //hellgrün
            heatColorImage.color = new Color32(0, 255, 0, 255);
        }
        else if (val < 0.6f)
        { //gelb
            heatColorImage.color = new Color32(255, 255, 0, 255);
        }
        else if (val < 0.8f)
        { //orange
            heatColorImage.color = new Color32(255, 165, 0, 255);
        }
        else
        { //rot
            heatColorImage.color = new Color32(255, 0, 0, 255);
        }

        int num = (int)(10f * val);

        if (num > 9) num = 9;

        heatText.text = num.ToString();

        return val;
    }

    private float UpdateFuelSlider()
    {
        int max = maxFuel;

        float val = (float)currentFuel / max;
        fuelSlider.value = val;

        if (val > 0.8f)
        { //dunkelgrün
            fuelColorImage.color = new Color32(0, 127, 0, 255);
        }
        else if (val > 0.6f)
        { //hellgrün
            fuelColorImage.color = new Color32(0, 255, 0, 255);
        }
        else if (val > 0.4f)
        { //gelb
            fuelColorImage.color = new Color32(255, 255, 0, 255);
        }
        else if (val > 0.2f)
        { //orange
            fuelColorImage.color = new Color32(255, 165, 0, 255);
        }
        else
        { //rot
            fuelColorImage.color = new Color32(255, 0, 0, 255);
        }

        int num = (int)(10f * val);

        if (num > 9) num = 9;

        fuelText.text = num.ToString();

        return val;
    }

    public void SetGroundHeight(int height)
    {
        groundHeight = height;
    }

    public int GetGroundHeight()
    {
        return groundHeight;
    }

    private void GoDead()
    {
        goLocked = false;

        for (int i = 0; i < deadChilds.Count; i++)
        {
            deadChilds[i].GetComponent<Rigidbody2D>().simulated = true;

            deadChilds[i].GetComponent<SpriteRenderer>().color = Color.white;

            Behaviour coll = deadChilds[i].GetComponent<BoxCollider2D>();
            if (coll == null)
            {
                coll = deadChilds[i].GetComponent<PolygonCollider2D>();
            }

            coll.enabled = true;
        }

        deadParent.SetActive(false);

        deadParent.transform.position = ffHandler.playerStartPos;
        deadParent.transform.rotation = Quaternion.identity;

        for(int i = 0; i < deadChilds.Count; i++)
        {
            deadChilds[i].GetComponent<DeadData>().ResetPos();
        }

        dead = false;
        PlayerGo();
    }

    public void PlayerGo(bool full = false)
    {
        if(goLocked)
        {
            return;
        }

        if(dead)
        { //um herauszufinden ob bereits runde gespielt -> pos reset
            goLocked = true;

            ResetPos();
            Invoke(nameof(GoDead), 0.75f);
            return;
        }

        dead = false;

        Vector3 pos = transform.position;
        pos.y += 31.005f;

        hatObj.transform.position = pos;
        hatObj.GetComponent<Rigidbody2D>().simulated = false;
        hatObj.transform.rotation = Quaternion.identity;

        float yDiff = currentHat.yDist;

        Vector3 hPos = transform.position;
        hPos.y += currentSkin.hatStart + yDiff; //18.662 = hat start

        hatObj.transform.position = hPos;

        yDiff = currentWing.xDist;

        Vector3 wPos = transform.position;
        wPos.y += currentSkin.wingStart + yDiff;

        wing.transform.position = wPos;

        playerLight.SetActive(true);
        playerCollider2.SetActive(true);
        transform.parent.GetChild(0).gameObject.SetActive(true); //enable itemholder

        GetComponent<CircleCollider2D>().isTrigger = true;
        GetComponent<BoxCollider2D>().isTrigger = true;

        if(!currentSkin.boxCollider)
        {
            GetComponent<CircleCollider2D>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = false;
        } else
        {
            GetComponent<BoxCollider2D>().offset = currentSkin.colliderOffset;
            GetComponent<BoxCollider2D>().size = currentSkin.colliderSize;

            GetComponent<CircleCollider2D>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = true;
        }

        groundHeight = 0;

        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();

        rb2D.velocity = Vector2.zero;

        rb2D.simulated = true;
        GetComponent<SpriteRenderer>().enabled = true;

        if (!FlatterFogelHandler.Instance.zigZag)
        {
            if (full)
            {
                rb2D.constraints = RigidbodyConstraints2D.FreezePositionX;
                rb2D.gravityScale = defaultGravityScale;
            }
            else
            {
                rb2D.gravityScale = 0;
                //animator play einbauen
            }
        } else
        {
            rb2D.gravityScale = 0;
        }
    }

    public void ResetGravityScale()
    {
        GetComponent<Rigidbody2D>().gravityScale = defaultGravityScale;
    }

    private void ResetImage()
    {
        transform.position = ffHandler.playerStartPos;

        GetComponent<SpriteRenderer>().enabled = true;
        if(currentSkin.wingSupport)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        }

        transform.Rotate(new Vector3(0, 0, 45));
    }

    private void ResetPos()
    {
        for(int i = 0; i < deadChilds.Count; i++)
        {
            deadChilds[i].GetComponent<Rigidbody2D>().simulated = false;

            Behaviour coll = deadChilds[i].GetComponent<BoxCollider2D>();
            if(coll == null)
            {
                coll = deadChilds[i].GetComponent<PolygonCollider2D>();
            }

            coll.enabled = false;

            deadChilds[i].transform.DOMove(
                deadChilds[i].GetComponent<DeadData>().originalPos, 0.7f);
            deadChilds[i].transform.DORotate(new Vector3(0, 0, 0), 0.7f);
        }

        hatObj.GetComponent<Rigidbody2D>().simulated = false;

        float yDiff = currentHat.yDist;

        Vector3 hPos = ffHandler.playerStartPos;
        hPos.y += currentSkin.hatStart + yDiff;

        hatObj.transform.DOMove(hPos, 0.7f);
        hatObj.transform.DORotate(new Vector3(0, 0, 0), 0.7f);

        Invoke(nameof(ResetImage), 0.7f);
    }

    public void PlayerFly(Vector3 mPos, bool zigZag, bool release = false)
    {
        Transform pITransform = FlatterFogelHandler.Instance.pauseImage.transform;

        if(dead || !GetComponent<Rigidbody2D>().simulated || landing ||
           (mPos.x > (pITransform.position.x - 37.5f) &&
            mPos.y > (pITransform.position.y - 37.5f)) ||
            transform.position.y > 1730)
        {
            return;
        }

        /*float maxY = 1760;

        if(FlatterFogelHandler.Instance.destructionMode)
        {
            maxY = 1337;
        }

        if(!OptionHandler.destructionTransition)
        {
            if (transform.position.y > maxY)
            {
                if (FlatterFogelHandler.Instance.destructionMode)
                {
                    OptionHandler.Instance.DestructionEnlarge();
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (OptionHandler.destructionEnlargeActive)
                {
                    OptionHandler.Instance.DestructionReduce();
                }
            }
        }*/

        if(!zigZag && !release)
        {
            SoundManager.Instance.PlaySound(SoundManager.Sound.Jump);

            Vector2 velocity = new Vector3(0, 800f);
            GetComponent<Rigidbody2D>().velocity = velocity;

            //Check nicht nötig da in funktion ausgeführt
            BossHandler.Instance.PlayerShoot();
            ffHandler.PlayerShoot();

            GameObject tap =
                ObjectPooler.Instance.SpawnFromPool("TapEffect", new Vector3(mPos.x, mPos.y, 501), Quaternion.identity);
            //ParticleSystem.CollisionModule cM = tap.GetComponent<ParticleSystem>().collision;
            //cM.SetPlane(0, bottomPlane);

            HandleRotation();
        } else
        {
            if(!release)
            { //startpunkt setzen
                zigZagHandler.StartTouch(mPos);
            } else
            { //endpunkt setzen und spieler bewegen
                zigZagHandler.EndTouch(mPos);
            }
        }
    }

    public void EnableDisableWings(bool enable)
    {
        if(enable)
        {
            wing.transform.rotation = Quaternion.identity;
            if(currentSkin.wingSupport)
            {
                wing.GetComponent<SpriteRenderer>().enabled = true;
            }

            minerTool.SetActive(false);

            GetComponent<SpriteRenderer>().flipX = false;
            transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
        } else
        {
            wing.GetComponent<SpriteRenderer>().enabled = false;
            minerTool.SetActive(true);
        }
    }
    public void HandleRotation()
    {
        if(landing)
        {
            return;
        }
        Rigidbody2D pRB = GetComponent<Rigidbody2D>();

        Vector2 vel = pRB.velocity;
        float rotation = (vel.y / 800) * 45;
        if (rotation < -45)
        {
            rotation = -45;
        }
        pRB.DORotate(rotation, 0.05f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "BigStone":
                if(collision.gameObject.transform.parent.GetComponent<MineData>().deathActive)
                {
                    Die(DeathCause.Crushed);
                }
                break;
        }
    }

    public void CollisionEnter2D(Collision2D collision)
    {
        if(!dead)
        {
            switch(collision.gameObject.tag)
            {
                case "D2DObj":
                    HandleCollision(collision.gameObject);
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!dead)
        {
            HandleCollision(collision.gameObject);
        }
    }

    private void HandleCollision(GameObject collObj)
    {
        switch (collObj.tag)
        {
            case "Blus":
                GameObject blus = collObj;

                lastBlusPosition = blus.transform.position;

                float yDiff = Mathf.Abs(lastBlusPosition.y - transform.position.y);

                float min = 55;

                if (FlatterFogelHandler.Instance.zigZag)
                {
                    min = 65;
                }

                if(tutHandler.mainTut == 0 && tutHandler.mainTutStep < 2)
                { //am anfang kein perfekten treffer erlauben
                    yDiff = 0;
                }

                bool pHit = false;

                if (yDiff > min)
                {
                    FlatterFogelHandler.nextCompleteDestruction = false;
                    ffHandler.SetScore(1, 1, 2, blus);
                }
                else
                { //näher dran -> mehr punkte
                    pHit = true;

                    FlatterFogelHandler.nextCompleteDestruction = true;
                    FlatterFogelHandler.Instance.AddPerfectHit();

                    ffHandler.SetScore(2, 1, 3, blus);

                    GameObject infoText =
                        ObjectPooler.Instance.SpawnFromPool("InfoText", transform.position, Quaternion.identity);

                    infoText.GetComponent<InfoText>().StartFlashing(ScoreHandler.Instance.perfectHitString);
                }

                if (TutorialHandler.Instance.mainTut == 0)
                {
                    if (blus.transform.parent.GetComponent<BlusData>().isCoin)
                    {
                        TutorialHandler.Instance.StartCoinGreat();
                    }
                    else
                    {
                        if (TutorialHandler.Instance.mainTutStep < 2)
                        {
                            TutorialHandler.Instance.StartMainTutGreat2();
                        }
                        else if (TutorialHandler.Instance.mainTutStep == 3)
                        { //perfect hit check
                            if (pHit)
                            {
                                TutorialHandler.Instance.StartRealPerfectHit();
                            }
                            else
                            {
                                TutorialHandler.Instance.StartAlmostHit();
                            }
                        }
                    }
                }

                //if(blus.transform.parent.GetComponent<BlusData>().isCoin)
                //{
                //    ShopHandler.Instance.UpdateBlus(1, 1, true);
                //}

                if (blus.transform.parent.GetComponent<BlusData>().modeChangeBlus)
                { //neuer Modus
                    ffHandler.ChangeMode();
                }
                else
                {
                    ffHandler.modeCurrentlyChanged = false;
                }

                break;
            case "Coin":
                GameObject coin = collObj.transform.parent.gameObject;
                coin.GetComponent<CoinHandler>().DestroyCoin();

                break;
            case "FF_Pipe":
            case "FF_PipeEnd":
            case "FF_World":
            case "FF_WorldGround":
            case "DestructablePipe":
            case "D2DObj":
                if (!ffHandler.gameActive ||
                    SROptions.Current.IgnorePipes ||
                    FlatterFogelHandler.Instance.miningMode)
                {
                    return;
                }

                if (ffHandler.tutHandler.mainTut == 0)
                {
                    bool isPipe = false;

                    if (collObj.CompareTag("FF_Pipe"))
                    { //pipe weg teleportieren
                        isPipe = true;

                        collObj.transform.parent.GetComponent<PipeHolder>().
                            GetAssignedBlus().transform.Translate(1250, 0, 0);

                        collObj.transform.parent.GetChild(0).Translate(1250, 0, 0);
                        collObj.transform.parent.GetChild(1).Translate(1250, 0, 0);
                    }
                    else if (collObj.CompareTag("FF_PipeEnd"))
                    {
                        isPipe = true;

                        collObj.transform.parent.parent.GetComponent<PipeHolder>().
                            GetAssignedBlus().transform.Translate(1250, 0, 0);

                        collObj.transform.parent.parent.GetChild(0).Translate(1000, 0, 0);
                        collObj.transform.parent.parent.GetChild(1).Translate(1000, 0, 0);
                    }

                    if (isPipe)
                    {
                        ffHandler.tutHandler.StartPipeHit();
                    }
                }
                else
                {
                    Die(DeathCause.Collision);
                }

                break;
            case "Spike":

                if (SROptions.Current.IgnoreMinus)
                {
                    return;
                }

                Die(DeathCause.Minus);

                break;
            case "Minus":

                bool ok = false;

                if (MieserHandler.Instance != null)
                {
                    if (!MieserHandler.Instance.IsDead())
                    {
                        ok = true;
                    }
                }

                if (ShootingPipeHandler.Instance.shootingPipesActive ||
                    !ShootingPipeHandler.Instance.endComplete)
                {
                    ok = true;
                }

                if(FlatterFogelHandler.Instance.destructionMode)
                {
                    ok = true;
                }

                if (ok)
                {
                    GameObject minus = collObj;
                    minus.GetComponent<MinusHandler>().Explode();

                    Die(DeathCause.MieserMinus);
                }

                break;
            case "Laser":

                if (!MieserHandler.Instance.IsDead())
                {
                    Die(DeathCause.MieserLaser);
                }

                break;
        }
    }

    /*public bool CheckStamina()
    {
        bool ok = false;

        if(stamina / (float)maxStaminaF <= 0.2f)
        { //ok
            ok = true;
            StartMining();
        } else
        {
            Die(DeathCause.Ground);
        }

        return ok;
    }*/

    public void StartMining()
    {
        /*GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);*/

        transform.parent.GetChild(0).gameObject.SetActive(false); //disable itemholder

        landing = true;
        StopStaminaFlash();

        fuelParent.SetActive(true);
        mineItemParent.SetActive(true);

        if(OptionHandler.kreuzPos == 1)
        { //rechts
            mineItemParent.transform.position = new Vector3(-381, 768, -300);
        } else
        { //links
            mineItemParent.transform.position = new Vector3(41, 768, -300);
        }

        heatRoutine = StartCoroutine(HandleHeat());

        GetComponent<CircleCollider2D>().isTrigger = false;
        GetComponent<BoxCollider2D>().isTrigger = false;

        //GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();

        DOTween.To(() => rb2D.velocity, x => rb2D.velocity = x, new Vector2(0, 0), 0.2f);

        //transform.DOMoveY(263.6f, 0.2f);
        Invoke(nameof(EndMineLanding), 0.2f);
    }

    private void EndMineLanding()
    {
        GetComponent<Rigidbody2D>().simulated = true;
        landing = false;
    }

    public void Die(DeathCause type)
    {
        if (dead)
        {
            Debug.LogWarning("Second Death invoked: " + type);
            return;
        }

        hatObj.GetComponent<Rigidbody2D>().simulated = true;

        BackgroundHandler.Instance.EnableBackground(true);

        dead = true;

        StopStaminaFlash();

        if(FlatterFogelHandler.Instance.miningMode)
        {
            MineHandler.Instance.EndMine();
        }

        SoundManager.Instance.PlaySound(SoundManager.Sound.Die);

        //GetComponent<Animator>().SetTrigger("Stop");
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        GetComponent<Rigidbody2D>().simulated = false;

        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        GetComponent<SpriteRenderer>().enabled = false;

        //fuelSlider.gameObject.SetActive(false);
        fuelParent.SetActive(false);
        mineItemParent.SetActive(false);

        deadParent.transform.position = new Vector3(transform.position.x, transform.position.y, -36);
        deadParent.transform.rotation = transform.rotation;

        deadParent.SetActive(true);

        Vector2 oldVel = GetComponent<Rigidbody2D>().velocity;

        oldVel.x = 100;

        string deathReason = "";

        FirebaseAnalytics.LogEvent("Death", "ID", (int)type);

        long score = (long)(ulong)ffHandler.GetScore();

        if (!ffHandler.battleRoyale && !ffHandler.destructionMode && !ffHandler.miningMode
            && !ffHandler.zigZag && !ffHandler.hardcore)
        { //classic, ja ist behindert i know
            FirebaseAnalytics.LogEvent("Result_Classic", "Score", score);
        } else if(ffHandler.destructionMode)
        {
            FirebaseAnalytics.LogEvent("Result_Destruction", "Score", score);
        } else if(ffHandler.miningMode)
        {
            FirebaseAnalytics.LogEvent("Result_Mining", "Score", score);
        }

        switch(type)
        {
            case DeathCause.Collision:
                deathReason = collisionString;
                break;
            case DeathCause.OutOfWorld:
                deathReason = outOfWorldString;
                break;
            case DeathCause.Minus:
                //oldVel.x = 250;
                deathReason = minusString; //"MINUSKOLLISION";
                break;
            case DeathCause.Ground:
                deathReason = groundCollisionString; //"BODENKOLLISION";
                break;
            case DeathCause.FuelEmpty:
                deathReason = fuelEmptyString; //"BLUSSIZIN ALLE";
                break;
            case DeathCause.Heatshield:
                deathReason = overheatedString; //"ÜBERHITZT";
                break;
            case DeathCause.MieserLaser:
                deathReason = vaporizedString; //"VERDAMPFT";
                break;
            case DeathCause.MieserMinus:
                deathReason = minusString; //"WEGGEMINUST";
                break;
            case DeathCause.Crushed:
                deathReason = crushedString; //"ZERQUETSCHT";
                break;
            case DeathCause.Burnt:
                deathReason = burntString; //"VERBRANNT";
                break;
            case DeathCause.Suicide:
                deathReason = suicideString; //"SELBSTMORD";
                break;
            default:
                deathReason = "Undefined: " + ((int)type).ToString();
                break;
        }

        deathText.text = deathReason;

        for (int i = 0; i < deadChilds.Count; i++)
        {
            deadChilds[i].GetComponent<SpriteRenderer>().color = sRenderer.color;
            deadChilds[i].GetComponent<Rigidbody2D>().simulated = true;
            deadChilds[i].GetComponent<Rigidbody2D>().velocity = oldVel;
        }

        BossHandler.Instance.StopBoss();

        if(!OptionHandler.hardcoreActive)
        {
            playerLight.SetActive(false);
        }

        playerCollider2.SetActive(false);
        ffHandler.PlayerDeath();
    }

    public void EnableDisableHardcore(bool enable)
    {
        if(enable)
        {
            playerLightObj.enabled = false;
            playerHardcoreLightObj.enabled = true;
        } else
        {
            playerLightObj.enabled = true;
            playerHardcoreLightObj.enabled = false;
        }
    }

    bool IsPointInRT(Vector3 point, Transform t, float width, float height)
    {

        // Get the left, right, top, and bottom boundaries of the rect
        float leftSide = t.position.x - width / 2;
        float rightSide = t.position.x + width / 2;
        float topSide = t.position.y + height / 2;
        float bottomSide = t.position.y - height / 2;

        //Debug.Log(leftSide + ", " + rightSide + ", " + topSide + ", " + bottomSide);

        // Check to see if the point is in the calculated bounds
        if (point.x >= leftSide &&
            point.x <= rightSide &&
            point.y >= bottomSide &&
            point.y <= topSide)
        {
            return true;
        }
        return false;
    }

    public bool IsInBounds(Vector2 pos, RectTransform transform, int dir)
    {
        bool ok = false;

        if (pos.x > transform.position.x - transform.rect.width / 2 &&
            pos.x < transform.position.x + transform.rect.width / 2)
        {
            if (pos.y > transform.position.y - transform.rect.height / 2 &&
                pos.y < transform.position.y + transform.rect.height / 2)
            {
                ok = true;
            }
        }

        return ok;
    }

    private void _MainUpdate()
    {
        if (FlatterFogelHandler.gamePaused) return;

        itemHolder.transform.position = transform.position;

        Touch[] touches = Input.touches;

        bool holdingDown = false;
        bool touchOK = false;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButton(0))
        { //check ob grounded
            touchOK = true;
#else
        if(touches.Length > 0)
        {
            if(touches[0].phase == TouchPhase.Began) touchOK = true;
#endif
            holdingDown = true;
        }

        if(animationRunning)
        {
            HandleAnimation();
        }

        if(hatAnimationRunning)
        {
            HandleHatAnimation();
        }

        if (FlatterFogelHandler.gameState != 2)
        {
            if (!touchOK) holdingDown = false;
        }

        if(!holdingDown)
        {
            if(mineMovementPrent.gameObject.activeSelf)
            {
                mineMovementPrent.gameObject.SetActive(false);
            }
        }

        if (FlatterFogelHandler.gameState == 1)
        { //Platformer-Mode

            if(dead)
            {
                return;
            }

            if(transform.position.y < 92)
            { //aus welt gefallen
                Die(DeathCause.OutOfWorld);
            }

            if(holdingDown)
            { //check ob grounded
                Collider2D[] colliders = 
                    Physics2D.OverlapCircleAll(transform.position, transform.GetComponent<RectTransform>().sizeDelta.y + 100);

                for(int i = 0; i < colliders.Length; i++)
                {
                    if(colliders[i].gameObject != null)
                    {
                        if (colliders[i].gameObject.CompareTag("Ground")) {
                            isGrounded = true;
                            break;
                        }
                    }
                }
            }

            if (holdingDown)
            { //check ob grounded
                if (!isJumping && isGrounded)
                {
                    isGrounded = false;
                    isJumping = true;
                    jumpTime = 0;

                    ffHandler.UpdateTap();
                } else if(isJumping)
                {
                    jumpTime += Time.deltaTime;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0, 750f);

                    if(jumpTime > 0.25f)
                    { //max jump time erreicht
                        isJumping = false;
                    }
                }
            } else
            {
                isJumping = false;
            }
        } else if(FlatterFogelHandler.gameState == 2)
        { //mining
            if(holdingDown && !playerMiner.IsMining() && //hier war mousebutton nicht down
                (OptionHandler.mineMode == 0 || OptionHandler.mineMode == 2) && FlatterFogelHandler.Instance.miningMode &&
                FlatterFogelHandler.Instance.gameActive && !FlatterFogelHandler.gamePaused &&
                !FlatterFogelHandler.clicked)
            {
                Vector3 pos;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                pos = Input.mousePosition;
#else
                pos = touches[0].position;
#endif
                pos.z = 500f;

                pos = mainCamera.ScreenToWorldPoint(pos);
                //-381 mitte

                bool ok = false;
                //pos.y += 790 - mainCamera.transform.position.y;

                if(!mineMovementPrent.gameObject.activeSelf && 
                    OptionHandler.mineMode == 2)
                {
                    Vector3 newPos = pos;
                    newPos.z = 0;

                    mineMovementPrent.gameObject.SetActive(true);
                    mineMovementPrent.position = newPos;
                }

                RectTransform left, right, up, down;
                left = mineMovementPrent.transform.GetChild(3).GetComponent<Image>().rectTransform;
                right = mineMovementPrent.transform.GetChild(1).GetComponent<Image>().rectTransform;
                up = mineMovementPrent.transform.GetChild(0).GetComponent<Image>().rectTransform;
                down = mineMovementPrent.transform.GetChild(2).GetComponent<Image>().rectTransform;

                if (OptionHandler.mineMode == 0)
                { //halten
                    if (pos.x < -483)
                    { //links
                        ok = true;

                        PlayerMineDir(3);
                    }

                    if (!ok)
                    { //runter
                        if (pos.x >= -483 && pos.x < -279)
                        {
                            ok = true;

                            PlayerMineDir(2);
                        }
                    }

                    if (!ok)
                    { //rechts
                        if (pos.x >= -279)
                        {
                            ok = true;

                            PlayerMineDir(1);
                        }
                    }
                }
                else if (OptionHandler.mineMode == 2)
                {
                    if(IsInBounds(pos, right, 0))
                    {
                        PlayerMineDir(1);
                    } else if(IsInBounds(pos, down, 1))
                    {
                        PlayerMineDir(2);
                    } else if(IsInBounds(pos, left, 2))
                    {
                        PlayerMineDir(3);
                    }
                }
            }
        }
    }

    public void SwipeDetector_OnSwipe(SwipeData data)
    {
        if(FlatterFogelHandler.gameState != 2 || playerMiner.IsMining() ||
            OptionHandler.mineMode == 0 || FlatterFogelHandler.gamePaused ||
            !FlatterFogelHandler.Instance.miningMode || !FlatterFogelHandler.Instance.gameActive ||
            FlatterFogelHandler.clicked)
        {
            return;
        }

        switch(data.Direction)
        {
            case SwipeDirection.Down:
                PlayerMineDir(2);
                break;
            case SwipeDirection.Left:
                PlayerMineDir(3);
                break;
            case SwipeDirection.Right:
                PlayerMineDir(1);
                break;
        }
    }

    public void PlayerMineDir(int dir)
    {
        switch(dir)
        {
            case 0: //hoch
                if (transform.position.y < 273)
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                    transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;

                    GetComponent<PlayerMiner>().ChangeDir(0);
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0, 500f);
                }
                break;
            case 3: //links
                bool result = MineHandler.Instance.HandleDir(3);

                //if(result)
                //{
                GetComponent<SpriteRenderer>().flipX = true;
                transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
                GetComponent<PlayerMiner>().ChangeDir(3);
                //}
                break;
            case 2: //runter
                result = MineHandler.Instance.HandleDir(2);

                if (result)
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                    transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
                    GetComponent<PlayerMiner>().ChangeDir(2);
                }
                break;
            case 1: //rechts
                result = MineHandler.Instance.HandleDir(1);

                //if(result)
                //{
                GetComponent<SpriteRenderer>().flipX = false;
                transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
                GetComponent<PlayerMiner>().ChangeDir(1);
                //}
                break;
        }
    }

    public bool IsDownPressed()
    {
        Vector3 pos;

        //bool touchOK = false;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        pos = Input.mousePosition;
        //touchOK = true;
#else
        Touch[] touches = Input.touches;
        pos = touches[0].position;
#endif
        pos.z = 500f;

        pos = mainCamera.ScreenToWorldPoint(pos);
        //-381 mitte

        pos.y += 790 - mainCamera.transform.position.y;

        if (pos.x >= -653 && pos.x <= -578 && pos.y >= 162.5f && pos.y <= 237.5f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
