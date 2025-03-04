using System.Reflection;
using UnityEngine;

namespace RuneTablet
{
    public static class Helper
    {
        public static Sprite GetSprite(string fileName)
        {
            // Read the image from the embedded resource into a byte array
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);

            byte[] imageBytes = new byte[stream.Length];
            stream.Read(imageBytes, 0, imageBytes.Length);

            // Create a new texture and load the image data into it
            Texture2D tex = new(1, 1);
            tex.LoadImage(imageBytes);

            // Create a new sprite from the texture
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public static string GetHashedName(MonoBehaviour component)
        {
            var position = component.transform.position;
            var hash = ((int)position.x * 31) + ((int)position.z * 37);
            return component.name + "-" + hash.ToString();
        }
        public static string GetPinName(string locationName)
        {
            return locationName switch {
                "Crypt2" or "Crypt3" or "Crypt4" => "Burial Chambers",
                "Mistlands_DvergrTownEntrance1" or "Mistlands_DvergrTownEntrance2" => "Infested Mine",
                "Hildir_crypt" => "Smouldering Tomb",
                "Hildir_cave" => "Howling Cavern",
                "Hildir_plainsfortress" => "Sealed Tower",
                "MountainCave02" => "Frost Caves",
                "GoblinCamp2" => "Goblin Camp",
                "SunkenCrypt4" => "Sunken Crypt",
                "StoneTower1" or "StoneTower3" => "Stone Tower",
                _ => "Dungeon",
            };
        }
    }
}
