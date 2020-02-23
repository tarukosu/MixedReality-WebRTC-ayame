@echo off

setlocal ENABLEDELAYEDEXPANSION

cd /d %~dp0

set FOLDER_1=Microsoft.MixedReality.WebRTC.Unity
set FOLDER_2=Microsoft.MixedReality.WebRTC.Unity.Editor
set FOLDER_3=Microsoft.MixedReality.WebRTC.Unity.Examples

set i=1
:BEGIN
call set f=%%FOLDER_!i!%%
if defined f (
  rem echo Create symbolic link !f!
  mklink /D ..\Assets\!f! ..\External\MixedReality-WebRTC\libs\Microsoft.MixedReality.WebRTC.Unity\Assets\!f!
  mklink ..\Assets\!f!.meta ..\External\MixedReality-WebRTC\libs\Microsoft.MixedReality.WebRTC.Unity\Assets\!f!.meta
  set /A i+=1
  goto :BEGIN
)

pause
