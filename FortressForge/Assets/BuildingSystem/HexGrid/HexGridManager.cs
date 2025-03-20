using UnityEngine;
using System.Collections.Generic;

public class HexGridManager : MonoBehaviour
{
    // Singleton-Instanz, damit von überall zugegriffen werden kann
    public static HexGridManager Instance { get; private set; }

    // Alle registrierten Grids werden hier abgelegt
    private Dictionary<int, HexGridData> allGrids = new Dictionary<int, HexGridData>();

    private int nextGridId = 0;

    private void Awake()
    {
        // Einfacher Singleton-Mechanismus
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Erstellt ein neues HexGrid mit angegebener Größe und Ursprung.
    /// OwnerId kann ein Spielername, eine Netzwerk-ID o. Ä. sein.
    /// </summary>
    public HexGridData CreateHexGrid(Vector3 origin, int radius, int height, string ownerId, float tileSize, float tileHeight)
    {
        HexGridData newGrid = new HexGridData(nextGridId, origin, radius, height, tileSize, tileHeight);
        newGrid.OwnerId = ownerId;

        allGrids.Add(nextGridId, newGrid);
        nextGridId++;

        return newGrid;
    }

    /// <summary>
    /// Gibt das HexGrid mit passender ID zurück (oder null, wenn nicht vorhanden).
    /// </summary>
    public HexGridData GetGridById(int gridId)
    {
        if (allGrids.TryGetValue(gridId, out HexGridData grid))
        {
            return grid;
        }
        return null;
    }

    // Hier kann man später z.B. RemoveGrid(gridId) ergänzen,
    // um ein HexGrid zu löschen/abzumelden.
}