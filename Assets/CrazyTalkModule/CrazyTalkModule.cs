using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public class CrazyTalkOption
{
    public String txt;
    public Int32 down;
    public Int32 up;
}

public class CrazyTalkOptions
{
    public List<CrazyTalkOption> options;
}

public class CrazyTalkModule : MonoBehaviour
{
    public TextAsset crazyTalkJson;
    public Text textDisplay;
    public Animator switchAnimator;
    public KMSelectable toggleSwitch;

    CrazyTalkOptions mOptions;
    CrazyTalkOption mOption;
    bool bSwitchState = true;
    bool bActive = false;
    KMBomb mBombInfo;
    int mCorrectSwitches = 0;

    private static int moduleIDCounter;
    private int moduleID;

    void Awake()
    {
        moduleIDCounter = 0;
    }

    void Start ()
    {
        moduleID = ++moduleIDCounter;
        switchAnimator.SetBool("IsUp", bSwitchState);
        mOptions = JsonConvert.DeserializeObject<CrazyTalkOptions>(crazyTalkJson.text);
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
