using TmEssentials;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic.TypeConverters;

internal sealed class TimeInt32Converter : IYamlTypeConverter
{    
    public bool Accepts(Type type) => type == typeof(TimeInt32) || type == typeof(TimeInt32?);

    public object? ReadYaml(IParser parser, Type type)
    {
        var scalar = parser.Consume<Scalar>();

        if (string.IsNullOrWhiteSpace(scalar.Value) || scalar.Value == "~")
        {
            return null;
        }

        var split = scalar.Value.Split(':', '.');

        if (split.Length != 3)
        {
            throw new Exception("TimeInt32 must be in the format mm:ss:fff");
        }
        
        var minutes = int.Parse(split[0]);
        var seconds = int.Parse(split[1]);
        var hundredths = int.Parse(split[2]);

        return new TimeInt32(minutes * 60 * 1000 + seconds * 1000 + hundredths * 10);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar("~"));
            return;
        }

        var val = (TimeInt32)value!;
        emitter.Emit(new Scalar(val.ToString(useHundredths: true)));
    }
}
