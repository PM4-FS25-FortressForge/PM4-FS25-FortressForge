# C# /Unity Code-Richtlinien für Fortress Forge

## Namenskonventionen
- **Klassen**: PascalCase
- **Klassen Variablen**:
    - **Öffentliche Variablen**: PascalCase
    - **Private Variablen**: camelCase mit Unterstrich am Anfang
    - **Konstanten**: UPPERCASE_WITH_UNDERSCORES
- **Methoden**: PascalCase
    - **Lokale Variablen**: camelCase
    - **Parameter**: camelCase

- **Interfaces**: IPascalCase (INameDesInterfaces)
- **Abstrakte Klassen**: PascalCase
- **Namespaces**: PascalCase
- **Enums**: PascalCase

## Namespaces
Jedes Skript sollte in einem Namespace sein. Der Name setzt sich aus `FortressForge.SubFolder.SubSubFolder` zusammen. Für die Namenskonventionen der Subfolder wird *PascalCase* verwendet.
- **Beispiel**: `namespace FortressForge.Camera`
- **Beispiel**: `namespace FortressForge.Network`


## Case-Erklärung mit Beispielen
- **PascalCase**: Jedes Wort beginnt mit einem Großbuchstaben, keine Leerzeichen
    - **Beispiel**: `ThisIsPascalCase`
- **camelCase**: Das erste Wort beginnt mit einem Kleinbuchstaben, jedes weitere Wort beginnt mit einem Großbuchstaben, keine Leerzeichen
    - **Beispiel**: `thisIsCamelCase`
- **camelCase mit Unterstrich am Anfang**: Das erste Wort beginnt mit einem Kleinbuchstaben, jedes weitere Wort beginnt mit einem Großbuchstaben, keine Leerzeichen, Unterstrich am Anfang
    - **Beispiel**: `_thisIsCamelCaseWithUnderscore`
- **UPPERCASE_WITH_UNDERSCORES**: Alle Buchstaben sind Großbuchstaben, Wörter sind durch Unterstriche getrennt
    - **Beispiel**: `THIS_IS_UPPERCASE_WITH_UNDERSCORES`

### Beispiel-Klasse
```csharp
namespace ExampleNamespace
{
    public class ExampleClass : IExampleInterface
    {
        private int _exampleVariable;
        public int ExampleVariable { get; set; }

        public const int EXAMPLE_CONSTANT = 0;

        public void ExampleMethod(int exampleParameter)
        {
            int exampleLocalVariable = 0;
        }
    }
}
```
## Code-Struktur
- **Klammern**: Klammerung auf neuer Zeile
- **Beispiel**:
   ```csharp
   public void Method()
   {
       // Code
   }
   ```

- **Kommentare**:
    - **Sprache**: Englisch
    - **Methoden/Klassen Dokumentation**: XML-Dokumentation
        - **Beispiel**:
        ```csharp
        /// <summary>
        /// This is a summary of the method.
        /// </summary>
        /// <param name="param1">This is the first parameter.</param>
        /// <param name="param2">This is the second parameter.</param>
        /// <returns>This method returns a string.</returns>
        public string Method(int param1, int param2)
        {
            // Code
        }
        ```

## Ordnerstruktur
Alles Skripte werden in den `Assets/Scripts/Subfolder` Ordner gespeichert.
### Beispiel
```
├── Assets/
│   └── Scripts/
│       ├── Camera/
│       │   ├── CameraController.cs
│       │   └── CameraMovement.cs
│       ├── RessourceManager/
│       │   └── RessourceManager.cs
│       ├── Network/
│       │   └── NetworkManager.cs
│       └── UIMenu/
│           └── ViewManager.cs
```


## Quelle
- [Unity Code Styles](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity)
- [Microsoft C# Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments)
