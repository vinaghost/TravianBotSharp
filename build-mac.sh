#!/usr/bin/env bash
# Builds publish/TravianBotSharp.app for macOS.
#
#   ./build-mac.sh              # Apple Silicon (osx-arm64)
#   RID=osx-x64 ./build-mac.sh  # Intel Mac
#
# Needs the .NET 10 SDK and Google Chrome in /Applications.
# The app is self-contained, so the Mac running it does not need .NET installed.
# Database, logs and browser profiles live in ~/Library/Application Support/TravianBotSharp.
set -euo pipefail
cd "$(dirname "$0")"

RID="${RID:-osx-arm64}"

# The dotnet on PATH may be an older SDK, so pick the first one that has a 10.x SDK.
find_dotnet() {
    for candidate in "${DOTNET:-}" "$(command -v dotnet || true)" "$HOME/.dotnet/dotnet"; do
        if [ -n "$candidate" ] && [ -x "$candidate" ] && "$candidate" --list-sdks 2>/dev/null | grep -q '^10\.'; then
            echo "$candidate"
            return
        fi
    done
    echo "error: .NET 10 SDK not found, install it from https://dot.net/download" >&2
    exit 1
}
DOTNET="$(find_dotnet)"
OUT="publish/$RID"
APP="publish/TravianBotSharp.app"
# The release workflow passes VERSION; local builds use the version from MainCore.csproj.
VERSION="${VERSION:-$(sed -n 's:.*<AssemblyVersion>\(.*\)</AssemblyVersion>.*:\1:p' MainCore/MainCore.csproj)}"

rm -rf "$OUT"
"$DOTNET" publish AvaloniaUI/AvaloniaUI.csproj -c Release -r "$RID" --self-contained true -p:AssemblyVersion="$VERSION" -o "$OUT"

# Apple Silicon only runs native code that carries a valid signature. Give any unsigned
# Mach-O file an ad-hoc one. This happens before the files go into the bundle: a bundle-level
# signature is not needed for a locally built app, and codesign cannot seal the .playwright folder.
find "$OUT" -type f -print0 | while IFS= read -r -d '' file; do
    if file -b "$file" | grep -q "Mach-O" && ! codesign --verify "$file" 2>/dev/null; then
        codesign --force --sign - "$file"
    fi
done

rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$OUT/." "$APP/Contents/MacOS/"

ICONSET_TMP="$(mktemp -d)"
sips -s format png WPFUI/favicon.ico --out "$ICONSET_TMP/icon.png" >/dev/null
sips -z 512 512 "$ICONSET_TMP/icon.png" --out "$ICONSET_TMP/icon512.png" >/dev/null
sips -s format icns "$ICONSET_TMP/icon512.png" --out "$APP/Contents/Resources/TravianBotSharp.icns" >/dev/null
rm -rf "$ICONSET_TMP"

cat > "$APP/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>TravianBotSharp</string>
    <key>CFBundleDisplayName</key>
    <string>Travian Bot Sharp</string>
    <key>CFBundleIdentifier</key>
    <string>com.vinaghost.travianbotsharp</string>
    <key>CFBundleExecutable</key>
    <string>TravianBotSharp</string>
    <key>CFBundleIconFile</key>
    <string>TravianBotSharp.icns</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

echo
echo "Built $APP"
