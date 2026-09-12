using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // A solid-color square sprite generated at runtime, so the prototype is visible/playable
    // without needing imported art yet.
    public static class ProceduralSprite
    {
        static Sprite _sharedSquare;

        public static Sprite Square()
        {
            if (_sharedSquare != null) return _sharedSquare;

            var texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            _sharedSquare = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _sharedSquare;
        }
    }
}
