@echo off
for /f "delims=" %%x in (version.txt) do set build=%%x
SET /A build=build+1
echo %build% > version.txt
SET version=1.14.%build%.0

ECHO [92mNew version is %version%[0m