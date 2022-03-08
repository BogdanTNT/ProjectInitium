using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Initium.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    public class InitiumMultipleChoiceNode : InitiumNode
    {
        public override void Initialize(string nodeName, InitiumGraphView InitiumGraphView, Vector2 position)
        {
            base.Initialize(nodeName, InitiumGraphView, position);

            DialogueType = InitiumDialogueType.MultipleChoice;

            InitiumChoiceSaveData choiceData = new InitiumChoiceSaveData()
            {
                Text = "New Choice"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN CONTAINER */

            Button addChoiceButton = InitiumElementUtility.CreateButton("Add Choice", () =>
            {
                InitiumChoiceSaveData choiceData = new InitiumChoiceSaveData()
                {
                    Text = "New Choice"
                };

                Choices.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("Initium-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT CONTAINER */

            foreach (InitiumChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            InitiumChoiceSaveData choiceData = (InitiumChoiceSaveData) userData;

            Button deleteChoiceButton = InitiumElementUtility.CreateButton("X", () =>
            {
                if (Choices.Count == 1)
                {
                    return;
                }

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);

                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("Initium-node__button");

            TextField choiceTextField = InitiumElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses(
                "Initium-node__text-field",
                "Initium-node__text-field__hidden",
                "Initium-node__choice-text-field"
            );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}