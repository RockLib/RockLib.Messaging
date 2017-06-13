echo off
goto :check_parameter

:check_parameter
if [%1]==[] goto :check_file
goto :set_api_key_from_parameter

:check_file
if exist apikey.txt goto :set_api_key_from_file
echo "The first parameter is the api key. If omitted, a file must exist named 'apikey.txt' that contains the api key."
goto :eof

:set_api_key_from_file
set /p ApiKey=<apikey.txt
goto :get_latest_package_and_push_it

:set_api_key_from_parameter
set ApiKey=%1
goto :get_latest_package_and_push_it

:get_latest_package_and_push_it
for /f %%i in ('dir /b/a-d/od/t:c Rock.Messaging.SQS.?.?.?*.nupkg') do set LAST=%%i
nuget push %LAST% %ApiKey% -source https://www.nuget.org