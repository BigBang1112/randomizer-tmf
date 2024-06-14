using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using RandomizerTMF.Logic.Services;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class GbxServiceTests
{
    public GbxServiceTests()
    {
        Gbx.LZO = new MiniLZO();
    }

    [Fact]
    public async Task Parse_ValidMapGbx_ReturnsFullNode()
    {
        var mapFile = "Randomizer TMF test track.Challenge.Gbx";
        var gbx = new GbxService();
        using var ms = new MemoryStream(await File.ReadAllBytesAsync(Path.Combine("Files", mapFile)));

        var node = gbx.Parse(ms);

        Assert.NotNull(node);
        Assert.IsType<CGameCtnChallenge>(node);
        Assert.NotNull(((CGameCtnChallenge)node).Blocks); // Possible indicator of full map parse
    }

    [Fact]
    public async Task Parse_ValidReplayGbx_ReturnsFullNode()
    {
        var mapFile = "petrp_Randomizer TMF test track.Replay.gbx";
        var gbx = new GbxService();
        using var ms = new MemoryStream(await File.ReadAllBytesAsync(Path.Combine("Files", mapFile)));

        var node = gbx.Parse(ms);

        Assert.NotNull(node);
        Assert.IsType<CGameCtnReplayRecord>(node);
        Assert.NotNull(((CGameCtnReplayRecord)node).Ghosts); // Possible indicator of full replay parse
    }

    [Fact]
    public async Task ParseHeader_ValidMapGbx_ReturnsFullNode()
    {
        var mapFile = "Randomizer TMF test track.Challenge.Gbx";
        var gbx = new GbxService();
        using var ms = new MemoryStream(await File.ReadAllBytesAsync(Path.Combine("Files", mapFile)));

        var node = gbx.ParseHeader(ms);

        Assert.NotNull(node);
        Assert.IsType<CGameCtnChallenge>(node);
        Assert.Null(((CGameCtnChallenge)node).Blocks); // Possible indicator of map header parse
    }

    [Fact]
    public async Task ParseHeader_ValidReplayGbx_ReturnsFullNode()
    {
        var mapFile = "petrp_Randomizer TMF test track.Replay.gbx";
        var gbx = new GbxService();
        using var ms = new MemoryStream(await File.ReadAllBytesAsync(Path.Combine("Files", mapFile)));

        var node = gbx.ParseHeader(ms);

        Assert.NotNull(node);
        Assert.IsType<CGameCtnReplayRecord>(node);
        Assert.Null(((CGameCtnReplayRecord)node).Ghosts); // Possible indicator of replay header parse
    }
}
