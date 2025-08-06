using System.Text.Json.Serialization;

namespace Common.Animals
{
    public class Mouse : Animal
    {
        public double Size { get; set; } // in cm
        public bool IsNocturnal { get; set; } = true;
        public string FavoriteFood { get; set; } = "Cheese";

        // Parameterless constructor for JSON deserialization
        [JsonConstructor]
        public Mouse() { }
        
        public override string MakeSound()
        {
            return "Squeak! Squeak!";
        }
        
        public override string GetInfo()
        {
            return $"{base.GetInfo()} - Size: {Size}cm, Nocturnal: {IsNocturnal}, Loves: {FavoriteFood}";
        }
        
        public string Hide()
        {
            return $"{Name} scurries into a tiny hole!";
        }
    }
}