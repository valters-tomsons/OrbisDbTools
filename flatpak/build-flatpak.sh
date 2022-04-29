#!/bin/sh

dotnet publish ../src/OrbisDbTools.Avalonia -c Release --self-contained -r linux-x64 -o bin
flatpak-builder build-dir me.tomsons.OrbisDbTools.yml --force-clean