#tool "nuget:?package=GitVersion.CommandLine&version=3.6.5"
#tool "nuget:?package=NUnit.Runners&version=2.6.4"

var CONFIGURATION = Argument<string>("c", "Release");
var NUGET_SOURCE = EnvironmentVariable("NUGET_SOURCE") ?? "http://offproget001.vistaprint.net:81/nuget";
var NUGET_APIKEY = EnvironmentVariable("NUGET_APIKEY");
var NUGET_LIBRARY = NUGET_SOURCE + "/Library";
var NUGET_DEPLOY = NUGET_SOURCE + "/Deploy";
var MYGET_DEPLOY = EnvironmentVariable("MYGET_DEPLOY");

var src = Directory("./src");
var dst = Directory("./artifacts");
var packages = dst + Directory("./packages");

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
    string text = System.IO.File.ReadAllText(project.ToString());
    var matchResult = System.Text.RegularExpressions.Regex.Match(text, "(<TargetFrameworks?>)(.*?)(</TargetFrameworks?)");
    var matchingvalue = matchResult.Groups[2].Value;
    if(matchingvalue.Contains("netstandard"))
    {
        Information($"{project.FullPath} treated as .NET Standard project because it contains: {matchResult.Groups[0].Value}");
        return true;
    }
    Information($"{project.FullPath}: treated as .NET Framework project");
    return false;
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
        var settings = new NuGetRestoreSettings {
            FallbackSource = new List<string> { NUGET_LIBRARY }
        };
        NuGetRestore(sln, settings);
    }
});

Task("SemVer").Does(() => {
    var settings = new GitVersionSettings {
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = src + File("CommonAssemblyInfo.cs"),
        OutputType = GitVersionOutput.Json
    };

    var version = GitVersion(settings);


    if (BuildSystem.IsRunningOnTeamCity) {
         BuildSystem.TeamCity.SetBuildNumber(version.SemVer);
    }

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
    if (BuildSystem.IsRunningOnTeamCity) {
        TeamCity.ImportData("nunit", dst.Path + "/TestResults.xml");
    }
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
            {"Configuration", CONFIGURATION}
        },
        OutputDirectory = packages
    };

    NuGetPack(GetProjectFiles().Where(file => !IsDotNetStandard(file)), settings);
});

Task("Push").Does(() => {
    Information("Pushing the nuget packages...");
    foreach(var package in GetFiles(packages.Path + "/*.nupkg").Where(path => !path.FullPath.Contains(".symbols."))) {
        NuGetPush(package, new NuGetPushSettings {
            Source = NUGET_LIBRARY,
            ApiKey = NUGET_APIKEY
        });

        NuGetPush(package, new NuGetPushSettings {
            Source = MYGET_DEPLOY,
            ApiKey = ""
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

Task("TeamCity")
  .IsDependentOn("Default")
  .IsDependentOn("Push");


RunTarget(Argument("target", "Default"));