using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;

internal class SpriteSlicer : EditorWindow
{
    /*

                Attention! After the sprites are sliced,
                they will be stacked in the Resources/ToSlice folder

                Editor path = Window -> SpriteSlicer

    */

    private List<Sprite> sprites = new List<Sprite>();
    private ReorderableList reorderableList;

    private Vector2 scrollPosition;

    #region Settings Variables
    private bool settings = false;

    private int pixelsPerUnit = 100;
    private int sliceWidth = 8;
    private int sliceHeight = 8;
    private int[] sliceOptions = new int[] { 8, 16, 24, 32, 48, 64, 96, 128, 256, 512 };
    private bool showAdvancedSettings = false;

    private Vector2 pivot = new Vector2(0.5f, 0.5f);

    private FilterMode filterMode = FilterMode.Point;
    #endregion

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

        GUILayout.Space(10);

        SpriteSettings();

        DrawSpriteList();

        GUILayout.Space(20);

        #region Clear All Sprites Button
        if (sprites.Count > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear All", GUILayout.Width(100), GUILayout.Height(20)))
            {
                sprites.Clear();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
        #endregion

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

        sprites.Clear();
        reorderableList.list = sprites;

        Repaint();
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
            ti.filterMode = filterMode;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spritePixelsPerUnit = pixelsPerUnit;

            List<SpriteMetaData> newData = new List<SpriteMetaData>();
            Texture2D spriteSheet = spriteSheets[z] as Texture2D;

            for (int i = 0; i < spriteSheet.width; i += sliceWidth)
            {
                for (int j = spriteSheet.height; j > 0; j -= sliceHeight)
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.pivot = pivot;
                    smd.alignment = 9; //Custom
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

        if (!showAdvancedSettings)
        {
            DrawSimpleSliders();
        }
        else
        {
            DrawAdvancedSliders();
        }

        GUIContent advancedToogleContent = new GUIContent("Advanced", "Enable advanced options for manual slice width and height.");
        showAdvancedSettings = EditorGUILayout.Toggle(advancedToogleContent, showAdvancedSettings);

        GUILayout.Space(5);

        GUILayout.Label("Quick Selection");

        ButtonGroup(buttonCount: 4, widthValue: 15, labelFormat: "{0}x{0}", onButtonClick: size =>
        {
            sliceWidth = sliceHeight = size;
        });

    }

    #region Slice Settings Methods
    private void DrawSimpleSliders()
    {
        sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, sliceOptions[0], sliceOptions[^1]);
        sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, sliceOptions[0], sliceOptions[^1]);

        sliceWidth = RoundToNearestOption(sliceWidth);
        sliceHeight = RoundToNearestOption(sliceHeight);
    }

    private void DrawAdvancedSliders()
    {
        sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, 1, 512);
        sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, 1, 512);
    }

    private int RoundToNearestOption(int value)
    {
        int closestValue = sliceOptions[0];
        float minDifference = Mathf.Abs(value - closestValue);

        for (int i = 0; i < sliceOptions.Length; i++)
        {
            int option = sliceOptions[i];
            float difference = Mathf.Abs(value - option);
            if (difference < minDifference)
            {
                minDifference = difference;
                closestValue = option;
            }
        }

        return closestValue;
    }
    #endregion

    private void SpriteSettings()
    {
        settings = EditorGUILayout.Toggle("Settings", settings);
        if (settings)
        {
            #region Pivot
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                pivot = EditorGUILayout.Vector2Field("Pivot", pivot, GUILayout.Width(200));
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Pixels Per Unit
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Pixels Per Unit", GUILayout.Width(120));
                pixelsPerUnit = EditorGUILayout.IntSlider(pixelsPerUnit, 1, 100);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            ButtonGroup(buttonCount: 3, widthValue: 12, labelFormat: "{0}", onButtonClick: size => pixelsPerUnit = size);

            EditorGUILayout.EndHorizontal();
            #endregion

            #region Filter Mode
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Filter Mode", GUILayout.Width(120));
                filterMode = (FilterMode)EditorGUILayout.EnumPopup(filterMode, GUILayout.Width(200));
            }
            EditorGUILayout.EndHorizontal();
            #endregion
        }
    }

    private void ButtonGroup(int buttonCount, int widthValue, string labelFormat, System.Action<int> onButtonClick)
    {
        EditorGUILayout.BeginHorizontal();

        float buttonWidth = (position.width - widthValue) / buttonCount;

        for (int i = 1; i <= buttonCount; i++)
        {
            int size = (int)Mathf.Pow(2, i + 3);
            if (GUILayout.Button(string.Format(labelFormat, size), GUILayout.Width(buttonWidth)))
            {
                onButtonClick(size);
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
