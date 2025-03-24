using System;
using System.Collections.Generic;
using FortressForge.BuildingSystem.HexGrid;
using UnityEngine;

namespace FortressForge.BuildingSystem
{
    /// <summary>
    /// This class is responsible for creating the hex grids for the players and instantiating the hex grid views.
    /// It is called once at the beginning of the game (in the loading screen).
    /// </summary>
    public class InitializationManager : MonoBehaviour
    {
        [Header("Referenzen für das GridView")] [SerializeField]
        private GameObject _tilePrefab; // Prefab, das pro Hexfeld instanziert wird

        void Start()
        {
            // 1) Grid für Spieler 1 erstellen
            HexGridData grid1 = HexGridManager.Instance.CreateHexGrid(
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
            gridView1.transform.SetParent(transform);
            gridView1.BuildGridView(_tilePrefab, grid1);

            // 4) Grid für Spieler 2 erstellen
            HexGridData grid2 = HexGridManager.Instance.CreateHexGrid(
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
            gridView2.transform.SetParent(transform);
            gridView2.BuildGridView(_tilePrefab, grid2);
        }
    }
}