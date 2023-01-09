using GBX.NET;
using TmEssentials;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic.TypeConverters;

internal sealed class Int3Converter : IYamlTypeConverter
{    
    public bool Accepts(Type type) => type == typeof(Int3) || type == typeof(Int3?);

    public object? ReadYaml(IParser parser, Type type)
    {
        _ = parser.Consume<SequenceStart>();
        
        var x = int.Parse(parser.Consume<Scalar>().Value);
        var y = int.Parse(parser.Consume<Scalar>().Value);
        var z = int.Parse(parser.Consume<Scalar>().Value);
        
        _ = parser.Consume<SequenceEnd>();

        return new Int3(x, y, z);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
        {
            emitter.Emit(new Scalar("~"));
            return;
        }

        var val = (Int3)value!;
        
        emitter.Emit(new SequenceStart(default, default, isImplicit: true, SequenceStyle.Flow));
        emitter.Emit(new Scalar(val.X.ToString()));
        emitter.Emit(new Scalar(val.Y.ToString()));
        emitter.Emit(new Scalar(val.Z.ToString()));
        emitter.Emit(new SequenceEnd());
    }
}
