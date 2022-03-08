using System.Collections.Generic;
using UnityEngine;

namespace Initium.ScriptableObjects
{
    using Data;
    using Enumerations;

    public class InitiumDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] public string Speaker { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<InitiumDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public InitiumDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialize(string dialogueName, string text, List<InitiumDialogueChoiceData> choices, InitiumDialogueType dialogueType, bool isStartingDialogue, string speaker)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;

            Speaker = speaker;
        }
    }
}