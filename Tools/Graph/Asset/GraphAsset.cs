using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Behavior.GraphFramework
{
    internal class GraphAsset : ScriptableObject
    {
        [SerializeReference] 
        public BlackboardAsset Blackboard;
        
        [SerializeField] 
        private string m_Description;
        public string Description
        {
            get => m_Description;
            set => m_Description = value;
        }
        
        [SerializeReference]
        private List<NodeModel> m_Nodes = new();
        public List<NodeModel> Nodes
        {
            get => m_Nodes;
            set => m_Nodes = value;
        }
        
        // TODO: Darren, This needs to be not used in the graph editor.
        // Used to indicate to graph editor that the UI should be refreshed.
        // Ideally, all remaining instances where MarkUndo() is used will instead notify the editor directly,
        // at which point we can remove this property.
        internal bool HasOutstandingChanges { get; set; }

        [SerializeField][HideInInspector]
        internal long m_VersionTimestamp;
        public long VersionTimestamp => m_VersionTimestamp;
        
        public void MarkUndo(string description)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, description);
#endif
            // There are still a few lingering non-command changes to asset data preceded by MarkUndo() calls.
            // In order to pick up these changes, set the asset dirty here too.
            SetAssetDirty();
        }     

        public void SaveAsset()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            // Note: Using AssetDatabase.SaveAssetIfDirty(this) saves the asset but doesn't pass the path to
            // AssetModificationProcessor.OnWillSaveAssets(), which we use to rebuild graphs which reference this one.
            // Instead, use AssetDatabase.SaveAssets().
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public void SetAssetDirty(bool setHasOutStandingChange = true)
        {
            m_VersionTimestamp = DateTime.Now.Ticks;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            if (setHasOutStandingChange)
            {
                HasOutstandingChanges = true;
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
            }
        }

        public virtual void OnValidate()
        {
            Nodes.RemoveAll(node => node == null);
            var nodesCopyList = new List<NodeModel>(Nodes);
            foreach (NodeModel node in nodesCopyList)
            {
                // holdovers for supporting older graphs - remove for 1.0.0
                if (node.Asset == null)
                {
                    node.Asset = this;
                }
                if (!node.HasPortModels)
                {
                    node.OnDefineNode();
                }
                node.OnValidate();
            }

            Blackboard?.OnValidate();
        }

        private void OnEnable()
        {
            EnsureAssetHasBlackboard();
        }

        internal virtual void EnsureAssetHasBlackboard()
        {
            string blackboardName = name + " Blackboard";
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            BlackboardAsset blackboard = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path)
                .FirstOrDefault(asset => asset is BlackboardAsset) as BlackboardAsset;
            if (blackboard != null)
            {
                if (Blackboard != null && blackboard == Blackboard)
                {
                    // Update the graph Blackboard name if needed.
                    if (blackboard.name != blackboardName)
                    {
                        blackboard.name = blackboardName;
                    }   
                }
                return;
            }
