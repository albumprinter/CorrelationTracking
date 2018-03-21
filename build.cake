#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"
#tool "nuget:?package=NUnit.Runners&version=2.6.4"

var CONFIGURATION = Argument<string>("c", "Release");
var NUGET_LIBRARY = "https://api.nuget.org/v3/index.json";
var NUGET_APIKEY = EnvironmentVariableOrFail("NUGET_API_KEY");

var src = Directory("./src");
var dst = Directory("./artifacts");
var packages = dst + Directory("./packages");

string EnvironmentVariableOrFail(string varName){
    return EnvironmentVariable(varName) ?? throw new Exception($"Can't find variable {varName}");
}

IEnumerable<FilePath> GetProjectFiles()
{
    return GetFiles(src.Path + "/*/*.csproj").Where(file=>
        !file.GetFilenameWithoutExtension().FullPath.EndsWith("Tests")
        && file.GetFilenameWithoutExtension().FullPath != "MQClient"
        && file.GetFilenameWithoutExtension().FullPath != "WebApp1"
        && file.GetFilenameWithoutExtension().FullPath != "WebApp2"
        && file.GetFilenameWithoutExtension().FullPath != "WebClient"
        && file.GetFilenameWithoutExtension().FullPath != "TopShelfDemo"
        ).OrderBy(x=>x.FullPath);
}

bool IsDotNetStandard(FilePath project)
{
    return System.IO.File.ReadAllText(project.FullPath).Contains("<Project Sdk=\"Microsoft.NET.Sdk\">");
}

Task("Clean").Does(() => {
    CleanDirectories(dst);
    CleanDirectories(src.Path + "/packages");
    CleanDirectories(src.Path + "/**/bin");
    CleanDirectories(src.Path + "/**/obj");
    CleanDirectories(src.Path + "/**/pkg");
});

Task("Restore").Does(() => {
    EnsureDirectoryExists(dst);
    EnsureDirectoryExists(packages);

    foreach(var sln in GetFiles(src.Path + "/*.sln")) {
        NuGetRestore(sln);
    }
});

Task("SemVer").Does(() => {
    var settings = new GitVersionSettings {
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = src + File("CommonAssemblyInfo.cs"),
        OutputType = GitVersionOutput.Json
    };

    var version = GitVersion(settings);

    Information("{{  FullSemVer: {0}", version.FullSemVer);
    Information("    NuGetVersionV2: {0}", version.NuGetVersionV2);
    Information("    InformationalVersion: {0}  }}", version.InformationalVersion);
});

Task("Build").Does(() => {
    foreach(var sln in GetFiles(src.Path + "/*.sln")) {
        MSBuild(sln, settings => settings
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .SetVerbosity(Verbosity.Normal)
            .SetConfiguration(CONFIGURATION)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            );
    }
});

Task("Test").Does(() => {
    Information("Running unit tests...");
    NUnit(src.Path + "/**/bin/" + CONFIGURATION + "/*.Test*.dll", new NUnitSettings {
        NoLogo = true,
        StopOnError = true,
        Exclude = "Integration",
        ResultsFile = dst + File("./TestResults.xml")
    });
});

Task("Pack").Does(() => {
    var msBuildSettings
        = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", GitVersion().NuGetVersionV2);

    var coreSettings = new DotNetCorePackSettings {
        Configuration = CONFIGURATION,
        OutputDirectory = packages,
        MSBuildSettings = msBuildSettings
    };

	foreach(var file in GetProjectFiles().Where(file=>IsDotNetStandard(file))) {
		DotNetCorePack(file.ToString(), coreSettings);
	}

    var settings = new NuGetPackSettings {
        Symbols = true,
        IncludeReferencedProjects = false,
        Verbosity = NuGetVerbosity.Detailed,
        Properties = new Dictionary<string, string> {
            { "Configuration", CONFIGURATION }
        },
        OutputDirectory = packages
    };

    NuGetPack(GetProjectFiles().Where(file => !IsDotNetStandard(file)), settings);
});

Task("Push").Does(() => {
    Information("Pushing the nuget packages...");
    foreach(var package in GetFiles(packages.Path + "/*.nupkg")) {
        NuGetPush(package, new NuGetPushSettings {
            Source = NUGET_LIBRARY,
            ApiKey = NUGET_APIKEY
        });
    }
});

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("SemVer")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Pack");

Task("BuildServer")
  .IsDependentOn("Default")
  .IsDependentOn("Push");

RunTarget(Argument("target", "Default"));
