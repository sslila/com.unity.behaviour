using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Behavior.GraphFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.Behavior.GenerativeAI
{
    [Serializable]
    internal class AINode
    {
        public string name;
        public string description;
        public string[] variables;
        public AINode[] nodes;
    }

    internal static class BranchGeneratorUtility
    {
        private static readonly List<Type> s_ExcludedCompositeTypes = new()
        {
            typeof(ParallelAllSuccess), typeof(ParallelAnyComposite),
            typeof(ParallelAnySuccess)
        };
        
        public static List<NodeModel> GenerateNodes(GraphAsset asset, Vector2 position, string response,
            List<NodeInfo> nodeInfos)
        {
            List<NodeModel> nodes = new();
            if (!IsValidJson(ref response))
            {
                return nodes;
            }

            asset.MarkUndo("Generate Branch");
            SequenceNodeModel sequenceNodeModel = null;
            int sequenceIndex = 0;
            PortModel port = null;

            AINode rootNode = JsonConvert.DeserializeObject<AINode>(response);
            CreateNodesRecursively(rootNode, null, 0);
            return nodes;

            void CreateNodesRecursively(AINode node, BehaviorGraphNodeModel parentNode, int portIndex)
            {
                //Current method to prevent errors cause by Switch nodes.
                if (parentNode?.OutputPortModels.Count() == 0)
                {
                    Debug.LogWarning("Parent node has no output ports: branch generation stopped.");
                    return;
                }

                port = parentNode?.OutputPortModels.ElementAt(portIndex);

                if (port is { IsFloating: true })
                {
                    port = port.Connections.First().NodeModel.OutputPortModels.First();
                    nodes.Add(port.NodeModel);
                }

                NodeInfo nodeInfo = nodeInfos.FirstOrDefault(info => info.Name == node.name);
                BehaviorGraphNodeModel behaviorGraphNodeModel = null;
                // If the node is not found in the registry, create a placeholder node.
                if (nodeInfo == null)
                {
                    var placeholderNodeModel =
                        asset.CreateNode(typeof(PlaceholderNodeModel), position, port, args: new object[]
                            {
                                null
                            })
                            as PlaceholderNodeModel;
                    placeholderNodeModel.Name = AddSpaceToName(node.name);
                    placeholderNodeModel.Story = node.description;
                    if (node.variables != null)
                    {
                        foreach (var variable in node.variables)
                        {
                            placeholderNodeModel.Variables.Add(new VariableInfo { Name = variable });
                        }
                    }
                    behaviorGraphNodeModel = placeholderNodeModel;
                }
                else
                {
                    behaviorGraphNodeModel =
                        asset.CreateNode(nodeInfo.ModelType, position, port, args: new[] { nodeInfo }) as
                            BehaviorGraphNodeModel;
                    if (nodeInfo.Variables is { Count: > 0 })
                    {
                        LinkVariablesToLinkFields(asset, nodeInfo, node.variables, behaviorGraphNodeModel);
                    }
                }

                if (sequenceNodeModel != null)
                {
                    asset.AddNodeToSequence(behaviorGraphNodeModel, sequenceNodeModel, sequenceIndex++);
                }

                nodes.Add(behaviorGraphNodeModel);
                // Offset so that we can use the auto-alignment feature.
                position += new Vector2(200f, 70f);

                bool hasChildren = typeof(Composite).IsAssignableFrom(behaviorGraphNodeModel.NodeType)
                                   || typeof(Modifier).IsAssignableFrom(behaviorGraphNodeModel.NodeType);
                if (hasChildren)
                {
                    // Check consecutive actions & conditions to determine if a sequence group should be created.
                    int j = 0;
                    bool canCreateSequenceGroup = HasConsecutiveActions(node.nodes, out int actionCount) 
                                                  && (typeof(SequenceComposite).IsAssignableFrom(behaviorGraphNodeModel.NodeType) 
                                                  || typeof(Modifier).IsAssignableFrom(behaviorGraphNodeModel.NodeType)) 
                                                  && node.nodes.Length > 1
                                                  && actionCount > 1;
                    if (canCreateSequenceGroup)
                    {
                        port = behaviorGraphNodeModel.OutputPortModels.ElementAt(0);
                        sequenceNodeModel = asset.CreateNode(typeof(SequenceNodeModel), position, port) as SequenceNodeModel;
                        nodes.Add(sequenceNodeModel);
                    }

                    for (int i = 0; i < node.nodes.Length; i++)
                    {
                        var child = node.nodes[i];
                        CreateNodesRecursively(child, behaviorGraphNodeModel, j);
                        
                        // Only increment if the parent has more than one output port.
                        if (behaviorGraphNodeModel.OutputPortModels.Count() > 1)
                            j++;
                        
                        // Break sequence if the last consecutive action is reached.
                        if (i == actionCount - 1)
                            BreakSequence();
                    }

                    //reset sequence after all children have been processed.
                    if (sequenceNodeModel != null)
                        BreakSequence();
                }
                else
                {
                    for (int i = 0; i < node.nodes.Length; i++)
                    {
                        var child = node.nodes[i];
                        if (i >= 1) // safety a non-composite node can only have one child. 
                        {
                            CreateNodesRecursively(child, null, 0);
                        }
                        else
                        {
                            CreateNodesRecursively(child, behaviorGraphNodeModel, i);
                        }
                    }
                }

                //TODO: instead, we could ask the LLM to pass the type of the node in the JSON. but this might increase generation time.
                bool HasConsecutiveActions(AINode[] childrenNodes, out int actionCount)
                {
                    actionCount = 0;
                    foreach (AINode n in childrenNodes)
                    {
                        NodeInfo info = nodeInfos.FirstOrDefault(info => info.Name == n.name);
                        if (info != null && !typeof(Action).IsAssignableFrom(info.Type))
                        {
                            return false;
                        }
                        
                        // If the info was null, it's a placeholder action node.
                        // Otherwise, it's a known action node type.
                        actionCount++;
                    }

                    return true;
                }
            }

            void BreakSequence()
            {
                sequenceIndex = 0;
                sequenceNodeModel = null;
            }
        }

        private static void LinkVariablesToLinkFields(GraphAsset asset, NodeInfo nodeInfo, string[] variables,
            BehaviorGraphNodeModel node)
        {
            var i = 0;
            foreach (var variable in variables)
            {
                string param = variable.Trim().CapitalizeFirstLetter();
                VariableInfo variableInfo = nodeInfo.Variables[i];
                VariableModel variableModel = asset.Blackboard.Variables.FirstOrDefault(v => v.Name == param);
                // Blackboard variable match, assign it to the link field.
                if (variableModel != null)
                    node.SetField(variableInfo.Name, variableModel, variableModel.Type);
                else
                    TryAssignParamValueToLinkField(node, variableInfo, param);
                i++;
            }
        }

        static bool TryAssignParamValueToLinkField(BehaviorGraphNodeModel node, VariableInfo variableInfo, string param)
        {
            Type type = variableInfo.Type;
            if (type.GetGenericArguments().Length == 0)
                return false;

            Type genericType = type.GetGenericArguments()[0];
            if (genericType == typeof(int) && int.TryParse(param, out int intValue))
            {
                node?.SetField(variableInfo.Name, intValue);
                return true;
            }

            if (genericType == typeof(float) && float.TryParse(param, out float floatValue))
            {
                node?.SetField(variableInfo.Name, floatValue);
                return true;
            }

            if (genericType == typeof(double) && double.TryParse(param, out double doubleValue))
            {
                node?.SetField(variableInfo.Name, doubleValue);
                return true;
            }

            if (genericType == typeof(bool) && bool.TryParse(param, out bool boolValue))
            {
                node?.SetField(variableInfo.Name, boolValue);
                return true;
            }

            if (genericType == typeof(ConditionOperator) &&
                Enum.TryParse(param, out ConditionOperator conditionOperator))
            {
                node?.SetField(variableInfo.Name, conditionOperator);
                return true;
            }

            if (genericType == typeof(string))
            {
                node?.SetField(variableInfo.Name, param);
                return true;
            }

            return false;
        }

        internal static string ReplaceInPrompt(string actions, string composites, string modifiers, string variables,
            string description, int promptMethod = 0)
        {
            TextAsset[] prompts =
            {
                ResourceLoadAPI.Load<TextAsset>("Packages/com.unity.behavior/Authoring/GenerativeAI/Assets/Prompts/0_JsonPrompt.txt")
            };

            string prompt = prompts[promptMethod].text;
            prompt = prompt.Replace("{actions}", actions);
            prompt = prompt.Replace("{composites}", composites);
            prompt = prompt.Replace("{modifiers}", modifiers);
            prompt = prompt.Replace("{variables}", variables);
            prompt = prompt.Replace("{description}", description);
            return prompt;
        }

        internal static string GetBlackboardVariables(List<VariableModel> variableModels)
        {
            StringBuilder sb = new();
            foreach (var variable in variableModels)
                sb.AppendLine($"{variable.Name} ({variable.Type.Name})");

            return sb.ToString();
        }

        internal static string GetComposites(List<NodeInfo> nodeInfos)
        {
            StringBuilder sb = new();
            IEnumerable<NodeInfo> compositeNodes =
                nodeInfos.Where(info => typeof(Composite).IsAssignableFrom(info.Type));
            compositeNodes = compositeNodes.OrderBy(n => n.Name.Length);
            foreach (NodeInfo nodeInfo in compositeNodes)
            {
                // Filter out excluded nodes.
                if (s_ExcludedCompositeTypes.Contains(nodeInfo.Type))
                    continue;
                sb.AppendLine($"{nodeInfo.Name}, \"{nodeInfo.Description}\"");
            }

            return sb.ToString();
        }

        internal static string GetActions(List<NodeInfo> nodeInfos)
        {
            // Wrap content with supplementary info from node registry and instructions.
            string ProcessStory(string story)
            {
                string pattern = @"\[(.*?)\]";
                return Regex.Replace(story, pattern, m => $"[{m.Groups[1].Value.ToLower()}]");
            }

            StringBuilder sb = new();
            IEnumerable<NodeInfo> actionNodes = nodeInfos.Where(info => typeof(Action).IsAssignableFrom(info.Type));
            foreach (NodeInfo nodeInfo in actionNodes)
            {
                string variables = nodeInfo.Variables != null
                    ? string.Join(" ",
                        nodeInfo.Variables.Select(n => $"{n.Name.ToLower()} ({GetVariableTypeName(n)})"))
                    : string.Empty;
                string description = typeof(EventAction).IsAssignableFrom(nodeInfo.Type)
                    ? nodeInfo.Description
                    : ProcessStory(nodeInfo.Story);
                sb.AppendLine($"{nodeInfo.Name}, \"{description}\", {variables}");
            }

            return sb.ToString();
        }

        static string GetVariableTypeName(VariableInfo varInfo)
        {
            Type type = varInfo.Type;
            if (type.IsGenericType) // Blackboard variable of type BlackboardVariable<T>
            {
                return GetRealTypeName(type.GetGenericArguments()[0]);
            }
            return type.Name;
        }

        static string GetRealTypeName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
            sb.Append('<');
            bool appendComma = false;
            foreach (Type arg in t.GetGenericArguments())
            {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealTypeName(arg));
                appendComma = true;
            }
            sb.Append('>');
            return sb.ToString();
        }

        internal static string GetModifiers(List<NodeInfo> nodeInfos)
        {
            var sb = new StringBuilder();
            IEnumerable<NodeInfo> modifierNodes =
                nodeInfos.Where(info => typeof(Modifier).IsAssignableFrom(info.Type));

            foreach (NodeInfo nodeInfo in modifierNodes)
            {
                string variables = nodeInfo.Variables != null
                    ? string.Join(" ",
                        nodeInfo.Variables.Select(n => $"{n.Name.ToLower()} ({GetVariableTypeName(n)})"))
                    : string.Empty;
                var description = nodeInfo.Description;
                sb.AppendLine($"{nodeInfo.Name}, \"{description}\", {variables}");
            }

            return sb.ToString();
        }

        private static bool IsValidJson(ref string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
            {
                return false;
            }

            strInput = strInput.Trim();

            try
            {
                JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                Debug.Log("error parsing json: " + jex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.Log("error parsing json: " + ex.Message);
                return false;
            }
        }

        private static string AddSpaceToName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            return Regex.Replace(name, "([A-Z])", " $1").Trim();
        }
    }
}