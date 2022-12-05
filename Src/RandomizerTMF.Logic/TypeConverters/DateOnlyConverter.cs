using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic.TypeConverters;

internal sealed class DateOnlyConverter : IYamlTypeConverter
{    
    public bool Accepts(Type type) => type == typeof(DateOnly);

    public object? ReadYaml(IParser parser, Type type)
    {
        var scalar = parser.Consume<Scalar>();
        return DateOnly.Parse(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var val = (DateOnly)value!;
        emitter.Emit(new Scalar(val.ToString("yyyy-MM-dd")));
    }
}
