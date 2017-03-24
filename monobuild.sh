#!/bin/bash
mono .paket/paket.bootstrapper.exe
exit_code=$?
if [ $exit_code -ne 0 ]; then
  exit $exit_code
fi
mono .paket/paket.exe restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
  exit $exit_code
fi
export NPM_FILE_PATH=$(which npm)
mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
