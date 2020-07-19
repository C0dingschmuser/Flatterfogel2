using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchHolder : MonoBehaviour
{
    public Image sprite, rewardSprite, collectSprite;
    public Button collectButton;
    public Achievement achievement;
    public TextMeshProUGUI titleText, descriptionText, progressText, rewardText;
    public Slider progressSlider;

    public void CollectClicked()
    {
        AchievementHandler.Instance.StartSpawnAchObjs(rewardSprite.transform.position, achievement.rewards[0].amount);
    }
}
