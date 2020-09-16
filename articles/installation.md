# Installation

## NuGet

Currently, there are no NuGet packages available for Pandemic Framework.

## Git Submodules

**Git Submodules** allow you to easily get the latest changes to the engine using a single command.  They also let you clone the engine's code directly into your own repository, and make changes to your game without ever interfering with the engine itself.

To set this up, do the following:

### 1. Create a repo.

```bash
mkdir myga,e
cd mygame

git init
git remote add origin https://github.com/MyUser/MyGame
```

### 2. Add the submodule.

```bash
mkdir third-party
cd third-party
git submodule add https://github.com/alkalinethunder/pandemic-framework
cd ..
git submodule update --remote --init --recursive
```

This will create a folder called `third-party` to store the framework code in, add the framework as a submodule, and then recursively update the submodule to pull it's code in and all of it's dependencies.

### 3. Create a .NET Core solution

```bash
mkdir src
cd src
dotnet new sln
mkdir MyGame
cd MyGame
dotnet new consoleapp
```

### 4. Add all the projects

You'll need to add your game project, as well as Pandemic Framework and SpriteFontPlus to your solution.

```bash
# in /src
dotnet sln add MyGame
dotnet sln add ../third-party/pandemic-framework/src/AlkalineThunder.Pandemic
dotnet sln add ../third-party/pandemic-framework/third-party/SpriteFontPlus/src/SpriteFontPlus.MonoGame.csproj
```

### 5. Add project references

The final step is to open your IDE of choice, and add both `AlkalineThunder.Pandemic` and `SpriteFontPlus.MonoGame` to your game's project as references.

### All done!

You now have a working Pandemic Framework game that does absolutely nothing!