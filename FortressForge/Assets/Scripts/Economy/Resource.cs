﻿using System;
using UnityEngine;

namespace FortressForge.Economy
{
    /// <summary>
    /// Represents a single resource in the game, such as Iron or Power.
    /// Tracks the current amount, its type, and an optional maximum cap.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Event triggered whenever the resource amount changes.
        /// </summary>
        public event Action OnChanged;
        
        private readonly ResourceType _type;

        /// <summary>
        /// Gets the type of this resource.
        /// </summary>
        public ResourceType Type => _type;

        private float _currentAmount;

        /// <summary>
        /// The difference in amount since the last update.
        /// </summary>
        public float DeltaAmount { get; private set; }

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
                    return;
                }
                float previousValue = _currentAmount;
                if (value > MaxAmount)
                {
                    Debug.Log($"[Resource] { _type } exceeded max ({value} > {MaxAmount}). Clamping.");
                    _currentAmount = MaxAmount;
                    return;
                }
                _currentAmount = value;
                if (Math.Abs(previousValue - _currentAmount) > Mathf.Epsilon)
                {
                    OnChanged?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Sets the current amount and updates the delta amount accordingly.
        /// Triggers the <see cref="OnChanged"/> event if the value changes.
        /// </summary>
        /// <param name="amount">The new amount to set.</param>
        public void SetCurrentAmountWithDeltaAmount(float amount)
        {
            var pastDeltaAmount = _currentAmount;
            DeltaAmount = amount - _currentAmount;
            CurrentAmount = amount;
            
            if (Math.Abs(pastDeltaAmount - DeltaAmount) > Mathf.Epsilon)
            {
                OnChanged?.Invoke();
            }
        }

        /// <summary>
        /// Adds the specified amount to the current amount and updates the delta.
        /// Triggers the <see cref="OnChanged"/> event if the value changes.
        /// </summary>
        /// <param name="amount">The amount to add (can be negative).</param>
        public void AddAmountWithDeltaAmount(float amount) {
            DeltaAmount = amount;
            CurrentAmount += amount;
            
            if (Math.Abs(amount) > Mathf.Epsilon)
            {
                OnChanged?.Invoke();
            }
        }

        /// <summary>
        /// Creates a new resource instance of the specified type, with a maximum capacity.
        /// </summary>
        /// <param name="type">The type of resource this instance represents.</param>
        /// <param name="maxAmount">The maximum amount the resource can hold.</param>
        /// <param name="currentAmount">The current amount the resource holds.</param>
        public Resource(ResourceType type, float maxAmount = 0, float currentAmount = 0)
        {
            _type = type;
            MaxAmount = maxAmount;
            DeltaAmount = 0;
            _currentAmount = currentAmount;
        }
    }
}