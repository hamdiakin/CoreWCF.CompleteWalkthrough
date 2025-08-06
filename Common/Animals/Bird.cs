using Newtonsoft.Json;

namespace Common.Animals
{
    public class Bird : Animal
    {
        public double WingSpan { get; set; }
        public bool CanFly { get; set; } = true;
        public string FeatherColor { get; set; } = string.Empty;

        // Parameterless constructor for JSON deserialization
        public Bird() { }
        
        public override string MakeSound()
        {
            return "Tweet! Chirp!";
        }
        
        public override string GetInfo()
        {
            return $"{base.GetInfo()} - Wingspan: {WingSpan}cm, Can Fly: {CanFly}, Feathers: {FeatherColor}";
        }
        
        public string Fly()
        {
            return CanFly ? $"{Name} soars through the sky!" : $"{Name} cannot fly.";
        }
    }
}