#endif
            if (Blackboard == null)
            {
                Blackboard = CreateInstance<BlackboardAsset>();
                Blackboard.name = blackboardName;
            }
            
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
                
            UnityEditor.AssetDatabase.AddObjectToAsset(Blackboard, this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        #region Model changing API
        public NodeModel CreateNode(Type nodeType, Vector2 position, PortModel connectedPort = null, object[] args = null)
        {
            var node = Activator.CreateInstance(nodeType, args) as NodeModel;
            node.Asset = this;
            node.Position = position;
            node.OnDefineNode();
            Nodes.Add(node);

            // Connect the node to the specified port. 
            if (connectedPort != null)
            {
                if (connectedPort.IsInputPort && node.TryDefaultOutputPortModel(out PortModel portModelTarget))
                {
                    portModelTarget.ConnectTo(connectedPort);
                }
                else if (connectedPort.IsOutputPort && node.TryDefaultInputPortModel(out portModelTarget))
                {
                    connectedPort.ConnectTo(portModelTarget);
                }
            }

            CreateNodePortsForNode(node);

            return node;
        }

        internal void CreateNodePortsForNode(NodeModel parentNode)
        {
            float offsetDeltaX = 200.0f * (parentNode.OutputPortModels.Count() - 1);
            float offsetX = -offsetDeltaX * (parentNode.OutputPortModels.Count() - 1) * 0.5f;
            foreach (PortModel portModel in parentNode.OutputPortModels)
            {
                if (portModel.Connections.Count() != 0 || portModel.IsDefaultOutputPort)
                {
                    // Port already exists.
                    continue;
                }

                NodeModel newNode = CreateNode(typeof(FloatingPortNodeModel), parentNode.Position + new Vector2(offsetX, 200.0f), portModel, new object[] { parentNode.ID, portModel.Name });
                offsetX += offsetDeltaX;
            }
        }

        public void DeleteNode(NodeModel node)
        {
            if (!Nodes.Contains(node)) //node has already been deleted. Ex: it was a part of a sequence.
                return;

            DeleteNodePortsForNode(node);

            List<NodeModel> nodesToDelete = new List<NodeModel>();
            foreach (PortModel inputPort in node.InputPortModels)
            {
                foreach (PortModel connection in inputPort.Connections.ToList())
                {
                    DeleteEdge(connection, inputPort);
                }
            }

            foreach (PortModel outputPort in node.OutputPortModels)
            {
                foreach (PortModel connection in outputPort.Connections.ToList())
                {
                    DeleteEdge(outputPort, connection);
                }
            }

            // If the node is a sequence, delete the nested nodes too.
            if (node is SequenceNodeModel sequenceModel)
            {
                nodesToDelete.AddRange(sequenceModel.Nodes);
            }
            else
            {
                // Otherwise, the node may be nested within a sequence group, so remove it there as well.
                // Note: This only affects sequences of actions nested together, not nodes linked with edges in an
                // implicit sequence.
                foreach (NodeModel parent in node.Parents)
                {
                    if (parent is not SequenceNodeModel sequence)
                    {
                        continue;
                    }

                    // Remove link between the sequence and the node being deleted.
                    sequence.Nodes.Remove(node);
                    if (sequence.Nodes.Count >= 2)
                    {
                        continue;
                    }

                    // If the sequence only has one child, remove the sequence and connect the parent to the child.
                    if (sequence.Nodes.Count == 1)
                    {
                        NodeModel child = sequence.Nodes.First();
                        child.Position = sequence.Position;
                        child.Parents.Remove(sequence);
                        sequence.Nodes.Remove(child);

                        // Connect the parent's edges to the remaining child.
                        PortModel sequenceOutputConnection = sequence.OutgoingConnections.FirstOrDefault();
                        if (sequenceOutputConnection != null && child.TryDefaultOutputPortModel(out PortModel childOutputPortModel))
                        {
                            ConnectEdge(childOutputPortModel, sequenceOutputConnection);
                        }
                        PortModel sequenceInputConnection = sequence.IncomingConnections.FirstOrDefault();
                        if (sequenceInputConnection != null && child.TryDefaultInputPortModel(out PortModel childInputPortModel))
                        {
                            ConnectEdge(childInputPortModel, sequenceInputConnection);
                        }
                    }

                    // Since the sequence no longer contains nodes, delete it.
                    nodesToDelete.Add(sequence);
                }
            }
            Nodes.Remove(node);

            foreach (NodeModel nodeToDelete in nodesToDelete)
            {
                if (Nodes.Contains(nodeToDelete))
                {
                    DeleteNode(nodeToDelete);
                }
            }
        }

        internal void DeleteNodePortsForNode(NodeModel node)
        {
            foreach (PortModel portModel in node.OutputPortModels)
            {
                foreach (PortModel connectedPort in portModel.Connections.ToArray())
                {
                    if (connectedPort.NodeModel is FloatingPortNodeModel)
                    {
                        DeleteNode(connectedPort.NodeModel);
                    }
                }
            }
        }

        public void ConnectEdge(PortModel startPort, PortModel endPort)
        {
            Assert.IsTrue(Nodes.Contains(startPort.NodeModel), $"Asset {this} does not contain node {startPort.NodeModel}.");
            Assert.IsTrue(Nodes.Contains(endPort.NodeModel), $"Asset {this} does not contain node {endPort.NodeModel}.");

            startPort.ConnectTo(endPort);
        }

        public void DeleteEdge(PortModel startPort, PortModel endPort)
        {
            startPort.RemoveConnectionTo(endPort);
            endPort.RemoveConnectionTo(startPort);
        }

        public void RemoveNodeFromSequence(NodeModel node)
        {
            Assert.IsTrue(Nodes.Contains(node), $"Asset {this} does not contain node {node}.");
            foreach (NodeModel parent in node.Parents)
            {
                if (parent is SequenceNodeModel parentSequence)
                {
                    parentSequence.Nodes.Remove(node);
                }
            }
        }

        public void AddNodeToSequence(NodeModel node, SequenceNodeModel sequence, int index)
        {
            Assert.IsTrue(Nodes.Contains(node), $"Asset {this} does not contain node {node}.");
            Assert.IsTrue(Nodes.Contains(sequence), $"Asset {this} does not contain node {sequence}.");

            // Remove node from any existing parent sequences
            foreach (NodeModel parent in node.Parents)
            {
                if (parent is SequenceNodeModel parentSequence)
                {
                    parentSequence.Nodes.Remove(node);
                }
            }

            // Add node to the new sequence model 
            if (index >= sequence.Nodes.Count)
            {
                sequence.Nodes.Add(node);
            }
            else
            {
                sequence.Nodes.Insert(index, node);
            }

            // disconnect ports
            foreach (PortModel portModel in node.AllPortModels)
            {
                foreach (PortModel connectedPort in portModel.Connections)
                {
                    connectedPort.RemoveConnectionTo(portModel);
                }
                portModel.ClearConnections();
            }
            node.Parents.Clear(); // disconnect from previous parents
            node.Parents.Add(sequence); // add parent link to sequence
        }
        #endregion
    }
}