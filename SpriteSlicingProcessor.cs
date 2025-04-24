using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpriteSlicingProcessor
{
    public static void Slice(Texture2D texture, string path, Vector2 pivot, int sliceWidth, int sliceHeight, int pixelsPerUnit,
                               FilterMode filterMode, SpriteSlicer.SpriteNamingScheme namingScheme, string prefix, int leadingZeros)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        if (ti == null) return;

        ti.isReadable = true;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.filterMode = filterMode;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.spritePixelsPerUnit = pixelsPerUnit;

        List<SpriteMetaData> newData = new List<SpriteMetaData>();

        int spriteIndex = 0;
        for (int x = 0; x < texture.width; x += sliceWidth)
        {
            for (int y = texture.height; y > 0; y -= sliceHeight)
            {
                Rect rect = new Rect(x, y - sliceHeight, sliceWidth, sliceHeight);
                string name = GenerateSpriteName(spriteIndex++, prefix, namingScheme, leadingZeros);

                newData.Add(new SpriteMetaData
                {
                    alignment = 9,
                    pivot = pivot,
                    name = name,
                    rect = rect
                });
            }
        }

        ti.spritesheet = newData.ToArray();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    private static string GenerateSpriteName(int index, string prefix, SpriteSlicer.SpriteNamingScheme scheme, int leadingZeros)
    {
        switch (scheme)
        {
            case SpriteSlicer.SpriteNamingScheme.PrefixParenthesesNumber:
                return $"{prefix}({index + 1})";
            case SpriteSlicer.SpriteNamingScheme.PrefixNumberWithLeadingZeros:
                return $"{prefix}{(index + 1).ToString($"D{leadingZeros}")}";
            default:
                return $"{prefix}{index + 1}";
        }
    }
}