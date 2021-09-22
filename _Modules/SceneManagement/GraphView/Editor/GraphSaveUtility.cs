using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mercury.SceneManagement;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mercury
{
    public class GraphSaveUtility
    {
        private DialogueGraphView  targetGraphView;
        private DialogueContainer  containerCache;
        private List<Edge>         Edges => targetGraphView.edges.ToList();
        private List<DialogueNode> Nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
        
        public static GraphSaveUtility GetInstance(DialogueGraphView _targetGraphView)
        {
            return new GraphSaveUtility
            {
                targetGraphView = _targetGraphView
            };
        }

        public void SaveGraph(string _fileName)
        {
            if (!Edges.Any()) return;

            DialogueContainer dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

            Edge[] connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
            
            for (var i = 0; i < connectedPorts.Length; i++)
            {
                Edge edge = connectedPorts[i];
                
                DialogueNode outputNode = edge.output.node as DialogueNode;
                DialogueNode inputNode  = edge.input.node as DialogueNode;

                dialogueContainer.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID   = outputNode.GUID,
                    TargetNodeGUID = inputNode.GUID,
                    PortName       = edge.output.portName
                });
            }

            foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
            {
                dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
                {
                    GUID = dialogueNode.GUID,
                    DialogueText = dialogueNode.DialogueText,
                    Position = dialogueNode.GetPosition().position
                });
            }

            string pathFolder = $"{MercuryInstaller.mercuryResourcesPath}{MercuryLibrarySO.Instance.Module_SceneManagement.Name}";
            string pathFull = $"{pathFolder}/{_fileName}.asset";

            if (!AssetDatabase.IsValidFolder(pathFolder))
            {
                EditorUtility.DisplayDialog("Invalid Path!", $"Please create folder \n{pathFolder}", "OK");
                return;
            }
            
            AssetDatabase.CreateAsset(dialogueContainer, pathFull);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public void LoadGraph(string _fileName)
        {
            string pathFolder = $"{MercuryLibrarySO.Instance.Module_SceneManagement.Name}";
            string pathFull   = $"{pathFolder}/{_fileName}";
            containerCache = Resources.Load<DialogueContainer>(pathFull);

            if (containerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found!", $"{_fileName}.asset doesn't exists", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }

        private void ClearGraph()
        {
            // Set entry points guid back from the save, discard existing
            Nodes.Find(x => x.EntryPoint).GUID = containerCache.NodeLinks[0].BaseNodeGUID;

            foreach (var node in Nodes)
            {
                if(node.EntryPoint) continue;
                
                // Remove edges that connected to this node
                Edges.Where(x => x.input.node == node).ToList().ForEach(edge=>targetGraphView.RemoveElement(edge));
                
                // Then remove the node
                targetGraphView.RemoveElement(node);
            }
        }
        
        private void CreateNodes()
        {
            foreach (var nodeData in containerCache.DialogueNodeData)
            {
                DialogueNode tempNode = targetGraphView.CreateDialogueNode(nodeData.DialogueText);
                tempNode.GUID = nodeData.GUID;
                targetGraphView.AddElement(tempNode);

                var nodePorts = containerCache.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.GUID);
                nodePorts.ForEach(x => targetGraphView.AddChoicePort(tempNode, x.PortName));
            }
        }
        
        private void ConnectNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var connections = containerCache.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[i].GUID).ToList();
                for (var j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGUID;
                    var targetNode     = Nodes.First(x => x.GUID == targetNodeGuid);

                    LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);
                    
                    targetNode.SetPosition(new Rect(containerCache.DialogueNodeData.First(x=>x.GUID == targetNodeGuid).Position, targetGraphView.defaultNodeSize));
                }
            }
        }

        private void LinkNodes(Port _output, Port _input)
        {
            var tempEdge = new Edge
            {
                input = _input,
                output = _output
            };
            
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            targetGraphView.AddElement(tempEdge);
        }
    }
}
