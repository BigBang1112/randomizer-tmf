using System.Text.RegularExpressions;

namespace RandomizerTMF.Logic;

internal static partial class CompiledRegex
{
    [GeneratedRegex("[^a-zA-Z0-9_.]+")]
    public static partial Regex SpecialCharRegex();
}
