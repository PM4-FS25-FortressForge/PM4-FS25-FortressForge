# PM4-FS25-FortressForge

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

## Branching Regeln
- **Branches**: Jedes Task sollte in einem eigenen Branch entwickelt werden
- **Branch-Namen**: Der Branch-Name sollte aus dem Task-Titel bestehen
- **Wichtig**: Vor dem Branch-Namen noch ein `Feature/` oder `Bug/` oder `Documentation/` -prefix hinzufügen (z.B. `Feature/Task-Title`)

## Scrum Regeln und Beschreibungen

### Scum Beschreibung
Was ist Scrum?
- Scrum ist ein agiles Framework, das Teams dabei hilft, komplexe Probleme zu lösen, indem es sich auf die kontinuierliche Verbesserung konzentriert, indem es auf die Prinzipien von Transparenz, Inspektion und Anpassung setzt.


### Aufgaben Aufbau
- **User Story**: Eine User Story ist eine kurze Beschreibung einer Funktion, die aus der Sicht des Benutzers geschrieben ist. Sie beschreibt, was der Benutzer tun kann und warum.
    - **Akzeptanzkriterien**: Die Akzeptanzkriterien sind die Bedingungen, die erfüllt sein müssen, damit die User Story als abgeschlossen betrachtet wird.
- **Task**: Ein Task ist eine Aufgabe, die erledigt werden muss, um die User Story zu vervollständigen.

Eine User Story wird durch den/die Entwickler in Tasks aufgeteilt. Jeder Task wird von einem Entwickler bearbeitet und sobald der Task abgeschlossen ist, wird er in den `main` Branch gemerged. (Auch wenn dabei die User Story noch nicht vollständig abgeschlossen ist)

### Sprint
- **Dauer**: 2 Wochen
- **Sprint Planning**: Am Anfang des Sprints wird ein Sprint Planning Meeting abgehalten, in dem die User Stories ausgewählt werden, die in diesem Sprint bearbeitet werden sollen.
- **Daily Standup**: Mehrmals in der Woche wird ein Standup Meeting abgehalten, in dem jeder Entwickler sagt, was er am die letzten Tage gemacht hat, was er heute machen wird und ob es irgendwelche Probleme gibt.
- **Sprint Review**: Am Ende des Sprints wird ein Sprint Review Meeting abgehalten, in dem die User Stories, die abgeschlossen wurden, präsentiert werden.
- **Sprint Retrospective**: Am Ende des Sprints wird ein Sprint Retrospective Meeting abgehalten, in dem das Team darüber spricht, was gut gelaufen ist, was nicht gut gelaufen ist und was verbessert werden kann.


## Quelle
- [Unity Code Styles](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity)
- [Microsoft C# Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments)
