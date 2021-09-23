using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mercury.SceneManagement
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(500, 300);
        
        public DialogueGraphView() 
        {    
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            
            this.AddElement(GenerateEntryPointNode());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private Port GeneratePort(DialogueNode _node, Direction _portDirection, Port.Capacity _capacity = Port.Capacity.Single)
        {
            return _node.InstantiatePort(Orientation.Horizontal, _portDirection, _capacity, typeof(float));
        }
        
        private DialogueNode GenerateEntryPointNode()
        {
            var dialogueNode = new DialogueNode
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                DialogueText = "ENTRYPOINT",
                EntryPoint = true
            };

            Port generatedPort = GeneratePort(dialogueNode, Direction.Output, Port.Capacity.Single);
            generatedPort.portName = "Next";
            dialogueNode.outputContainer.Add(generatedPort);

            dialogueNode.capabilities &= ~Capabilities.Movable;
            dialogueNode.capabilities &= ~Capabilities.Deletable;
            
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
            
            dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

            return dialogueNode;
        }

        public void CreateNode(string _nodeName)
        {
            this.AddElement(CreateDialogueNode(_nodeName));
        }

        public DialogueNode CreateDialogueNode(string _nodeName)
        {
            DialogueNode dialogueNode = new DialogueNode
            {
                title = _nodeName,
                DialogueText = _nodeName,
                GUID = Guid.NewGuid().ToString()
            };

            Port inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            dialogueNode.inputContainer.Add(inputPort);

            dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            
            Button button = new Button(() => { AddChoicePort(dialogueNode); });
            button.text = "New Choice";
            dialogueNode.titleContainer.Add(button);

            TextField textField = new TextField(String.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                dialogueNode.DialogueText = evt.newValue;
                dialogueNode.title        = evt.newValue;
            });
            textField.SetValueWithoutNotify(dialogueNode.title);
            dialogueNode.mainContainer.Add(textField);
            
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
            dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));
            
            return dialogueNode;
        }

        public void AddChoicePort(DialogueNode _dialogueNode, string _overridenPortName = "")
        {
            Port generatedPort = GeneratePort(_dialogueNode, Direction.Output);

            //Label oldLabel = generatedPort.contentContainer.Q<Label>("type");
            //generatedPort.contentContainer.Remove(oldLabel);
            
            var outputPortCount = _dialogueNode.outputContainer.Query("connector").ToList().Count();
            var choicePortName  = string.IsNullOrEmpty(_overridenPortName) ? $"Choice {outputPortCount}" : _overridenPortName;

            TextField textField = new TextField
            {
                name = string.Empty,
                value = choicePortName
            };
            
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);

            Button deleteButton = new Button(() => RemovePort(_dialogueNode, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            
            generatedPort.portName = choicePortName;
            
            _dialogueNode.outputContainer.Add(generatedPort);
            _dialogueNode.RefreshExpandedState();
            _dialogueNode.RefreshPorts();
        }

        private void RemovePort(DialogueNode _dialogueNode, Port _generatedPort)
        {
            var targetEdge = edges.ToList().Where(x => x.output.portName == _generatedPort.portName && x.output.node == _generatedPort.node);

            if (targetEdge.Any())
            {
                Edge edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            _dialogueNode.outputContainer.Remove(_generatedPort);
            _dialogueNode.RefreshExpandedState();
            _dialogueNode.RefreshPorts();
        }
    }
}
