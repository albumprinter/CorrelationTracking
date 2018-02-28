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
        OutputType = GitVersionOutput.Json,
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = src + File("CommonAssemblyInfo.cs")
    };

    if (BuildSystem.IsRunningOnTeamCity) {
        settings.OutputType = GitVersionOutput.BuildServer;
    }

    var version = GitVersion(settings);

    if (settings.OutputType == GitVersionOutput.Json) {
        Information("{{  FullSemVer: {0}", version.FullSemVer);
        Information("    NuGetVersionV2: {0}", version.NuGetVersionV2);
        Information("    InformationalVersion: {0}  }}", version.InformationalVersion);
        System.IO.File.WriteAllText(dst.Path + "/VERSION", version.NuGetVersionV2);
    }

    //Set version in csproj file for .Net Standard project
    foreach(var file in GetFiles(src.Path + "/Correlation.Core.Standard/Correlation.Core.Standard.csproj")) {
        Information("Applying version " + version.NuGetVersionV2 + " for file " + file.ToString());
        string text = System.IO.File.ReadAllText(file.ToString());
        text = System.Text.RegularExpressions.Regex.Replace(text, "(<Version>)(.*?)(</Version>)", m => m.Groups[1].Value + version.NuGetVersionV2 + m.Groups[3].Value);
        text = System.Text.RegularExpressions.Regex.Replace(text, "(<AssemblyVersion>)(.*?)(</AssemblyVersion>)", m => m.Groups[1].Value + version.AssemblySemVer + m.Groups[3].Value);
        text = System.Text.RegularExpressions.Regex.Replace(text, "(<FileVersion>)(.*?)(</FileVersion>)", m => m.Groups[1].Value + version.AssemblySemVer + m.Groups[3].Value);
        System.IO.File.WriteAllText(file.ToString(), text);
    }
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
    var settings = new NuGetPackSettings {
        Symbols = true,
        IncludeReferencedProjects = true,
        Verbosity = NuGetVerbosity.Detailed,
        Properties = new Dictionary<string, string> {
            {"Configuration", CONFIGURATION}
        },
        OutputDirectory = packages
    };

    var clients = new FilePath [] {
        src + File("Correlation.Core/Correlation.Core.csproj"),
        src + File("Correlation.Http/Correlation.Http.csproj"),
        src + File("Correlation.IIS/Correlation.IIS.csproj"),
        src + File("Correlation.WCF/Correlation.WCF.csproj"),
        src + File("Correlation.Asmx/Correlation.Asmx.csproj"),
        src + File("Correlation.MassTransit/Correlation.MassTransit.csproj"),
        src + File("Tracing.Http/Tracing.Http.csproj"),
        src + File("Tracing.IIS/Tracing.IIS.csproj"),
        src + File("Tracing.WCF/Tracing.WCF.csproj"),
        src + File("Tracing.Asmx/Tracing.Asmx.csproj"),
        src + File("Tracing.AmazonSqs/Tracing.AmazonSqs.csproj"),
        src + File("Tracing.MassTransit/Tracing.MassTransit.csproj"),
        src + File("Correlation.Log4net/Correlation.Log4net.csproj"),
        src + File("CorrelationTracking/CorrelationTracking.csproj"),
        src + File("CorrelationTracking.Http/CorrelationTracking.Http.csproj"),
        src + File("CorrelationTracking.IIS/CorrelationTracking.IIS.csproj"),
        src + File("CorrelationTracking.MassTransit/CorrelationTracking.MassTransit.csproj"),
        src + File("Correlation.AmazonSqs/Correlation.AmazonSqs.csproj"),
        src + File("Correlation.AmazonSns/Correlation.AmazonSns.csproj")
    };
    NuGetPack(clients, settings);

    var coreSettings = new DotNetCorePackSettings {
        Configuration = CONFIGURATION,
        OutputDirectory = packages
    };

    DotNetCorePack(src + File("Correlation.Core.Standard/Correlation.Core.Standard.csproj"), coreSettings);
});

Task("Push").Does(() => {
    Information("Pushing the nuget packages...");
    foreach(var package in GetFiles(packages.Path + "/*.nupkg").Where(path => !path.FullPath.Contains(".symbols."))) {
        NuGetPush(package, new NuGetPushSettings {
            Source = NUGET_LIBRARY,
            ApiKey = NUGET_APIKEY
        });
    }

    Information("Pushing the myget packages...");
    foreach(var package in GetFiles(packages.Path + "/Correlation.Core.Standard.*.nupkg").Where(path => !path.FullPath.Contains(".symbols."))) {
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
  .IsDependentOn("Pack")
  ;

Task("TeamCity")
  .IsDependentOn("Default")
  .IsDependentOn("Push");

RunTarget(Argument("target", "Default"));