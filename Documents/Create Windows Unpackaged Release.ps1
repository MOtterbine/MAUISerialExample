## RUN THIS RIGHT FROM HERE as Administrator if needed (Documents folder)

$winPlatform = 'win10-x64';
$winVer = 'net8.0-windows10.0.19041.0';


## publishing the actual, usable, unpackaged app
dotnet publish "..\MAUIAppSerialExample\MAUIAppSerialExample.csproj" -f $winVer -c Release -p:RuntimeIdentifierOverride=$winPlatform -p:WindowsPackageType=None



Read-Host -Prompt "Files are in the 'Publish' folder. Press <Enter> to continue"