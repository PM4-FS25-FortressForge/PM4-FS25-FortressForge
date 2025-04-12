using System.Collections.Generic;
using FortressForge.BuildingSystem.HexTile;
using UnityEngine;

namespace FortressForge.BuildingSystem.BuildingData
{
    [CreateAssetMenu(fileName = "New Core", menuName = "Buildings/CoreBuilding")]
    public class CoreBuildingTemplate : BaseBuildingTemplate {
        public new void Awake()
        {
            ShapeData = new List<HexTileCoordinate>
            {
                new HexTileCoordinate(0, 0, 0),
                new HexTileCoordinate(0, 1, 0),
                new HexTileCoordinate(1, 0, 0),
            };
            base.Awake();
        }
    }
}
