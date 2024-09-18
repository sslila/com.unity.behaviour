namespace Unity.Behavior
{
    [NodeModelInfo(typeof(WaitForAnyComposite))]
    [NodeModelInfo(typeof(WaitForAllComposite))]
    internal class JoinNodeModel : BehaviorGraphNodeModel
    {
        public override int MaxInputsAccepted => int.MaxValue;

        public JoinNodeModel(NodeInfo nodeInfo) : base(nodeInfo) { }
        
        protected JoinNodeModel(JoinNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
        }
    }
}