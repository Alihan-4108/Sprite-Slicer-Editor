using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;

public class SpriteSlicer : EditorWindow
{
    /*

                Attention! After the sprites are sliced,
                they will be stacked in the Resources/ToSlice folder

                Editor path = Window -> SpriteSlicer

    */

    private int sliceWidth;
    private int sliceHeight;

    private List<Sprite> sprites = new List<Sprite>();
    private ReorderableList reorderableList;

    private Vector2 scrollPosition;

    [MenuItem("Window/SpriteSlicer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSlicer>("Sprite Slicer");
    }

    private void OnEnable()
    {
        InitializeReorderableList();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

        SliceSettings();
        DrawSpriteList();

        GUILayout.Space(20);

        #region Slice Button
        if (GUILayout.Button("Slice", GUILayout.Height(30)))
        {
            Slice();
        }
        #endregion

        EditorGUILayout.EndScrollView();

        HandleDragAndDrop();
    }

    private void Slice()
    {
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
                MoveSpriteToFolder(sprite, folderPath);
            }
        }

        AssetDatabase.Refresh();
        SpriteSliceCore();
    }

    private void SpriteSliceCore()
    {
        Object[] spriteSheets = Resources.LoadAll("ToSlice", typeof(Texture2D));
        if (spriteSheets.Length == 0)
        {
            Debug.LogWarning("No sprites found in the ToSlice folder");
            return;
        }

        for (int z = 0; z < spriteSheets.Length; z++)
        {
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

    private void MoveSpriteToFolder(Sprite sprite, string folderPath)
    {
        string spritePath = AssetDatabase.GetAssetPath(sprite);
        string spriteDirectory = Path.GetDirectoryName(spritePath);

        if (!spriteDirectory.EndsWith("ToSlice"))
        {
            string newSpritePath = Path.Combine(folderPath, Path.GetFileName(spritePath));
            AssetDatabase.CopyAsset(spritePath, newSpritePath);
            AssetDatabase.DeleteAsset(spritePath);
        }
    }

    private void SliceSettings()
    {
        GUILayout.Label("Sprite Slicer Settings", EditorStyles.boldLabel);

        sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, 1, 512);
        sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, 1, 512);

        GUILayout.Space(20);

        SliceSizeButtons();
    }

    private void SliceSizeButtons()
    {
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
    }

    private void DrawSpriteList()
    {
        GUILayout.Space(20);
        reorderableList.DoLayoutList();
    }

    private void InitializeReorderableList()
    {
        reorderableList = new ReorderableList(sprites, typeof(Sprite), true, true, true, true)
        {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Sprites"),
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index >= 0 && index < sprites.Count)
                {
                    EditorGUI.BeginChangeCheck();
                    sprites[index] = (Sprite)EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width - 30, EditorGUIUtility.singleLineHeight),
                        sprites[index], typeof(Sprite), false);

                    if (EditorGUI.EndChangeCheck() && sprites[index] == null)
                    {
                        sprites.RemoveAt(index);
                    }

                    Rect removeButtonRect = new Rect(rect.x + rect.width - 30, rect.y, 30, EditorGUIUtility.singleLineHeight);
                    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 12,
                        fixedWidth = 30,
                    };
                    if (GUI.Button(removeButtonRect, "X", buttonStyle))
                    {
                        sprites.RemoveAt(index);
                    }
                }
            },
            onAddCallback = list => sprites.Add(null),
            onRemoveCallback = list => sprites.RemoveAt(list.index)
        };
    }


    #region DragAndDropSystem
    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            AddDraggedObjectsToList();
            evt.Use();
        }

    }

    private void AddDraggedObjectsToList()
    {
        foreach (Object draggedObject in DragAndDrop.objectReferences)
        {
            Sprite sprite = draggedObject as Sprite;
            if (sprite == null)
            {
                sprite = ConvertTextureToSprite(draggedObject);
            }

            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
    }

    private Sprite ConvertTextureToSprite(Object draggedObject)
    {
        if (draggedObject is Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }
    #endregion
}
