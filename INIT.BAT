@echo off
SET _neosDir= "C:\Program Files (x86)\Steam\steamapps\common\NeosVR"
if EXIST %_neosDir% goto write
SET /P _neosDir= Please enter NeosVR directory:
(
    echo ^<Project^> ^<PropertyGroup^>
    echo ^<NeosInstallDir^>%_neosDir%^</NeosInstallDir^>
    echo ^</PropertyGroup^> ^</Project^>
) > NeosInstall.props
exit
:write
echo|set /p= "<Project><PropertyGroup><NeosInstallDir>C:\Program Files (x86)\Steam\steamapps\common\NeosVR</NeosInstallDir></PropertyGroup></Project>" > NeosInstall.props
