set PROJDIR=..\SolenoidExpressions
msbuild %PROJDIR%\SolenoidExpressions.csproj /t:Rebuild /p:Configuration=Release;TargetFrameworkVersion=v4.0;NugetBuild=true
msbuild %PROJDIR%\SolenoidExpressions.csproj /t:Rebuild /p:Configuration=Release;TargetFrameworkVersion=v4.5;NugetBuild=true

if EXIST lib rd /s/q lib
md lib\net40
md lib\net45
copy %PROJDIR%\bin\Release\net40 lib\net40
copy %PROJDIR%\bin\Release\net45 lib\net45

NuGet.exe pack Solenoid.Expressions.1.0.0.1.nuspec