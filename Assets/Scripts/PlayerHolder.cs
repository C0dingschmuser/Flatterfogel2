﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class PlayerHolder : MonoBehaviour
{
    public Image skinImage = null, wingImage = null, hatImage = null;

    [SerializeField]
    private TextMeshProUGUI nameText = null, scoreText = null;

    [SerializeField]
    private GameObject nameObj;

    public Skin skin;
    public Wing wing;
    public Hat hat;
    public Pipe pipe;
    public Color pipeColor;

    public void LoadPlayer(Skin skin, Wing wing, Hat hat, Pipe pipe, Color pipeColor, string username, ulong score, bool top = false)
    {
        if(skin.wingSupport)
        {
            wingImage.gameObject.SetActive(true);

            if (skin.overrideWing != null)
            {
                wing = skin.overrideWing;
            }
        } else
        {
            wingImage.gameObject.SetActive(false);
        }

        skinImage.sprite = skin.sprite;
        wingImage.sprite = wing.sprite[0];
        hatImage.sprite = hat.sprite;

        this.skin = skin;
        this.wing = wing;
        this.hat = hat;
        this.pipe = pipe;
        this.pipeColor = pipeColor;

        if(hat.itemID == 0)
        {
            hatImage.gameObject.SetActive(false);
        } else
        {
            hatImage.gameObject.SetActive(true);

            float yDiff = hat.yDist;

            Vector3 pos = transform.position;
            pos.y += skin.hatStart + yDiff + 37.458f;

            hatImage.transform.position = pos;
        }

        nameText.text = username;
        scoreText.text = score.ToString();

        nameText.color = Color.black;
        scoreText.color = Color.black;

        nameObj.SetActive(false);
        Timing.RunCoroutine(_EnableInfo(0.2f, top).CancelWith(this.gameObject));
    }

    public void PlayerClicked()
    {

    }

    public void SetPlayer()
    {
        nameText.color = Color.red;
        scoreText.color = Color.red;
    }

    private IEnumerator<float> _EnableInfo(float waitTime, bool top)
    {
        yield return Timing.WaitForSeconds(waitTime);

        nameObj.SetActive(true);

        if (top)
        {
            nameObj.transform.position = new Vector3(transform.position.x, 240.487f, -0.1f);
        }
        else
        {
            nameObj.transform.position = new Vector3(transform.position.x, 181.761f, -0.1f);
        }
    }
}
