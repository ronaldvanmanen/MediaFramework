#!/usr/bin/env bash
set -euox pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

###########################################################################
# CONFIGURATION
###########################################################################

DOTNET_DIRECTORY=".dotnet"

DOTNET_ARCHITECTURE="<auto>"
DOTNET_CHANNEL="7.0"
DOTNET_VERSION="latest"

DOTNET_INSTALL_SCRIPT_URI="https://dot.net/v1/dotnet-install.sh"

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

###########################################################################
# EXECUTION
###########################################################################

# Parse command line parameters
ARGS=("$@")

while [[ $# -gt 0 ]]; do
  NAME="$(echo "$1" | awk '{print tolower($0)}')"
  case $NAME in
    --architecture)
      DOTNET_ARCHITECTURE=$2
      shift 2
      ;;
    *)
      shift 1
      ;;
  esac
done

# Download dotnet install script
DOTNET_INSTALL_SCRIPT="$DOTNET_DIRECTORY/dotnet-install.sh"
mkdir -p "$DOTNET_DIRECTORY"
curl -Lsfo "$DOTNET_INSTALL_SCRIPT" "$DOTNET_INSTALL_SCRIPT_URI"
chmod +x "$DOTNET_INSTALL_SCRIPT"

# Install dotnet
DOTNET_INSTALL_DIRECTORY="$DOTNET_DIRECTORY/$DOTNET_ARCHITECTURE"
mkdir -p "$DOTNET_INSTALL_DIRECTORY"
bash "$DOTNET_INSTALL_SCRIPT" --channel "$DOTNET_CHANNEL" --version "$DOTNET_VERSION" --install-dir "$DOTNET_INSTALL_DIRECTORY" --architecture "$DOTNET_ARCHITECTURE"
PATH="$DOTNET_INSTALL_DIRECTORY:$PATH:"

# Restore dotnet tools (e.g. cake)
dotnet tool restore
LAST_EXITCODE=$?
if [ "$LAST_EXITCODE" != 0 ]; then
    return "$LAST_EXITCODE"
fi

# Run cake
dotnet cake "${ARGS[@]}"
LAST_EXITCODE=$?
if [ "$LAST_EXITCODE" != 0 ]; then
    return "$LAST_EXITCODE"
fi
