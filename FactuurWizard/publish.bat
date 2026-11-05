@echo off
dotnet publish FactuurWizard.csproj --self-contained -r win-x64 -o .\publish
vpk pack --packId FactuurWizard --packVersion 1.0.3 --packDir .\publish --mainExe FactuurWizard.exe --icon .\Assets\logo.ico --packTitle "Factuur Wizard" --noPortable --runtime win-x64 --msi --instLocation PerMachine