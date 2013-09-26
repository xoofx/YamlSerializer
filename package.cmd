echo off
setlocal
REM call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\vc\vcvarsall.bat" x86
call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\vc\vcvarsall.bat" x86
set ProjectName=YamlSerializer
set TargetDir=%~dp0\%ProjectName%\Bin\Release\
set Zip=%~dp0\External\7-Zip\7z.exe
del %ProjectName%.zip
del %ProjectName%Src.zip
cd "%~dp0"
msbuild /tv:4.0 "/p:Configuration=Release;Platform=Any CPU" %ProjectName%.sln
"%Zip%" a -r "%TargetDir%%ProjectName%Src.zip" %ProjectName% %ProjectName%Test -x %ProjectName%\bin\* -x %ProjectName%\Help\* -x %ProjectName%\obj\* -x %ProjectName%\publish\* -x %ProjectName%Test\bin\* -x %ProjectName%Test\obj\* -x *.VisualState.xml
cd "%TargetDir%"
copy ..\..\Readme.txt Readme.txt
copy ..\..\ChangeLog.txt ChangeLog.txt
copy "..\..\Help\Release\YAML Serializer.chm" "YAML Serializer.chm"
"%Zip%" a %ProjectName%.zip Readme.txt ChangeLog.txt %ProjectName%.XML %ProjectName%.dll "YAML Serializer.chm"
