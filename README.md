# Sunberry-Village
An expansion mod for Stardew Valley. 

## Compilation
Generally speaking, compiling this mod is as simple as cloning the repository and hitting Build. If you have Stardew Valley installed in a non-standard location and ModBuildConfig is unable to automatically construct the `GamePath` property, you'll need to create a `stardewvalley.targets` file to point it in the right direction. Instructions for doing so can be found [here](https://github.com/Pathoschild/SMAPI/blob/develop/docs/technical/mod-package.md#how-to-set-options). By creating a **global** `GamePath` property, you avoid the need to modify the `.csproj` file at all, making it easy for other developers to clone and build the repository.
