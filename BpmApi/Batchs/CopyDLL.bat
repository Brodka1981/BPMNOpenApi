setlocal ENABLEDELAYEDEXPANSION
set _path=%0
set _path=%_path:BpmApi\Batchs\CopyDLL.bat=!""!%

xcopy /Y !_path!"BpmServiceFields\bin\Debug\net9.0\BpmServiceFields.dll" !_path!"BpmApi\Add-ins\"

xcopy /Y !_path!"BpmServiceFields\bin\Debug\net9.0\BpmServiceFields.dll" !_path!"BpmApi\bin\Debug\net9.0\Add-ins\"

xcopy /Y !_path!"BpmServiceTasks\bin\Debug\net9.0\BpmServiceTasks.dll" !_path!"BpmApi\Add-ins\"

xcopy /Y !_path!"BpmServiceTasks\bin\Debug\net9.0\BpmServiceTasks.dll" !_path!"BpmApi\bin\Debug\net9.0\Add-ins\"