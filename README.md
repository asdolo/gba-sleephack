# About
This is a fork of [Dwedit's sleephack](https://www.dwedit.org/dwedit_board/viewtopic.php?id=306) (assembly patch code only, not the patcher tool) with a refactor for including a cleaner way to configure both Sleep and Wake-up button combinations, as well as a configurable Hard Reset patch.

These are the default button combinations:

| Sleep | Wake-up | Hard Reset |
| - | - | - |
| `A+B+Select+Start` | `L+R+Select` | `Select+Start+B` |

# Patching instructions

Download `sleephack.zip` from [Dwedit's sleephack post](https://www.dwedit.org/dwedit_board/viewtopic.php?id=306) (the second one, which is Release #2) and replace their `patch.bin` with the one from this repository. Then patch the ROM by running the following command on a `cmd` or a `PowerShell` terminal:

```bash
sleephack <input_file.gba> <output_file.gba>
```

# Configure button combinations

To change the button combinations you have to rebuild the `patch.bin` file. Check the [Build instructions](#build-instructions) to do so.

You have to build your own bitmask by checking the following table:
| Bitmask | Button |
| - | - |
| `00 0000 0001` | A |
| `00 0000 0010` | B |
| `00 0000 0100` | Select |
| `00 0000 1000` | Start |
| `00 0001 0000` | Right |
| `00 0010 0000` | Left |
| `00 0100 0000` | Up |
| `00 1000 0000` | Down |
| `01 0000 0000` | R |
| `10 0000 0000` | L |

For example, `A+B+Right` would be `00 0001 0011`, and `Select+Start+R` would be `01 0000 1100`.

Just change the bitmasks defined at lines 82, 83 and 84 on `patch.s` with your custom ones.

```asm
SLEEP_BUTTON_MASK	= 0b0000001111	@ A+B+Select+Start
WAKE_UP_BUTTON_MASK	= 0b1100000100	@ L+R+Select
HARD_RESET_BUTTON_MASK	= 0b0000001110	@ Select+Start+B
```

These are the default button combinations:

| Sleep | Wake-up | Hard Reset |
| - | - | - |
| `A+B+Select+Start` | `L+R+Select` | `Select+Start+B` |

# Disabling one of the patches

By default, `patch.bin` comes with both Sleep and Hard Reset patches applied. You can disable one of them by replacing one of the *branch* instructions with a *no-operation* instruction and rebuilding the `patch.bin` file. Check the [Build instructions](#build-instructions) to do so.

| - | Yes |  No |
| - | - | - |
| Want both Sleep and Hard Reset patches? | Don't change anything 😛 | Keep reading |
| Want Sleep patch only? | Replace `beq reset_now` with `nop` at line `125` | Keep reading |
| Want Hard Reset patch only? | Replace `beq sleep_now` with `nop` at line `120` |  You want nothing! Don't use this patch at all 😛 |

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

# Credits

Dan Weiss (Dwedit)
July 1, 2007
