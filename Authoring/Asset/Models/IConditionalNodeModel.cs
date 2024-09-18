using System.Collections.Generic;

namespace Unity.Behavior
{
    internal interface IConditionalNodeModel
    {
        List<ConditionModel> ConditionModels { get; set; }

        bool RequiresAllConditionsTrue { get; set; }

        bool ShouldTruncateNodeUI { get; set; }

        void RemoveCondition(ConditionModel conditionModel)
        {
            if (ConditionModels.Contains(conditionModel))
            {
                ConditionModels.Remove(conditionModel);
            }
        }
        
        public static List<ConditionModel> GetConditionModelCopies(IConditionalNodeModel originalModel, BehaviorGraphNodeModel newModel)
        {
            List<ConditionModel> copyModels = new List<ConditionModel>();
            foreach (ConditionModel model in originalModel.ConditionModels)
            {
                copyModels.Add(model.Copy(model, newModel));
            }

            return copyModels;
        }

        public static void UpdateConditionModels(IConditionalNodeModel node)
        {
            // Remove any leftover condition models that might have been deleted.
            for (int index = node.ConditionModels.Count - 1; index >= 0; index--)
            {
                ConditionModel conditionModel = node.ConditionModels[index];

                ConditionInfo info = NodeRegistry.GetConditionInfoFromTypeID(conditionModel.ConditionTypeID);
                if (info == null)
                {
                    node.ConditionModels.Remove(conditionModel);
                }
                else if (conditionModel.ConditionType.text != info.Type.AssemblyQualifiedName)
                {
                    conditionModel.ConditionType = info.Type;
                }
            }
            
            // Validate and update existing condition models. 
            foreach (ConditionModel conditionModel in node.ConditionModels)
            {
                conditionModel.Validate();
            }
        }
    }
}