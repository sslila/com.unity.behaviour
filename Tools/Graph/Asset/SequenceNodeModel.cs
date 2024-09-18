using System.Collections.Generic;
using UnityEngine;


namespace Unity.Behavior.GraphFramework
{
    internal class SequenceNodeModel : NodeModel
    {
        [SerializeReference]
        public List<NodeModel> Nodes = new List<NodeModel>();
        public override bool IsSequenceable => true;

        public SequenceNodeModel(){ }
        
        protected SequenceNodeModel(SequenceNodeModel nodeModelOriginal, GraphAsset asset) : base(nodeModelOriginal, asset)
        {
            // Nodes collection cannot be duplicated here, cause we could not add them to the GraphManager, create the UI, etc.
            // that's why this has to be done by the caller of Duplicate() :(
        } 

        public override void OnValidate()
        {
            base.OnValidate();
            
            Nodes.RemoveAll(node =>
            {
                bool invalidLink = !node.Parents.Contains(this);
                if (invalidLink)
                {
                    Debug.LogWarning($"Missing parent link for {node} at {node.Position} in sequence {ID} at {Position}");
                }
            
                return invalidLink;
            });
        }
    }
}