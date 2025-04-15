using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;
using UnityEngine;

namespace FortressForge.HexGrid
{
    public class HexGridAssembler
    {
        private readonly IHexGridDataFactory _dataFactory;
        private readonly IHexGridViewFactory _viewFactory;

        public HexGridAssembler(IHexGridDataFactory dataFactory, IHexGridViewFactory viewFactory)
        {
            _dataFactory = dataFactory;
            _viewFactory = viewFactory;
        }

        public HexGridAssembly CreateHexGrid(
            int id,
            Vector3 origin,
            int radius,
            float tileSize,
            float tileHeight,
            GameObject tilePrefab)
        {
            HexGridData data = _dataFactory.CreateData(id, origin, radius, tileSize, tileHeight);
            
            HexGridView view = _viewFactory.CreateView(data, tilePrefab);
            
            return new HexGridAssembly(data, view);
        }
    }
}