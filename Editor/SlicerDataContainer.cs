using System.Collections.Generic;
using UnityEngine;

namespace Alihan4108.SpriteSlicer
{
    [System.Serializable]
    public class SlicerDataContainer : ScriptableObject
    {
        public List<Sprite> sprites = new List<Sprite>();
    }
}
