@echo off
SET _neosDir= "C:\Program Files (x86)\Steam\steamapps\common\NeosVR"
if EXIST %_neosDir% goto write
SET /P _neosDir= Please enter NeosVR directory:
:write
rm NeosInstall.props
(
    echo ^<Project^> ^<PropertyGroup^>
    echo ^<NeosInstallDir^>%_neosDir%^</NeosInstallDir^>
    echo ^</PropertyGroup^> ^</Project^>
) > NeosInstall.props
