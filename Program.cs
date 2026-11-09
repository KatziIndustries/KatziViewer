using SmashFramework;
using SDL3;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;
using System.Numerics;
using KatziDrip;

string? filePath = null;
if (args.Length > 0)
    filePath = args[0];

Smash.Init();
Drip.Load(Drip.DefaultThemeName);

float zoom = 0.5f;
float preferredZoom = 0.5f;

Vector2? dragStartPos = null;

Vector2 cameraPosition = new();
Vector2 preferredPosition = new();

Window window = Window.Create("Katzi Viewer", 800, 600, SDL.WindowFlags.Resizable);
Renderer renderer = new(window);
renderer.SetVSyncEnabled(true);

Stream? embeddedStream = typeof(Program).Assembly.GetManifestResourceStream($"KatziViewer.Assets.LiberationMono-Regular.ttf");

if (embeddedStream == null)
    throw new Exception("Failed to load Font somehow wtf");

Font font = AssetManager.LoadFont("Font", embeddedStream, renderer);

Texture2D? texture = null;
TryLoadTexture();

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

        if (e.Type == (uint)SDL.EventType.DropFile)
        {
            string? path = Marshal.PtrToStringUTF8(e.Drop.Data);
            if (path != null && File.Exists(path))
            {
                filePath = path;
                TryLoadTexture();
            }
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
        preferredZoom = 0.5f;
        preferredPosition = new();
    }

    if (Input.IsLeftMouseDown())
    {
        if (dragStartPos.HasValue)
        {
            cameraPosition += dragStartPos.Value - (Input.MousePosition - (window.Size / 2) + cameraPosition);
            preferredPosition = cameraPosition;
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

    if (cameraPosition != preferredPosition)
    {
        cameraPosition = MathHelper.LarpVector(cameraPosition, preferredPosition, 30 * (float)deltaTime);
    }

    renderer.Clear(Drip.BaseColor.ToColor());

    if (texture != null)
    {
        float scaleX = window.Width / (texture.Width / zoom);
        float scaleY = window.Height / (texture.Height / zoom);

        float scale = Math.Min(scaleX, scaleY);

        Vector2 texturePosition = window.Size / 2 - (texture.Size / 2 * scale) - cameraPosition;
        renderer.RenderTexture(texture, texturePosition, Color.White, scale);

        Rectangle rectangle = new(texturePosition, texture.Size * scale);
        renderer.RenderRectangle(rectangle, Drip.PrimaryAccentColor.ToColor());
    }
    else
    {
        string text = "Drag and Drop Image here to View it :3 UwU";

        Vector2 textSize = font.MeasureString(text, 24);
        Vector2 textPosition = window.Size / 2 - textSize / 2 - cameraPosition;
        renderer.RenderString(font, 24, text, Vector2.Round(textPosition), Color.White);
    }

    renderer.RenderPresent();
}

AssetManager.Dispose();
Smash.Quit();

void TryLoadTexture()
{
    if (filePath is not null)
    {
        if (texture != null)
            texture.Dispose();

        nint textureHandle = Image.LoadTexture(renderer.Handle, filePath);

        if (textureHandle != 0)
            texture = new(textureHandle);

        preferredZoom = 0.5f;
        preferredPosition = new();
    }
}
