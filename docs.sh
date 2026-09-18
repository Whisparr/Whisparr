PLATFORM=$1

if [ "$PLATFORM" = "Windows" ]; then
  RUNTIME="win-x64"
elif [ "$PLATFORM" = "Linux" ]; then
  RUNTIME="linux-x64"
elif [ "$PLATFORM" = "Mac" ]; then
  RUNTIME="osx-x64"
else
  echo "Platform must be provided as first arguement: Windows, Linux or Mac"
  exit 1
fi

outputFolder='_output'
testPackageFolder='_tests'

rm -rf $outputFolder
rm -rf $testPackageFolder

slnFile=src/Whisparr.sln

platform=Posix

dotnet clean $slnFile -c Debug
dotnet clean $slnFile -c Release

dotnet msbuild -restore $slnFile -p:Configuration=Debug -p:Platform=$platform -p:RuntimeIdentifiers=$RUNTIME -t:PublishAllRids

# Read the generator version from the project so the CLI cannot drift from it.
swaggerVersion=$(sed -n 's/.*Swashbuckle\.AspNetCore\.SwaggerGen" Version="\([^"]*\)".*/\1/p' src/NzbDrone.Host/Whisparr.Host.csproj)

dotnet new tool-manifest --force
dotnet tool install --version "$swaggerVersion" Swashbuckle.AspNetCore.Cli

consoleDll=$(echo $outputFolder/net*/$RUNTIME/Whisparr.Console.dll)

dotnet tool run swagger tofile --output ./src/Whisparr.Api.V3/openapi.json "$consoleDll" v3 &

sleep 45

kill %1

exit 0
