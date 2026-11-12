# Third-party components

The application's MIT license does not replace upstream licenses.

| Component | Version/source | License |
| --- | --- | --- |
| SDL2.dll, Windows x64 | [SDL 2.32.10 official release](https://github.com/libsdl-org/SDL/releases/tag/release-2.32.10) | [zlib](licenses/SDL2.txt) |
| gamecontrollerdb.txt | [SDL_GameControllerDB](https://github.com/mdqinc/SDL_GameControllerDB), snapshot fetched while preparing this app | [zlib](licenses/SDL_GameControllerDB.txt) |
| Nefarius.ViGEm.Client | [NuGet 1.21.256](https://www.nuget.org/packages/Nefarius.ViGEm.Client/1.21.256) | [MIT](licenses/ViGEm.NET.txt) |
| Native ViGEmClient bundled by the NuGet package | [ViGEmClient](https://github.com/nefarius/ViGEmClient) | [MIT](licenses/ViGEmClient.txt) |
| .NET runtime in portable builds | 8.0.34 | [MIT](licenses/NET-LICENSE.txt), [third-party notices](licenses/NET-THIRD-PARTY-NOTICES.txt) |

Vendored file SHA-256 checksums:

```text
B37740A72A7A9706216DF9F0134894BB7A850B356FD149398C67D874CBCFACB4  src/GameSirSimple/SDL2.dll
A2299EF257B68C62B9FCCE39FC59B5FB3A5A7068214F766A96C21595B55A11C3  src/GameSirSimple/gamecontrollerdb.txt
```

[ViGEmBus](https://github.com/nefarius/ViGEmBus) is a separately installed dependency and is not included in the repository or portable ZIP. [HidHide](https://github.com/nefarius/HidHide) is optional and is also not bundled. Their upstream projects govern their distribution and licensing.
