using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Alihan4108.SpriteSlicer
{
    public class SpriteSlicerWindow : EditorWindow
    {
        private SlicerDataContainer dataContainer;
        private ReorderableList reorderableList;

        private Vector2 scrollPosition;

        #region Settings Variables
        private int pixelsPerUnit = 100;
        private int sliceWidth = 8;
        private int sliceHeight = 8;
        private int[] sliceOptions = new int[] { 8, 16, 24, 32, 48, 64, 96, 128, 256, 512 };

        private bool showAdvancedSettings = true;
        private bool showSliceSettings = true;
        private bool showSpriteSettings = false;

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

        private bool appendSlicedToParentName = true;
        #endregion

        [MenuItem("Tools/SpriteSlicer")]
        public static void ShowWindow()
        {
            GetWindow<SpriteSlicerWindow>("Sprite Slicer");
        }

        private void OnEnable()
        {
            dataContainer = ScriptableObject.CreateInstance<SlicerDataContainer>();
            InitializeReorderableList();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

            showSliceSettings = EditorGUILayout.Foldout(showSliceSettings, "Slice Settings", true, EditorStyles.foldoutHeader);
            if (showSliceSettings) SliceSettings();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            showSpriteSettings = EditorGUILayout.Foldout(showSpriteSettings, "Sprite Settings", true, EditorStyles.foldoutHeader);
            if (showSpriteSettings) SpriteSettings();

            DrawSpriteList();

            GUILayout.Space(20);

            #region Clear All Sprites Button
            if (dataContainer.sprites.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                Color originalColor = GUI.backgroundColor;

                GUI.backgroundColor = Color.red;

                if (GUILayout.Button("Clear All", GUILayout.Width(100), GUILayout.Height(20)))
                {
                    dataContainer.sprites.Clear();
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
            if (dataContainer.sprites.Count == 0)
            {
                EditorUtility.DisplayDialog("No Sprites", "Please add at least one sprite to slice.", "OK");
                return;
            }

            foreach (var sprite in dataContainer.sprites)
            {
                if (sprite == null) continue;

                string path = AssetDatabase.GetAssetPath(sprite);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (tex != null)
                {
                    SpriteSlicingProcessor.Slice(tex, path, pivot, sliceWidth, sliceHeight, pixelsPerUnit, filterMode,
                                                       namingScheme, spritePrefix, leadingZeros, appendSlicedToParentName);
                }
            }

            AssetDatabase.Refresh();

            dataContainer.sprites.Clear();
            reorderableList.list = dataContainer.sprites;
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

                EditorGUILayout.LabelField(
                    new GUIContent("Pivot", "Defines the center point of each sliced sprite. (0,0) = bottom-left, (0.5,0.5) = center, (1,1) = top-right."),
                    GUILayout.Width(120)
                );

                // Vector2Field çizimi
                pivot = EditorGUILayout.Vector2Field(
                    GUIContent.none,
                    pivot,
                    GUILayout.Width(150)
                );

                // Son çizilen rect'i al
                Rect lastRect = GUILayoutUtility.GetLastRect();

                float buttonWidth = 75;
                float buttonHeight = lastRect.height + 2;

                Rect buttonRect = new Rect(lastRect.xMax + 5, lastRect.y - 1, buttonWidth, buttonHeight);
                if (GUI.Button(buttonRect, "Default"))
                {
                    pivot = new Vector2(0.5f, 0.5f);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            #region Pixels Per Unit
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("The Pixels Per Unit (PPU) value determines the physical size of the sprite in the scene. For example, 100 pixels = 1 Unity unit.", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent("Pixels Per Unit", "Number of pixels that equal one Unity unit."), GUILayout.Width(120));
                pixelsPerUnit = EditorGUILayout.IntSlider(pixelsPerUnit, 1, 100);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            ButtonGroup(buttonCount: 3, widthValue: 12, labelFormat: "{0}", onButtonClick: size => pixelsPerUnit = size);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            #endregion

            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            #region Filter Mode
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent("Filter Mode", "Determines how the sprite texture will appear when zoomed in or out."), GUILayout.Width(120));
                filterMode = (FilterMode)EditorGUILayout.EnumPopup(filterMode, GUILayout.Width(200));
            }

            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            #region Sprite Naming Scheme
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Choose the naming pattern for sliced sprites. This helps in organizing sprite assets.", MessageType.None);

            namingScheme = (SpriteNamingScheme)EditorGUILayout.EnumPopup(new GUIContent("Naming Scheme", "How the sliced sprites will be named."), namingScheme);

            if (namingScheme == SpriteNamingScheme.PrefixNumberWithLeadingZeros)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(new GUIContent("Leading Zeros", "Specifies how many digits should be used for numbering (e.g., 001, 002, 003)."), GUILayout.Width(120));
                    leadingZeros = EditorGUILayout.IntSlider(leadingZeros, 1, 5);
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            #region Sprite Prefix
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(new GUIContent("Sprite Prefix", "Text added as a prefix to the name of each sliced sprite."), GUILayout.Width(120));
                spritePrefix = EditorGUILayout.TextField(spritePrefix, GUILayout.Width(200));
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            #region Append '(Slice)' to Parent Sprite Name
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("If enabled, '(Slice)' will be added to the original sprite name to indicate that it has been sliced.", MessageType.None);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Add '(Slice)' to Parent Sprite Name", GUILayout.Width(202));
            appendSlicedToParentName = EditorGUILayout.Toggle(appendSlicedToParentName);
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
            reorderableList = new ReorderableList(dataContainer.sprites, typeof(Sprite), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Sprites"),

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index >= 0 && index < dataContainer.sprites.Count)
                    {
                        EditorGUI.BeginChangeCheck();

                        Sprite originalSprite = dataContainer.sprites[index];

                        Sprite newSprite = (Sprite)EditorGUI.ObjectField(
                            new Rect(rect.x, rect.y, rect.width - 30, EditorGUIUtility.singleLineHeight),
                            originalSprite, typeof(Sprite), false);

                        //Değişiklik olduğunda
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(dataContainer, "Change Sprite");
                            dataContainer.sprites[index] = newSprite;
                            EditorUtility.SetDirty(dataContainer);

                            if (newSprite == null)
                            {
                                Undo.RecordObject(dataContainer, "Remove Sprite");
                                dataContainer.sprites.RemoveAt(index);
                                EditorUtility.SetDirty(dataContainer);
                                return;
                            }
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
                            Undo.RecordObject(dataContainer, "Remove Sprite");
                            dataContainer.sprites.RemoveAt(index);
                            EditorUtility.SetDirty(dataContainer);
                        }
                    }
                },

                //Listenin "+" yani eleman ekleme butonuna tıklarsam
                onAddCallback = list =>
                {
                    Undo.RecordObject(dataContainer, "Add Sprite");
                    dataContainer.sprites.Add(null);
                    EditorUtility.SetDirty(dataContainer);
                },

                //listenin "-" yani eleman kaldırma butonuna tıklarsam
                onRemoveCallback = list =>
                {
                    if (list.index >= 0 && list.index < dataContainer.sprites.Count)
                    {
                        Undo.RecordObject(dataContainer, "Remove Sprite");
                        dataContainer.sprites.RemoveAt(list.index);
                        EditorUtility.SetDirty(dataContainer);
                    }
                }
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
            bool anyAdded = false;

            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                Sprite sprite = draggedObject as Sprite;

                if (sprite == null)
                {
                    sprite = ConvertTextureToSprite(draggedObject);
                }

                if (sprite != null)
                {
                    if (!dataContainer.sprites.Contains(sprite))
                    {
                        if (!anyAdded)
                        {
                            Undo.RecordObject(dataContainer, "Add Sprite Drag & Drop");
                            anyAdded = true;
                        }

                        dataContainer.sprites.Add(sprite);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Duplicate Sprite", $"The sprite '{sprite.name}' is already in the list. Duplicate entries are not allowed.", "OK");
                    }
                }
            }

            if (anyAdded)
                EditorUtility.SetDirty(dataContainer);
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

}
