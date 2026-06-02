#!/bin/bash
echo "Starting PMCSsE_Backend in background..."
echo "Logs will be written to PMCSsE_Backend/Logs"
dotnet PMCSsE_Backend.dll background
echo "Process started."
exit 0