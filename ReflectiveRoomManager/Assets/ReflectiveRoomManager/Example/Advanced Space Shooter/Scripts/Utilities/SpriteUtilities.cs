using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace Examples.SpaceShooter.Utilities
{
    public static class SpriteUtilities
    {
        public static byte[] ConvertSpriteToByte(Sprite sprite)
        {
            var data = new TextureConvertData
            {
                textureBytes = sprite.texture.GetRawTextureData(),
                width = sprite.texture.width,
                height = sprite.texture.height
            };

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();

            bf.Serialize(ms, data);

            var bytes = ms.ToArray();

            return bytes;
        }

        public static Sprite ConvertByteToSprite(byte[] bytes)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream(bytes);

            var data = (TextureConvertData)bf.Deserialize(ms);

            var rect = new Rect(0, 0, data.width, data.height);
            var pivot = new Vector2(0.5f, 0.5f);
            
            var texture = new Texture2D((int)data.width, (int)data.height,
                TextureFormat.RGBA32, false);
            
            texture.LoadRawTextureData(data.textureBytes);
            texture.Apply();
            
            var sprite = Sprite.Create(texture, rect, pivot);

            return sprite;
        }
    }

    [System.Serializable]
    public struct TextureConvertData
    {
        public byte[] textureBytes;
        public float width;
        public float height;
    }
}