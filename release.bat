ECHO off
set /p version="Enter version for this release: "
cd bin\Release
7z a -tzip TheMessengerRandomizerAP.zip TheMessengerRandomizerAP
ren TheMessengerRandomizerAP.zip TheMessengerRandomizerAP-%version%.zip