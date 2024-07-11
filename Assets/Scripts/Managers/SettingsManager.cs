using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    public static bool IsSFXEnabled = true;
    public static bool IsBGMEnabled = true;
    public static bool IsVibrationEnabled = true;

    public string Theme;
    public string ThemeFetch = "Number";

    [SerializeField] Button _sfxButton, _bgmButton, _vibrationButton;
    [SerializeField] TextMeshProUGUI _sfxStatus, _bgmStatus, _vibrationStatus;
    [SerializeField] Image _alphabetTheme, _numberTheme;

    private Color32 _enableColor = new(99, 231, 159, 255);
    private Color32 _disableColor = new(234, 82, 85, 255);

    public void SwitchSFX()
    {
        if (IsSFXEnabled)
        {
            IsSFXEnabled = false;
            _sfxButton.image.color = _disableColor;
            _sfxStatus.text = "OFF";

            PlayerPrefs.SetInt("SFXIsEnable", 0);
        }
        else
        {
            IsSFXEnabled = true;
            _sfxButton.image.color = _enableColor;
            _sfxStatus.text = "ON";

            PlayerPrefs.SetInt("SFXIsEnable", 1);
        }
    }

    public void SwitchBGM()
    {
        if (IsBGMEnabled)
        {
            IsBGMEnabled = false;
            _bgmButton.image.color = _disableColor;
            _bgmStatus.text = "OFF";

            PlayerPrefs.SetInt("BGMIsEnable", 0);
        }
        else
        {
            IsBGMEnabled = true;
            _bgmButton.image.color = _enableColor;
            _bgmStatus.text = "ON";

            PlayerPrefs.SetInt("BGMIsEnable", 1);
        }
    }

    public void SwitchVibration()
    {
        if (IsVibrationEnabled)
        {
            IsVibrationEnabled = false;
            _vibrationButton.image.color = _disableColor;
            _vibrationStatus.text = "OFF";

            PlayerPrefs.SetInt("VibrationIsEnable", 0);
        }
        else
        {
            IsVibrationEnabled = true;
            _vibrationButton.image.color = _enableColor;
            _vibrationStatus.text = "ON";

            PlayerPrefs.SetInt("VibrationIsEnable", 1);
        }
    }

    public void LoadButtonsState()
    {
        IsSFXEnabled = !Convert.ToBoolean(PlayerPrefs.GetInt("SFXIsEnable", 1));
        IsBGMEnabled = !Convert.ToBoolean(PlayerPrefs.GetInt("BGMIsEnable", 1));
        IsVibrationEnabled = !Convert.ToBoolean(PlayerPrefs.GetInt("VibrationIsEnable", 1));

        SwitchBGM();
        SwitchSFX();
        SwitchVibration();
    }

    public void LoadThemeState()
    {
        Theme = ThemeFetch;

        if (Theme == "Alphabet")
        {
            _alphabetTheme?.gameObject.SetActive(true);
            _numberTheme?.gameObject.SetActive(false);
        }
        else
        {
            _alphabetTheme?.gameObject.SetActive(false);
            _numberTheme?.gameObject.SetActive(true);
        }
    }
}
