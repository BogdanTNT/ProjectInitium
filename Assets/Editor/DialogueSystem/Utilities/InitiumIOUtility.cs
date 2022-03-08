using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Initium.Utilities
{
    using Data;
    using Data.Save;
    using Elements;
    using ScriptableObjects;
    using Windows;

    public static class InitiumIOUtility
    {
        private static InitiumGraphView graphView;

        private static string graphFileName;
        private static string containerFolderPath;

        private static List<InitiumNode> nodes;
        private static List<InitiumGroup> groups;

        private static Dictionary<string, InitiumDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, InitiumDialogueSO> createdDialogues;

        private static Dictionary<string, InitiumGroup> loadedGroups;
        private static Dictionary<string, InitiumNode> loadedNodes;

        public static void Initialize(InitiumGraphView InitiumGraphView, string graphName)
        {
            graphView = InitiumGraphView;

            graphFileName = graphName;
            containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphName}";

            nodes = new List<InitiumNode>();
            groups = new List<InitiumGroup>();

            createdDialogueGroups = new Dictionary<string, InitiumDialogueGroupSO>();
            createdDialogues = new Dictionary<string, InitiumDialogueSO>();

            loadedGroups = new Dictionary<string, InitiumGroup>();
            loadedNodes = new Dictionary<string, InitiumNode>();
        }

        public static void Save()
        {
            CreateDefaultFolders();

            GetElementsFromGraphView();

            InitiumGraphSaveDataSO graphData = CreateAsset<InitiumGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");

            graphData.Initialize(graphFileName);

            InitiumDialogueContainerSO dialogueContainer = CreateAsset<InitiumDialogueContainerSO>(containerFolderPath, graphFileName);

            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        private static void SaveGroups(InitiumGraphSaveDataSO graphData, InitiumDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (InitiumGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(InitiumGroup group, InitiumGraphSaveDataSO graphData)
        {
            InitiumGroupSaveData groupData = new InitiumGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(InitiumGroup group, InitiumDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            InitiumDialogueGroupSO dialogueGroup = CreateAsset<InitiumDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<InitiumDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, InitiumGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        private static void SaveNodes(InitiumGraphSaveDataSO graphData, InitiumDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (InitiumNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);

                    continue;
                }

                ungroupedNodeNames.Add(node.DialogueName);
            }

            UpdateDialoguesChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraph(InitiumNode node, InitiumGraphSaveDataSO graphData)
        {
            List<InitiumChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            InitiumNodeSaveData nodeData = new InitiumNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Speaker = node.speaker,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }

        private static void SaveNodeToScriptableObject(InitiumNode node, InitiumDialogueContainerSO dialogueContainer)
        {
            InitiumDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<InitiumDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);

                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<InitiumDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);

                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode(),
                node.speaker
            );

            createdDialogues.Add(node.ID, dialogue);

            SaveAsset(dialogue);
        }

        private static List<InitiumDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<InitiumChoiceSaveData> nodeChoices)
        {
            List<InitiumDialogueChoiceData> dialogueChoices = new List<InitiumDialogueChoiceData>();

            foreach (InitiumChoiceSaveData nodeChoice in nodeChoices)
            {
                InitiumDialogueChoiceData choiceData = new InitiumDialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (InitiumNode node in nodes)
            {
                InitiumDialogueSO dialogue = createdDialogues[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; ++choiceIndex)
                {
                    InitiumChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                    {
                        continue;
                    }

                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, InitiumGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, InitiumGraphSaveDataSO graphData)
        {
            if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        public static void Load()
        {
            InitiumGraphSaveDataSO graphData = LoadAsset<InitiumGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", graphFileName);

            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Could not find the file!",
                    "The file at the following path could not be found:\n\n" +
                    $"\"Assets/Editor/DialogueSystem/Graphs/{graphFileName}\".\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                );

                return;
            }

            InitiumEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<InitiumGroupSaveData> groups)
        {
            foreach (InitiumGroupSaveData groupData in groups)
            {
                InitiumGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<InitiumNodeSaveData> nodes)
        {
            foreach (InitiumNodeSaveData nodeData in nodes)
            {
                List<InitiumChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);

                InitiumNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                node.speaker = nodeData.Speaker;

                node.Draw();

                graphView.AddElement(node);

                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                {
                    continue;
                }

                InitiumGroup group = loadedGroups[nodeData.GroupID];

                node.Group = group;

                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach (KeyValuePair<string, InitiumNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    InitiumChoiceSaveData choiceData = (InitiumChoiceSaveData) choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        continue;
                    }

                    InitiumNode nextNode = loadedNodes[choiceData.NodeID];

                    Port nextNodeInputPort = (Port) nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                    graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }

        private static void CreateDefaultFolders()
        {
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");

            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");

            CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }

        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(InitiumGroup);

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is InitiumNode node)
                {
                    nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    InitiumGroup group = (InitiumGroup) graphElement;

                    groups.Add(group);

                    return;
                }
            });
        }

        public static void CreateFolder(string parentFolderPath, string newFolderName)
        {
            if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
        }

        public static void RemoveFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory($"{path}.meta");
            FileUtil.DeleteFileOrDirectory($"{path}/");
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static List<InitiumChoiceSaveData> CloneNodeChoices(List<InitiumChoiceSaveData> nodeChoices)
        {
            List<InitiumChoiceSaveData> choices = new List<InitiumChoiceSaveData>();

            foreach (InitiumChoiceSaveData choice in nodeChoices)
            {
                InitiumChoiceSaveData choiceData = new InitiumChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }
    }
}