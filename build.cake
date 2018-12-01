#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var build = Argument("build", "1.0.0");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isAppVeyorBuild = AppVeyor.IsRunningOnAppVeyor;

// Define directories.
var buildDir = Directory("./FluxDayAutomation/FluxDayAutomation/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
	.IsDependentOn("Clean")
	.Does(() =>
{
	NuGetRestore("./FluxDayAutomation/FluxDayAutomation.sln");
});

Task("Build")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
{
	if(IsRunningOnWindows())
	{
	  // Use MSBuild
	  MSBuild("./FluxDayAutomation/FluxDayAutomation.sln", new MSBuildSettings().SetConfiguration(configuration));
	}
	else
	{
	  // Use XBuild
	  XBuild("./FluxDayAutomation/FluxDayAutomation.sln", settings =>
		settings.SetConfiguration(configuration));
	}
});

Task("Connect-ReportPortal")
	.IsDependentOn("Build")
	.Does(() =>
{
	System.IO.File.WriteAllText("tools/nunit.consolerunner.3.9.0/tools/ReportPortal.addins", "../../../FluxDayAutomation/FluxDayAutomation/bin/" + configuration + "/ReportPortal.NUnitExtension.dll\r\n../../../FluxDayAutomation/FluxDayAutomation/bin/" + configuration + "/FluxDayAutomation.dll");
});

Task("Run-Unit-Tests")
	.IsDependentOn("Connect-ReportPortal")
	.ContinueOnError()
	.Does(() =>
{
	NUnit3("./FluxDayAutomation/FluxDayAutomation/**/bin/" + configuration + "/FluxDayAutomation.dll", new NUnit3Settings {
		NoResults = true
	});
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
