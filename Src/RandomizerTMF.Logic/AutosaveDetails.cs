using GBX.NET.Engines.Game;
using TmEssentials;

namespace RandomizerTMF.Logic;

public record AutosaveDetails(TimeInt32 Time,
                              int? Score,
                              int? Respawns,
                              string? MapName,
                              string? MapEnvironment,
                              string? MapCar,
                              TimeInt32 MapBronzeTime,
                              TimeInt32 MapSilverTime,
                              TimeInt32 MapGoldTime,
                              TimeInt32 MapAuthorTime,
                              int MapAuthorScore,
                              CGameCtnChallenge.PlayMode? MapMode)
{
    public bool HasBronzeMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= MapBronzeTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Platform && Respawns <= MapBronzeTime.TotalMilliseconds)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Score >= MapBronzeTime.TotalMilliseconds);
    
    public bool HasSilverMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= MapSilverTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Platform && Respawns <= MapSilverTime.TotalMilliseconds)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Score >= MapSilverTime.TotalMilliseconds);
    
    public bool HasGoldMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= MapGoldTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Platform && Respawns <= MapGoldTime.TotalMilliseconds)
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Score >= MapGoldTime.TotalMilliseconds);

    public bool HasAuthorMedal => (MapMode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= MapAuthorTime)
        || (MapMode is CGameCtnChallenge.PlayMode.Platform && ((Respawns == 0 && Time <= MapAuthorTime) || Respawns <= MapAuthorScore))
        || (MapMode is CGameCtnChallenge.PlayMode.Stunts && Score >= MapAuthorScore);
}