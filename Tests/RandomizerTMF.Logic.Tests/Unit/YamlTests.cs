namespace RandomizerTMF.Logic.Tests.Unit;

public class YamlTests
{
    [Fact]
    public void Serializer_DoesNotThrow()
    {
        var exception = Record.Exception(() => Yaml.Serializer);

        Assert.Null(exception);
    }
    
    [Fact]
    public void Deserializer_DoesNotThrow()
    {
        var exception = Record.Exception(() => Yaml.Deserializer);

        Assert.Null(exception);
    }
}
