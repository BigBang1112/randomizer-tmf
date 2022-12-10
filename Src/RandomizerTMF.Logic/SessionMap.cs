using GBX.NET;
using GBX.NET.Engines.Game;

namespace RandomizerTMF.Logic;

/// <summary>
/// Real-time session map object
/// </summary>
public class SessionMap : ISessionMap
{
    public CGameCtnChallenge Map { get; }
    public DateTimeOffset ReceivedAt { get; }
    public TimeSpan? LastTimestamp { get; set; }

    public CGameCtnChallengeParameters? ChallengeParameters => Map.ChallengeParameters;
    public CGameCtnChallenge.PlayMode? Mode => Map.Mode;
    public string MapUid => Map.MapUid;
    public Ident MapInfo => Map.MapInfo;

    public SessionMap(CGameCtnChallenge map, DateTimeOffset receivedAt)
    {
        Map = map;
        ReceivedAt = receivedAt;
    }
}