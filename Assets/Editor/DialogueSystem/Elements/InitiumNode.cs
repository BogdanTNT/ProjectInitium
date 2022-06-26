using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Speakers.Characters;

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
        public string altSpeaker;
        public ParticleForQuest particle;
        public QuestType type;
        public List<Item> fetchItems;

        public override void OnSelected()
        {
            base.OnSelected();

            foreach(Item i in fetchItems)
            {
                Debug.Log(i.alternativeName);
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

            speaker = GetSpeaker(0);
            altSpeaker = GetAltName(0);
            particle = ParticleForQuest.BeginQuest;
            fetchItems = new List<Item>();

            DialogueName = nodeName;
            Choices = new List<InitiumChoiceSaveData>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.zero));

            graphView = InitiumGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("Initium-node__main-container");
            extensionContainer.AddToClassList("Initium-node__extension-container");
        }

        TextField dialogueNameTextField;
        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            dialogueNameTextField = InitiumElementUtility.CreateTextField(DialogueName, null, callback =>
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
                choices = AllSpeakers(),
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


            CheckQuestType();


            Foldout textFoldout = InitiumElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = InitiumElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "Initium-node__text-field",
                "Initium-node__quote-text-field"
            );

            textFoldout.Add(textTextField);

            customDataContainer.Add(textFoldout);


            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }

        Foldout itemToAddListFoldout;
        VisualElement customItemContainer = new VisualElement();
        private void CheckQuestType()
        {
            if (type == QuestType.Fetch || type == QuestType.Reward)
            {
                itemToAddListFoldout = InitiumElementUtility.CreateFoldout("Item Foldout", true);

                Button addNewItemButton = InitiumElementUtility.CreateButton("Add New Item", () =>
                {
                    Item newItem = new Item();

                    fetchItems.Add(newItem);

                    itemToAddListFoldout.Add(AddNewItem(fetchItems.Last(), itemToAddListFoldout));
                });

                addNewItemButton.AddToClassList("Initium-node__button");

                itemToAddListFoldout.Add(addNewItemButton);

                for (int i = 0; i < fetchItems.Count; i++)
                {
                    AddNewItem(fetchItems[i], itemToAddListFoldout);
                }

                customItemContainer.AddToClassList("Initium-node__custom-data-container");

                customItemContainer.Add(itemToAddListFoldout);

                extensionContainer.Add(customItemContainer);
            }
            else
            {
                if (extensionContainer.Contains(customItemContainer))
                    extensionContainer.Remove(customItemContainer);

                if (customItemContainer.Contains(itemToAddListFoldout))
                    customItemContainer.Remove(itemToAddListFoldout);
            }
        }

        private VisualElement AddNewItem(Item itemToAdd, VisualElement itemToAddListFoldout)
        {
            Foldout itemToAddFoldout = InitiumElementUtility.CreateFoldout(itemToAdd.alternativeName, true);

            TextField itemToAddId = InitiumElementUtility.CreateTextArea(itemToAdd.id, "Item id", callback =>
            {
                itemToAdd.id = callback.newValue;
            });

            TextField itemToAddName = InitiumElementUtility.CreateTextArea(itemToAdd.alternativeName, "Human item name", callback =>
            {
                itemToAdd.alternativeName = callback.newValue;
                itemToAddFoldout.text = itemToAdd.alternativeName;
            });

            TextField quantity = InitiumElementUtility.CreateTextArea(itemToAdd.quantity.ToString(), "Quanitity", callback => itemToAdd.quantity = Int32.Parse(callback.newValue));

            itemToAddFoldout.Add(itemToAddId);
            itemToAddFoldout.Add(itemToAddName);
            itemToAddFoldout.Add(quantity);


            // Redstone position Foldout
            Foldout redstoneBlockPositionFoldout = InitiumElementUtility.CreateFoldout("Redstone block position", true);

            TextField redstoneX = InitiumElementUtility.CreateTextField(itemToAdd.redstoneBlock.x.ToString(), "X", callback => itemToAdd.redstoneBlock.x = Int32.Parse(callback.newValue));
            TextField redstoneY = InitiumElementUtility.CreateTextField(itemToAdd.redstoneBlock.y.ToString(), "Y", callback => itemToAdd.redstoneBlock.y = Int32.Parse(callback.newValue));
            TextField redstoneZ = InitiumElementUtility.CreateTextField(itemToAdd.redstoneBlock.z.ToString(), "Z", callback => itemToAdd.redstoneBlock.z = Int32.Parse(callback.newValue));

            redstoneBlockPositionFoldout.Add(redstoneX);
            redstoneBlockPositionFoldout.Add(redstoneY);
            redstoneBlockPositionFoldout.Add(redstoneZ);

            itemToAddFoldout.Add(redstoneBlockPositionFoldout);

            // remove item button
            Button removeItem = InitiumElementUtility.CreateButton("Remove Item", () =>
            {
                fetchItems.Remove(itemToAdd);

                itemToAddListFoldout.Remove(itemToAddFoldout);
            });

            itemToAddFoldout.Add(removeItem);

            itemToAddListFoldout.Add(itemToAddFoldout);
            return itemToAddFoldout;
        }

        private void ChangeSpeaker(ChangeEvent<string> evt)
        {
            speaker = evt.newValue;
            altSpeaker = GetAltName(IndexOfSpeaker(speaker));

            dialogueNameTextField.value = speaker;
            if (Group == null)
            {
                graphView.RemoveUngroupedNode(this);

                DialogueName = evt.newValue;

                graphView.AddUngroupedNode(this);

                return;
            }

            InitiumGroup currentGroup = Group;

            graphView.RemoveGroupedNode(this, Group);

            DialogueName = evt.newValue;

            graphView.AddGroupedNode(this, currentGroup);
        }

        private void ChangeParticle(ChangeEvent<ParticleForQuest> evt)
        {
            particle = evt.newValue;
        }

        private void ChangeQuestType(ChangeEvent<QuestType> evt)
        {
            type = evt.newValue;
            CheckQuestType();
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