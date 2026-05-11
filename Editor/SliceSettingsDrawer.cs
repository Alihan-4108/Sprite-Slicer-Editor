using UnityEditor;
using UnityEngine;

namespace Alihan4108.SpriteSlicer
{
    public static class SliceSettingsDrawer
    {
        public static void Draw(ref int sliceWidth, ref int sliceHeight, int[] sliceOptions, ref bool useCustomSize)
        {
            GUILayout.Label("Sprite Slicer Settings", EditorStyles.boldLabel);

            GUILayout.Space(5);

            // Kullanıcıya 'Custom Size' seçeneğini gösteren bir buton ekleyelim
            useCustomSize = EditorGUILayout.Toggle("Enable Predefined Sizes", useCustomSize);

            // Eğer Custom Size aktifse, sliceWidth ve sliceHeight değerlerini sliceOptions dizisi ile sınırlayacağız
            if (useCustomSize)
            {
                sliceWidth = EditorGUILayout.IntPopup("Slice Width", sliceWidth, GetOptionLabels(sliceOptions), sliceOptions);
                sliceHeight = EditorGUILayout.IntPopup("Slice Height", sliceHeight, GetOptionLabels(sliceOptions), sliceOptions);
            }
            else
            {
                // Eğer Custom Size seçilmediyse, daha geniş bir aralıkta dilimleme yapılabilir
                sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, 1, 512);
                sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, 1, 512);
            }
        }

        // Slice Options dizisindeki sayılara karşılık gelen etiketleri döndüren bir yardımcı fonksiyon
        private static string[] GetOptionLabels(int[] options)
        {
            string[] labels = new string[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                labels[i] = options[i].ToString();  // Etiketler, sayıları string'e çevirerek gösterir
            }
            return labels;
        }
    }

}