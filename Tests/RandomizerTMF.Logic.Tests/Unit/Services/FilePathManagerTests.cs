using RandomizerTMF.Logic.Services;
using System.IO.Abstractions.TestingHelpers;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class FilePathManagerTests
{
    private readonly char slash = Path.DirectorySeparatorChar;
    private readonly string userDataPath = @$"C:{Path.DirectorySeparatorChar}Test{Path.DirectorySeparatorChar}UserData";

    [Fact]
    public void UserDataDirectoryPath_GetSetIsEqual()
    {
        // Arrange
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);

        // Act
        manager.UserDataDirectoryPath = userDataPath;

        // Assert
        Assert.Equal(userDataPath, manager.UserDataDirectoryPath);
    }

    [Fact]
    public void UserDataDirectoryPath_AutosavesDirectoryPathIsCorrect()
    {
        // Arrange
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);
        
        var expectedAutosavesPath = @$"C:{slash}Test{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves";

        // Act
        manager.UserDataDirectoryPath = userDataPath;

        // Assert
        Assert.Equal(expectedAutosavesPath, manager.AutosavesDirectoryPath);
    }

    [Fact]
    public void UserDataDirectoryPath_DownloadedMapsDirectoryNull_DownloadedDirectoryPathIsCorrect()
    {
        // Arrange
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);
        
        var expectedDownloadedPath = @$"C:{slash}Test{slash}UserData{slash}Tracks{slash}Challenges{slash}Downloaded{slash}_RandomizerTMF";

        // Act
        manager.UserDataDirectoryPath = userDataPath;

        // Assert
        Assert.Equal(expectedDownloadedPath, manager.DownloadedDirectoryPath);
    }

    [Fact]
    public void UserDataDirectoryPath_DownloadedMapsDirectoryEmpty_DownloadedDirectoryPathIsCorrect()
    {
        // Arrange
        var config = new RandomizerConfig
        {
            DownloadedMapsDirectory = ""
        };
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);
        
        var expectedDownloadedPath = @$"C:{slash}Test{slash}UserData{slash}Tracks{slash}Challenges{slash}Downloaded{slash}_RandomizerTMF";

        // Act
        manager.UserDataDirectoryPath = userDataPath;

        // Assert
        Assert.Equal(expectedDownloadedPath, manager.DownloadedDirectoryPath);
    }

    [Fact]
    public void UserDataDirectoryPath_DownloadedMapsDirectoryNotNull_DownloadedDirectoryPathIsCorrect()
    {
        // Arrange
        var config = new RandomizerConfig
        {
            DownloadedMapsDirectory = "CustomDirectory"
        };
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);
        
        var expectedDownloadedPath = @$"C:{slash}Test{slash}UserData{slash}Tracks{slash}Challenges{slash}Downloaded{slash}CustomDirectory";

        // Act
        manager.UserDataDirectoryPath = userDataPath;

        // Assert
        Assert.Equal(expectedDownloadedPath, manager.DownloadedDirectoryPath);
    }

    [Fact]
    public void UserDataDirectoryPath_UpdatedEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);
        
        var eventRaised = false;
        manager.UserDataDirectoryPathUpdated += () => eventRaised = true;

        // Act
        manager.UserDataDirectoryPath = userDataPath;

        // Assert
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void UserDataDirectoryPath_SetNull_SomeDirectoryPathsAreNull()
    {
        // Arrange
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var manager = new FilePathManager(config, fileSystem);

        // Act
        manager.UserDataDirectoryPath = null;

        // Assert
        Assert.Null(manager.AutosavesDirectoryPath);
        Assert.Null(manager.DownloadedDirectoryPath);
    }

    [Fact]
    public void SessionsDirectoryPath_EqualsSession()
    {
        // Assert
        Assert.Equal(expected: "Sessions", actual: FilePathManager.SessionsDirectoryPath);
    }

    [Theory]
    [InlineData("myfile.txt", "myfile.txt")]
    [InlineData("my:file.txt", "my_file.txt")]
    [InlineData("my*file.txt", "my_file.txt")]
    [InlineData("my\"file.txt", "my_file.txt")]
    [InlineData("my/file.txt", "my_file.txt")]
    public void ClearFileName_ReturnsUsableString(string fileName, string expectedResult)
    {
        // Act
        var result = FilePathManager.ClearFileName(fileName);

        // Assert
        Assert.Equal(expectedResult, actual: result);
    }

    [Fact]
    public void UpdateGameDirectory_Valid_ReturnsValidResultAndPathsAreSet()
    {
        // Arrange
        var gameDirectoryPath = @$"C:{slash}Test{slash}GameDirectory";
        var nadeoIniFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}Nadeo.ini";
        var tmForeverExeFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}TmForever.exe";
        var tmUnlimiterExeFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}TmInfinity.exe";
        
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { nadeoIniFilePath, new MockFileData("") },
            { tmForeverExeFilePath, new MockFileData("binary") },
            { tmUnlimiterExeFilePath, new MockFileData("binary") }
        });
        var manager = new FilePathManager(config, fileSystem);
        
        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var expectedUserDataDirectoryPath = $"{myDocuments}{slash}TmForever";

        // Act
        var result = manager.UpdateGameDirectory(gameDirectoryPath);

        // Assert
        Assert.Null(result.NadeoIniException);
        Assert.Null(result.TmForeverException);
        Assert.Null(result.TmUnlimiterException);
        Assert.Equal(expectedUserDataDirectoryPath, manager.UserDataDirectoryPath);
        Assert.Equal(tmForeverExeFilePath, manager.TmForeverExeFilePath);
        Assert.Equal(tmUnlimiterExeFilePath, manager.TmUnlimiterExeFilePath);
    }

    [Fact]
    public void UpdateGameDirectory_NadeoIniNotFound_ReturnsFileNotFoundExceptionAndPathsAreSet()
    {
        // Arrange
        var gameDirectoryPath = @$"C:{slash}Test{slash}GameDirectory";
        var tmForeverExeFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}TmForever.exe";
        var tmUnlimiterExeFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}TmInfinity.exe";

        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { tmForeverExeFilePath, new MockFileData("binary") },
            { tmUnlimiterExeFilePath, new MockFileData("binary") }
        });
        var manager = new FilePathManager(config, fileSystem);
        
        // Act
        var result = manager.UpdateGameDirectory(gameDirectoryPath);

        // Assert
        Assert.IsType<FileNotFoundException>(result.NadeoIniException);
        Assert.Null(manager.UserDataDirectoryPath);
        Assert.Null(result.TmForeverException);
        Assert.Null(result.TmUnlimiterException);
        Assert.Equal(tmForeverExeFilePath, manager.TmForeverExeFilePath);
        Assert.Equal(tmUnlimiterExeFilePath, manager.TmUnlimiterExeFilePath);
    }

    [Fact]
    public void UpdateGameDirectory_TmForeverNotFound_ReturnsFileNotFoundExceptionAndPathsAreSet()
    {
        // Arrange
        var gameDirectoryPath = @$"C:{slash}Test{slash}GameDirectory";
        var nadeoIniFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}Nadeo.ini";
        var tmUnlimiterExeFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}TmInfinity.exe";

        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { nadeoIniFilePath, new MockFileData("") },
            { tmUnlimiterExeFilePath, new MockFileData("binary") }
        });
        var manager = new FilePathManager(config, fileSystem);

        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var expectedUserDataDirectoryPath = $"{myDocuments}{slash}TmForever";

        // Act
        var result = manager.UpdateGameDirectory(gameDirectoryPath);

        // Assert
        Assert.IsType<FileNotFoundException>(result.TmForeverException);
        Assert.Null(manager.TmForeverExeFilePath);
        Assert.Null(result.NadeoIniException);
        Assert.Null(result.TmUnlimiterException);
        Assert.Equal(expectedUserDataDirectoryPath, manager.UserDataDirectoryPath);
        Assert.Equal(tmUnlimiterExeFilePath, manager.TmUnlimiterExeFilePath);
    }

    [Fact]
    public void UpdateGameDirectory_TmUnlimiterNotFound_ReturnsFileNotFoundExceptionAndPathsAreSet()
    {
        // Arrange
        var gameDirectoryPath = @$"C:{slash}Test{slash}GameDirectory";
        var nadeoIniFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}Nadeo.ini";
        var tmForeverExeFilePath = @$"C:{slash}Test{slash}GameDirectory{slash}TmForever.exe";
        
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { nadeoIniFilePath, new MockFileData("") },
            { tmForeverExeFilePath, new MockFileData("binary") }
        });
        var manager = new FilePathManager(config, fileSystem);

        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var expectedUserDataDirectoryPath = $"{myDocuments}{slash}TmForever";

        // Act
        var result = manager.UpdateGameDirectory(gameDirectoryPath);

        // Assert
        Assert.IsType<FileNotFoundException>(result.TmUnlimiterException);
        Assert.Null(manager.TmUnlimiterExeFilePath);
        Assert.Null(result.NadeoIniException);
        Assert.Null(result.TmForeverException);
        Assert.Equal(expectedUserDataDirectoryPath, manager.UserDataDirectoryPath);
        Assert.Equal(tmForeverExeFilePath, manager.TmForeverExeFilePath);
    }
}
