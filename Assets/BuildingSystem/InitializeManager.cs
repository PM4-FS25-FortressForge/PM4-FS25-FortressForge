using System;
using System.Collections.Generic;
using UnityEngine;

public class InitializeManager : MonoBehaviour
{
    [Header("Referenzen für das GridView")]
    [SerializeField] private GameObject tilePrefab;       // Prefab, das pro Hexfeld instanziert wird

    private Dictionary<string, HexGridView> gridViewByPlayerId = new Dictionary<string, HexGridView>();
    void Start()
    {
        // 1) Grid für Spieler 1 erstellen
        HexGrid grid1 = HexGridManager.Instance.CreateHexGrid(
            new Vector3(800, 0, 800), 
            5, 
            2, 
            "Spieler1", 
            10f, 
            10f);
        
        // 2) Ein HexGridView-Objekt in der Szene erzeugen
        HexGridView gridView1 = Instantiate(
            new GameObject("Player 1 HexGridView").AddComponent<HexGridView>(),
            grid1.Origin,
            Quaternion.identity);
        gridView1.transform.SetParent(this.transform);
        gridViewByPlayerId.Add(grid1.OwnerId, gridView1);
        
        // 3) HexGridView-Objekt bauen
        gridView1.BuildGridView(tilePrefab, grid1);

        // 4) Grid für Spieler 2 erstellen
        HexGrid grid2 = HexGridManager.Instance.CreateHexGrid(
            new Vector3(300, 0, 300), 
            5, 
            2, 
            "Spieler2", 
            10f, 
            10f);
        
        // 5) Ein weiteres HexGridView-Objekt in der Szene erzeugen
        HexGridView gridView2 = Instantiate(
            new GameObject("Player 2 HexGridView").AddComponent<HexGridView>(),
            grid2.Origin, 
            Quaternion.identity);
        gridView2.transform.SetParent(this.transform);
        gridViewByPlayerId.Add(grid2.OwnerId, gridView2);
        
        gridView2.BuildGridView(tilePrefab, grid2);
    }
    
    void Update()
    {
        
    }
}
