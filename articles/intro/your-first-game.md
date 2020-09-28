# Your First Game

Pandemic Framework makes it relatively easy to set up a simple game without knowing anything about MonoGame itself.  Although the engine is heavily focused on its UI system, it is possible to do 2D graphics rendering through an easy API.

This article will show you how to set up a **Hello World** program using both the 2D renderer and the GUI system.  In this article you'll learn the basic concepts of **Engine Modules**, **Scenes,** the **Skinning System**, and **Controls**.  Let's get started.

## Step 1: Get the engine running.

If you haven't already, make sure you have [set up your Visual Studio project with the engine](installation.md).  Once that's done, we can continue.

## Step 2: Getting a blank window on-screen

We need a blank window for the Pandemic Framework to render objects to.  Luckily, this is all taken care of for you.  All we need right now is a single line of code in our `Program.cs`:

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

The important parts of this code are:

1. `using AlkalineThunder.Pandemic;` - this lets us access a lot of the Pandemic Frameworks API.
2. `GameUtils.Run()` - This starts the engine up, creates our game window, and starts the game loop.  Once the game has been exited, our program will end.
3. `typeof(Program).Assembly` - This isn't really important in this tutorial, but the engine needs it so that it can figure out where to properly store the players settings and save data for your game.

Run your game, and you should see a black screen with nothing going on.  If you press `F1`, you snould see Pandemic Framework's **Dev Console**.  You won't need this right now, but it's a good sign that things are working.

## Step 3: Creating a Game Module

Pandemic is built to be highly extensible and to support modding and plugins.  To facilitate this, we have implemented a simple **Engine Module** system.  Many of the engines core features are built as Modules, and every game must have at least one Module that acts as the Game Module.

The Game Module keeps track of the games state, and is responsible for loading/unloading any data and assets needed throughout the entire game.  In this tutorial, we need it to load our Hello World scene that we'll implement later.

1. Create a new empty C# class, `MyGameModule.cs`

```C#
namespace MyGame
{
    public class MyGameModule
    {
        // ...
    }
}
```

2. Now, using the Pandemic Framework, let's turn this into an Engine Module.

```C#
using AlkalineThunder.Pandemic;

nasmespace MyGame
{
    public class MyGameModule : EngineModule
    {

    }
}
```

3. Now, the engine will make sure your game gets properly loaded.  To prove that our `MyGameModule` is loading, let's have it read some text from the console.

```C#
public class MyGameModule : EngineModule
{
    protected override void OnInitialize()
    {
        Console.WriteLine("This module is loading!");
        var line = Console.ReadLine();
        Console.WriteLine(line);
    }
}
```

Normally, you wouldn't ever need to do this - but this'll let us know that our code works.  Please, for the love of nil, remove everything to do with `Console` after you try this out.

After we're done, our code should look like this:

```C#
using AlkalineThunder.Pandemic;

namespace MyGame
{
    public class MyGameModule : EngineModule
    {
        protected override void OnInitialize()
        {
            // ...
        }
    }
}
```

## Step 3: Creating a Hello World scene

Now that we have our Game Module working, let's create a Scene for our game.  Every game must have at least one Scene in it that the Game Module can load - otherwise, you'll just see black!

1. Create a new C# class, `HelloWorldScene`.  We'll inherit from the `Scene` class this time.

```C#
uwing AlkalineThunder.Pandemic.Scenes;

namespace MyGame
{
    public class HelloWorldScene : Scene
    {

    }
}
```

2. In our `MyGameModule`, we need to tell the engine that we need access to the Scene System module.

```C#
using AlkalineThunder.Pandemic;
using AlkalineThunder.Pandemic.Scenes;

namespace MyGame
{
    [RequiresModule(typeof(SceneSystem))]
    public class MyGameModule : EngineModule
    {
        // ...
    }
}
```

The **RequiresModule** attribute tells the engine that it MUST load the specified module BEFORE loading our MyGameModule.  If the given module fails to load, then so will our `MyGameModule`.  Since `SceneSystem` is built into the engine, this error will never happen, but it will ensure that `SceneSystem` is always loaded first - and so we can use it.

3. Get our `Scene` to load:

First, we'll need to get a reference to the `SceneSystem` in our `MyGameModule`:

```C#
private SceneSystem SceneSystem
    => GetModule<SceneSysteM>();
```

Next, we need to load our `HelloWorldScene`.  In `MyGameModule`:

```C#
protected override void OnLoadContent()
{
    SceneSystem.GoToScene<HelloWorldScene>();
}
```

This will tell the engine to load our `HelloWorldScene` once the engine has fully initialized and is ready to run.  But, our screen will still be black.  So let's fix that.

## Step 4: Let's render some text.

Our `HelloWorldScene` will simply draw the text "Hello World" to the screen to show that it's working.  This is very easy to implement.

First, in our `HelloWorldScene`, let's add the following method:

```C#
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

// ...

protected override void OnDraw(GameTime gameTime, SpriteRocket2D renderer)
{
    renderer.Begin();

    // ...

    renderer.End();
}
```

This method will run every frame and let us draw things to our Scene.

To draw some text, we need a Font and a Color.  We can get both of these from the **Skinning System**.  This will allow your game to respect the users Dark Mode preference, but isn't required.

Let's quickly get two `Color` values from the skin, one for the background and one for the foreground.

```C#
using AlkalineThunder.Pandemic.Skinning;

// ...

var background = Skin.GetSkinColor(SkinColor.Default);
var foreground = Skin.GetSkinColor(SkinColor.Text);
```

And for our font, we'll just use the skin's Paragraph font.

```C#
var font = Skin.GEtFont(SkinFontStyle.Paragraph);
```

With this, we can clear the screen to the background color and draw "Hello World" using the foreground color and font.

```C#
protected override void OnDraw(GameTime gameTime, SpriteRocket2D renderer)
{
    var background = Skin.GetSkinColor(SkinColor.Default);
    var foreground = Skin.GetSkinColor(SkinColor.Text);

    var font = Skin.GEtFont(SkinFontStyle.Paragraph);

    renderer.Begin();
    
    // Draw the background.
    renderer.FillRectangle(Gui.BoundingBox, background);

    // Draw "Hello world!"
    renderer.DrawString(font, "Hello world!", Vector2.Zero, foreground);

    renderer.End();
}
```

You've just successfully made your first game using the Scene System.  Your challenge now is to do the same thing using the GUI system.  This guide should steer you in the right direction, so good luck!
