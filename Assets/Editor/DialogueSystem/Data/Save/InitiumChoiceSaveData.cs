using System;
using UnityEngine;

namespace Initium.Data.Save
{
    [Serializable]
    public class InitiumChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}