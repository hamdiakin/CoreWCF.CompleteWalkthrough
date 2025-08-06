using System.Text.Json.Serialization;

namespace Common.Animals
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Dog))]
    [JsonDerivedType(typeof(Bird))]
    [JsonDerivedType(typeof(Mouse))]
    [JsonDerivedType(typeof(Cat))]
    public class Animal
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Species { get; set; } = string.Empty;
        public AnimalType Type { get; set; }
        
        // Parameterless constructor for JSON deserialization
        public Animal() { }
        
        public virtual string MakeSound()
        {
            return "Unknown sound";
        }
        
        public virtual string GetInfo() => $"{Name} is a {Age} year old {Species} ({Type})";
    }
}