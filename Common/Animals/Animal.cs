using Newtonsoft.Json;

namespace Common.Animals
{
    public class Animal
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Species { get; set; } = string.Empty;
        public AnimalType Type { get; set; } = AnimalType.Dog;
        
        // Parameterless constructor for JSON deserialization
        public Animal() { }
        
        public virtual string MakeSound()
        {
            return "Unknown sound";
        }
        
        public virtual string GetInfo() => $"{Name} is a {Age} year old {Species} ({Type})";
    }
}