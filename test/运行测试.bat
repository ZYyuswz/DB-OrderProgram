@echo off
chcp 65001 >nul
title Oracle数据库连接测试

echo ====================================
echo       Oracle数据库连接测试程序
echo ====================================
echo.

cd /d "%~dp0"
dotnet run

echo.
echo 测试完成，按任意键退出...
pause >nul
