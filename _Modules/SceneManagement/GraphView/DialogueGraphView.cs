using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mercury
{
    public class DialogueGraphView : GraphView
    {
        private readonly Vector2 defaultNodeSize = new Vector2(400, 300);
        
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

            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();
            
            dialogueNode.SetPosition(new Rect(new Vector2(100, 200), defaultNodeSize));

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

            Button button = new Button(() => { AddChoicePort(dialogueNode); });
            button.text = "New Choice";
            dialogueNode.titleContainer.Add(button);
            
            dialogueNode.RefreshExpandedState();
            dialogueNode.RefreshPorts();

            dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));
            
            return dialogueNode;
        }

        
        private void AddChoicePort(DialogueNode _dialogueNode)
        {
            var generatedPort = GeneratePort(_dialogueNode, Direction.Output);

            var outputPortCount = _dialogueNode.outputContainer.Query("connector").ToList().Count();
            generatedPort.portName  = $"Choice {outputPortCount}";
            
            _dialogueNode.outputContainer.Add(generatedPort);
            
            _dialogueNode.RefreshExpandedState();
            _dialogueNode.RefreshPorts();
        }
    }
}
