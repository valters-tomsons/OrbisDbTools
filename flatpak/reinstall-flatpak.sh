#!/bin/sh

echo "Building flatpak package..."
./build-flatpak.sh

echo "Trying to install flatpak package..."
./install-flatpak.sh
