using FortressForge.HexGrid.Data;
using FortressForge.HexGrid.View;

namespace FortressForge.HexGrid
{
    public sealed class HexGridAssembly
    {
        public HexGridData Data { get; }
        public HexGridView View { get; }

        public HexGridAssembly(HexGridData data, HexGridView view)
        {
            Data = data;
            View = view;
        }
    }
}