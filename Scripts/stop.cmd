taskkill /F /IM dotnet.exe
rem I didn't shut down each process separately, I decided to shut them all down with one command

FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker stop %%i
rem turns off all enabled containers in the docker