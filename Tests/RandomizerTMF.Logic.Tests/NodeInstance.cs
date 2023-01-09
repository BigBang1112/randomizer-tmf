using GBX.NET;
using System.Diagnostics;

namespace RandomizerTMF.Logic.Tests;

internal static class NodeInstance
{
    public static T Create<T>() where T : Node
    {
        return (T)(Activator.CreateInstance(typeof(T), nonPublic: true) ?? throw new UnreachableException());
    }
}
