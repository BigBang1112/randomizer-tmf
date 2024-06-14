using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using Moq;
using RandomizerTMF.Logic.Services;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerTMF.Logic.Tests.Integration;

public class SessionDataTests
{
    [Fact]
    public void SerializeAndDeserializeProperly()
    {
        var serialized = new SessionData();
        serialized.Rules.RequestRules.Environment = [EEnvironment.Desert];
        serialized.Rules.RequestRules.UploadedAfter = DateOnly.MaxValue;

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        serialized.Serialize(w);

        using var newMs = new MemoryStream(ms.ToArray());
        using var r = new BinaryReader(newMs);
        var deserialized = new SessionData();
        deserialized.Deserialize(r);

        serialized.ShouldCompare(deserialized, compareConfig: new() { MaxDifferences = 10 });
    }
}
