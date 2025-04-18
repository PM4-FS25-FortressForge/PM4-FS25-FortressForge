﻿using UnityEngine;

namespace FortressForge.Economy
{
    /// <summary>
    /// Represents a single resource in the game, such as Iron or Power.
    /// Tracks the current amount, its type, and an optional maximum cap.
    /// </summary>
    public class Resource
    {
        private readonly ResourceType _type;

        private float _currentAmount;

        /// <summary>
        /// The maximum amount this resource can hold.
        /// </summary>
        public float MaxAmount { get; }

        /// <summary>
        /// The current quantity of this resource.
        /// Automatically clamps to MaxAmount if a higher value is set.
        /// </summary>
        public float CurrentAmount
        {
            get => _currentAmount;
            set
            {
                if (value < 0)
                {
                    Debug.LogError($"[Resource] {_type} attempted to go below 0. Value: {value}");
                    throw new System.ArgumentOutOfRangeException(nameof(value), $"[Resource] {_type} cannot go below 0.");
                }
                if (value > MaxAmount)
                {
                    Debug.Log($"[Resource] { _type } exceeded max ({value} > {MaxAmount}). Clamping.");
                    _currentAmount = MaxAmount;
                }
                else 
                {
                    _currentAmount = value;
                }
            }
        }

        /// <summary>
        /// Creates a new resource instance of the specified type, with a maximum capacity.
        /// </summary>
        /// <param name="type">The type of resource this instance represents.</param>
        /// <param name="maxAmount">The maximum amount the resource can hold.</param>
        public Resource(ResourceType type, float maxAmount = float.MaxValue)
        {
            _type = type;
            MaxAmount = maxAmount;
            _currentAmount = 0;
        }
    }
}