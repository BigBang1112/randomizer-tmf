using RandomizerTMF.Logic.TypeConverters;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public static class Yaml
{
    public static ISerializer Serializer { get; } = new SerializerBuilder()
        .WithTypeConverter(new DateOnlyConverter())
        .WithTypeConverter(new DateTimeOffsetConverter())
        .WithTypeConverter(new TimeInt32Converter())
        .WithTypeConverter(new Int3Converter())
        .Build();

    public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
        .WithTypeConverter(new DateOnlyConverter())
        .WithTypeConverter(new DateTimeOffsetConverter())
        .WithTypeConverter(new TimeInt32Converter())
        .WithTypeConverter(new Int3Converter())
        .IgnoreUnmatchedProperties()
        .Build();
}
