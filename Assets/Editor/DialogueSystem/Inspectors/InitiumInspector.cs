using System.Collections.Generic;
using UnityEditor;

namespace Initium.Inspectors
{
    using Utilities;
    using ScriptableObjects;

    [CustomEditor(typeof(InitiumDialogue))]
    public class InitiumInspector : Editor
    {
        /* Dialogue Scriptable Objects */
        private SerializedProperty dialogueContainerProperty;
        private SerializedProperty dialogueGroupProperty;
        private SerializedProperty dialogueProperty;

        /* Filters */
        private SerializedProperty groupedDialoguesProperty;
        private SerializedProperty startingDialoguesOnlyProperty;

        /* Indexes */
        private SerializedProperty selectedDialogueGroupIndexProperty;
        private SerializedProperty selectedDialogueIndexProperty;

        private void OnEnable()
        {
            dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
            dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
            dialogueProperty = serializedObject.FindProperty("dialogue");

            groupedDialoguesProperty = serializedObject.FindProperty("groupedDialogues");
            startingDialoguesOnlyProperty = serializedObject.FindProperty("startingDialoguesOnly");

            selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
            selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDialogueContainerArea();

            InitiumDialogueContainerSO currentDialogueContainer = (InitiumDialogueContainerSO) dialogueContainerProperty.objectReferenceValue;

            if (currentDialogueContainer == null)
            {
                StopDrawing("Select a Dialogue Container to see the rest of the Inspector.");

                return;
            }

            DrawFiltersArea();

            bool currentGroupedDialoguesFilter = groupedDialoguesProperty.boolValue;
            bool currentStartingDialoguesOnlyFilter = startingDialoguesOnlyProperty.boolValue;
            
            List<string> dialogueNames;

            string dialogueFolderPath = $"Assets/DialogueSystem/Dialogues/{currentDialogueContainer.FileName}";

            string dialogueInfoMessage;

            if (currentGroupedDialoguesFilter)
            {
                List<string> dialogueGroupNames = currentDialogueContainer.GetDialogueGroupNames();

                if (dialogueGroupNames.Count == 0)
                {
                    StopDrawing("There are no Dialogue Groups in this Dialogue Container.");

                    return;
                }

                DrawDialogueGroupArea(currentDialogueContainer, dialogueGroupNames);

                InitiumDialogueGroupSO dialogueGroup = (InitiumDialogueGroupSO) dialogueGroupProperty.objectReferenceValue;

                dialogueNames = currentDialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialoguesOnlyFilter);

                dialogueFolderPath += $"/Groups/{dialogueGroup.GroupName}/Dialogues";

                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Dialogues in this Dialogue Group.";
            }
            else
            {
                dialogueNames = currentDialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnlyFilter);

                dialogueFolderPath += "/Global/Dialogues";

                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Ungrouped Dialogues in this Dialogue Container.";
            }

            if (dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMessage);

                return;
            }

            DrawDialogueArea(dialogueNames, dialogueFolderPath);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDialogueContainerArea()
        {
            InitiumInspectorUtility.DrawHeader("Dialogue Container");

            dialogueContainerProperty.DrawPropertyField();

            InitiumInspectorUtility.DrawSpace();
        }

        private void DrawFiltersArea()
        {
            InitiumInspectorUtility.DrawHeader("Filters");

            groupedDialoguesProperty.DrawPropertyField();
            startingDialoguesOnlyProperty.DrawPropertyField();

            InitiumInspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea(InitiumDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            InitiumInspectorUtility.DrawHeader("Dialogue Group");

            int olInitiumelectedDialogueGroupIndex = selectedDialogueGroupIndexProperty.intValue;

            InitiumDialogueGroupSO oldDialogueGroup = (InitiumDialogueGroupSO) dialogueGroupProperty.objectReferenceValue;

            bool isOldDialogueGroupNull = oldDialogueGroup == null;

            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;

            UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndexProperty, olInitiumelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);

            selectedDialogueGroupIndexProperty.intValue = InitiumInspectorUtility.DrawPopup("Dialogue Group", selectedDialogueGroupIndexProperty, dialogueGroupNames.ToArray());

            string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndexProperty.intValue];

            InitiumDialogueGroupSO selectedDialogueGroup = InitiumIOUtility.LoadAsset<InitiumDialogueGroupSO>($"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);

            dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;

            InitiumInspectorUtility.DrawDisabledFielInitium(() => dialogueGroupProperty.DrawPropertyField());

            InitiumInspectorUtility.DrawSpace();
        }

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            InitiumInspectorUtility.DrawHeader("Dialogue");

            int olInitiumelectedDialogueIndex = selectedDialogueIndexProperty.intValue;

            InitiumDialogueSO oldDialogue = (InitiumDialogueSO) dialogueProperty.objectReferenceValue;

            bool isOldDialogueNull = oldDialogue == null;

            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;

            UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndexProperty, olInitiumelectedDialogueIndex, oldDialogueName, isOldDialogueNull);

            selectedDialogueIndexProperty.intValue = InitiumInspectorUtility.DrawPopup("Dialogue", selectedDialogueIndexProperty, dialogueNames.ToArray());

            string selectedDialogueName = dialogueNames[selectedDialogueIndexProperty.intValue];

            InitiumDialogueSO selectedDialogue = InitiumIOUtility.LoadAsset<InitiumDialogueSO>(dialogueFolderPath, selectedDialogueName);

            dialogueProperty.objectReferenceValue = selectedDialogue;

            InitiumInspectorUtility.DrawDisabledFielInitium(() => dialogueProperty.DrawPropertyField());
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            InitiumInspectorUtility.DrawHelpBox(reason, messageType);

            InitiumInspectorUtility.DrawSpace();

            InitiumInspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work properly at Runtime!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int olInitiumelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;

                return;
            }

            bool oldIndexIsOutOfBounInitiumOfNamesListCount = olInitiumelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBounInitiumOfNamesListCount || oldPropertyName != optionNames[olInitiumelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);

                    return;
                }

                indexProperty.intValue = 0;
            }
        }
    }
}