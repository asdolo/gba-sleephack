# About
This is a mirror of [Dwedit's sleephack](https://www.dwedit.org/dwedit_board/viewtopic.php?id=306) (assembly patch code only, not the patcher tool).

# Build instructions

The `patch.s` file is a file containing assembly code for ARM which will run on the GBA/emulator. You need to compile it into raw bytes so the ARM CPU that's inside the GBA is able to execute it. For this we will use `devkitARM` from `devkitPro`.

## Installing devkitARM

### For Windows

There's a [graphical installer](https://github.com/devkitPro/installer/releases/latest) for Windows. Just run it and make sure to check "GBA Development" in the components selection step.

### For Unix (Linux/macOS)

On Unix-like platforms such as Linux/macOS, they use `pacman` for managing all the packages.

1. Install devkitPro's package manager from [here](https://github.com/devkitPro/pacman/releases/latest).

2. Run `sudo dkp-pacman -S devkitARM` to install devkitARM binaries.

Make sure to add the `devkitARM/bin` directory to the `PATH` environment variable no matter if you're on Windows, Linux or macOS. In my case (macOS) it was on `/opt/devkitpro/devkitARM/bin`. For Windows it should be `C:\devkitPro\devkitARM\bin` if you select the default installation path.

## Building patch.bin

```bash
arm-none-eabi-as patch.s -o patch.o
arm-none-eabi-objcopy -O binary patch.o patch.bin
```

# Changelog
## Release #2
- Fixed mistake which made it fail to patch Lufia
- Sound is corrected after waking up
- Now hides the Start button from the game when activating sleep mode

# Credits

Dan Weiss (Dwedit)
July 1, 2007
