using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text.RegularExpressions;

// 
// Original File by Perky
// 

public class CrazyTalkOption
{
    public string txt;
    public int down;
    public int up;
}

public class CrazyTalkJSON
{
    public List<CrazyTalkOption> options;
    public string twitchHelpMsg;
}

public class CrazyModSetting
{
    public LangID language;
    public string langList;
}

///Use ISO 639-1
public enum LangID
{
    ja = 0,
    en,
    ru,
    de,
    es
}

public class CrazyTalkTranslatedModule : MonoBehaviour
{
    public List<TextAsset> crazyTalkJsons;
    public Text textDisplay;
    public Animator switchAnimator;
    public KMSelectable toggleSwitch;
    public KMModSettings modSettings;

    CrazyTalkJSON mJson;
    CrazyTalkOption mOption;
    bool bSwitchState = true;
    bool bActive = false;
    KMBomb mBombInfo;
    int mCorrectSwitches = 0;

    private static int mlangID;
    private static int moduleIDCounter;
    private int moduleID;

    void Awake()
    {
        moduleIDCounter = 0;
        mlangID = -1;
    }

    void Start ()
    {
        //LangID Setup
        if (mlangID < 0)
        {
            if (Application.isEditor)
            {
                //Change language from here
                mlangID = (int)LangID.es;
            }
            else
            {
                try
                {
                    //Load ModConfig
                    CrazyModSetting setting = JsonConvert.DeserializeObject<CrazyModSetting>(modSettings.Settings);
                    mlangID = (int)setting.language;
                }
                catch (Exception e)
                {
                    Debug.LogFormat("[Crazy Talk Translated] Error Loading ModSettings: {0}", e);
                    mlangID = (int)LangID.ja;
                }
            }
        }

        moduleID = ++moduleIDCounter;
        switchAnimator.SetBool("IsUp", bSwitchState);
        mJson = JsonConvert.DeserializeObject<CrazyTalkJSON>(crazyTalkJsons[mlangID].text);
        mOption = mJson.options[UnityEngine.Random.Range(0, mJson.options.Count)];
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        toggleSwitch.OnInteract += ToggleSwitch;

        TwitchHelpMessage = mJson.twitchHelpMsg;

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

    public string TwitchHelpMessage = "Toggle the switch down and up with !{0} toggle 4 5. The order is down, then up.";
    public IEnumerator ProcessTwitchCommand(string command)
    {
        if (!bActive)
        {
            yield break;
        }

        Match modulesMatch = Regex.Match(command, "^toggle ([0-9]) ([0-9])", RegexOptions.IgnoreCase);
        if (!modulesMatch.Success)
        {
            yield break;
        }

        int down = int.Parse(modulesMatch.Groups[1].Value);
        int up = int.Parse(modulesMatch.Groups[2].Value);

        int second;
        bool isUp;
        while (mCorrectSwitches < 2)
        {
            second = (int)Math.Floor(GetComponent<KMBombInfo>().GetTime()) % 10;
            isUp = bSwitchState;

            if ((isUp && second == down) || (!isUp && second == up))
                yield return toggleSwitch;

            else yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }
}
