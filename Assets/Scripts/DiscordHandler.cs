using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;
using UnityEngine.EventSystems;

public class DiscordHandler : MonoBehaviour
{
    public static DiscordHandler Instance;
    public GraphicRaycaster raycaster;

    private bool open = false;
    
    public void Setup()
    {
        Instance = this;
        open = false;
    }

    public void CheckStart()
    {
        int started = ObscuredPrefs.GetInt("Player_Discord", 0);

        if(started == 0)
        {
            if(TutorialHandler.Instance.mainTut > 0)
            {
                started = 1;
                ActivateDiscord();
                ObscuredPrefs.SetInt("Player_Discord", started);
            }
        }
    }

    public void ActivateDiscord()
    {
        open = true;

        gameObject.SetActive(true);
        raycaster.enabled = true;
    }

    public void OpenDiscord()
    {
        CloseDiscord();
        Application.OpenURL("https://discord.gg/BCmBHu8");
    }

    public void CloseDiscord()
    {
        open = false;

        raycaster.enabled = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if(open)
        {
            if(!raycaster.enabled)
            {
                raycaster.enabled = true;
            }
        }
    }
}
