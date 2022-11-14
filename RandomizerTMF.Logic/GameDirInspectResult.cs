namespace RandomizerTMF.Logic;

public record GameDirInspectResult(Exception? NadeoIniException, Exception? TmForeverException, Exception? TmUnlimiterException);
