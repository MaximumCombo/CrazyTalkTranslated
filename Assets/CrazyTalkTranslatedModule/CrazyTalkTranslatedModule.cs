using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

// 
// Original File by Perky
// 

public class CrazyTalkOption
{
    public string txt;
    public int down;
    public int up;
}

public class CrazyTalkOptions
{
    public List<CrazyTalkOption> options;
}

///Use ISO 639-1
public enum LangID
{
    JA = 0,
    EN,
}


public class CrazyTalkTranslatedModule : MonoBehaviour
{
    public List<TextAsset> crazyTalkJsons;
    public Text textDisplay;
    public Animator switchAnimator;
    public KMSelectable toggleSwitch;

    CrazyTalkOptions mOptions;
    CrazyTalkOption mOption;
    bool bSwitchState = true;
    bool bActive = false;
    KMBomb mBombInfo;
    int mCorrectSwitches = 0;

    private static int mlangID = -1;
    private static int moduleIDCounter;
    private int moduleID;

    void Awake()
    {
        moduleIDCounter = 0;

        if (mlangID < 0)
        {
            if (Application.isEditor)
            {
                //Change language from here
                mlangID = (int)LangID.JA;
            }
            else
            {
                //Load ModConfig
            }
        }
    }

    void Start ()
    {
        moduleID = ++moduleIDCounter;
        switchAnimator.SetBool("IsUp", bSwitchState);
        mOptions = JsonConvert.DeserializeObject<CrazyTalkOptions>(crazyTalkJsons[mlangID].text);
        mOption = mOptions.options[UnityEngine.Random.Range(0, mOptions.options.Count)];
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        toggleSwitch.OnInteract += ToggleSwitch;

        Debug.LogFormat("[Crazy Talk Translated #{0}] Phrase: \"{1}\"", moduleID, mOption.txt);
        Debug.LogFormat("[Crazy Talk Translated #{0}] Down: \"{1}\", Up: \"{2}\"", moduleID, mOption.down, mOption.up);
    }

    void OnActivate()
    {
        bActive = true;
        textDisplay.text = mOption.txt;
    }

    bool ToggleSwitch()
    {
        bSwitchState = !bSwitchState;
        bool isUp = bSwitchState;
        switchAnimator.SetBool("IsUp", isUp);

        GetComponent<KMAudio>().PlaySoundAtTransform("crazytalk_rocker_switch", transform);
        int second = (int)Math.Floor(GetComponent<KMBombInfo>().GetTime()) % 10;
        if (bActive && ((isUp && second == mOption.up) || (!isUp && second == mOption.down)))
        {
            mCorrectSwitches++;
            if (mCorrectSwitches >= 2)
                GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            mCorrectSwitches = 0;
            GetComponent<KMBombModule>().HandleStrike();
        }

        return false;
    }
}
