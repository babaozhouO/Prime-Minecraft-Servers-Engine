#!/bin/bash
printf "\033]0;PMCSsE_Backend - Foreground\007"
echo "Starting PMCSsE_Backend in foreground..."
dotnet PMCSsE_Backend.dll
read -p "Press any key to continue..."
exit 0