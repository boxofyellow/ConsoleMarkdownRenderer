#!/bin/bash
for p in **/*Tests.csproj; do
    echo "Testing $p"
    dotnet test "$p" "$@" || break;
done