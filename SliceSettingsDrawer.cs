using UnityEditor;
using UnityEngine;

public static class SliceSettingsDrawer
{
    public static void Draw(ref int sliceWidth, ref int sliceHeight, int[] sliceOptions, ref bool showAdvancedSettings)
    {
        GUILayout.Label("Sprite Slicer Settings", EditorStyles.boldLabel);

        GUILayout.Space(5);

        if (!showAdvancedSettings)
        {
            sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, sliceOptions[0], sliceOptions[^1]);
            sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, sliceOptions[0], sliceOptions[^1]);

            sliceWidth = RoundToNearestOption(sliceWidth, sliceOptions);
            sliceHeight = RoundToNearestOption(sliceHeight, sliceOptions);
        }
        else
        {
            sliceWidth = EditorGUILayout.IntSlider("Slice Width", sliceWidth, 1, 512);
            sliceHeight = EditorGUILayout.IntSlider("Slice Height", sliceHeight, 1, 512);
        }

        GUIContent advancedToggle = new GUIContent("Advanced", "Enable advanced options for manual slice width and height.");
        showAdvancedSettings = EditorGUILayout.Toggle(advancedToggle, showAdvancedSettings);
    }

    // Yuvarlama fonksiyonu
    private static int RoundToNearestOption(int value, int[] sliceOptions)
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
}