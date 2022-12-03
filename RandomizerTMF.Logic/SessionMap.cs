using GBX.NET;
using GBX.NET.Engines.Game;

namespace RandomizerTMF.Logic;

public class SessionMap
{
    public CGameCtnChallenge Map { get; }
    public string TrackId { get; }
    public DateTimeOffset ReceivedAt { get; }
    public TimeSpan? LastChangeAt { get; set; }

    /// <summary>
    /// Tells when the track was first released on TMX.
    /// </summary>
    public DateTimeOffset? ReleasedAt { get; }

    public CGameCtnChallengeParameters? ChallengeParameters => Map.ChallengeParameters;
    public CGameCtnChallenge.PlayMode? Mode => Map.Mode;
    public string MapUid => Map.MapUid;
    public Ident MapInfo => Map.MapInfo;

    public SessionMap(CGameCtnChallenge map, string trackId, DateTimeOffset receivedAt)
    {
        Map = map;
        TrackId = trackId;
        ReceivedAt = receivedAt;
    }
}