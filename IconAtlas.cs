using ExileCore2;
using ExileCore2.Shared;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RectangleF = ExileCore2.Shared.RectangleF;

namespace MapIcons;

public class IconAtlas
{
    private readonly Graphics _graphics;

    public string Name { get; }
    public string FilePath { get; }
    public IntPtr TextureId { get; }
    public Vector2 IconSize { get; }
    public Vector2 AtlasSize { get; }
    public int IconsPerRow { get; }
    public int TotalIcons { get; }
    public IconAtlas(Graphics graphics, string name, string filePath, Vector2 iconSize) {
        if (graphics == null) throw new ArgumentNullException(nameof(graphics));

        if (File.Exists(filePath)) {

            using (Image<Rgba32> image = Image.Load<Rgba32>(filePath)) {
                AtlasSize = new Vector2(image.Width, image.Height);
                Log.Write($"Image Size: {AtlasSize}");
            }

            if (AtlasSize.X != AtlasSize.Y || (AtlasSize.X % 2 != 0)) {
                throw new Exception("Invalid image size. The image dimensions must be the same and powers of two.");
            }

            if (graphics.InitImage(name, filePath)) {
                TextureId = graphics.GetTextureId(name);
                Log.Write($"Icons Loaded from: {filePath}");
            }
            else {
                throw new Exception("Failed to initialize image.");
            }
        }
        else {
            throw new FileNotFoundException($"Texture file not found: {filePath}");
        }

        Name = name;
        FilePath = filePath;
        IconSize = iconSize;
        IconsPerRow = (int)(AtlasSize.X / iconSize.X);
        TotalIcons = (int)(AtlasSize.X / iconSize.X) * (int)(AtlasSize.Y / iconSize.Y);
    }
    public RectangleF GetIconUV(int iconIndex) {
        float x = (iconIndex % IconsPerRow) * IconSize.X / AtlasSize.X;
        float y = (iconIndex / IconsPerRow) * IconSize.Y / AtlasSize.Y;
        return new RectangleF(x, y, IconSize.X / AtlasSize.X, IconSize.Y / AtlasSize.Y);
    }

    public (Vector2 uv0, Vector2 uv1) GetIconUVs(int iconIndex) {
        int x = iconIndex % IconsPerRow;
        int y = iconIndex / IconsPerRow;

        float u0 = (float)x * IconSize.X / AtlasSize.X;
        float v0 = (float)y * IconSize.Y / AtlasSize.Y;
        float u1 = (float)(x + 1) * IconSize.X / AtlasSize.X;
        float v1 = (float)(y + 1) * IconSize.Y / AtlasSize.Y;

        Vector2 uv0 = new Vector2(u0, v0);
        Vector2 uv1 = new Vector2(u1, v1);
        return (uv0, uv1);
    }
}
