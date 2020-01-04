$ErrorActionPreference = "Stop"

dotnet test -c Release `
    --collect:"XPlat Code Coverage" `
    --settings test/coverletArgs.runsettings
if ($LASTEXITCODE -ne 0) { exit 1 }

dotnet pack -c Release --no-build
 if ($LASTEXITCODE -ne 0) { exit 1 }
