# Build Instructions for The Messenger Randomizer Mod

## Prerequisites

### 1. Install The Messenger Game
Make sure The Messenger is installed via Steam. By default, this is at:
```
C:\Program Files (x86)\Steam\steamapps\common\The Messenger
```

### 2. Configure GamePath

This project uses a centralized `GamePath` property in MSBuild. Follow these steps:

1. Copy the template file:
   ```powershell
   Copy-Item -Path "Directory.Build.props.user.example" -Destination "Directory.Build.props.user"
   ```

2. Edit `Directory.Build.props.user` and replace the path with your local game installation:
   ```xml
   <Project>
     <PropertyGroup>
       <GamePath>C:\Your\Path\To\The Messenger</GamePath>
     </PropertyGroup>
   </Project>
   ```

3. Save the file. The build system will automatically use this path.

### 3. Install Courier Mod Loader

1. Download `Courier-vX.X.zip` from the [Courier releases page](https://github.com/Brokemia/Courier/releases)
2. Extract the contents to your `TheMessenger.exe` folder (overwrite any files with the same name)
3. Run `MiniInstaller.exe` in the game directory
   - **Important**: If you have files `Assembly-CSharp.Postman.mm.dll` and `Assembly-CSharp.Postman.mm.pdb` from a previous install, delete them before running the installer
   - On Windows, you may need to "Unblock" the zip file in File Explorer (right-click → Properties → Unblock)

After installation, verify these files exist in `TheMessenger_Data\Managed\`:
- `Assembly-CSharp.dll`
- `MMHOOK_Assembly-CSharp.dll`
- `Assembly-CSharp-firstpass.dll`
- `UnityEngine.dll` (and related Unity DLLs)
- `Mono.Cecil.dll`
- `MonoMod.Utils.dll`

## Building the Project

### Using Visual Studio
1. Open `TheMessengerRandomizer.sln` in Visual Studio
2. Build → Build Solution (Ctrl+Shift+B)

## Troubleshooting

### Build fails with "GamePath invalid" error

**Solution**: Verify your `Directory.Build.props.user` file:
- Check that the file exists in the workspace root
- Verify the GamePath points to a valid The Messenger installation
- Confirm the path contains `TheMessenger_Data\Managed\` folder with game DLLs
- Restart Visual Studio if you just created the `.user` file

### Missing assembly references after setting GamePath

**Solution**: Make sure your game installation is fully updated and Courier is properly installed. Run `MiniInstaller.exe` again to regenerate hook files.
