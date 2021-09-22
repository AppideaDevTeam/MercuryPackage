using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mercury.SceneManagement
{
    [Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public List<NodeLinkData>     NodeLinks        = new List<NodeLinkData>();
        public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();
    }
}
