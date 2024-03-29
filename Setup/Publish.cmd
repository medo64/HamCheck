@ECHO OFF
SETLOCAL enabledelayedexpansion

SET        FILE_SETUP=".\HamCheck.iss"
SET     FILE_SOLUTION="..\Source\HamCheck.sln"
SET   FILE_EXECUTABLE="..\Binaries\HamCheck.exe"
SET  FILES_EXECUTABLE="..\Binaries\HamCheck.exe" "..\Binaries\HamCheckExam.dll"
SET       FILES_OTHER="..\Binaries\ReadMe.txt" "..\Binaries\License.txt"

SET   COMPILE_TOOL_17="%PROGRAMFILES%\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"
SET   COMPILE_TOOL_16="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"

SET        SETUP_TOOL="%PROGRAMFILES(x86)%\Inno Setup 5\iscc.exe"
SET        MERGE_TOOL="%PROGRAMFILES(x86)%\Microsoft\ILMerge\ILMerge.exe"

SET       SIGN_TOOL_1="%PROGRAMFILES(X86)%\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"
SET       SIGN_TOOL_2="%PROGRAMFILES(X86)%\Windows Kits\10\App Certification Kit\signtool.exe"
SET       SIGN_TOOL_3="%PROGRAMFILES(X86)%\Windows Kits\10\bin\x86\signtool.exe"
SET   SIGN_THUMBPRINT="e9b444fffb1375ece027e40d8637b6da3fdaaf0e"
SET SIGN_TIMESTAMPURL="http://timestamp.sectigo.com"

SET        GIT_TOOL_1="%PROGRAMFILES%\Git\mingw64\bin\git.exe"

SET          RAR_TOOL="%PROGRAMFILES%\WinRAR\WinRAR.exe"


ECHO --- DISCOVER TOOLS
ECHO.

IF EXIST %COMPILE_TOOL_17% (
    ECHO Visual Studio 2022
    SET COMPILE_TOOL=%COMPILE_TOOL_17%
) ELSE (
    IF EXIST %COMPILE_TOOL_16% (
        ECHO Visual Studio 2019
        SET COMPILE_TOOL=%COMPILE_TOOL_16%
    ) ELSE (
        ECHO Cannot find Visual Studio^^!
        PAUSE && EXIT /B 255
    )
)

IF EXIST %MERGE_TOOL% (
    ECHO IL Merge
) ELSE (
    ECHO Cannot find IL Merge^^!
    PAUSE && EXIT /B 255
)

IF EXIST %SETUP_TOOL% (
    ECHO Inno Setup 5
) ELSE (
    ECHO Cannot find Inno Setup 5^^!
    PAUSE && EXIT /B 255
)

IF EXIST %SIGN_TOOL_1% (
    ECHO Windows SignTool 10 ^(ClickOnce^)
    SET SIGN_TOOL=%SIGN_TOOL_1%
) ELSE (
    IF EXIST %SIGN_TOOL_2% (
        ECHO Windows SignTool 10 ^(App Certification Kit^)
        SET SIGN_TOOL=%SIGN_TOOL_2%
    ) ELSE (
        IF EXIST %SIGN_TOOL_3% (
            ECHO Windows SignTool 10 ^(SDK^)
            SET SIGN_TOOL=%SIGN_TOOL_3%
        ) ELSE (
            ECHO Cannot find Windows SignTool^^!
            PAUSE && EXIT /B 255
        )
    )
)

IF EXIST %GIT_TOOL_1% (
    ECHO Git
    SET GIT_TOOL=%GIT_TOOL_1%
) ELSE (
    GIT_TOOL="git"
)

IF NOT EXIST %RAR_TOOL% (
    ECHO WinRAR not present^^!
)

ECHO.
ECHO.


ECHO --- DISCOVER VERSION
ECHO.

FOR /F "delims=" %%N IN ('%GIT_TOOL% log -n 1 --format^=%%h') DO @SET VERSION_HASH=%%N%

IF NOT [%VERSION_HASH%]==[] (
    FOR /F "delims=" %%N IN ('%GIT_TOOL% rev-list --count HEAD') DO @SET VERSION_NUMBER=%%N%
    %GIT_TOOL% diff --exit-code --quiet
    IF ERRORLEVEL 1 SET VERSION_HASH=%VERSION_HASH%+
    ECHO Revision: %VERSION_HASH%
)

ECHO.
ECHO.


ECHO --- BUILD SOLUTION
ECHO.

RMDIR /Q /S "..\Binaries" 2> NUL
%COMPILE_TOOL% /Build "Release" %FILE_SOLUTION%
IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%

COPY ..\README.md ..\Binaries\ReadMe.txt > NUL
IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%

COPY ..\LICENSE.md ..\Binaries\License.txt > NUL
IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%

ECHO Completed.

ECHO.
ECHO.


