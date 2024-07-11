using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ByteBrewSDK;

public class ByteBrewManager : Singleton<ByteBrewManager>
{
    public void Initialize()
    {
        ByteBrew.InitializeByteBrew();
    }

    public void TapSettingsEvent()
    {
        ByteBrew.NewCustomEvent("tap_settings");
    }

    public void TapNewGameEvent(int moves)
    {
        ByteBrew.NewCustomEvent("tap_new_game", $"moves = {moves}");
    }

    public void GameOverEvent(int moves, float time, string theme, string highestTile)
    {
        var _gameOver = new Dictionary<string, string>()
        {
            {"moves", $"{moves}"},
            {"time", $"{time}"},
            {"theme",$"{theme}"},
            {"highest_tile", $"{highestTile}"}
        };

        ByteBrew.NewCustomEvent("game_over", _gameOver);
    }
}
