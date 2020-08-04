using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using DG.Tweening;

public class ShootingPipeHandler : MonoBehaviour
{
    public static ShootingPipeHandler Instance;

    public bool shootingPipesActive = false, endComplete = true, firstPipeSpawned = false,
        firstZoomDone = false, firstZoomComplete = false, movementStopped = false,
        shootingOK = false;
    private float lastScrollSpeed;

    private CoroutineHandle mainHandle;

    private GameObject firstPipe = null;
    private Coroutine endRoutine = null;
    private Tween scrollSpeedTween = null;

    private void Awake()
    {
        Instance = this;
    }

    public void StartShootingPipes()
    {
        StartSlowdown(FlatterFogelHandler.scrollSpeed);
        firstPipe = null;

        firstPipeSpawned = false;
        firstZoomDone = false;
        shootingPipesActive = true;
        endComplete = false;
        movementStopped = false;
        shootingOK = false;

        endRoutine = StartCoroutine(EndShootingPipes(30f));
        mainHandle = Timing.RunCoroutine(Util._EmulateUpdate(_MainUpdate, this));
    }

    public void FirstPipeSpawn(GameObject pipe)
    {
        firstPipe = pipe;
        firstPipeSpawned = true;
    }

    public void StartSlowdown(float currentScrollSpeed)
    {
        lastScrollSpeed = currentScrollSpeed;
        FlatterFogelHandler.scrollSpeed = 
            FlatterFogelHandler.defaultScrollSpeed;
    }

    public void EndShootingPipes()
    {
        if(endRoutine != null)
        { //beenden falls bereits läuft
            StopCoroutine(endRoutine);
        }

        if(scrollSpeedTween != null)
        {
            scrollSpeedTween.Kill();
        }

        endRoutine = StartCoroutine(EndShootingPipes(0, false, true));

        firstPipeSpawned = false;
        firstZoomDone = false;
        shootingPipesActive = false;
        movementStopped = false;
        shootingOK = false;

        Timing.KillCoroutines(mainHandle);
    }

    private IEnumerator EndShootingPipes(float waitTime, bool fade = true, bool force = false)
    {
        if(fade)
        {
            yield return new WaitForSeconds(waitTime * 0.8f);

            shootingPipesActive = false;

            scrollSpeedTween = DOTween.To(() => FlatterFogelHandler.scrollSpeed,
                x => FlatterFogelHandler.scrollSpeed = x, lastScrollSpeed, waitTime * 0.19f);

            yield return new WaitForSeconds(waitTime * 0.2f);
        }

        FlatterFogelHandler.scrollSpeed = lastScrollSpeed;
        FlatterFogelHandler.Instance.SetInternalScore(0);

        endComplete = true;

        if(!force)
        {
            Timing.RunCoroutine(SpawnEndCoins(0.5f, 6));
        }
    }

    private IEnumerator<float> SpawnEndCoins(float time, int coins)
    {
        if (!FF_PlayerData.Instance.dead)
        { //nur ausführen wenn alive
            while (coins > 0)
            {
                Vector3 newPos = new Vector3(Random.Range(-438, -85),
                    Random.Range(223, 1052), 0);

                FlatterFogelHandler.Instance.SpawnCoin(newPos);

                coins--;
                yield return Timing.WaitForSeconds(time);
            }
        }


    }

    public void ZoomComplete()
    {
        firstZoomComplete = true;
        shootingOK = true;

        FlatterFogelHandler.scrollSpeed =
            FlatterFogelHandler.defaultScrollSpeed;
    }

    private void _MainUpdate()
    {
        if(shootingPipesActive)
        {
            if(firstPipe != null)
            {
                if (!firstZoomDone)
                {
                    if (firstPipe.transform.position.x <= 30)
                    { //begin zoomin
                        firstZoomDone = true;
                        firstZoomComplete = false;
                        movementStopped = false;

                        FlatterFogelHandler.Instance.StartZoomOnBoss(
                            firstPipe.transform.GetChild(0).GetChild(0).transform.position, 0.5f, 2f);
                    }
                }
                else if (!firstZoomComplete)
                {
                    if (firstPipe.transform.position.x <= -68 && !movementStopped)
                    { //stop movement
                        movementStopped = true;

                        FlatterFogelHandler.scrollSpeed = 0;
                    }
                }
            }
        }
    }
}
