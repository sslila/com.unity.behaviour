#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class EdgeConnectManipulator : PointerManipulator
    {
        public bool AllowCycles { get; set; } = false;

        private Edge Edge { get; set; }
        private Port Target => target as Port;
        private GraphView View => Target.GetFirstAncestorOfType<GraphView>();
        private NodeUI HoveredNode { get; set; }
        private readonly HashSet<NodeUI> m_ValidConnectTargets = new ();

        internal EdgeConnectManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
            {
                return;
            }
            
            // todo: not sure if this is needed, cause we should not allow port UIs without port model
            if (Target.PortModel == null)
            {
                return;
            }
            
            target.CaptureMouse();
            evt.StopImmediatePropagation();

            Edge = new Edge();
            Edge.AddToClassList("ActiveEdgeConnectManipulator");
            View.contentContainer.Add(Edge);
            if (Target.PortModel.IsInputPort)
            {
                Edge.End = Target;
                Edge.StartPosition = evt.position;
            }
            else if(Target.PortModel.IsOutputPort)
            {
                Edge.Start = Target;
                Edge.EndPosition = evt.position;
            }

            PopulatePotentialConnectTargets();
            HighlightTargets();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!target.HasMouseCapture())
            {
                return;
            }
            NodeUI previousHoveredNode = HoveredNode;
            HighlightHoveredTarget(evt.position);
            evt.StopImmediatePropagation();

            if (HoveredNode == null)
            {
                if (Target.PortModel.IsInputPort)
                {
                    Edge.StartPosition = evt.position;
                    Edge.Start = null;
                }
                else if(Target.PortModel.IsOutputPort)
                {
                    Edge.EndPosition = evt.position;
                    Edge.End = null;
                }
            }
            else
            {
                if (previousHoveredNode != HoveredNode)
                {
                    if (Target.PortModel.IsInputPort)
                    {
                        Edge.Start = GetPotentialPortFromNode(HoveredNode);
                    }
                    else if (Target.PortModel.IsOutputPort)
                    {
                        Edge.End = GetPotentialPortFromNode(HoveredNode);
                    }
                }
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            RemoveHighlightTargets();
            if (!target.HasMouseCapture())
            {
                return;
            }
            target.ReleaseMouse();
            evt.StopImmediatePropagation();

            // Remove temporary edge.
            Edge.RemoveFromHierarchy();
            Edge = null;
            
            PortDataFlowType targetSide = Target.PortModel.IsInputPort ? PortDataFlowType.Output : PortDataFlowType.Input; 
            NodeUI nodeAtMousePosition = View.ViewState.Nodes.FirstOrDefault(node => 
                node.ContainsPoint(node.WorldToLocal(evt.position)) && node.GetFirstAncestorOfType<Group>() == null);
            Port portAtMousePosition = nodeAtMousePosition?.GetAllPortUIs()
                .FirstOrDefault(port => port.IsPortActive && port.PortModel.PortDataFlowType == targetSide 
                                                          && port.ContainsPoint(port.WorldToLocal(evt.position)));
            Port portUITarget = null;
            
            // If a valid port exists at the mouse position, use it.
            if (portAtMousePosition != null)
            {
                VisualElement commonAncestor = portAtMousePosition.FindCommonAncestor(Target);
                if (commonAncestor != View.contentContainer)
                {
                    // The ports share a common ancestor node and shouldn't be linked.
                    // They either come from the same node or are in the same sequence.
                    return;
                }
                
                portUITarget = portAtMousePosition;
            }
            // if node and not port, connect to default port 
            else if (nodeAtMousePosition != null)
            {
                portUITarget = targetSide == PortDataFlowType.Input
                    ? nodeAtMousePosition.GetFirstInputPort()
                    : nodeAtMousePosition.GetFirstOutputPort();
            }
            
            // If no valid port was found or the node is already connected by an edge, show the node search.
            if (portUITarget == null || Target.PortModel.Connections.Contains(portUITarget.PortModel))
            {
                View.ShowNodeSearch(evt.position, Target.GetFirstAncestorOfType<NodeUI>().Model.FindPortModelByName(Target.name));
                return;
            }

            if (Target.PortModel.NodeModel == portUITarget.PortModel.NodeModel)
            {
                // Don't allow connections to self.
                return;
            }
            
            // Connect ports.
            PortModel outputPortModel, inputPortModel;
            if (Target.PortModel.IsInputPort)
            {
                NodeUI outputNodeUI = portUITarget.GetFirstAncestorOfType<NodeUI>();
                NodeUI inputNodeUI = Target.GetFirstAncestorOfType<NodeUI>();
                
                if (!AllowCycles && GraphUILayoutUtility.IsAnAncestor(outputNodeUI, inputNodeUI))
                {
                    // Ensure graph remains acyclic by not linking nodes to their ancestors.
                    return;
                }
                
                outputPortModel = portUITarget.PortModel;
                inputPortModel = Target.PortModel;
            } 
            else
            {
                NodeUI outputNodeUI = Target.GetFirstAncestorOfType<NodeUI>();
                NodeUI inputNodeUI = portUITarget.GetFirstAncestorOfType<NodeUI>();
                
                if (!AllowCycles && GraphUILayoutUtility.IsAnAncestor(outputNodeUI, inputNodeUI))
                {
                    // Ensure graph remains acyclic by not linking nodes to their ancestors.
                    return;
                }
                
                outputPortModel = Target.PortModel;
                inputPortModel = portUITarget.PortModel;
            }

            if (outputPortModel.IsFloating || inputPortModel.IsFloating)
            {
                // Do not allow manually connecting to floating ports models.
                return;
            }
            View.Dispatcher.DispatchImmediate(new ConnectEdgeCommand(outputPortModel,inputPortModel));
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!target.HasMouseCapture())
            {
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                RemoveHighlightTargets();
                target.ReleaseMouse();
                evt.StopImmediatePropagation();
                if (Edge != null)
                {
                    Edge.RemoveFromHierarchy();
                    Edge = null;
                }
            }
        }

        private Port GetPotentialPortFromNode(NodeUI node)
        {
            return Target.PortModel.PortDataFlowType == PortDataFlowType.Input ? 
                node.GetFirstOutputPort() : node.GetFirstInputPort();
        }

        private void PopulatePotentialConnectTargets()
        {
            m_ValidConnectTargets.Clear();
            
            PortDataFlowType targetPortType = Target.PortModel.PortDataFlowType;
            foreach (NodeUI nodeUI in View.ViewState.Nodes)
            {
                // Don't allow connections to nested nodes.
                if (nodeUI.IsInSequence)
                {
                    continue;
                }

                // Don't allow connections to self.
                if (Target.PortModel.NodeModel == nodeUI.Model)
                {
                    continue;
                }

                // If the target is an input, check if the node has available outputs.
                if (targetPortType == PortDataFlowType.Input)
                {
                    var outputPorts = nodeUI.GetOutputPortUIs().Where(port => !port.PortModel.IsFloating);
                    int outputs = outputPorts.Sum(port => port.Edges.Count);
                    if (outputPorts.Count() != 0 && outputs < nodeUI.Model.MaxOutputsAccepted)
                    {
                        m_ValidConnectTargets.Add(nodeUI);
                    }
                }
                // If the target is an output, check if the node has available inputs.
                else
                {
                    var inputPorts = nodeUI.GetInputPortUIs().Where(port => !port.PortModel.IsFloating);
                    int inputs = inputPorts.Sum(port => port.Edges.Count);
                    if (inputPorts.Count() != 0 && inputs < nodeUI.Model.MaxInputsAccepted)
                    {
                        m_ValidConnectTargets.Add(nodeUI);
                    }
                }
            }
        }
        
        private void HighlightTargets()
        {
            if (m_ValidConnectTargets == null)
            {
                return;
            }

            View.AddToClassList("HighlightInputs");
            Target.GetFirstAncestorOfType<NodeUI>().AddToClassList("EdgeConnectPort");
            foreach (NodeUI node in m_ValidConnectTargets)
            {
                node.AddToClassList("PotentialNodeConnect");
            }
        }

        private void RemoveHighlightTargets()
        {
            HoveredNode?.RemoveFromClassList("HoveredNodeConnectOutput");
            HoveredNode?.RemoveFromClassList("HoveredNodeConnectInput");
            HoveredNode = null;
            if (m_ValidConnectTargets == null)
            {
                return;
            }

            View.RemoveFromClassList("HighlightInputs");
            Target.GetFirstAncestorOfType<NodeUI>().RemoveFromClassList("EdgeConnectPort");
            foreach (NodeUI node in m_ValidConnectTargets)
            {
                node.RemoveFromClassList("PotentialNodeConnect");
            }
        }
        
        private void HighlightHoveredTarget(Vector2 mouseWorldPos)
        {
            if (m_ValidConnectTargets == null)
            {
                return;
            }

            if (HoveredNode != null) 
            {
                if (HoveredNode.ContainsPoint(HoveredNode.WorldToLocal(mouseWorldPos)))
                {
                    return;
                }

                if (Target.PortModel.IsInputPort)
                {
                    HoveredNode.RemoveFromClassList("HoveredNodeConnectOutput");
                }
                else if (Target.PortModel.IsOutputPort)
                {
                    HoveredNode.RemoveFromClassList("HoveredNodeConnectInput");
                }

                HoveredNode = null;
            }

            foreach (NodeUI node in m_ValidConnectTargets)
            {
                Vector2 localPoint = node.WorldToLocal(mouseWorldPos);
                if (node.ContainsPoint(localPoint))
                {
                    HoveredNode = node;
                    if (Target.PortModel.IsInputPort)
                    {
                        HoveredNode.AddToClassList("HoveredNodeConnectOutput");
                    }
                    else if (Target.PortModel.IsOutputPort)
                    {
                        HoveredNode.AddToClassList("HoveredNodeConnectInput");
                    }
                    return;
                }
            }
        }
    }
}
#endif