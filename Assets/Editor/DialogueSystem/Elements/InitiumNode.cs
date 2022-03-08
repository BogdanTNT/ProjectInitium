using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Initium.Elements
{
    using Data.Save;
    using Enumerations;
    using UnityEditor;
    using UnityEditor.UIElements;
    using Utilities;
    using Windows;

    public class InitiumNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<InitiumChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public InitiumDialogueType DialogueType { get; set; }
        public InitiumGroup Group { get; set; }

        protected InitiumGraphView graphView;
        private Color defaultBackgroundColor;

        public string speaker;
        public ParticleForQuest particle;
        public QuestType type;

        public static string[] Speakers
        {
            get
            {
                return new string[]
                {
                    "Inn", "Blacksmith", "Butcher"
                };
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(string nodeName, InitiumGraphView InitiumGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            speaker = Speakers[0];
            particle = ParticleForQuest.Info;

            DialogueName = nodeName;
            Choices = new List<InitiumChoiceSaveData>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.zero));

            graphView = InitiumGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("Initium-node__main-container");
            extensionContainer.AddToClassList("Initium-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            TextField dialogueNameTextField = InitiumElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    DialogueName = target.value;

                    graphView.AddUngroupedNode(this);

                    return;
                }

                InitiumGroup currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);

                DialogueName = target.value;

                graphView.AddGroupedNode(this, currentGroup);
            });

            dialogueNameTextField.AddClasses(
                "Initium-node__text-field",
                "Initium-node__text-field__hidden",
                "Initium-node__filename-text-field"
            );

            titleContainer.Insert(0, dialogueNameTextField);


            PopupField<string> speakerField = new PopupField<string>()
            {
                choices = Speakers.ToList(),
                value = speaker
            };

            speakerField.RegisterValueChangedCallback(ChangeSpeaker);

            titleContainer.Insert(1, speakerField);

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */

            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("Initium-node__custom-data-container");

            Foldout textFoldout = InitiumElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = InitiumElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "Initium-node__text-field",
                "Initium-node__quote-text-field"
            );

            PopupField<ParticleForQuest> particleField = new PopupField<ParticleForQuest>()
            {
                choices = ((ParticleForQuest[])Enum.GetValues(typeof(ParticleForQuest))).ToList(),
                value = particle
            };

            particleField.RegisterValueChangedCallback(ChangeParticle);

            extensionContainer.Add(particleField);


            PopupField<QuestType> questTypeField = new PopupField<QuestType>()
            {
                choices = ((QuestType[])Enum.GetValues(typeof(QuestType))).ToList(),
                value = type
            };

            questTypeField.RegisterValueChangedCallback(ChangeQuestType);

            extensionContainer.Add(questTypeField);


            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        private void ChangeSpeaker(ChangeEvent<string> evt)
        {
            speaker = evt.newValue;
        }

        private void ChangeParticle(ChangeEvent<ParticleForQuest> evt)
        {
            particle = evt.newValue;
        }

        private void ChangeQuestType(ChangeEvent<QuestType> evt)
        {
            type = evt.newValue;
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}