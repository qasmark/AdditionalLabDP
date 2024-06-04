SET PATH_TO_PROCESS=%~dp0..\DistributedSystem.Process\
SET PATH_TO_LOGGER=%~dp0..\DistributedSystem.Logger\
SET PATH_TO_MANAGER=%~dp0..\DistributedSystem.DataManager\

cd %PATH_TO_PROCESS%
dotnet build

cd %PATH_TO_LOGGER%
dotnet build

cd %PATH_TO_MANAGER%
dotnet build