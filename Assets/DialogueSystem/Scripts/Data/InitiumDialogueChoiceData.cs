using System;
using UnityEngine;

namespace Initium.Data
{
    using ScriptableObjects;

    [Serializable]
    public class InitiumDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public InitiumDialogueSO NextDialogue { get; set; }
    }
}