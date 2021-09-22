using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mercury.SceneManagement
{
    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView graphView;
        private string            fileName = "New Setup";
        
        [MenuItem("Graph/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            DialogueGraph window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            //GenerateMiniMap();
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap {anchored = true};
            miniMap.SetPosition(new Rect(10,30,200,140));
            
            graphView.Add(miniMap);
        }

        private void ConstructGraphView()
        {
            graphView = new DialogueGraphView
            {
                name = "Dialogue Graph"
            };
            
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void GenerateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            TextField fileNameTextField = new TextField("FileName:");
            fileNameTextField.SetValueWithoutNotify(fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
            toolbar.Add(fileNameTextField);
            
            toolbar.Add(new Button(()=> RequestDataOperation(true)){text = "Save Data"});
            toolbar.Add(new Button(()=> RequestDataOperation(false)){text = "Load Data"});
            
            var nodeCreateButton  = new Button(() => { graphView.CreateNode("Dialogue Node"); });
            nodeCreateButton.text = "Create Node";
            toolbar.Add(nodeCreateButton);
            
            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(bool _save)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter the valid file name", "OK");
            }

            GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(graphView);
            
            if(_save) saveUtility.SaveGraph(fileName);
            else saveUtility.LoadGraph(fileName);
        }

        private void SaveData()
        {
            
        }
        private void LoadData()
        {
            
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }
    }
}
