#!/bin/bash
for p in **/*.csproj; do
    echo "Building $p"
    dotnet build "$p" "$@" || break;
done
