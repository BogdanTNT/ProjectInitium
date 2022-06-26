using System.Collections.Generic;
using UnityEngine;

namespace Initium.ScriptableObjects
{
    public class InitiumDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<InitiumDialogueGroupSO, List<InitiumDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<InitiumDialogueSO> UngroupedDialogues { get; set; }

        //[field: SerializeField] public List<string> Speakers { get; set; }
        //[field: SerializeField] public List<string> AlternativeName { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            //Speakers = new List<string>();
            //AlternativeName = new List<string>();

            DialogueGroups = new SerializableDictionary<InitiumDialogueGroupSO, List<InitiumDialogueSO>>();
            UngroupedDialogues = new List<InitiumDialogueSO>();
        }

        public List<string> GetDialogueGroupNames()
        {
            List<string> dialogueGroupNames = new List<string>();

            foreach (InitiumDialogueGroupSO dialogueGroup in DialogueGroups.Keys)
            {
                dialogueGroupNames.Add(dialogueGroup.GroupName);
            }

            return dialogueGroupNames;
        }

        public List<string> GetGroupedDialogueNames(InitiumDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
        {
            List<InitiumDialogueSO> groupedDialogues = DialogueGroups[dialogueGroup];

            List<string> groupedDialogueNames = new List<string>();

            foreach (InitiumDialogueSO groupedDialogue in groupedDialogues)
            {
                if (startingDialoguesOnly && !groupedDialogue.IsStartingDialogue)
                {
                    continue;
                }

                groupedDialogueNames.Add(groupedDialogue.DialogueName);
            }

            return groupedDialogueNames;
        }

        public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();

            foreach (InitiumDialogueSO ungroupedDialogue in UngroupedDialogues)
            {
                if (startingDialoguesOnly && !ungroupedDialogue.IsStartingDialogue)
                {
                    continue;
                }

                ungroupedDialogueNames.Add(ungroupedDialogue.DialogueName);
            }

            return ungroupedDialogueNames;
        }
    }
}