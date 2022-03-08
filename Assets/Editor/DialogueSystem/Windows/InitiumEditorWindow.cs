using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Initium.Windows
{
    using System;
    using Utilities;

    public class InitiumEditorWindow : EditorWindow
    {
        private InitiumGraphView graphView;

        private readonly string defaultFileName = "DialoguesFileName";

        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

        [MenuItem("Window/Initium Dialogue")]
        public static void Open()
        {
            GetWindow<InitiumEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();

            AdInitiumtyles();
        }

        private void AddGraphView()
        {
            graphView = new InitiumGraphView(this);

            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = InitiumElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = InitiumElementUtility.CreateButton("Save", () => Save());

            Button loadButton = InitiumElementUtility.CreateButton("Load", () => Load());
            Button clearButton = InitiumElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = InitiumElementUtility.CreateButton("Reset", () => ResetGraph());

            miniMapButton = InitiumElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            toolbar.AdInitiumtyleSheets("DialogueSystem/InitiumToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AdInitiumtyles()
        {
            rootVisualElement.AdInitiumtyleSheets("DialogueSystem/InitiumVariables.uss");
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid file name.", "Please ensure the file name you've typed in is valid.", "Roger!");

                return;
            }

            InitiumIOUtility.Initialize(graphView, fileNameTextField.value);
            InitiumIOUtility.Save();
        }

        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();

            InitiumIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            InitiumIOUtility.Load();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();

            UpdateFileName(defaultFileName);
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();

            miniMapButton.ToggleInClassList("Initium-toolbar__button__selected");
        }

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
}