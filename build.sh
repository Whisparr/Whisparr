#! /usr/bin/env bash
set -e

outputFolder='_output'
testPackageFolder='_tests'
artifactsFolder="_artifacts";

ProgressStart()
{
    echo "Start '$1'"
}

ProgressEnd()
{
    echo "Finish '$1'"
}

UpdateVersionNumber()
{
    if [ "$WHISPARRVERSION" != "" ]; then
        echo "Updating Version Info"
        sed -i'' -e "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$WHISPARRVERSION<\/AssemblyVersion>/g" src/Directory.Build.props
        sed -i'' -e "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BUILD_SOURCEBRANCHNAME}<\/AssemblyConfiguration>/g" src/Directory.Build.props
        sed -i'' -e "s/<string>10.0.0.0<\/string>/<string>$WHISPARRVERSION<\/string>/g" distribution/osx/Whisparr.app/Contents/Info.plist
    fi
}

EnableExtraPlatformsInSDK()
{
    SDK_PATH=$(dotnet --list-sdks | grep -P '6\.\d\.\d+' | head -1 | sed 's/\(6\.[0-9]*\.[0-9]*\).*\[\(.*\)\]/\2\/\1/g')
    BUNDLEDVERSIONS="${SDK_PATH}/Microsoft.NETCoreSdk.BundledVersions.props"
    if grep -q freebsd-x64 $BUNDLEDVERSIONS; then
        echo "Extra platforms already enabled"
    else
        echo "Enabling extra platform support"
        sed -i.ORI 's/osx-x64/osx-x64;freebsd-x64/' "$BUNDLEDVERSIONS"
    fi
}

EnableExtraPlatforms()
{
    if grep -qv freebsd-x64 src/Directory.Build.props; then
        sed -i'' -e "s^<RuntimeIdentifiers>\(.*\)</RuntimeIdentifiers>^<RuntimeIdentifiers>\1;freebsd-x64</RuntimeIdentifiers>^g" src/Directory.Build.props
    fi
}

LintUI()
{
    ProgressStart 'ESLint'
    yarn lint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    if [ "$os" = "windows" ]; then
        yarn stylelint-windows
    else
        yarn stylelint-linux
    fi
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder
    rm -rf $testPackageFolder

    slnFile=src/Whisparr.sln

    if [ $os = "windows" ]; then
        platform=Windows
    else
        platform=Posix
    fi

    dotnet clean $slnFile -c Debug
    dotnet clean $slnFile -c Release

    # If no specific RID was provided, default to a single sensible RID for the current host
    # to avoid building/publishing every RuntimeIdentifier on a single hosted agent
    if [[ -z "$RID" ]]; then
        if [ "$os" = "windows" ]; then
            RID=win-x64
        else
            unameOut=$(uname -s)
            case "$unameOut" in
                Darwin*) RID=osx-x64 ;;
                *) RID=linux-x64 ;;
            esac
        fi
        echo "No RID specified — defaulting to $RID to reduce disk usage on this agent"
    fi

    if [[ -z "$FRAMEWORK" ]]; then
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -p:RuntimeIdentifiers=$RID -t:PublishAllRids
    else
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -p:RuntimeIdentifiers=$RID -p:TargetFramework=$FRAMEWORK -t:PublishAllRids
    fi

    ProgressEnd 'Build'
}

YarnInstall()
{
    ProgressStart 'yarn install'
    yarn install --frozen-lockfile --network-timeout 120000
    ProgressEnd 'yarn install'
}

RunWebpack()
{
    ProgressStart 'Running webpack'
    yarn run build --env production
    ProgressEnd 'Running webpack'
}

