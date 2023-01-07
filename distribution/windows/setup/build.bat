REM SET WHISPARR_VERSION=1
REM SET BRANCH=develop
echo ##teamcity[progressStart 'Building setup file']
inno\ISCC.exe whisparr.iss
echo ##teamcity[progressFinish 'Building setup file']

echo ##teamcity[publishArtifacts 'distribution\windows\setup\output\*%RUNTIME%*.exe']
