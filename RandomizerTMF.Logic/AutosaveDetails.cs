using GBX.NET.Engines.Game;
using TmEssentials;

namespace RandomizerTMF.Logic;

public record AutosaveDetails(TimeInt32 Time,
                              string? MapName,
                              string? MapEnvironment,
                              TimeInt32 MapBronzeTime,
                              TimeInt32 MapSilverTime,
                              TimeInt32 MapGoldTime,
                              TimeInt32 MapAuthorTime,
                              CGameCtnChallenge.PlayMode? MapMode)
{
    public bool HasBronzeMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle or CGameCtnChallenge.PlayMode.Platform && Time <= MapBronzeTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Time >= MapBronzeTime);
    
    public bool HasSilverMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle or CGameCtnChallenge.PlayMode.Platform && Time <= MapSilverTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Time >= MapSilverTime);
    
    public bool HasGoldMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle or CGameCtnChallenge.PlayMode.Platform && Time <= MapGoldTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Time >= MapGoldTime);

    public bool HasAuthorMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle or CGameCtnChallenge.PlayMode.Platform && Time <= MapAuthorTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Time >= MapAuthorTime);
}