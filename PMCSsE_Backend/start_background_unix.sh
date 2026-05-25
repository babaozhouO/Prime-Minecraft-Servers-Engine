#!/bin/bash
echo "Starting PMCSsE_Backend in background..."
echo "Logs will be written to PMCSsE_Backend/Logs"
nohup dotnet PMCSsE_Backend.dll background &>/dev/null &
echo "Process started."
exit 0