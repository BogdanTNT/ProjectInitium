using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Initium.Windows
{
    using Elements;
    using Enumerations;

    public class InitiumSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private InitiumGraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(InitiumGraphView InitiumGraphView)
        {
            graphView = InitiumGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    userData = InitiumDialogueType.SingleChoice,
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    userData = InitiumDialogueType.MultipleChoice,
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Groups"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case InitiumDialogueType.SingleChoice:
                {
                    InitiumSingleChoiceNode singleChoiceNode = (InitiumSingleChoiceNode) graphView.CreateNode("DialogueName", InitiumDialogueType.SingleChoice, localMousePosition);

                    graphView.AddElement(singleChoiceNode);

                    return true;
                }

                case InitiumDialogueType.MultipleChoice:
                {
                    InitiumMultipleChoiceNode multipleChoiceNode = (InitiumMultipleChoiceNode) graphView.CreateNode("DialogueName", InitiumDialogueType.MultipleChoice, localMousePosition);

                    graphView.AddElement(multipleChoiceNode);

                    return true;
                }

                case Group _:
                {
                    graphView.CreateGroup("DialogueGroup", localMousePosition);

                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}