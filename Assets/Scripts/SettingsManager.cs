using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static readonly string SOUND_VOLUME_KEY = "soundVolume";
    public static readonly string TEXT_SPEED_KEY = "textSpeed";
    public static readonly string AUTO_SPEED_KEY = "autoSpeed";
    public static readonly string SKIP_UNREAD_KEY = "skipUnread";

    public static readonly float SOUND_VOLUME_DEFAULT = 5.0f;
    public static readonly float TEXT_SPEED_DEFAULT = 5.0f;
    public static readonly float AUTO_SPEED_DEFAULT = 5.0f;
    public static readonly int SKIP_UNREAD_DEFAULT = 0;

    public Slider[] sliderItems;
    public Toggle checkBox;

    public static float getSoundVolume(float value)
    {
        return Mathf.Clamp(20.0f * Mathf.Log10(value * 0.1f), -80.0f, 0.0f);
    }

    public static float getTextSpeed(float value)
    {
        return 0.006f * (10.0f - value);
    }

    public static float getAutoSpeed(float value)
    {
        return 0.01f * (10.0f - value);
    }

    public static bool getSkipUnread(int value)
    {
        return (value == 1);
    }

    void Start()
    {
        sliderItems[0].SetValueWithoutNotify(PlayerPrefs.GetFloat(SOUND_VOLUME_KEY, SOUND_VOLUME_DEFAULT));
        sliderItems[1].SetValueWithoutNotify(PlayerPrefs.GetFloat(TEXT_SPEED_KEY, TEXT_SPEED_DEFAULT));
        sliderItems[2].SetValueWithoutNotify(PlayerPrefs.GetFloat(AUTO_SPEED_KEY, AUTO_SPEED_DEFAULT));
        checkBox.SetIsOnWithoutNotify(PlayerPrefs.GetInt(SKIP_UNREAD_KEY, SKIP_UNREAD_DEFAULT) == 1);
    }

    public void settingsValueChanged(int num)
    {
        switch (num)
        {
            case 0:
                PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, sliderItems[num].value);
                break;
            case 1:
                PlayerPrefs.SetFloat(TEXT_SPEED_KEY, sliderItems[num].value);
                break;
            case 2:
                PlayerPrefs.SetFloat(AUTO_SPEED_KEY, sliderItems[num].value);
                break;
            case 3:
                PlayerPrefs.SetInt(SKIP_UNREAD_KEY, checkBox.isOn ? 1 : 0);
                break;
        }        
    }
}
