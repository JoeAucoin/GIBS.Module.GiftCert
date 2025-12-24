@echo off
set TargetFramework=%1
set ProjectName=%2

del "*.nupkg"
"..\..\oqtane.framework\oqtane.package\FixProps.exe"
"..\..\oqtane.framework-6.2.1-Source\oqtane.package\nuget.exe" pack %ProjectName%.nuspec -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
XCOPY "*.nupkg" "..\..\oqtane.framework-6.2.1-Source\Oqtane.Server\Packages\" /Y