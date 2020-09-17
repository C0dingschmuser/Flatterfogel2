using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanguageItemHandler : MonoBehaviour
{
    public string languageID;

    public void Clicked()
    {
        OptionHandler.Instance.LanguageClicked(languageID);
    }
}
