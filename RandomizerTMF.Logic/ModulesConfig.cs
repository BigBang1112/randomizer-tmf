namespace RandomizerTMF.Logic;

public class ModulesConfig
{
    public ModuleConfig Control { get; set; } = new() { X = 40, Y = -40, Width = 250, Height = 300 };
    public ModuleConfig History { get; set; } = new() { X = 40, Y = -380, Width = 250, Height = 300 };
    public ModuleConfig Status { get; set; } = new() { X = -40, Y = -500, Width = 350, Height = 200 };
    public ModuleConfig Progress { get; set; } = new() { X = -40, Y = -120, Width = 350, Height = 350 };
}
