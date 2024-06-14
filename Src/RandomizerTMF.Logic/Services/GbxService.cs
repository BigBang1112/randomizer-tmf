using GBX.NET;
using GBX.NET.Engines.MwFoundations;

namespace RandomizerTMF.Logic.Services;

public interface IGbxService
{
    CMwNod? Parse(Stream stream);
    CMwNod? ParseHeader(Stream stream);
}

public class GbxService : IGbxService
{
    public CMwNod? Parse(Stream stream)
    {
        return Gbx.ParseNode(stream);
    }

    public CMwNod? ParseHeader(Stream stream)
    {
        return Gbx.ParseHeaderNode(stream);
    }
}
