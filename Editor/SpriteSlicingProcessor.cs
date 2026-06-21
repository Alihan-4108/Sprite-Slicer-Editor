using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;

namespace Alihan4108.SpriteSlicer
{
    public class SpriteSlicingProcessor
    {
        public static void Slice(Texture2D texture, string path, Vector2 pivot, int sliceWidth, int sliceHeight, int pixelsPerUnit,
                FilterMode filterMode, SpriteSlicerWindow.SpriteNamingScheme namingScheme, string prefix, int leadingZeros, bool appendSlicedToParentName)
        {
            string parentSpriteName = System.IO.Path.GetFileNameWithoutExtension(path);
            string newParentName = appendSlicedToParentName ? parentSpriteName + " (Sliced)" : parentSpriteName;
            string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), newParentName + ".png");

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;

            ti.isReadable = true;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = filterMode;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spritePixelsPerUnit = pixelsPerUnit;

            if (appendSlicedToParentName)
            {
                AssetDatabase.RenameAsset(path, newParentName);
                AssetDatabase.Refresh();
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(ti);
            dataProvider.InitSpriteEditorDataProvider();

            List<SpriteRect> spriteRects = new List<SpriteRect>();
            List<SpriteNameFileIdPair> nameFileIdPairs = new List<SpriteNameFileIdPair>();

            int spriteIndex = 0;
            for (int y = texture.height; y > 0; y -= sliceHeight)
            {
                for (int x = 0; x < texture.width; x += sliceWidth)
                {
                    Rect rect = new Rect(x, y - sliceHeight, sliceWidth, sliceHeight);
                    string name = GenerateSpriteName(spriteIndex++, prefix, namingScheme, leadingZeros);

                    var spriteRect = new SpriteRect()
                    {
                        name = name,
                        spriteID = GUID.Generate(),
                        rect = rect,
                        alignment = SpriteAlignment.Custom,
                        pivot = pivot
                    };

                    spriteRects.Add(spriteRect);
                    nameFileIdPairs.Add(new SpriteNameFileIdPair(name, spriteRect.spriteID));
                }
            }

            dataProvider.SetSpriteRects(spriteRects.ToArray());

            var nameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);

            dataProvider.Apply();
            ti.SaveAndReimport();
        }

        private static string GenerateSpriteName(int index, string prefix, SpriteSlicerWindow.SpriteNamingScheme scheme, int leadingZeros)
        {
            switch (scheme)
            {
                case SpriteSlicerWindow.SpriteNamingScheme.PrefixParenthesesNumber:
                    return $"{prefix}({index + 1})";
                case SpriteSlicerWindow.SpriteNamingScheme.PrefixNumberWithLeadingZeros:
                    return $"{prefix}{(index + 1).ToString($"D{leadingZeros}")}";
                default:
                    return $"{prefix}{index + 1}";
            }
        }
    }
}
