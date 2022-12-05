using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic.TypeConverters;

internal sealed class DateTimeOffsetConverter : IYamlTypeConverter
{    
    public bool Accepts(Type type) => type == typeof(DateTimeOffset);

    public object? ReadYaml(IParser parser, Type type)
    {
        var scalar = parser.Consume<Scalar>();
        return DateTimeOffset.Parse(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (DateTimeOffset)value!;
        emitter.Emit(new Scalar(val.ToString("yyyy-MM-dd HH:mm:ss")));
    }
}
