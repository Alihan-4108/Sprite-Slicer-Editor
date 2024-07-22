using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SpriteSlicer : EditorWindow
{
    /*
    
                 Attention! After the sprites are sliced,
                 they will be stacked in the Resources/ToSlice folder
   
     */

    private int sliceWidth;
    private int sliceHeight;

    private List<Sprite> sprites = new List<Sprite>();

    private Vector2 scrollPosition;

    [MenuItem("Window/SliceSprites")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSlicer>("Sprite Slicer");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

        GUILayout.Label("Sprite Slicer Settings", EditorStyles.boldLabel);

        sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, 1, 512);
        sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, 1, 512);

        GUILayout.Space(20);

        #region Prepared sizing settings
        EditorGUILayout.BeginHorizontal();

        int buttonCount = 4;
        float buttonWidth = (position.width - 15) / buttonCount;

        for (int i = 1; i <= buttonCount; i++)
        {
            int size = (int)Mathf.Pow(2, i + 3);
            if (GUILayout.Button($"{size}x{size}", GUILayout.Width(buttonWidth)))
            {
                sliceWidth = sliceHeight = size;
            }
        }

        EditorGUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(20);

        #region Sprite Fields
        for (int i = 0; i < sprites.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            sprites[i] = (Sprite)EditorGUILayout.ObjectField(sprites[i], typeof(Sprite), false);

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                sprites.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Sprite Add Button
        if (GUILayout.Button("Add Sprite", GUILayout.Height(30)))
        {
            sprites.Add(null);
        }
        #endregion

        #region Slice Button
        if (GUILayout.Button("Slice", GUILayout.Height(30)))
        {
            Slice();
        }
        #endregion

        EditorGUILayout.EndScrollView();
    }

    private void Slice()
    {
        //Create folder path
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string folderPath = Path.Combine(resourcesPath, "ToSlice");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite sprite = sprites[i];

            if (sprite != null)
            {
                string spritePath = AssetDatabase.GetAssetPath(sprite);
                string spriteDirectory = Path.GetDirectoryName(spritePath);

                if (!spriteDirectory.EndsWith("ToSlice"))
                {
                    // Kopyalama işlemi
                    string newSpritePath = Path.Combine("Assets/Resources/ToSlice", Path.GetFileName(spritePath));
                    AssetDatabase.CopyAsset(spritePath, newSpritePath);
                    AssetDatabase.DeleteAsset(spritePath);
                }

            }
        }

        AssetDatabase.Refresh();

        SpriteSlice();
    }

    private void SpriteSlice()
    {
        Object[] spriteSheets = Resources.LoadAll("ToSlice", typeof(Texture2D));
        if (spriteSheets.Length == 0)
        {
            Debug.LogWarning("No sprites found in the ToSlice folder");
            return;
        }

        for (int z = 0; z < spriteSheets.Length; z++)
        {
            Debug.Log(spriteSheets[z]);

            string path = AssetDatabase.GetAssetPath(spriteSheets[z]);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.isReadable = true;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            List<SpriteMetaData> newData = new List<SpriteMetaData>();

            Texture2D spriteSheet = spriteSheets[z] as Texture2D;

            for (int i = 0; i < spriteSheet.width; i += sliceWidth)
            {
                for (int j = spriteSheet.height; j > 0; j -= sliceHeight)
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.pivot = new Vector2(0.5f, 0.5f);
                    smd.alignment = 9;
                    smd.name = (spriteSheet.height - j) / sliceHeight + ", " + i / sliceWidth;
                    smd.rect = new Rect(i, j - sliceHeight, sliceWidth, sliceHeight);

                    newData.Add(smd);
                }
            }

            ti.spritesheet = newData.ToArray();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        if (spriteSheets.Length != 0)
        {
            Debug.Log("Done Slicing!");
        }
    }
}
