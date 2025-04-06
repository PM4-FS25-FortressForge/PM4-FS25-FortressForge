using System.Collections.Generic;
using FortressForge.Serializables;
using UnityEngine;

namespace FortressForge
{
    [CreateAssetMenu(fileName = "GameStartConfiguration", menuName = "Configurations/GameStartConfiguration")]
    public class GameStartConfiguration : ScriptableObject
    {
        [Header("PlayerIds und ihre HexGrid Zugehörigkeit")]
        public List<PlayerIdHexGridIdTuples> PlayerIdsHexGridIdTuplesList;
        
        [Header("HexGrid origin Koordinaten")]
        public List<Vector3> HexGridOrigins;
    }
}
