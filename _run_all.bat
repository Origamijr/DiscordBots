REM Copy token file to location allowing for debug testing
copy Tokens.txt DelBot\bin\Debug\Tokens.txt
copy Tokens.txt MarxBot\bin\Debug\netcoreapp2.2\Tokens.txt
copy Tokens.txt TestBot\bin\Debug\Tokens.txt

REM Start C# bots
start cmd /k DelBot_run.bat
start cmd /k MarxBot_run.bat
start cmd /k TestBot_run.bat

REM Start Python bots
start cmd /k NablaBot_run.bat
