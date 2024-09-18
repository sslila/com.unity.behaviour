using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(StartOnEvent))]
    internal class StartOnEventModel : EventNodeModel
    {
        public const string k_RestartOnNewMessageNodeUITitleName = "(Restart)";
        public const string k_TriggerOnceNodeUITitleName = "(Once)";
        public const string k_TriggerModeFieldName = "Mode";
        public const string k_TriggerModeTooltips =
            "Select the event trigger behavior." +
            "\n- 'Default': the node will trigger only when it is idling and no child node is running." +
            "\n- 'Restart': ends all children nodes and then restart the execution from the node." +
            "\n- 'Once': the node will trigger only once and stop listening to the event channel.";

        public override bool IsDuplicatable => true;
        public override bool IsSequenceable => false;
        public override bool IsRoot => true;

        public override bool HasDefaultInputPort => false;
        public override int MaxInputsAccepted => 0;

        public StartOnEvent.TriggerBehavior TriggerBehavior;

        public StartOnEventModel(NodeInfo nodeInfo) : base(nodeInfo) { }

        protected StartOnEventModel(StartOnEventModel original, BehaviorAuthoringGraph asset)  : base(original, asset)  { }
    }
}