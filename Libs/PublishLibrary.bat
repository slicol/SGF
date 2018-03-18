set CurDir=%~dp0
set LibName=%1
echo LibName=%LibName%
pdb2mdb %LibName%.dll
xcopy %LibName%.dll %CurDir%..\..\GAD\Snaker2\Assets\Plugins\ManagedLib\  /y
xcopy %LibName%.dll.mdb %CurDir%..\..\GAD\Snaker2\Assets\Plugins\ManagedLib\  /y
