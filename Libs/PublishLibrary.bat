set CurDir=%~dp0
set LibName=%1
echo LibName=%LibName%
pdb2mdb %LibName%.dll
xcopy %LibName%.dll %CurDir%..\..\SpiceUnity\SpiceUnity\SpiceUnityApp\Assets\Plugins\ManagedLibs\  /y
xcopy %LibName%.dll.mdb %CurDir%..\..\SpiceUnity\SpiceUnity\SpiceUnityApp\Assets\Plugins\ManagedLibs\  /y
