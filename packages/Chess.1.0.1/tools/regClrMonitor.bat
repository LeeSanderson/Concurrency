@echo off
REM Run this batch file with elevated priveledges in order to register the required components to run CHESS 
regsvr32 /u /s Microsoft.ExtendedReflection.ClrMonitor.X86.dll
regsvr32 Microsoft.ExtendedReflection.ClrMonitor.X86.dll
    