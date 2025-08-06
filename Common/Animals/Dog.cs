using System.Text.Json.Serialization;

namespace Common.Animals
{
    public class Dog : Animal
    {
        public string Breed { get; set; } = string.Empty;
        public bool IsGoodBoy { get; set; } = true;

        // Parameterless constructor for JSON deserialization
        [JsonConstructor]
        public Dog() { }
        
        public override string MakeSound()
        {
            return "Woof! Woof!";
        }
        
        public override string GetInfo()
        {
            return $"{base.GetInfo()} - Breed: {Breed}, Good Boy: {IsGoodBoy}";
        }
        
        public string Fetch()
        {
            return $"{Name} fetches the ball!";
        }
    }
}