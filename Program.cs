using SmashFramework;
using SDL3;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;
using System.Numerics;
using KatziDrip;

if (args.Length <= 0 || args.Length > 1) 
{   
    Console.WriteLine("Invalid amount of arguments, expected 1 argument");
    return;
}

string filePath = args[0];

if (!File.Exists(filePath)) 
{
    Console.WriteLine($"File at {filePath} not found");
}

Smash.Init();
Drip.Load(Drip.DefaultThemeName);

float zoom = 1.0f;
float preferredZoom = 1.0f;

Vector2? dragStartPos = null;

Vector2 cameraPosition = new();

Window window = Window.Create("Katzi Viewer", 800, 600, SDL.WindowFlags.Resizable);
Renderer renderer = new(window);
renderer.SetVSyncEnabled(true);

Texture2D texture = AssetManager.LoadTexture("Image", filePath, renderer);

ulong lastTime = SDL.GetPerformanceCounter();

bool running = true;
while (running) 
{
    ulong currentTime = SDL.GetPerformanceCounter();
    double deltaTime = (double)(currentTime - lastTime) / SDL.GetPerformanceFrequency();
    lastTime = currentTime;
    
    Input.Update();
    while (SDL.PollEvent(out SDL.Event e)) 
    {
        if (e.Type == (uint)SDL.EventType.Quit)         
        {
            running = false;
        }
        
        Input.Event(e);
    }
    
    if (Input.ScrollWheelDelta != 0)   
    {
        preferredZoom += Input.ScrollWheelDelta / (7 / zoom);    
        preferredZoom = Math.Max(preferredZoom, 0.01f);
    }
    
    if (Input.IsMiddleMousePressed()) 
    {
        preferredZoom = 1.0f;
    }
    
    if (Input.IsLeftMouseDown()) 
    {
        if (dragStartPos.HasValue)
        {
            cameraPosition += dragStartPos.Value - (Input.MousePosition - (window.Size / 2) + cameraPosition);
        }
        
        dragStartPos = Input.MousePosition - (window.Size / 2) + cameraPosition;
    }
    else 
    {
        dragStartPos = null;
    }
    
    if (zoom != preferredZoom)  
    {
        zoom = MathHelper.Larp(zoom, preferredZoom, 30 * (float)deltaTime);
    } 
 
    renderer.Clear(Drip.BaseColor.ToColor());
    
    float scaleX = window.Width / (texture.Width / zoom);
    float scaleY = window.Height / (texture.Height / zoom); 
    
    float scale = Math.Min(scaleX, scaleY);
    
    renderer.RenderTexture(texture, ((window.Size / 2 - (texture.Size / 2 * scale)) - cameraPosition), Color.White, scale);
    
    renderer.RenderPresent();
}

AssetManager.Dispose();
Smash.Quit();