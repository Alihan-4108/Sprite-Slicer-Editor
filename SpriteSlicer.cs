using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class SpriteSlicer : EditorWindow
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
    private int pixelsPerUnit = 100;
    private int sliceWidth = 8;
    private int sliceHeight = 8;
    private int[] sliceOptions = new int[] { 8, 16, 24, 32, 48, 64, 96, 128, 256, 512 };

    private bool showAdvancedSettings = false;
    private bool showSliceSettings = true;
    private bool showSpriteSettings = true;

    private Vector2 pivot = new Vector2(0.5f, 0.5f);

    private FilterMode filterMode = FilterMode.Point;

    //Sprite Naming
    public enum SpriteNamingScheme
    {
        PrefixNumber,    // Prefix + number for example sprite_1
        PrefixParenthesesNumber,  // Prefix + number in parentheses for example sprite_(1)
        PrefixNumberWithLeadingZeros  // Prefix + number filled with zeros for example sprite001
    }
    private SpriteNamingScheme namingScheme = SpriteNamingScheme.PrefixNumber;
    private string spritePrefix = "sprite_";
    private int leadingZeros = 3;
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

        showSliceSettings = EditorGUILayout.Foldout(showSliceSettings, "Slice Settings", true, EditorStyles.foldoutHeader);
        if (showSliceSettings) SliceSettings();

        GUILayout.Space(10);

        showSpriteSettings = EditorGUILayout.Foldout(showSpriteSettings, "Sprite Settings", true, EditorStyles.foldoutHeader);
        if (showSpriteSettings) SpriteSettings();

        DrawSpriteList();

        GUILayout.Space(20);

        #region Clear All Sprites Button
        if (sprites.Count > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Original color save
            Color originalColor = GUI.backgroundColor;

            // Clear All Button - Kırmızı renk
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Clear All", GUILayout.Width(100), GUILayout.Height(20)))
            {
                sprites.Clear();
            }

            GUI.backgroundColor = originalColor;

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
        #endregion

        #region Slice Button
        Color originalSliceColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Slice", GUILayout.Height(30)))
        {
            Slice();
        }

        GUI.backgroundColor = originalSliceColor;
        #endregion

        EditorGUILayout.EndScrollView();

        HandleDragAndDrop();
    }

    private void Slice()
    {
        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("No Sprites", "Please add at least one sprite to slice.", "OK");
            return;
        }

        foreach (var sprite in sprites)
        {
            if (sprite == null) continue;

            string path = AssetDatabase.GetAssetPath(sprite);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (tex != null)
            {
                SpriteSlicingProcessor.Slice(tex, path, pivot, sliceWidth, sliceHeight, pixelsPerUnit, filterMode, namingScheme, spritePrefix, leadingZeros);
            }
        }

        AssetDatabase.Refresh();

        sprites.Clear();
        reorderableList.list = sprites;
        Repaint();

        Debug.Log("Done Slicing!");
    }

    private void SliceSettings()
    {
        SliceSettingsDrawer.Draw(ref sliceWidth, ref sliceHeight, sliceOptions, ref showAdvancedSettings);
    }

    private void SpriteSettings()
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

        #region Sprite Naming Scheme
        GUILayout.Space(15);
        namingScheme = (SpriteNamingScheme)EditorGUILayout.EnumPopup("Naming Scheme", namingScheme);

        if (namingScheme == SpriteNamingScheme.PrefixNumberWithLeadingZeros)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Leading Zeros", GUILayout.Width(120));
                leadingZeros = EditorGUILayout.IntSlider(leadingZeros, 1, 5);
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Sprite Prefix
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Sprite Prefix", GUILayout.Width(120));
            spritePrefix = EditorGUILayout.TextField(spritePrefix, GUILayout.Width(200));
        }
        EditorGUILayout.EndHorizontal();
        #endregion
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