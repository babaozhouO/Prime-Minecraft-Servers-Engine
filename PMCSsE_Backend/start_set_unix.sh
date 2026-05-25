#!/bin/bash
printf "\033]0;PMCSsE_Backend - Config\007"
echo "Starting PMCSsE_Backend in config mode"
dotnet PMCSsE_Backend.dll first
read -p "Press any key to continue..."
exit 0