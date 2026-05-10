#!/bin/bash
# Script to check API compatibility between current build and latest NuGet package
# Usage: ./check-api-compat.sh <package-id> <project-path> <baseline-dll-name> <current-dll-name> <framework> <output-file> <verbosity>

set -e
set -o pipefail

PACKAGE_ID="$1"
PROJECT_PATH="$2"
BASELINE_DLL_NAME="$3"
CURRENT_DLL_NAME="$4"
FRAMEWORK="$5"
OUTPUT_FILE="$6"
VERBOSITY="$7"

if [ -z "$PACKAGE_ID" ] || [ -z "$PROJECT_PATH" ] || [ -z "$BASELINE_DLL_NAME" ] || [ -z "$CURRENT_DLL_NAME" ] || [ -z "$FRAMEWORK" ] || [ -z "$OUTPUT_FILE" ] || [ -z "$VERBOSITY" ]; then
    echo "Usage: $0 <package-id> <project-path> <baseline-dll-name> <current-dll-name> <framework> <output-file> <verbosity>"
    echo "  package-id:        NuGet package ID (e.g., boxofyellow.consolemarkdownrenderer)"
    echo "  project-path:      Path to project file or directory (e.g., ConsoleMarkdownRenderer.csproj)"
    echo "  baseline-dll-name: Name of the DLL without extension inside the published baseline NuGet package (e.g., ConsoleMarkdownRenderer)"
    echo "  current-dll-name:  Name of the DLL without extension produced by the current build (e.g., BoxOfYellow.ConsoleMarkdownRenderer)"
    echo "  framework:         Target framework (e.g., net10.0)"
    echo "  output-file:       Path to output file for results"
    echo "  verbosity:         Verbosity level (normal or high)"
    exit 1
fi

PACKAGE_ID_LOWER=$(echo "$PACKAGE_ID" | tr '[:upper:]' '[:lower:]')

echo "=== API Compatibility Check: $PACKAGE_ID ===" | tee "$OUTPUT_FILE"
echo "" | tee -a "$OUTPUT_FILE"

# Step 1: Find the latest version on NuGet first (needed for build)
echo "Fetching latest version of $PACKAGE_ID from NuGet..." | tee -a "$OUTPUT_FILE"
VERSION=$(curl -s "https://api.nuget.org/v3-flatcontainer/$PACKAGE_ID_LOWER/index.json" | jq -r '.versions[-1]' 2>/dev/null || echo "")

if [ -z "$VERSION" ] || [ "$VERSION" == "null" ]; then
    echo "ERROR: Could not find $PACKAGE_ID on NuGet" | tee -a "$OUTPUT_FILE"
    exit 1
fi

echo "Found version: $VERSION" | tee -a "$OUTPUT_FILE"

# Step 2: Download and extract the baseline package
BASELINE_DIR="./baseline/$PACKAGE_ID_LOWER"
mkdir -p "$BASELINE_DIR"

NUPKG_URL="https://api.nuget.org/v3-flatcontainer/$PACKAGE_ID_LOWER/$VERSION/$PACKAGE_ID_LOWER.$VERSION.nupkg"
echo "Downloading $NUPKG_URL..." | tee -a "$OUTPUT_FILE"
curl -sL "$NUPKG_URL" -o "$BASELINE_DIR/package.nupkg"

if [ ! -f "$BASELINE_DIR/package.nupkg" ]; then
    echo "ERROR: Failed to download package from NuGet" | tee -a "$OUTPUT_FILE"
    exit 1
fi

unzip -q -o "$BASELINE_DIR/package.nupkg" -d "$BASELINE_DIR"

# Find the baseline DLL for the specified framework.
# Temporary fallback: the most recently published baseline still uses the old DLL name,
# but a future release will publish under the new (current) name. Prefer the old name
# if present, otherwise fall back to the new name.
BASELINE_DLL="$BASELINE_DIR/lib/$FRAMEWORK/$BASELINE_DLL_NAME.dll"

if [ ! -f "$BASELINE_DLL" ]; then
    BASELINE_DLL_FALLBACK="$BASELINE_DIR/lib/$FRAMEWORK/$CURRENT_DLL_NAME.dll"
    if [ -f "$BASELINE_DLL_FALLBACK" ]; then
        echo "Baseline DLL not found at $BASELINE_DLL; using $BASELINE_DLL_FALLBACK" | tee -a "$OUTPUT_FILE"
        BASELINE_DLL="$BASELINE_DLL_FALLBACK"
    else
        echo "ERROR: Could not find baseline DLL at $BASELINE_DLL or $BASELINE_DLL_FALLBACK" | tee -a "$OUTPUT_FILE"
        exit 1
    fi
fi

# Step 3: Build the package locally with the same version as baseline
echo "" | tee -a "$OUTPUT_FILE"
echo "Building $PROJECT_PATH for $FRAMEWORK with version $VERSION..." | tee -a "$OUTPUT_FILE"
dotnet build --configuration Release --framework "$FRAMEWORK" -p:Version="$VERSION" "$PROJECT_PATH"
echo "" | tee -a "$OUTPUT_FILE"

# Determine the output directory based on project path
if [[ "$PROJECT_PATH" == *.csproj ]]; then
    PROJECT_DIR=$(dirname "$PROJECT_PATH")
    if [ "$PROJECT_DIR" == "." ]; then
        CURRENT_DLL="./bin/Release/$FRAMEWORK/$CURRENT_DLL_NAME.dll"
    else
        CURRENT_DLL="$PROJECT_DIR/bin/Release/$FRAMEWORK/$CURRENT_DLL_NAME.dll"
    fi
else
    CURRENT_DLL="$PROJECT_PATH/bin/Release/$FRAMEWORK/$CURRENT_DLL_NAME.dll"
fi

if [ ! -f "$CURRENT_DLL" ]; then
    echo "ERROR: Could not find current DLL at $CURRENT_DLL" | tee -a "$OUTPUT_FILE"
    exit 1
fi

# Step 4: Report versions and paths
echo "" | tee -a "$OUTPUT_FILE"
echo "=== Comparison Details ===" | tee -a "$OUTPUT_FILE"
echo "Baseline version: $VERSION" | tee -a "$OUTPUT_FILE"
echo "Current build version: $VERSION" | tee -a "$OUTPUT_FILE"
echo "Baseline assembly: $BASELINE_DLL" | tee -a "$OUTPUT_FILE"
echo "Current assembly: $CURRENT_DLL" | tee -a "$OUTPUT_FILE"
echo "" | tee -a "$OUTPUT_FILE"

# Step 5: Run the API compatibility check
echo "=== API Compatibility Results ===" | tee -a "$OUTPUT_FILE"
# Non-strict mode allows additive changes (new methods, new enum values, new properties)
# while still catching actual breaking changes like removed members or changed signatures
# Disable errexit temporarily to capture exit code properly
set +e
apicompat --left-assembly "$BASELINE_DLL" --right-assembly "$CURRENT_DLL" --verbosity "$VERBOSITY" 2>&1 | tee -a "$OUTPUT_FILE"
APICOMPAT_EXIT_CODE=${PIPESTATUS[0]}
set -e

echo "" | tee -a "$OUTPUT_FILE"
echo "=== Check Complete ===" | tee -a "$OUTPUT_FILE"

# Exit with the apicompat exit code to properly signal success/failure
exit $APICOMPAT_EXIT_CODE
