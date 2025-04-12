using UnityEngine;
using UnityEngine.InputSystem;
using FortressForge.BuildingSystem.BuildingData;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.HexTile;
using System.Collections.Generic;

public class BuildViewController : MonoBehaviour, BuildActionHandler.IPreviewModeActions
{
    public HexGridView HexGridView;
    public HexGridData HexGridData;
    private BaseBuildingTemplate _selectedBuildingTemplate;
    private GameObject _previewBuilding;
    private List<BaseBuildingTemplate> _placedBuildings = new();
    private bool _isPreviewMode = false;

    private BuildActionHandler _input;

    private void Awake()
    {
        _input = new BuildActionHandler();
    }

    private void OnEnable()
    {
        _input.PreviewMode.SetCallbacks(this);
        _input.PreviewMode.Enable();
    }

    private void OnDisable()
    {
        _input.PreviewMode.Disable();
        _input.PreviewMode.SetCallbacks(null); // Cleanup
    }

    private void Update()
    {
        if (_isPreviewMode && _previewBuilding != null)
        {
            MovePreviewObject();
        }
    }

    public void OnPlaceAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryPlaceBuilding();
    }

    public void OnExitBuildMode(InputAction.CallbackContext context)
    {
        if (context.performed)
            ExitBuildMode();
    }

    public void OnRotateBuilding(InputAction.CallbackContext context)
    {
        if (context.performed)
            RotateObject(60f); // Or whatever angle you want for rotation
    }

    public void PreviewSelectedBuilding(BaseBuildingTemplate building)
    {
        _selectedBuildingTemplate = Instantiate(building);
        _selectedBuildingTemplate.ShapeData = new List<HexTileCoordinate>
        {
            new HexTileCoordinate(0, 0, 0)
        };

        if (_previewBuilding != null)
            Destroy(_previewBuilding);

        _previewBuilding = Instantiate(_selectedBuildingTemplate.BuildingPrefab);
        _isPreviewMode = true;
    }

    private void MovePreviewObject()
    {
        var coord = HexGridView.GetCurrentlyHoveredHexTileCoordinate();
        if (coord != default)
        {
            Vector3 snappedPos = coord.GetWorldPosition(HexGridData.TileRadius, HexGridData.TileHeight);
            _previewBuilding.transform.position = snappedPos;
        }
    }

    public void TryPlaceBuilding()
    {
        if (!_isPreviewMode) return;

        HexTileCoordinate hexCoord = HexGridView.GetCurrentlyHoveredHexTileCoordinate();

        if (HexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuildingTemplate) && hexCoord != default)
        {
            Instantiate(_selectedBuildingTemplate.BuildingPrefab, _previewBuilding.transform.position,
                _previewBuilding.transform.rotation);
            BaseBuildingTemplate copy = Instantiate(_selectedBuildingTemplate);
            _placedBuildings.Add(copy);
            ExitBuildMode();
        }
    }

    public void ExitBuildMode()
    {
        if (!_isPreviewMode) return;

        _isPreviewMode = false;
        Destroy(_previewBuilding);
        _selectedBuildingTemplate = null;
    }

    public void RotateObject(float angle)
    {
        if (!_isPreviewMode || _previewBuilding == null) return;

        Vector3 currentRotation = _previewBuilding.transform.eulerAngles;
        currentRotation.y += angle;
        _previewBuilding.transform.rotation = Quaternion.Euler(currentRotation);
    }
}
