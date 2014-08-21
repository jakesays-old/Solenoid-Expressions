REM execute in ($ProjectDir)

SET PARSEDIR=%CD%\Parser\

IF EXIST "%PARSEDIR%ExpressionLexer.cs" del "%PARSEDIR%ExpressionLexer.cs"
IF EXIST "%PARSEDIR%ExpressionParser.cs" del "%PARSEDIR%ExpressionParser.cs"
IF EXIST "%PARSEDIR%ExpressionParserTokenTypes.cs" del "%PARSEDIR%ExpressionParserTokenTypes.cs"
IF EXIST "%PARSEDIR%ExpressionParserTokenTypes.txt" del "%PARSEDIR%ExpressionParserTokenTypes.txt"

..\BuildTools\antlr-2.7.6\antlr-2.7.6.exe -o %PARSEDIR% Expression.g