PackageFiles()
{
    local folder="$1"
    local framework="$2"
    local runtime="$3"

    # If there is no published output for this framework/runtime, skip packaging this runtime.
    if [ ! -d "$outputFolder/$framework/$runtime/publish" ]; then
        echo "Publish folder not found for $framework/$runtime — skipping package creation"
        return 0
    fi

    rm -rf $folder
    mkdir -p $folder
    cp -r $outputFolder/$framework/$runtime/publish/* $folder
    if [ -d "$outputFolder/Whisparr.Update/$framework/$runtime/publish" ]; then
        cp -r $outputFolder/Whisparr.Update/$framework/$runtime/publish $folder/Whisparr.Update
    fi
    if [ -d "$outputFolder/UI" ]; then
        cp -r $outputFolder/UI $folder
    fi

    echo "Adding LICENSE"
    cp LICENSE $folder
}

PackageLinux()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating $runtime Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Whisparr

    PackageFiles "$folder" "$framework" "$runtime"
    # If PackageFiles skipped because publish output was missing, the folder won't exist — skip further steps.
    if [ ! -d "$folder" ]; then
        echo "No package folder created for $runtime — skipping package"
        ProgressEnd "Creating $runtime Package for $framework"
        return 0
    fi

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Whisparr.Windows"
    rm $folder/Whisparr.Windows.*

    echo "Adding Whisparr.Mono to UpdatePackage"
    cp $folder/Whisparr.Mono.* $folder/Whisparr.Update
    if [ "$framework" = "net6.0" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Whisparr.Update
        cp $folder/libMonoPosixHelper.* $folder/Whisparr.Update
    fi

    ProgressEnd "Creating $runtime Package for $framework"
}

PackageMacOS()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating MacOS Package for $framework $runtime"

    local folder=$artifactsFolder/$runtime/$framework/Whisparr

    PackageFiles "$folder" "$framework" "$runtime"
    # If PackageFiles skipped because publish output was missing, the folder won't exist — skip further steps.
    if [ ! -d "$folder" ]; then
        echo "No package folder created for $runtime — skipping package"
        ProgressEnd "Creating MacOS Package for $framework $runtime"
        return 0
    fi

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Whisparr.Windows"
    rm $folder/Whisparr.Windows.*

    echo "Adding Whisparr.Mono to UpdatePackage"
    cp $folder/Whisparr.Mono.* $folder/Whisparr.Update
    if [ "$framework" = "net6.0" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Whisparr.Update
        cp $folder/libMonoPosixHelper.* $folder/Whisparr.Update
    fi

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating macOS App Package for $framework $runtime"

    local folder="$artifactsFolder/$runtime-app/$framework"

    rm -rf $folder
    mkdir -p $folder
    cp -r distribution/osx/Whisparr.app $folder
    mkdir -p $folder/Whisparr.app/Contents/MacOS

    echo "Copying Binaries"
    cp -r $artifactsFolder/$runtime/$framework/Whisparr/* $folder/Whisparr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $folder/Whisparr.app/Contents/MacOS/Whisparr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageWindows()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating Windows Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Whisparr

    PackageFiles "$folder" "$framework" "$runtime"
    # If PackageFiles skipped because publish output was missing, the folder won't exist — skip further steps.
    if [ ! -d "$folder" ]; then
        echo "No package folder created for $runtime — skipping package"
        ProgressEnd "Creating Windows Package for $framework"
        return 0
    fi

    # Copy additional windows-specific publish output if present
    if [ -d "$outputFolder/$framework-windows/$runtime/publish" ]; then
        cp -r $outputFolder/$framework-windows/$runtime/publish/* $folder
        # Re-copy UI folder after Windows-specific files to ensure it's not overwritten
        if [ -d "$outputFolder/UI" ]; then
            echo "Re-copying UI folder after Windows-specific files"
            cp -r $outputFolder/UI $folder
        fi
    else
        echo "Windows-specific publish folder not found for $framework/$runtime — skipping extra copy"
    fi

    echo "Removing Whisparr.Mono"
    rm -f $folder/Whisparr.Mono.*
    rm -f $folder/Mono.Posix.NETStandard.*
    rm -f $folder/libMonoPosixHelper.*

    echo "Adding Whisparr.Windows to UpdatePackage"
    cp $folder/Whisparr.Windows.* $folder/Whisparr.Update

    ProgressEnd 'Creating Windows Package'
}

Package()
{
    local framework="$1"
    local runtime="$2"
    local SPLIT

    IFS='-' read -ra SPLIT <<< "$runtime"

    case "${SPLIT[0]}" in
        linux|freebsd*)
            PackageLinux "$framework" "$runtime"
            ;;
        win)
            PackageWindows "$framework" "$runtime"
            ;;
        osx)
            PackageMacOS "$framework" "$runtime"
            PackageMacOSApp "$framework" "$runtime"
            ;;
    esac
}

BuildInstaller()
{
    local framework="$1"
    local runtime="$2"

    ./_inno/ISCC.exe distribution/windows/setup/whisparr.iss "//DFramework=$framework" "//DRuntime=$runtime"
}

InstallInno()
{
    ProgressStart "Installing portable Inno Setup"

    rm -rf _inno
    curl -s --output innosetup.exe "https://files.jrsoftware.org/is/6/innosetup-${INNOVERSION:-6.2.2}.exe"
    mkdir _inno
    ./innosetup.exe //portable=1 //silent //currentuser //dir=.\\_inno
    rm innosetup.exe

    ProgressEnd "Installed portable Inno Setup"
}

RemoveInno()
{
    rm -rf _inno
}

PackageTests()
{
    local framework="$1"
    local runtime="$2"

    cp test.sh "$testPackageFolder/$framework/$runtime/publish"

    rm -f $testPackageFolder/$framework/$runtime/*.log.config

    ProgressEnd 'Creating Test Package'
}

# Use mono or .net depending on OS
case "$(uname -s)" in
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        # on windows, use dotnet
        os="windows"
        ;;
    *)
        # otherwise use mono
        os="posix"
        ;;
esac

POSITIONAL=()

if [ $# -eq 0 ]; then
    echo "No arguments provided, building everything"
    BACKEND=YES
    FRONTEND=YES
    PACKAGES=YES
    INSTALLER=NO
    LINT=YES
    ENABLE_EXTRA_PLATFORMS=NO
    ENABLE_EXTRA_PLATFORMS_IN_SDK=NO
fi

while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    --backend)
        BACKEND=YES
        shift # past argument
        ;;
    --enable-bsd|--enable-extra-platforms)
        ENABLE_EXTRA_PLATFORMS=YES
        shift # past argument
        ;;
    --enable-extra-platforms-in-sdk)
        ENABLE_EXTRA_PLATFORMS_IN_SDK=YES
        shift # past argument
        ;;
    -r|--runtime)
        RID="$2"
        shift # past argument
        shift # past value
        ;;
    -f|--framework)
        FRAMEWORK="$2"
        shift # past argument
        shift # past value
        ;;
    --frontend)
        FRONTEND=YES
        shift # past argument
        ;;
    --packages)
        PACKAGES=YES
        shift # past argument
        ;;
    --installer)
        INSTALLER=YES
        shift # past argument
        ;;
    --lint)
        LINT=YES
        shift # past argument
        ;;
    --all)
        BACKEND=YES
        FRONTEND=YES
        PACKAGES=YES
        LINT=YES
        shift # past argument
        ;;
    *)    # unknown option
        POSITIONAL+=("$1") # save it in an array for later
        shift # past argument
        ;;
esac
done
set -- "${POSITIONAL[@]}" # restore positional parameters

if [ "$ENABLE_EXTRA_PLATFORMS_IN_SDK" = "YES" ];
then
    EnableExtraPlatformsInSDK
fi

if [ "$BACKEND" = "YES" ];
then
    UpdateVersionNumber
    if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
    then
        EnableExtraPlatforms
    fi
    Build
    # If a specific runtime was requested, package tests only for that runtime.
    # Otherwise, package tests for all supported runtimes (default framework net6.0).
    if [[ -n "$RID" ]];
    then
        if [[ -z "$FRAMEWORK" ]]; then
            FRAMEWORK="net6.0"
        fi
        PackageTests "$FRAMEWORK" "$RID"
    else
        if [[ -z "$FRAMEWORK" ]]; then
            FRAMEWORK="net6.0"
        fi
        PackageTests "$FRAMEWORK" "win-x64"
        PackageTests "$FRAMEWORK" "win-x86"
        PackageTests "$FRAMEWORK" "linux-x64"
        PackageTests "$FRAMEWORK" "linux-musl-x64"
        PackageTests "$FRAMEWORK" "osx-x64"
        if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
        then
            PackageTests "$FRAMEWORK" "freebsd-x64"
        fi
    fi
fi

if [[ "$LINT" = "YES" || "$FRONTEND" = "YES" ]];
then
    YarnInstall
fi

if [ "$LINT" = "YES" ];
then
    LintUI
fi

if [ "$FRONTEND" = "YES" ];
then
    RunWebpack
fi

if [ "$PACKAGES" = "YES" ];
then
    UpdateVersionNumber

    # Package for the requested runtime only when provided; otherwise package all runtimes.
    if [[ -n "$RID" ]];
    then
        if [[ -z "$FRAMEWORK" ]]; then
            FRAMEWORK="net6.0"
        fi
        Package "$FRAMEWORK" "$RID"
    else
        if [[ -z "$FRAMEWORK" ]]; then
            FRAMEWORK="net6.0"
        fi
        Package "$FRAMEWORK" "win-x64"
        Package "$FRAMEWORK" "win-x86"
        Package "$FRAMEWORK" "linux-x64"
        Package "$FRAMEWORK" "linux-musl-x64"
        Package "$FRAMEWORK" "linux-arm64"
        Package "$FRAMEWORK" "linux-musl-arm64"
        Package "$FRAMEWORK" "linux-arm"
        Package "$FRAMEWORK" "linux-musl-arm"
        Package "$FRAMEWORK" "osx-x64"
        Package "$FRAMEWORK" "osx-arm64"
        if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
        then
            Package "$FRAMEWORK" "freebsd-x64"
        fi
    fi
fi

if [ "$INSTALLER" = "YES" ];
then
    InstallInno
    BuildInstaller "net6.0" "win-x64"
    BuildInstaller "net6.0" "win-x86"
    RemoveInno
fi