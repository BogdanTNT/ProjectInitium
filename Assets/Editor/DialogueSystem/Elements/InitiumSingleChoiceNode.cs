using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Initium.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    public class InitiumSingleChoiceNode : InitiumNode
    {
        public override void Initialize(string nodeName, InitiumGraphView InitiumGraphView, Vector2 position)
        {
            base.Initialize(nodeName, InitiumGraphView, position);

            DialogueType = InitiumDialogueType.SingleChoice;

            InitiumChoiceSaveData choiceData = new InitiumChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT CONTAINER */

            foreach (InitiumChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
