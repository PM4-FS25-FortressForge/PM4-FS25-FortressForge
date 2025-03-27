using UnityEngine;
using FortressForge.BuildingSystem.HexGrid;
using FortressForge.BuildingSystem.BuildingData;

public class PlayerController : MonoBehaviour
{
    public HexGridView hexGridView;
    public HexGridData hexGridData;

    private BaseBuilding _selectedBuilding;
    private GameObject _previewBuilding;

    private bool _isDragging = false;

    public void SetSelectedBuilding(BaseBuilding building)
    {
        _selectedBuilding = building;

        if (_previewBuilding != null)
        {
            Destroy(_previewBuilding);
        }

        // Instantiate the building's prefab for preview
        _previewBuilding = Instantiate(_selectedBuilding.buildingPrefab);
        _isDragging = true;
    }

    void Update()
    {
        if (_isDragging && _previewBuilding != null)
        {
            MovePreviewObject();
            
            if (Input.GetMouseButtonDown(0)) // First click to place
            {
                TryPlaceBuilding();
            }
        }
    }

    private void MovePreviewObject()
    {
        Vector3 worldPos = hexGridView.GetMouseWorldPosition();
        (int, int, int) hexCoord = hexGridView.WorldPositionToHexCoord(worldPos);

        Vector3 snappedPos = hexGridView.CalculateWorldPosition(hexCoord, hexGridView.transform.position);
        _previewBuilding.transform.position = snappedPos;
    }

    private void TryPlaceBuilding()
    {
        Vector3 worldPos = _previewBuilding.transform.position;
        (int, int, int) hexCoord = hexGridView.WorldPositionToHexCoord(worldPos);

        if (hexGridData.ValidateBuildingPlacement(hexCoord, _selectedBuilding))
        {
            // Place the final building at the correct position
            Instantiate(_selectedBuilding.buildingPrefab, _previewBuilding.transform.position, Quaternion.identity);
            
            _isDragging = false;
            Destroy(_previewBuilding);
            _selectedBuilding = null;
        }
        else
        {
            // If placement is invalid, destroy the preview
            Destroy(_previewBuilding);
            _isDragging = false;
            _selectedBuilding = null;
        }
    }

    
}


/*

using UnityEngine.UI;
using System.Collections.Generic;
using FortressForge.BuildingSystem.HexGrid;

namespace FortressForge
{
    public class PlayerController : MonoBehaviour
    {
        public ButtonManager _buttonManager;
        private GameObject _currentObject;
        private bool _isDragging = false;
        private GameObject _currentPrefab;
        private HexGridView _hexGridView;
        private HexGridData _hexGridData;

        void Start()
        {
            _hexGridView = FindObjectOfType<HexGridView>();
            // Add listeners to buttons
            _buttonManager.button1.onClick.AddListener(() => StartDragging(_buttonManager.prefab1)); // Spawn prefab1
            _buttonManager.button2.onClick.AddListener(() => StartDragging(_buttonManager.prefab2)); // Spawn prefab2
        }

        void Update()
        {
            if (!_isDragging || _currentObject == null) return;

            // Get the mouse position in world space
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Adjust for camera distance
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Convert world position to hex coordinates (snap to hex grid)
            Vector3 hexCoord = _hexGridView.WorldPositionToHexCoord(worldPos);

            // Snap the object to the hex grid position using HexGridView's method
            Vector3 snappedPos =  _hexGridView.CalculateWorldPosition((Mathf.RoundToInt(hexCoord.x), Mathf.RoundToInt(hexCoord.y), Mathf.RoundToInt(hexCoord.z)), hexCoord);
            
            _currentObject.transform.position = snappedPos;

            // Place the object when left click is pressed
            if (Input.GetMouseButtonDown(0))
            {
                // Validate placement
                if (HexGridData.ValidateBuildingPlacement((Mathf.RoundToInt(hexCoord.x), Mathf.RoundToInt(hexCoord.y), Mathf.RoundToInt(hexCoord.z)), _currentObject))
                {
                    // Place the object
                    _isDragging = false;
                    _currentObject = null;
                }
                else
                {
                    // If invalid, destroy the object
                    Destroy(_currentObject);
                    _isDragging = false;
                    _currentObject = null;
                }
            }

            // Rotate the object when the '1' or '2' key is pressed
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                RotateObject(45); 
            }
        }

        

        // This method can be used for any prefab
        void StartDragging(GameObject prefab)
        {
            if (!_isDragging)
            {
                _currentPrefab = prefab; // Set the current prefab
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f; // Distance from the camera
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                _currentObject = Instantiate(_currentPrefab, worldPos, Quaternion.identity); // Instantiate the prefab
                _isDragging = true; // Start dragging
            }
        }

        // Rotate the object based on key input (1 or 2)
        void RotateObject(float angle)
        {
            if (_currentObject != null)
            {
                // Get the current rotation around the Y-axis
                Vector3 currentRotation = _currentObject.transform.eulerAngles;

                // Increment the current rotation by the specified angle around the Y-axis
                currentRotation.y += angle;

                // Apply the new rotation while keeping other axes unchanged
                _currentObject.transform.rotation = Quaternion.Euler(currentRotation);
            }
        }
    }
}

*/