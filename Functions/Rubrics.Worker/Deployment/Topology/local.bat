
If not exist .\SetEnvironment.local.bat Exit -5
call .\SetEnvironment.local.bat

set evn=%environmentName%
set sb=%sbConnectionString%
set q=%queueSizeInGB%

call "%~dp0\ASBTopologyCreation.bat" "%evn%" "%sb%" "%q%"
