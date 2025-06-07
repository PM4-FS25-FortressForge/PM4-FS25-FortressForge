using System;
using System.Collections.Generic;
using FortressForge.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace FortressForge.BuildingSystem.Weapons
{
    /// <summary>
    /// Manages weapon buildings in the game, allowing for registration, selection, and interaction with weapon input handlers.
    /// </summary>
    public class WeaponBuildingManager : MonoBehaviour
    {
        public static WeaponBuildingManager Instance { get; private set; }

        public event Action OnWeaponBuildingsChanged;

        public event Action OnWeaponBuildingSelectedByPrefab;

        private readonly List<WeaponDataContainer> _weaponBuildings = new();

        private int SelectedWeaponBuildingId { get; set; } = -1;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Registers a weapon building by adding it to the list of weapon buildings.
        /// </summary>
        /// <param name="building">The weapon input handler representing the building to register.</param>
        public void RegisterWeaponBuilding(WeaponInputHandler building)
        {
            if (_weaponBuildings.Any(container => container.WeaponInputHandler == building))
            {
                return;
            }

            WeaponClasses weaponClass = building.WeaponClass;
            int id = GetNextAvailableId();
            Label label = new()
            {
                text = building.name + id
            };
            AddWeaponBuilding(building, weaponClass, label);
            OnWeaponBuildingsChanged?.Invoke();
        }

        /// <summary>
        /// Unregisters a weapon building by removing it from the list of weapon buildings.
        /// </summary>
        /// <param name="building">The weapon input handler representing the building to unregister.</param>
        public void UnregisterWeaponBuilding(WeaponInputHandler building)
        {
            RemoveWeaponBuildingByInputHandler(building);
            OnWeaponBuildingsChanged?.Invoke();
        }

        /// <summary>
        /// Invokes the selection of a weapon building based on the provided weapon input handler.
        /// </summary>
        /// <param name="weaponInputHandler">The weapon input handler representing the building to select.</param>
        public void InvokeSelectedWeaponBuilding(WeaponInputHandler weaponInputHandler)
        {
            SelectedWeaponBuildingId = _weaponBuildings.FirstOrDefault(container => container.WeaponInputHandler == weaponInputHandler)?.ID ?? -1;

            if (SelectedWeaponBuildingId == -1)
            {
                Debug.LogWarning("Selected Weapon Building not found.");
                return;
            }

            DeselectAllWeaponBuildings();

            OnWeaponBuildingSelectedByPrefab?.Invoke();
        }

        /// <summary>
        /// Selects a weapon building by its label, allowing for interaction with the corresponding weapon input handler.
        /// </summary>
        /// <param name="label">The label of the weapon building to select.</param>
        public void SelectWeaponBuildingByLabel(Label label)
        {
            WeaponDataContainer selectedContainer = _weaponBuildings.FirstOrDefault(container => container.Label == label);
            if (selectedContainer != null)
            {
                SelectedWeaponBuildingId = selectedContainer.ID;
                DeselectAllWeaponBuildings();
                selectedContainer.WeaponInputHandler.SelectWeapon();
            }
            else
            {
                Debug.LogWarning("No weapon building found with the specified label.");
            }
        }

        /// <summary>
        /// Deselects all weapon buildings by exiting fight mode for each weapon input handler.
        /// </summary>
        private void DeselectAllWeaponBuildings()
        {
            foreach (WeaponDataContainer container in _weaponBuildings)
            {
                container.WeaponInputHandler.ExitFightMode();
            }
        }

        /// <summary>
        /// Adds a weapon building to the list of weapon buildings.
        /// </summary>
        /// <param name="weaponInputHandler">The weapon input handler representing the building to add.</param>
        /// <param name="weaponClass">The weapon class of the building to add.</param>
        /// <param name="label">The label for the weapon building.</param>
        private void AddWeaponBuilding(WeaponInputHandler weaponInputHandler, WeaponClasses weaponClass, Label label)
        {
            int id = GetNextAvailableId();

            WeaponDataContainer weaponDataContainer = new(weaponInputHandler, weaponClass, label, id);
            _weaponBuildings.Add(weaponDataContainer);
        }

        /// <summary>
        /// Removes a weapon building from the list of weapon buildings based on the provided weapon input handler.
        /// </summary>
        /// <param name="weaponInputHandler">The weapon input handler representing the building to remove.</param>
        private void RemoveWeaponBuildingByInputHandler(WeaponInputHandler weaponInputHandler)
        {
            _weaponBuildings.RemoveAll(container => container.WeaponInputHandler == weaponInputHandler);
        }

        /// <summary>
        /// Gets the next available ID for a weapon building by checking existing IDs in the list.
        /// </summary>
        /// <returns>The next available ID as an integer.</returns>
        private int GetNextAvailableId()
        {
            HashSet<int> usedIds = new();
            foreach (WeaponDataContainer container in _weaponBuildings)
            {
                usedIds.Add(container.ID);
            }

            int id = 1;
            while (usedIds.Contains(id))
            {
                id++;
            }

            return id;
        }

        /// <summary>
        /// Gets the label of the currently selected weapon building.
        /// </summary>
        /// <returns>A label representing the selected weapon building, or null if no building is selected.</returns>
        public Label GetSelectedWeaponBuildingLabel()
        {
            WeaponDataContainer selectedContainer = _weaponBuildings.FirstOrDefault(container => container.ID == SelectedWeaponBuildingId);
            return selectedContainer?.Label;
        }

        /// <summary>
        /// Fires the selected weapon building by invoking the fire method on its input handler.
        /// </summary>
        public void FireSelectedWeaponBuilding()
        {
            WeaponInputHandler selectedWeaponInputHandler = GetSelectedWeaponInputHandler();
            if (selectedWeaponInputHandler != null)
            {
                selectedWeaponInputHandler.FireWeapon();
            }
            else
            {
                Debug.LogWarning("No weapon building selected or found.");
            }
        }

        /// <summary>
        /// Rotates the selected weapon building by invoking the rotate method on its input handler.
        /// </summary>
        /// <param name="direction">The direction to rotate the weapon building.</param>
        public void RotateSelectedWeaponStart(AxisDirection direction)
        {
            GetSelectedWeaponInputHandler()?.OnRotateWeaponFromOverlayStart(direction);
        }

        /// <summary>
        /// Stops the rotation of the selected weapon building by invoking the stop method on its input handler.
        /// </summary>
        public void RotateSelectedWeaponStop()
        {
            GetSelectedWeaponInputHandler()?.OnRotateWeaponFromOverlayStop();
        }

        /// <summary>
        /// Adjusts the angle of the selected weapon building by invoking the adjust angle start method on its input handler.
        /// </summary>
        /// <param name="direction">The direction to adjust the weapon angle.</param>
        public void AdjustSelectedWeaponAngleStart(AxisDirection direction)
        {
            GetSelectedWeaponInputHandler()?.OnAdjustWeaponAngleFromOverlayStart(direction);
        }

        /// <summary>
        /// Stops the adjustment of the angle of the selected weapon building by invoking the stop method on its input handler.
        /// </summary>
        public void AdjustSelectedWeaponAngleStop()
        {
            GetSelectedWeaponInputHandler()?.OnAdjustWeaponAngleFromOverlayStop();
        }

        /// <summary>
        /// Gets the weapon input handler for the currently selected weapon building.
        /// </summary>
        /// <returns>The weapon input handler for the selected weapon building, or null if no building is selected.</returns>
        private WeaponInputHandler GetSelectedWeaponInputHandler()
        {
            return _weaponBuildings.FirstOrDefault(container => container.ID == SelectedWeaponBuildingId)?.WeaponInputHandler;
        }

        /// <summary>
        /// Gets all weapon building labels.
        /// </summary>
        /// <returns> A list of labels for all weapon buildings.</returns>
        public List<Label> GetAllWeaponBuildingLabels()
        {
            return _weaponBuildings.Select(container => container.Label).ToList();
        }


        /// <summary>
        /// A container class to hold weapon data, including the input handler, weapon class, label, and ID.
        /// </summary>
        private class WeaponDataContainer
        {
            public WeaponInputHandler WeaponInputHandler { get; private set; }
            public WeaponClasses WeaponClass { get; private set; }
            public Label Label { get; private set; }
            public int ID { get; private set; }

            public WeaponDataContainer(WeaponInputHandler weaponInputHandler, WeaponClasses weaponClass, Label label, int id)
            {
                WeaponInputHandler = weaponInputHandler;
                WeaponClass = weaponClass;
                Label = label;
                ID = id;
            }
        }
    }
}