using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI GameOverTitle;
    public CanvasGroup GameOverScreen;

    public GameObject SettingsScreen;
    public GameObject LoadingScreen;

    public void GameOverActive(bool status)
    {
        GameOverScreen?.gameObject.SetActive(status);
        GameOverScreen.DOFade(status ? 1f : 0f, status ? 0.5f : 0f);

        GameOverTitle.text = "You earned " + TileBoard.Instance.ScoreText.text +
                            " points with " + TileBoard.Instance.MovesText.text + " in " + TileBoard.Instance.TimerText.text + ".";
    }

    public void LoadingActive(bool status)
    {
        LoadingScreen.SetActive(status);
    }

    public void SettingsActive(bool status)
    {
        SettingsScreen.SetActive(status);
    }
}
