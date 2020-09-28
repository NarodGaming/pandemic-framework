# Pandemic Framework

**Pandemic Framework** is a MonoGame-based game engine and GUI framework written in .NET Core.

## Why "Pandemic?"

This engine was developed for [Socially Distant](https://github.com/alkalinethunder/socially-distant).  In Socially Distant, you play as a private investigator and hacker working to uncover a conspiracy and find the truth behind a global **pandemic.**

## Main features

Pandemic Framework is designed for games that require a somewhat complex user interface.  Its feature set therefore has that in mind.

 - Easy-to-use Scene System.
 - Flexible keyboard/mouse input system.  Gamepad and Touch support coming soon.
 - Database-style Save System with GZIP compression and multiple slots.
 - Skinning System with Light and Dark Mode support.
 - Powerful and extendable Developer Console.
 - Flexible high-quality text rendering system using a fork of [SpriteFontPlus](https://github.com/alkalinethunder/SpriteFontPlus).
 - Container-based, event-driven UI system with an easy-to-write XML layout system.
 - Built-in game modding support.
 - Zero-boilerplate Crash Handler.
 - Extensive, easy-to-use debugging and logging features.

### Future Features

 - PAK archive support
 - Save file encryption
 - Discord Rich Presence support
 - Encrypted game assets
 - Graphical editor environment
 - Scripting support

## How to Install

Currently, there are **no NuGet packages** available for the Pandemic Framework.  Until NuGet packages are available, below is the recommended way to set up Pandemic Framework.

1. Make sure you have .NET Core 3.1.
2. Create a new Git repository.
3. Add `pandemic-framework` as a Git submodule:
   ```
   mkdir third-party
   cd third-party
   git submodule add https://github.com/alkalinethunder/pandemic-framework
   cd ..
   git submodule update --init --remote --recursive
   ```
4. Create an empty Console app:
   ```
   mkdir src
   cd src
   dotnet new sln
   mkdir MyGame
   cd MyGame
   dotnet new consoleapp
   cd ..
   dotnet sln add MyGame
   ```
5. Add the Pandemic Framework to your solution as well:
   ```
   dotnet sln add ../third-party/pandemic-framework/src/AlkalineThunder.Pandemic
   dotnet sln add ../third-party/pandemic-framework/third-party/SpriteFontPlus/src/SpriteFontPlus/SpriteFontPlus.MonoGame.csproj
   ```
6. And add them as Project References as well.

You may also need to install the relevant NuGet packages, such as MonoGame, that are used by the engine - however I'm not sure if they get pulled in automatically.

## Getting started

All you should need for Pandemic Framework to boot up, after following the steps above, is to have just a few lines of boilerplate in your `Program.cs`:

```C#
using AlkalineThunder.Pandemic;

namespace MyGame
{
    class Program
    {
        static void Main(string[] args)
        {
            GameUtils.Run(typeof(Program).Assembly);
        }
    }
}
```

If all goes well, you should get a black MonoGame window.  Press F1, type `help`, hit Enter, and see what happens.
