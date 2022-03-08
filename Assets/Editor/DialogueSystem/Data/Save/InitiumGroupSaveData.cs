using System;
using UnityEngine;

namespace Initium.Data.Save
{
    [Serializable]
    public class InitiumGroupSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}