using System.IO.Abstractions.TestingHelpers;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class NadeoIniTests
{
    [Fact]
    public void Parse_UserSubDirIsSet()
    {
        // Arrange
        string nadeoIniFilePath = "Nadeo.ini";
        string iniContent = @"
# Comment
; Comment
[Section]
Key=Value
UserSubDir=CustomUserDataDirectory
";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { nadeoIniFilePath, new MockFileData(iniContent) }
        });


        // Act
        var result = NadeoIni.Parse(nadeoIniFilePath, fileSystem);

        // Assert
        Assert.Equal(expected: "CustomUserDataDirectory", actual: result.UserSubDir);
    }

    [Fact]
    public void TestParse_DefaultUserSubDir()
    {
        // Arrange
        string nadeoIniFilePath = "Nadeo.ini";
        string iniContent = @"
# Comment
; Comment
[Section]
Key=Value
";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { nadeoIniFilePath, new MockFileData(iniContent) }
        });
        
        // Act
        var result = NadeoIni.Parse(nadeoIniFilePath, fileSystem);

        // Assert
        Assert.Equal(expected: "TmForever", actual: result.UserSubDir);
    }
}
