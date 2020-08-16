using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Achievement")]
public class Achievement : ScriptableObject
{
    public string identifier;
    public Sprite[] mainSprite;
    public CostData[] rewards;
    public LocalizedString title, description;
    public MainMode unlockMode = null;
    public string titleString, descriptionString; //assigned beim laden
    public int step = 0, maxStep = 10, upgradeStep = 0, maxUpgradeSteps = 0;
    public float stepMultiplier = 2, rewardMultiplier = 2;
    public bool completed = false, rewardCollected = false, upgradable = false;

}
