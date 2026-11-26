# BanMod

[🇮🇹 Read this README in Italian](README_IT.md)

## Before using BanMod: choose the correct mode

BanMod provides two modes. Choose the mode that matches the features you actually use:

- **Modded +25**: use this for gameplay changes, custom roles, host features that change game behavior, or anything that may affect another player's experience. Follow the current [Among Us Mod Policy](https://www.innersloth.com/among-us-mod-policy/) and Innersloth's technical requirements.
- **Vanilla**: intended only for compatible anti-cheat functionality and local visual changes that do not change gameplay or another player's experience. “Vanilla” is the BanMod mode name; it does not mean the client itself is unmodified.

When in doubt, treat the lobby as modded.

In **Modded +25**, commands remain unchanged. To hide command text from other players, replace `/` with `/cmd`: for example, `/bm blu` becomes `/cmd bm blu`.

> [!CAUTION]
> BanMod includes anti-cheat and compatibility checks. Unknown or incompatible mods/components may cause BanMod or Extra Services to be disabled. If you use another legitimate mod, contact the administrator so compatibility can be reviewed.

---

Lobby moderation, anti-abuse protection, host controls, custom roles, and configurable game modes for Among Us.

[Website](https://banmod.online/) · [Instructions](https://banmod.online/instructions) · [Downloads](https://banmod.online/downloads) · [Privacy, Terms & Cookies](https://banmod.online/policies)

## Description

BanMod is a Windows mod for Among Us based on BepInEx IL2CPP.

The public GPLv3 core provides moderation tools, anti-abuse protections, host administration, gameplay options, custom roles, and supporting interfaces.

The public repository contains the core. Optional server-delivered features, historically called “Premium” and now described as **Extra Services**, are separate from the public core and are not required to compile or use it.

## Main features

- **Moderation:** persistent bans and blocks, suspicious-player lists, name and word filters, spam protection, AFK management, and player administration.
- **Host controls:** automatic start, meeting and voting rules, tasks, sabotages, doors, maps, lobby messages, summaries, and configurable actions.
- **Roles and modes:** custom or modified roles, presets, role configuration, Hide and Seek improvements, and testing modes.
- **Client and visual tools:** configurable keys, zoom in permitted states, decorations, dark theme, custom interfaces, outfit/skin menus, and local options.
- **Anti-cheat and connected services:** optional verification, reports, server messages, anti-abuse systems, and lobby services.

Host, debug, or testing tools should be used only in appropriate environments and with respect for the other players in the lobby.

## Images

The existing repository image assets in `docs/images/` are part of the project documentation and should be kept unchanged.

BanMod custom skins are separate proprietary content and are not distributed under GPLv3. See [LICENSES.md](LICENSES.md).

## Requirements

- A legitimate copy of Among Us for Windows PC.
- A game version supported by the current BanMod release.
- The correct package for Steam or Epic Games.
- Permission to extract files into the folder containing `Among Us.exe`.

Among Us updates may break compatibility. Always check the latest release before installing BanMod or reporting an issue.

## Installation

1. Download the current package from the [official download page](https://banmod.online/downloads).
2. Select the Steam or Epic Games version.
3. Open the folder containing `Among Us.exe`.
4. Extract **all** files from the BanMod ZIP into that folder.
5. Make sure `Among Us.exe` and the `BepInEx` folder are at the same level.
6. Start Among Us. After BepInEx finishes loading, BanMod should appear in the main menu.

**Steam:** Library → right-click Among Us → Manage → Browse local files.  
**Epic Games:** Library → three-dot menu next to Among Us → Manage → folder icon.

### Updating and uninstalling

When instructed by the release notes, back up the BanMod data/configuration folder you want to preserve.

Remove obsolete or duplicate BanMod DLLs from `BepInEx/plugins` and do not mix files from different releases.

To uninstall, save any presets or configuration files you want to keep, then use the platform file-verification feature:

- **Steam:** Properties → Installed Files → Verify integrity of game files.
- **Epic Games:** Manage → Verify.

## Default controls

- `Delete`: opens the main BanMod menu.
- `F10`: opens the keybind configuration menu.

Keys and menus may change depending on the release or host permissions. Check the in-game guide and the [official instructions](https://banmod.online/instructions).

## Extra Services

Some optional features are delivered separately through official BanMod services.

They are not required to compile or use the GPLv3 core. Their availability, compatibility requirements, service rules, privacy information, and applicable terms are maintained on the official website:

**https://banmod.online/policies**

Separate components may also have separate licensing terms. See [LICENSES.md](LICENSES.md).

## Policy and responsible use

Use BanMod responsibly and do not use it for cheating, harassment, malicious interference, API abuse, bypassing protections, false reports, or gaining unfair advantages.

Use the correct BanMod mode for the active features and respect the consent and experience of other players.

The complete and current rules for BanMod services, privacy, data processing, security, reports, Community/Chat, Extra Services, Terms of Use, and cookies are maintained here:

**[BanMod — Privacy, Terms & Cookies](https://banmod.online/policies)**

For Among Us-specific requirements, always consult the current:

**[Among Us Mod Policy — Innersloth](https://www.innersloth.com/among-us-mod-policy/)**

A short repository policy summary is also available in [POLICY.md](POLICY.md).

## Forks and modified builds

The GPLv3-covered public core may be studied, modified, and redistributed according to GPLv3.

If you distribute a modified build:

- preserve the applicable license, notices, and attributions;
- clearly identify it as unofficial and modified;
- provide the corresponding source when required by GPLv3;
- do not include private server components, credentials, tokens, personal data, proprietary BanMod assets, or game files without authorization;
- do not imply endorsement by BanMod, GianniBart, Among Us, or Innersloth.

See [LICENSE](LICENSE) and [LICENSES.md](LICENSES.md) for the applicable licensing details.

## Building from source

The project uses .NET 6 and BepInEx IL2CPP packages:

```bash
git clone https://github.com/GiannBart/BanMod.git
cd BanMod
dotnet restore
dotnet build -c Release
```

Before building, review `BanMod.csproj`: remove developer-specific Windows paths, configure IL2CPP assemblies and metadata using your legitimate game installation, and remove local post-build targets.

Do not publish secrets, credentials, local configurations, Among Us binaries, `Among Us_Data`, `GameAssembly.dll`, or other game files.

The DLL is normally generated in:

```text
bin/Release/net6.0/
```

## Contributions and credits

Issues and pull requests for the GPL core are welcome when they respect people, applicable law, licenses, and the project's goals.

Do not submit proprietary components, unlawfully obtained game code, secret endpoints, credentials, or personal data.

BanMod contains original work and portions inspired by or derived from open-source projects. Preserve all notices contained in source files and in `Resources/Credits and License.txt`.

Main credited projects include:

- Town of Host
- Town of Host Enhanced
- EndlessHostRoles
- AmongUsRevamped
- MalumMenu
- TheOtherRoles / TheOtherHats
- BetterAmongUs
- GameLogger
- NLayer components and contributors, under the MIT License where indicated
- Spam bot database by AUGL (https://augl.net)

Credits do not imply affiliation or endorsement.

## Licenses

- BanMod public core: GNU GPLv3, except files carrying a different compatible notice.
- Third-party code and libraries: their original licenses and notices.
- Separate server-delivered components: see [LICENSES.md](LICENSES.md).
- BanMod custom skins: separate proprietary content.
- Among Us names, characters, logos, and related material belong to Innersloth LLC and/or their licensors.

See [LICENSE](LICENSE) and [LICENSES.md](LICENSES.md).

## Innersloth notice

BanMod is an unofficial community-made mod and is not affiliated with Innersloth.

Official notice:

> This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

Always consult the current [Among Us Mod Policy](https://www.innersloth.com/among-us-mod-policy/) before use.

## Support

- Website: https://banmod.online/
- Email: `banmod.giannibart@gmail.com`
- Discord: `GianniBart`
- Telegram: `@GianniBart`
- Bugs in the public GPL core: GitHub Issues

When reporting an issue, include the BanMod version, Among Us version, platform, reproduction steps, and sanitized logs. Do not publish tokens, friend codes, player identifiers, email addresses, private messages, or other personal data.
