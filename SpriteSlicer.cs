using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

public class SpriteSlicer : EditorWindow
{
    private int sliceWidth = 64;
    private int sliceHeight = 64;
    private string folderName = "ToSlice";


    [MenuItem("Window/EditorHelper/SliceSprites")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSlicer>("Sprite Slicer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Slicer Settings", EditorStyles.boldLabel);

        sliceWidth = EditorGUILayout.IntField("Slice Width", sliceWidth);
        sliceHeight = EditorGUILayout.IntField("Slice Height", sliceHeight);
        //folderName = EditorGUILayout.TextField("Folder Path", folderName);


        GUILayout.Space(20);
        GUILayout.Label("Create New Folder", EditorStyles.boldLabel);

        folderName = EditorGUILayout.TextField("Klasör Adý", folderName);

        if (GUILayout.Button("Create Folder"))
        {
            CreateFileInResources();
        }


        if (GUILayout.Button("Slice Sprites"))
        {
            SliceSprites();
        }
    }

    private void SliceSprites()
    {
        // Change the below for the with and height dimensions of each sprite within the spritesheets

        // Change the below for the path to the folder containing the sprite sheets (warning: not tested on folders containing anything other than just spritesheets!)
        // Ensure the folder is within 'Assets/Resources/' (the below example folder's full path within the project is 'Assets/Resources/ToSlice')

        Object[] spriteSheets = Resources.LoadAll(folderName, typeof(Texture2D)); //ToSlice klasörü içine atýlan bütün Texture2D objelerini bul ve dizinin içine at
        if (spriteSheets.Length != 0)
        {
            Debug.Log("spriteSheets.Length: " + spriteSheets.Length);
        }
        else
        {
            Debug.LogWarning("There are no sprites in the folder");
        }

        for (int z = 0; z < spriteSheets.Length; z++)
        {
            Debug.Log("z: " + z + " spriteSheets[z]: " + spriteSheets[z]);

            string path = AssetDatabase.GetAssetPath(spriteSheets[z]); //Textureleri yolunu bul
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter; //Bu kod parçasý, path deðiþkeni ile belirtilen texture dosyasýnýn import ayarlarýný TextureImporter nesnesi üzerinden kontrol etmenizi saðlar.
            ti.isReadable = true; //Textureyi okunabilir yap
            ti.spriteImportMode = SpriteImportMode.Multiple; //Sprite modunu multiple yap

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

    private void CreateFileInResources()
    {
        string resourcesYolu = Path.Combine(Application.dataPath, "Resources");
        string klasorYolu = Path.Combine(resourcesYolu, folderName);

        // Resources klasörünün var olduðundan emin ol
        if (!Directory.Exists(resourcesYolu))
        {
            Directory.CreateDirectory(resourcesYolu);
        }

        // Klasörü oluþtur
        Directory.CreateDirectory(klasorYolu);
        AssetDatabase.Refresh();
        Debug.Log("File Created");
    }
}
