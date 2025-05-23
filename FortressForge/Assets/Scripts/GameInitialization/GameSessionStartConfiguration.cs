using System.Collections.Generic;
using System.Linq;
using FortressForge.BuildingSystem.BuildingData;
using UnityEngine;

namespace FortressForge.GameInitialization
{
    /// <summary>
    /// Contains information needed to start a play session.
    /// Such as Player Grid Assignments, Game Mode, and other configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "PlaySessionStartConfiguration", menuName = "Configurations/PlaySessionStartConfiguration")]
    public class GameSessionStartConfiguration : ScriptableObject
    {
        [Header("HexGrid origin Koordinaten")]
        public List<Vector3> HexGridOrigins;
    }
}