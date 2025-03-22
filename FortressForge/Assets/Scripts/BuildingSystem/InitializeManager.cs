using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.HexGrid;
using UnityEngine;

namespace FortressForge.BuildingSystem {
    public class InitializeManager : MonoBehaviour {
        [Header("Referenzen für das GridView")]
        [SerializeField] private GameObject _tilePrefab; // Prefab, das pro Hexfeld instanziert wird

        void Start() {
            // 1) Grid für Spieler 1 erstellen
            HexGridData grid1 = HexGridManager.INSTANCE.CreateHexGrid(
                new Vector3(800, 0, 800),
                5,
                2,
                "Spieler1",
                10f,
                10f);

            // 2) Ein HexGridView-Objekt in der Szene erzeugen
            HexGridView gridView1 = Instantiate(
                new GameObject("Player 1 HexGridView").AddComponent<HexGridView>(),
                grid1.origin,
                Quaternion.identity);
            gridView1.transform.SetParent(transform);
            gridView1.BuildGridView(_tilePrefab, grid1);

            // 4) Grid für Spieler 2 erstellen
            HexGridData grid2 = HexGridManager.INSTANCE.CreateHexGrid(
                new Vector3(300, 0, 300),
                5,
                2,
                "Spieler2",
                10f,
                10f);

            // 5) Ein weiteres HexGridView-Objekt in der Szene erzeugen
            HexGridView gridView2 = Instantiate(
                new GameObject("Player 2 HexGridView").AddComponent<HexGridView>(),
                grid2.origin,
                Quaternion.identity);
            gridView2.transform.SetParent(transform);
            gridView2.BuildGridView(_tilePrefab, grid2);
        }
    }
}
