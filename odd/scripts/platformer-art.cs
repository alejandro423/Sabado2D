using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlatformerTools
{
    /// <summary>
    /// Generates the platformer sprite set as PNG assets with sprite import settings.
    /// Run: unity command script run --file odd/scripts/platformer-art.cs --entry PlatformerTools.PlatformerArt.Generate
    /// </summary>
    public static class PlatformerArt
    {
        public static string Generate()
        {
            string artDir = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Art");
            Directory.CreateDirectory(artDir);

            Color32 clear = new Color32(0, 0, 0, 0);
            int size = 32;

            // Player: rounded blue square with eyes.
            Color32 outline = new Color32(27, 58, 92, 255);
            Color32 body = new Color32(110, 198, 255, 255);
            Color32 eye = new Color32(20, 20, 30, 255);
            Color32[] player = New(size * size, clear);
            FillRect(player, size, 4, 3, 24, 26, outline);
            FillRect(player, size, 5, 4, 22, 24, body);
            FillRect(player, size, 10, 17, 5, 6, eye);
            FillRect(player, size, 18, 17, 5, 6, eye);
            FillRect(player, size, 13, 9, 7, 2, eye);
            Save(player, size, size, artDir, "player", 32);

            // Ground: reusable tiled dirt block with a grass cap.
            Color32 dirt = new Color32(139, 90, 43, 255);
            Color32 dirtDark = new Color32(112, 71, 33, 255);
            Color32 grass = new Color32(76, 175, 80, 255);
            Color32 grassDark = new Color32(56, 142, 60, 255);
            Color32[] ground = New(size * size, dirt);
            for (int i = 0; i < size; i += 7)
            {
                FillRect(ground, size, i, 2, 1, 20, dirtDark);
            }

            FillRect(ground, size, 0, 24, 32, 8, grass);
            FillRect(ground, size, 0, 24, 32, 1, grassDark);
            FillRect(ground, size, 0, 28, 32, 1, new Color32(104, 195, 106, 255));
            Save(ground, size, size, artDir, "ground", 32);

            // Coin: gold disc with a light centre.
            int coinSize = 16;
            Color32[] coin = New(coinSize * coinSize, clear);
            Color32 gold = new Color32(255, 200, 40, 255);
            Color32 goldDark = new Color32(212, 150, 18, 255);
            Color32 goldLight = new Color32(255, 240, 160, 255);
            for (int y = 0; y < coinSize; y++)
            {
                for (int x = 0; x < coinSize; x++)
                {
                    float dx = x + 0.5f - 8f;
                    float dy = y + 0.5f - 8f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d <= 7.2f)
                    {
                        coin[y * coinSize + x] = d > 5.6f ? goldDark : gold;
                    }

                    if (d <= 3.2f)
                    {
                        coin[y * coinSize + x] = goldLight;
                    }
                }
            }

            Save(coin, coinSize, coinSize, artDir, "coin", 32);

            // Wall: brick pattern without the grass cap so vertical tiling reads as a wall.
            Color32 brick = new Color32(122, 82, 52, 255);
            Color32 brickDark = new Color32(94, 62, 38, 255);
            Color32[] wall = New(size * size, brick);
            for (int row = 0; row < 4; row++)
            {
                FillRect(wall, size, 0, row * 8, 32, 1, brickDark);
                int offset = row % 2 == 0 ? 0 : 8;
                for (int x = offset; x < 32; x += 16)
                {
                    FillRect(wall, size, x, row * 8, 1, 8, brickDark);
                }
            }

            Save(wall, size, size, artDir, "wall", 32);

            // Goal: door the player has to reach.
            int goalW = 32, goalH = 48;
            Color32[] goal = New(goalW * goalH, clear);
            Color32 frame = new Color32(120, 72, 36, 255);
            Color32 frameDark = new Color32(86, 50, 24, 255);
            Color32 inner = new Color32(38, 132, 96, 255);
            FillRect(goal, goalW, 2, 0, 28, 46, frameDark);
            FillRect(goal, goalW, 3, 0, 26, 45, frame);
            FillRect(goal, goalW, 6, 0, 20, 40, inner);
            FillRect(goal, goalW, 7, 1, 18, 38, new Color32(46, 156, 112, 255));
            FillCircle(goal, goalW, goalH, 24, 20, 3, new Color32(255, 214, 64, 255));
            Save(goal, goalW, goalH, artDir, "goal", 32);

            AssetDatabase.Refresh();
            return "generated 5 sprites in Assets/Art";
        }

        private static Color32[] New(int count, Color32 fill)
        {
            Color32[] pixels = new Color32[count];
            for (int i = 0; i < count; i++)
            {
                pixels[i] = fill;
            }

            return pixels;
        }

        private static void FillRect(Color32[] pixels, int width, int x, int y, int rectWidth, int rectHeight, Color32 color)
        {
            for (int yy = y; yy < y + rectHeight; yy++)
            {
                for (int xx = x; xx < x + rectWidth; xx++)
                {
                    if (xx >= 0 && yy >= 0 && xx < width && yy * width + xx < pixels.Length)
                    {
                        pixels[yy * width + xx] = color;
                    }
                }
            }
        }

        private static void FillCircle(Color32[] pixels, int width, int height, int cx, int cy, int radius, Color32 color)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    bool inside = (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius;
                    if (inside && x >= 0 && y >= 0 && x < width && y < height)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        private static void Save(Color32[] pixels, int width, int height, string directory, string fileName, int pixelsPerUnit)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(directory, fileName + ".png"), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            string assetPath = "Assets/Art/" + fileName + ".png";
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }
}
