#!/bin/bash
PID=$(ps aux | grep '[s]kynet etc/config' | awk '{print $2}')
if [ -n "$PID" ]; then
    kill $PID
    echo "Server stopped (pid=$PID)"
else
    echo "Server is not running"
fi
