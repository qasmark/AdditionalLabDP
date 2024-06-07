SET PATH_TO_PROCESS=%~dp0..\DistributedSystem.Process\
SET PATH_TO_LOGGER=%~dp0..\DistributedSystem.Logger\
SET PATH_TO_MANAGER=%~dp0..\DistributedSystem.DataManager\

start "Nats" docker run -p 4222:4222 -ti nats:latest

cd %PATH_TO_PROCESS%
start "Process 0" /d %PATH_TO_PROCESS% dotnet run --no-build 0
start "Process 1" /d %PATH_TO_PROCESS% dotnet run --no-build 1
start "Process 2" /d %PATH_TO_PROCESS% dotnet run --no-build 2
start "Process 3" /d %PATH_TO_PROCESS% dotnet run --no-build 3

cd %PATH_TO_LOGGER%
start "Logger" /d %PATH_TO_LOGGER% dotnet run --no-build

cd %PATH_TO_MANAGER%
start "Manager" /d %PATH_TO_MANAGER% dotnet run --no-build