using Newtonsoft.Json;

namespace Common.Animals
{
    public class Cat : Animal
    {
        public string Color { get; set; } = string.Empty;
        public bool IsIndoor { get; set; } = true;
        public int LivesRemaining { get; set; } = 9;

        // Parameterless constructor for JSON deserialization
        public Cat() { }
        
        public override string MakeSound()
        {
            return "Meow! Purr...";
        }
        
        public override string GetInfo()
        {
            return $"{base.GetInfo()} - Color: {Color}, Indoor: {IsIndoor}, Lives: {LivesRemaining}";
        }
        
        public string Hunt()
        {
            return $"{Name} stalks its prey silently...";
        }
    }
}