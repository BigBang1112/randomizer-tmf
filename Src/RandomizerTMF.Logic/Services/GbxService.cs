using GBX.NET;

namespace RandomizerTMF.Logic.Services;

public interface IGbxService
{
    Node? Parse(Stream stream);
    Node? ParseHeader(Stream stream);
}

public class GbxService : IGbxService
{
    public Node? Parse(Stream stream)
    {
        return GameBox.ParseNode(stream);
    }

    public Node? ParseHeader(Stream stream)
    {
        return GameBox.ParseNodeHeader(stream);
    }
}
