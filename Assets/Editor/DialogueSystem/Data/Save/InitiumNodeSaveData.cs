using System;
using System.Collections.Generic;
using UnityEngine;

namespace Initium.Data.Save
{
    using Enumerations;

    [Serializable]
    public class InitiumNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Speaker { get; set; }
        [field: SerializeField] public string AltSpeaker { get; set; }
        [field: SerializeField] public ParticleForQuest Particle { get; set; }
        [field: SerializeField] public QuestType Type { get; set; }
        [field: SerializeField] public List<Item> FetchItems { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<InitiumChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public InitiumDialogueType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}