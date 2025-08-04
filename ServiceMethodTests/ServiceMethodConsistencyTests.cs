using Common;
using NUnit.Framework;

namespace ServiceMethodTests
{
    [TestFixture]
    public class ServiceMethodConsistencyTests
    {
        [Test]
        public void InterfaceMethods_ShouldMatch_EnumValues()
        {
            // Arrange
            var enumValues = Enum.GetValues<ServiceMethodType>()
                .Select(e => e.ToString())
                .ToHashSet();

            var interfaceMethods = typeof(IEchoService)
                .GetMethods()
                .Select(m => m.Name)
                .ToHashSet();

            // Act & Assert
            var missingInEnum = interfaceMethods.Except(enumValues).ToList();
            var missingInInterface = enumValues.Except(interfaceMethods).ToList();

            // Build error message if there are mismatches
            var errors = new List<string>();
            
            if (missingInEnum.Any())
            {
                errors.Add($"Interface methods missing from enum: {string.Join(", ", missingInEnum)}");
            }
            
            if (missingInInterface.Any())
            {
                errors.Add($"Enum values missing from interface: {string.Join(", ", missingInInterface)}");
            }

            // Assert that there are no mismatches
            Assert.That(errors.Count, Is.EqualTo(0), 
                $"Service method mismatch detected:\n{string.Join("\n", errors)}");
        }

        [Test]
        public void EnumValues_ShouldBe_ValidInterfaceMethods()
        {
            // Arrange
            var enumValues = Enum.GetValues<ServiceMethodType>();
            var interfaceType = typeof(IEchoService);

            // Act & Assert
            foreach (var enumValue in enumValues)
            {
                var methodName = enumValue.ToString();
                var method = interfaceType.GetMethod(methodName);
                
                Assert.That(method, Is.Not.Null, $"Method {methodName} not found in interface");
                Assert.That(method!.Name, Is.EqualTo(methodName));
            }
        }

        [Test]
        public void InterfaceMethods_ShouldHave_CorrespondingEnumValues()
        {
            // Arrange
            var interfaceMethods = typeof(IEchoService)
                .GetMethods()
                .Select(m => m.Name)
                .ToList();

            var enumValues = Enum.GetValues<ServiceMethodType>()
                .Select(e => e.ToString())
                .ToList();

            // Act & Assert
            foreach (var methodName in interfaceMethods)
            {
                Assert.That(enumValues, Does.Contain(methodName), 
                    $"Enum missing method: {methodName}");
            }
        }

        [Test]
        public void EnumValues_ShouldNotBe_Empty()
        {
            // Arrange & Act
            var enumValues = Enum.GetValues<ServiceMethodType>();

            // Assert
            Assert.That(enumValues, Is.Not.Empty, "ServiceMethodType enum should have at least one value");
            Assert.That(enumValues.Length, Is.GreaterThan(0), "ServiceMethodType enum should have at least one value");
        }

        [Test]
        public void InterfaceMethods_ShouldNotBe_Empty()
        {
            // Arrange & Act
            var interfaceMethods = typeof(IEchoService).GetMethods();

            // Assert
            Assert.That(interfaceMethods, Is.Not.Empty, "IEchoService interface should have at least one method");
            Assert.That(interfaceMethods.Length, Is.GreaterThan(0), "IEchoService interface should have at least one method");
        }
    }
} 