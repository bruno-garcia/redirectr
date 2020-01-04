#!/bin/bash
set -e

# clean up old test results
find test -name "TestResults" -type d -prune -exec rm -rf '{}' +

dotnet test -c Release \
    --collect:"XPlat Code Coverage" \
    --settings test/coverletArgs.runsettings \
    /p:UseSourceLink=true

dotnet pack -c Release --no-build
