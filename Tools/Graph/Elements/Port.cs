#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class Port : VisualElement
    {
        private static readonly CustomStyleProperty<string> s_PortStyleProperty = new ("--port-style");
        private static readonly CustomStyleProperty<string> s_PortOrientationProperty = new ("--port-orientation");
        
        private const string k_EdgePortClassName = "EdgePort";
        private const string k_SocketPortClassName = "SocketPort";
        
        private const string k_VerticalClassName = "VerticalPort";
        private const string k_HorizontalPortClassName = "HorizontalPort";
        
        private const string k_InputClassName = "InputPort";
        private const string k_OutputClassName = "OutputPort";

        public PortModel PortModel { get; set; }

        PortStyle m_Style = PortStyle.Edge;
        public PortStyle Style
        {
            get => m_Style;
            set
            {
                m_Style = value;
                UpdatePortStyle();
            }
        }

        PortOrientation m_Orientation = PortOrientation.Vertical;
        public PortOrientation Orientation
        {
            get => m_Orientation;
            set
            {
                m_Orientation = value;
                UpdatePortOrientation();
            }
        }

        private readonly List<Edge> m_Edges = new List<Edge>();
        public List<Edge> Edges => m_Edges;
        
        public bool IsPortActive => !GetFirstAncestorOfType<NodeUI>().IsInSequence;

        public Port(PortModel portModel)
        {
            PortModel = portModel;
            name = portModel.Name;
            
            AddToClassList("Port");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/PortStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/Graph/Assets/PortLayout.uxml").CloneTree(this);
            this.AddManipulator(new EdgeConnectManipulator());

            UpdatePortStyle();
            UpdatePortOrientation();
            AddToClassList(portModel.IsInputPort ? k_InputClassName : k_OutputClassName);

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            if (PortModel != null && PortModel.IsFloating)
            {
                pickingMode = PickingMode.Ignore;
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            ICustomStyle customStyle = e.customStyle;
            if (customStyle.TryGetValue(s_PortStyleProperty, out string portStyleValue))
            {
                portStyleValue = portStyleValue.ToLower();
                if (portStyleValue.Equals("socket"))
                {
                    Style = PortStyle.Socket;
                }
                else if (portStyleValue.Equals("edge"))
                {
                    Style = PortStyle.Edge;
                }
            }
            if (customStyle.TryGetValue(s_PortOrientationProperty, out string portOrientationValue))
            {
                portOrientationValue = portStyleValue.ToLower();
                if (portOrientationValue.Equals("vertical"))
                {
                    Orientation = PortOrientation.Vertical;
                }
                else if (portOrientationValue.Equals("horizontal"))
                {
                    Orientation = PortOrientation.Horizontal;
                }
            }
        }

        private void UpdatePortStyle()
        {
            RemoveFromClassList(k_EdgePortClassName);
            RemoveFromClassList(k_SocketPortClassName);

            if (Style == PortStyle.Edge)
            {
                AddToClassList(k_EdgePortClassName);
            }
            else if (Style == PortStyle.Socket)
            {
                AddToClassList(k_SocketPortClassName);
            }
        }

        private void UpdatePortOrientation()
        {
            RemoveFromClassList(k_VerticalClassName);
            RemoveFromClassList(k_HorizontalPortClassName);

            if (Orientation == PortOrientation.Vertical)
            {
                AddToClassList(k_VerticalClassName);
            }
            else if (Orientation == PortOrientation.Horizontal)
            {
                AddToClassList(k_HorizontalPortClassName);
            }
        }
    }
}
#endif