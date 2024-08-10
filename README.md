# WinApiPrank

Winapi pranks implemented in dotnet and avaliable in a single executable with nice CLI to run on your friend's PC.

## build

```sh
dotnet publish
```

The build artifacts are avaliable at - `bin\Release\net8.0\win-x64\publish\`, in order to not have the victim installing the correct .net version (and cause we dont have an installer) we compile it as self contained and single file.

Unfurtunetly AheadOfTime compliation does not work with `Cococna`.

In order to enable the BSOD feature you can pass the flag `-p:DefineConstants=EnableBSOD` to the build command.

## Commands

- `key-hook` - hook the `left-ctrl` key and on continous rappid presses will trggier a popup (or a BSOD if enabled at build)
- `falling-windows` - will make the focused window start slowly drip/fall/melt downwards.