using System;

namespace SonarQubeTest
{
    public class SonarQubeExample
    {
        // Hardcoded credentials (Security Issue)
        private const string Password = "123456"; // Noncompliant: Sensitive data should not be hardcoded

        // Unused variable (Code Smell)
        private int unusedVariable = 42;

        // Magic Number (Code Smell)
        public int CalculateArea(int radius)
        {
            return radius * radius * 3; // Noncompliant: Should use Math.PI instead of a magic number
        }

        // Method with no implementation (Dead Code)
        public void UnusedMethod() 
        {
            // TODO: Implement this method (but it's left empty, which SonarQube may flag)
        }

        // Naming convention issue (Class members should be PascalCase)
        public void badMethodName()
        {
            Console.WriteLine("This method name does not follow proper C# naming conventions.");
        }

        // Null reference issue (Potential Bug)
        public void NullReferenceCheck()
        {
            string text = null;
            Console.WriteLine(text.Length); // Noncompliant: Possible NullReferenceException
        }
    }
}