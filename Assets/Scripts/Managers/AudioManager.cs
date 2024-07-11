using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource _slideSound, _mergeSound, _gameOverSound, _tapSound, _backgroundMusic;

    private void Update()
    {
        if (SettingsManager.IsBGMEnabled)
        {
            if (!_backgroundMusic.isPlaying)
            {
                _backgroundMusic.Play();
            }
        }
        else
        {
            _backgroundMusic.Stop();
        }
    }

    public void SlideSFX()
    {
        if (SettingsManager.IsSFXEnabled)
        {
            _slideSound.Play();
        }
    }

    public void MergeSFX()
    {
        if (SettingsManager.IsSFXEnabled)
        {
            _mergeSound.Play();
        }
    }

    public void GameOverSFX()
    {
        if (SettingsManager.IsSFXEnabled)
        {
            _gameOverSound.Play();
        }
    }

    public void TapSFX()
    {
        if (SettingsManager.IsSFXEnabled)
        {
            _tapSound.Play();
        }
    }

    public void Vibration()
    {
        if (SettingsManager.IsVibrationEnabled)
        {
            Handheld.Vibrate();
        }
    }
}
