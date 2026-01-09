#!/bin/bash

outputFolder=_output
artifactsFolder=_artifacts
uiFolder="$outputFolder/UI"
framework="${FRAMEWORK:=net10.0}"

rm -rf $artifactsFolder
mkdir $artifactsFolder

for runtime in _output/*
do
  name="${runtime##*/}"
  folderName="$runtime/$framework"
  whisparrFolder="$folderName/Whisparr"
  archiveName="Whisparr.$BRANCH.$WHISPARR_VERSION.$name"

  if [[ "$name" == 'UI' ]]; then
    continue
  fi

  echo "Creating package for $name"

  echo "Copying UI"
  cp -r $uiFolder $whisparrFolder

  echo "Setting permissions"
  find $whisparrFolder -name "ffprobe" -exec chmod a+x {} \;
  find $whisparrFolder -name "Whisparr" -exec chmod a+x {} \;
  find $whisparrFolder -name "Whisparr.Update" -exec chmod a+x {} \;

  if [[ "$name" == *"osx"* ]]; then
    echo "Creating macOS package"

    packageName="$name-app"
    packageFolder="$outputFolder/$packageName"

    rm -rf $packageFolder
    mkdir $packageFolder

    cp -r distribution/macOS/Whisparr.app $packageFolder
    mkdir -p $packageFolder/Whisparr.app/Contents/MacOS

    echo "Copying Binaries"
    cp -r $whisparrFolder/* $packageFolder/Whisparr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $packageFolder/Whisparr.app/Contents/MacOS/Whisparr.Update

    echo "Packaging macOS app Artifact"
    (cd $packageFolder; zip -rq "../../$artifactsFolder/$archiveName-app.zip" ./Whisparr.app)
  fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "./$artifactsFolder/$archiveName.tar.gz" -C $folderName Whisparr
	fi

  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        (cd $folderName; 7z a -tzip "../../../$artifactsFolder/$archiveName.zip" ./Whisparr)
      else
      (cd $folderName; zip -rq "../../../$artifactsFolder/$archiveName.zip" ./Whisparr)
    fi
	fi
done
