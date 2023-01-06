using RandomizerTMF.Logic.TypeConverters;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public static class Yaml
{
    public static IReadOnlyCollection<IYamlTypeConverter> TypeConverters { get; } = new List<IYamlTypeConverter>
    {
        new DateOnlyConverter(),
        new DateTimeOffsetConverter(),
        new TimeInt32Converter(),
        new Int3Converter()
    };

    public static ISerializer Serializer { get; } = CreateSerializerBuilder().Build();
    public static IDeserializer Deserializer { get; } = CreateDeserializerBuilder().Build();

    private static SerializerBuilder CreateSerializerBuilder()
    {
        var builder = new SerializerBuilder();

        foreach (var typeConverter in TypeConverters)
        {
            builder = builder.WithTypeConverter(typeConverter);
        }

        return builder;
    }

    private static DeserializerBuilder CreateDeserializerBuilder()
    {
        var builder = new DeserializerBuilder()
            .IgnoreUnmatchedProperties();

        foreach (var typeConverter in TypeConverters)
        {
            builder = builder.WithTypeConverter(typeConverter);
        }

        return builder;
    }
}
