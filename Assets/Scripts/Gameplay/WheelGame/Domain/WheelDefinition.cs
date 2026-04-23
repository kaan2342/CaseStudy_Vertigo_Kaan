using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.WheelGame.Domain
{
    [Serializable]
    public sealed class WheelDefinition
    {
        [SerializeField] private List<WheelSliceDefinition> slices = new List<WheelSliceDefinition>();
        public IReadOnlyList<WheelSliceDefinition> Slices => slices;
    }
}
