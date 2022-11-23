namespace RandomizerTMF.Logic;

public class ModulesConfig
{
    public ModuleConfig Control { get; set; } = new() { X = 40, Y = 700, Width = 250, Height = 250 };
    public ModuleConfig Status { get; set; } = new() { X = 1510, Y = 325, Width = 350, Height = 200 };
    public ModuleConfig Progress { get; set; } = new() { X = 1510, Y = 560, Width = 350, Height = 350 };
}
