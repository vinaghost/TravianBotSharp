# TravianBotSharp (TBS)

[![alt text][image]][hyperlink]

[hyperlink]: https://discord.gg/mBa4f2K
[image]:https://discordapp.com/api/guilds/740829801446637601/widget.png?style=banner4

**TravianBotSharp** is a **free** Travian bot supports [Travian Official](https://www.travian.com).

## Quick start

1. Download TBS from **GitHub Releases**.
2. Extract files to a local folder.
3. Make sure you are on **Windows** (on macOS see [below](#macos)) and using the **latest Google Chrome**.
4. Run `TravianBotSharp.exe`.
5. Add your account and start setup.

## Main features

- Multi-account support
- Auto builder (support use resource from hero inventory, instant upgrade)
- Auto start adventures
- Auto start farm lists
- Auto claim daily quest/ village quest
- Auto NPC
- Auto train troop

## Tech specification

TBS runs on Windows (`WPFUI`) and macOS (`AvaloniaUI`).

### macOS

1. Download `TBS-<version>-macOS.zip` from **GitHub Releases** (Apple Silicon) and unzip it.
2. Make sure Google Chrome is installed in `/Applications`.
3. The app is not notarized by Apple, so macOS blocks it after download. Remove the quarantine flag once:
   ```bash
   xattr -dr com.apple.quarantine TravianBotSharp.app
   ```
4. Open `TravianBotSharp.app`.

Database, logs and browser profiles are stored in `~/Library/Application Support/TravianBotSharp`.

To build it yourself, install the [.NET 10 SDK](https://dot.net/download) and run `./build-mac.sh` (`RID=osx-x64 ./build-mac.sh` on an Intel Mac). The app is written to `publish/TravianBotSharp.app`.

## FAQs

User guides are available in [FAQs/README.md](FAQs/README.md).

- Getting started
- Add account / proxy
- Account settings
- Village build queue
- NPC and troop training
- Farming setup

