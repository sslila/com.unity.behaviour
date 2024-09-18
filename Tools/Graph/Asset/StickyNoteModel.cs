using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class StickyNoteModel : NodeModel
    {
        [SerializeField]
        public string Text = "Sticky Note!";

        public override bool HasDefaultInputPort => false;
        public override bool HasDefaultOutputPort => false;

        public StickyNoteModel() {}
        
        protected StickyNoteModel(StickyNoteModel nodeModelOriginal, GraphAsset asset) : base(nodeModelOriginal, asset)
        {
            Text = nodeModelOriginal.Text;
        }
    }
}
