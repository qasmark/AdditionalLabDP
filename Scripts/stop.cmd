taskkill /F /IM dotnet.exe
rem не стал выключать каждый процесс по отдельности, решил выключить все одной командой

FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker stop %%i
rem также выключиает все всключенные контейнеры в докере