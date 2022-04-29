# Flatpak Guide

## Requirements

* `flatpak`
* `flatpak-builder`

## Setup
Install the [required](me.tomsons.OrbisDbTools.yml) runtime sdk:

```
flatpak install org.freedesktop.Sdk/x86_64/21.08
```

## Build

Use `build-flatpak.sh` to build the package