using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referenzen für das GridView")]
    [SerializeField] private HexGridView gridViewPrefab;  // Prefab mit HexGridView-Script dran
    [SerializeField] private GameObject tilePrefab;       // Prefab, das pro Hexfeld instanziert wird
    
    private Dictionary<string, HexGridView> gridViewByPlayerId = new Dictionary<string, HexGridView>();
    void Start()
    {
        // 1) Grid für Spieler 1 erstellen
        HexGrid grid1 = HexGridManager.Instance.CreateHexGrid(
            new Vector3(800, 0, 800), 
            5, 
            2, 
            "Spieler 1", 
            10f, 
            10f);
        
        // 2) Ein HexGridView-Objekt in der Szene erzeugen
        HexGridView gridView = Instantiate(gridViewPrefab, new Vector3(800, 0, 800), Quaternion.identity);
        gridView.transform.SetParent(this.transform);
        gridViewByPlayerId.Add(grid1.OwnerId, gridView);
        
        // 3) HexGridView-Objekt bauen
        gridView.BuildGridView(tilePrefab, grid1);

        // 4) Grid für Spieler 2 erstellen
        HexGrid grid2 = HexGridManager.Instance.CreateHexGrid(
            new Vector3(300, 0, 300), 
            5, 
            2, 
            "Spieler 2", 
            10f, 
            10f);
        
        // 5) Ein weiteres HexGridView-Objekt in der Szene erzeugen
        HexGridView gridView2 = Instantiate(gridViewPrefab, new Vector3(300, 0, 300), Quaternion.identity);
        gridView2.transform.SetParent(this.transform);
        gridViewByPlayerId.Add(grid2.OwnerId, gridView2);
        
        gridView2.BuildGridView(tilePrefab, grid2);
    }
    
    void Update()
    {
        
    }
}