CERTUTIL -silent -verifystore -user My %SIGN_THUMBPRINT% > NUL
IF %ERRORLEVEL%==0 (
    ECHO --- SIGN SOLUTION
    ECHO.
    
    IF [%SIGN_TIMESTAMPURL%]==[] (
        %SIGN_TOOL% sign /s "My" /sha1 %SIGN_THUMBPRINT% /td sha256 /v %FILES_EXECUTABLE%
    ) ELSE (
        %SIGN_TOOL% sign /s "My" /sha1 %SIGN_THUMBPRINT% /td sha256 /tr %SIGN_TIMESTAMPURL% /v %FILES_EXECUTABLE%
    )
    IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%
) ELSE (
    ECHO --- DID NOT SIGN SOLUTION
    IF NOT [%SIGN_THUMBPRINT%]==[] (
        ECHO.
        ECHO No certificate with hash %SIGN_THUMBPRINT%.
    ) 
)
ECHO.
ECHO.


ECHO --- BUILD SETUP
ECHO.

RMDIR /Q /S ".\Temp" 2> NUL
CALL %SETUP_TOOL% /DVersionHash=%VERSION_HASH% /O".\Temp" %FILE_SETUP%
IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%

FOR /F %%I IN ('DIR ".\Temp\*.exe" /B') DO SET _SETUPEXE=%%I
ECHO Setup is in file %_SETUPEXE%

ECHO.
ECHO.


ECHO --- RENAME BUILD
ECHO.

SET _OLDSETUPEXE=%_SETUPEXE%
IF NOT [%VERSION_HASH%]==[] (
    SET _SETUPEXE=!_SETUPEXE:000=-rev%VERSION_NUMBER%-%VERSION_HASH%!
)
IF NOT "%_OLDSETUPEXE%"=="%_SETUPEXE%" (
    ECHO Renaming %_OLDSETUPEXE% to %_SETUPEXE%
    MOVE ".\Temp\%_OLDSETUPEXE%" ".\Temp\%_SETUPEXE%"
) ELSE (
    ECHO No rename needed.
)

ECHO.
ECHO.


CERTUTIL -silent -verifystore -user My %SIGN_THUMBPRINT% > NUL
IF %ERRORLEVEL%==0 (
    ECHO --- SIGN SETUP
    ECHO.
    
    IF [%SIGN_TIMESTAMPURL%]==[] (
        %SIGN_TOOL% sign /s "My" /sha1 %SIGN_THUMBPRINT% /td sha256 /v ".\Temp\%_SETUPEXE%"
    ) ELSE (
        %SIGN_TOOL% sign /s "My" /sha1 %SIGN_THUMBPRINT% /td sha256 /tr %SIGN_TIMESTAMPURL% /v ".\Temp\%_SETUPEXE%"
    )
    IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%
) ELSE (
    ECHO --- DID NOT SIGN SETUP
)
ECHO.
ECHO.


IF EXIST %MERGE_TOOL% (
    ECHO --- MERGE ASSEMBLIES
    ECHO.
    
    %MERGE_TOOL% /targetplatform:v4 /keyfile:..\Source\HamCheck\Properties\App.snk /out:..\Binaries\HamCheckPortable.exe %FILES_EXECUTABLE%
    IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%

    ECHO Completed.

    ECHO.
    ECHO.

    
    CERTUTIL -silent -verifystore -user My %SIGN_THUMBPRINT% > NUL
    IF %ERRORLEVEL%==0 (
        ECHO --- RESIGN SOLUTION
        ECHO.
        
        IF [%SIGN_TIMESTAMPURL%]==[] (
            %SIGN_TOOL% sign /s "My" /sha1 %SIGN_THUMBPRINT% /td sha256 /v ..\Binaries\HamCheckPortable.exe
        ) ELSE (
            %SIGN_TOOL% sign /s "My" /sha1 %SIGN_THUMBPRINT% /td sha256 /tr %SIGN_TIMESTAMPURL% /v ..\Binaries\HamCheckPortable.exe
        )
        IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%
        ECHO.
        ECHO.
    )
    
    ECHO --- BUILD ZIP
    ECHO.

    IF EXIST %RAR_TOOL% (
        ECHO Zipping into %_SETUPEXE:.exe=.zip%
        "%PROGRAMFILES%\WinRAR\WinRAR.exe" a -afzip -ep -m5 ".\Temp\%_SETUPEXE:.exe=.zip%" ..\Binaries\HamCheckPortable.exe %FILES_OTHER%
        IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%
    ) ELSE (
        ECHO No WinRAR
    )

    ECHO.
    ECHO.
)


ECHO --- RELEASE
ECHO.

MKDIR "..\Releases" 2> NUL
MOVE ".\Temp\*.*" "..\Releases\." > NUL
IF ERRORLEVEL 1 PAUSE && EXIT /B %ERRORLEVEL%
RMDIR /Q /S ".\Temp"

ECHO.


ECHO --- DONE
ECHO.

explorer /select,"..\Releases\%_SETUPEXE%"
