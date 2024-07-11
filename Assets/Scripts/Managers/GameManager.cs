using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using ByteBrewSDK;
using System;

public class GameManager : Singleton<GameManager>
{
    public bool IsLoaded;
    public bool IsFetched;

    public int Score;
    public int Moves;

    public int InitialUndos;
    public int RemainingUndos;
    public int RewardedUndos;

    public IObjectPool<Tile> _tilePool;

    public float _timeRemaining = 0;
    public bool _timeIsRunning = true;

    [SerializeField] Tile _tile;
    [SerializeField] bool _collectionCheck = true;
    [SerializeField] int _defaultCapacity = 10;
    [SerializeField] int _maxSize = 16;

    private void InitializeGame()
    {
        IsFetched = false;
        IsLoaded = false;

        ByteBrewManager.Instance.Initialize();
        FirebaseManager.Instance.Initialize();
        AppLovinManager.Instance.Initialize();

        InitializeObjectPool();
    }

    public void Start()
    {
        InitializeGame();

        UIManager.Instance.LoadingActive(true);
        SettingsManager.Instance.LoadButtonsState();

        StartCoroutine(WaitForFetching());
        StartCoroutine(WaitForLoadScene());
    }

    private IEnumerator WaitForLoadScene()
    {
        yield return new WaitUntil(() => IsLoaded);

        UIManager.Instance.LoadingActive(false);
        AppLovinManager.Instance.ShowBannerAd();
    }

    private IEnumerator WaitForFetching()
    {
        float timer = 0;

        while (timer < 3f && !IsFetched)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        SettingsManager.Instance.LoadThemeState();
        SceneManager.LoadScene("Game");
    }

    private void InitializeObjectPool()
    {
        _tilePool = new ObjectPool<Tile>(CreateTiles, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
                                               _collectionCheck, _defaultCapacity, _maxSize);
    }

    public void NewGame()
    {
        TileBoard.Instance.Caretaker._mementos.Clear();
        TileBoard.Instance.enabled = true;

        UIManager.Instance.GameOverActive(false);
        AudioManager.Instance.TapSFX();

        SetMoves(true);
        SetScore(0);
        TileBoard.Instance.HighscoreText.text = LoadHighscore().ToString();

        GameManager.Instance.LoadRemainingUndos();
        TileBoard.Instance.UndoRemainingText.text = (RemainingUndos == 0) ? "+" : RemainingUndos.ToString();

        _timeIsRunning = true;
        _timeRemaining = 0;

        TileBoard.Instance._highestIndex = 0;
        TileBoard.Instance.ClearBoard();
        TileBoard.Instance.SpawnTiles();
        TileBoard.Instance.SpawnTiles();
    }

    public void OnClickTryAgain()
    {
        AppLovinManager.Instance.ShowInterstitialAd();

        NewGame();
    }

    public void OnClickNewGame()
    {
        FirebaseManager.Instance.TapNewGameEvent(Moves);
        ByteBrewManager.Instance.TapNewGameEvent(Moves);

        NewGame();
    }

    public void UndoMove()
    {
        TileBoard.Instance.Undo();
        AudioManager.Instance.TapSFX();
    }

    public void AddUndoRemaining()
    {
        if (RemainingUndos == 0)
        {
            TileBoard.Instance.enabled = false;
            AppLovinManager.Instance.ShowRewardedAd();
            TileBoard.Instance.enabled = true;

            AudioManager.Instance.TapSFX();
        }
    }

    public void GameOver()
    {
        _timeIsRunning = false;
        TileBoard.Instance.enabled = false;
        TileBoard.Instance.Caretaker._mementos.Clear();

        UIManager.Instance.GameOverActive(true);
        AudioManager.Instance.GameOverSFX();
        AudioManager.Instance.Vibration();

        FirebaseManager.Instance.GameOverEvent(Moves, (int)_timeRemaining,
                                        SettingsManager.Instance.Theme, TileBoard.Instance._highestTile);
        ByteBrewManager.Instance.GameOverEvent(Moves, (int)_timeRemaining,
                                        SettingsManager.Instance.Theme, TileBoard.Instance._highestTile);
    }

    public void OpenSettings()
    {
        _timeIsRunning = false;
        TileBoard.Instance.enabled = false;

        TileBoard.Instance.Buttons.sortingOrder = 0;

        UIManager.Instance.SettingsActive(true);
        AudioManager.Instance.TapSFX();

        FirebaseManager.Instance.TapSettingsEvent();
        ByteBrewManager.Instance.TapSettingsEvent();
    }

    public void CloseSettings()
    {
        if (!UIManager.Instance.GameOverScreen.isActiveAndEnabled)
        {
            _timeIsRunning = true;
        }

        TileBoard.Instance.enabled = true;
        TileBoard.Instance.Buttons.sortingOrder = 1;

        UIManager.Instance.SettingsActive(false);
        AudioManager.Instance.TapSFX();
    }

    private void SaveHighscore()
    {
        if (Score > LoadHighscore())
        {
            PlayerPrefs.SetInt("highscore", Score);
        }
    }

    private int LoadHighscore()
    {
        return PlayerPrefs.GetInt("highscore", 0);
    }

    public void IncreaseScore(int points)
    {
        SetScore(Score + points);
        SaveHighscore();

        TileBoard.Instance.HighscoreText.text = LoadHighscore().ToString();
    }

    private void SetScore(int score)
    {
        Score = score;
        TileBoard.Instance.ScoreText.text = Score.ToString();
    }

    public void SaveRemainingUndos()
    {
        PlayerPrefs.SetInt("remainingundos", RemainingUndos);
    }

    public void LoadRemainingUndos()
    {
        RemainingUndos = PlayerPrefs.GetInt("remainingundos", InitialUndos);
    }

    public void SetMoves(bool New)
    {
        if (New)
        {
            Moves = 0;
            TileBoard.Instance.MovesText.text = Moves.ToString() + " move";
        }
        else
        {
            Moves++;
            TileBoard.Instance.MovesText.text = Moves.ToString() + (Moves > 1 ? " moves" : " move");
        }
    }

    private Tile CreateTiles()
    {
        var tile = Instantiate(_tile, TileBoard.Instance._grid.transform);
        tile.ObjectPool = _tilePool;

        return tile;
    }

    private void OnReleaseToPool(Tile tile)
    {
        tile.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0f);
        tile.gameObject.SetActive(false);
    }

    private void OnGetFromPool(Tile tile)
    {
        tile.gameObject.SetActive(true);
    }

    private void OnDestroyPooledObject(Tile tile)
    {
        Destroy(tile.gameObject);
    }

    public void UndoUI()
    {
        TileBoard.Instance.ScoreText.text = Score.ToString();
        TileBoard.Instance.MovesText.text = Moves.ToString() + ((Moves > 1) ? " moves" : " move");
    }
}
