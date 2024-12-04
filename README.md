# About
This is a Sleep & Hard Reset patching tool for GBA ROMs.

The assembly code of the patch itself is on `patch.s`, and it's a fork of [Dwedit's sleephack](https://www.dwedit.org/dwedit_board/viewtopic.php?id=306) with a refactor for including a cleaner way to configure both Sleep and Wake-up button combinations, as well as a configurable Hard Reset patch.

# Installation

## Windows 

Download and run the .exe

## macOS/Linux

Download the zip and extract it. Make sure to make the binary file executable:

```
chmod +x gba-sleephack-patcher-tool
```

Then run it:

```
./gba-sleephack-patcher-tool
```

# Usage

```
gba-sleephack-patcher-tool <Input file path> <Output file path> [options]

PARAMETERS
* Input file path
* Output file path

OPTIONS
  --no-sleep        Disable Sleep patch
  --no-hard-reset   Disable Hard Reset patch
  --sleep-combo     Sleep button combination. Default: "L+R+Select".
  --wake-up-combo   Wake up button combination. Default: "Select+Start".
  --hard-reset-combo  Hard reset button combination. Default: "L+R+Select+Start".
  -h|--help         Shows help text.
  --version         Shows version information.
```

## Examples

Command:
```bash
gba-sleephack-patcher-tool rom.gba rom_patched.gba
```

Output:
```bash
Sleep button combination: L+R+Select
Wake up button combination: Select+Start
Hard reset button combination: L+R+Select+Start

Done! Patched ROM saved as rom_patched.gba
```

---

Command:
```bash
gba-sleephack-patcher-tool rom.gba rom_patched.gba --no-hard-reset
```

Output:
```bash
Sleep button combination: L+R+Select
Wake up button combination: Select+Start

Done! Patched ROM saved as rom_patched.gba
```

---

Command:
```bash
gba-sleephack-patcher-tool rom.gba rom_patched.gba --no-hard-reset --sleep-combo "Up+L+R"
```

Output:
```bash
Sleep button combination: Up+L+R
Wake up button combination: Select+Start

Done! Patched ROM saved as rom_patched.gba
```

---

Command:
```bash
gba-sleephack-patcher-tool rom.gba rom_patched.gba --sleep-combo "Up+L+R" --wake-up-combo "Down+Start" --hard-reset-combo "A+B+R"
```

Output:
```bash
Sleep button combination: Up+L+R
Wake up button combination: Down+Start
Hard reset button combination: A+B+R

Done! Patched ROM saved as rom_patched.gba
```

# Build instructions for patch.s

**⚠️ Note: this is just for curious people. You don't have to follow this section if you're only going to use the patcher tool.**

The `patch.s` file is a file containing assembly code for ARM which will run on the GBA/emulator. We need to compile it into raw bytes so the ARM CPU that's inside the GBA is able to execute it. For this we will use `devkitARM` from `devkitPro`.

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

# Technical notes

The patcher tool uses a slightly variation of the `patch.bin` file included here. The only change is on lines 82, 83 and 84 from `patch.s`. Why?

- Button combinations are defined in a 10 bit long bitmask.
- Those bitmasks are loaded later in code with an `LDR` pseudoinstruction.
- `LDR` is a pseudoinstruction.
  - If the number defined by the bitmask fits on a 32bit `MOV` instruction, the compiler **will** use a `MOV` instruction. Check [here](https://developer.arm.com/documentation/dui0473/m/writing-arm-assembly-language/load-immediate-values-using-ldr-rd---const) for details.
  - Some numbers fit on a 32 `MOV` instructions, some other doesn't.
  - So, some bitmasks fit on a 32 `MOV` instructions, some other doesn't.
  - So, some button combinations fit on a 32 `MOV` instructions, some other doesn't.
- We want a standard way to quickly define our own button combinations without needing to build `patch.bin` every time.
- We want to force the compiler to use an `LDR` instruction. That way any bitmask for a button combination will be defined in the same offset on `patch.bin` and the patcher tool can just replace it.

So for generating the same patch used by the patcher tool, just replace lines 82, 83 and 84 from `patch.s` with:

```asm
SLEEP_BUTTON_MASK	= 0b1010101010
WAKE_UP_BUTTON_MASK	= 0b1110101011
HARD_RESET_BUTTON_MASK	= 0b1011101011
```

Just ignore the button combinations represented by those bitmasks. I just picked those to ensure they won't fit on a `MOV` instruction and they're different from each other.

# Credits

Original Sleephack by Dan Weiss (Dwedit)
July 1, 2007
