using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    void Start()
    {
        float value = PlayerPrefs.GetFloat(SettingsManager.SOUND_VOLUME_KEY, SettingsManager.SOUND_VOLUME_DEFAULT);
        audioMixer.SetFloat("soundVolume", SettingsManager.getSoundVolume(value));
    }

    public void setVolume(Slider slider)
    {
        audioMixer.SetFloat("soundVolume", SettingsManager.getSoundVolume(slider.value));
    }
}
