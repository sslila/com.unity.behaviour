#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class CreateVariableFromLinkFieldCommandHandler : CommandHandler<CreateVariableFromLinkFieldCommand>
    {
        public override bool Process(CreateVariableFromLinkFieldCommand command)
        {
            // Debug.LogError($"CreateVariableFromLinkFieldCommandHandler: {command.VariableType}");
            VariableModel variable = Activator.CreateInstance(command.VariableType, command.Args) as VariableModel;
            variable.Name = command.Name;
            DispatcherContext.BlackboardAsset.Variables.Add(variable);
            BlackboardView.FocusOnVariableNameField(variable);

            BaseLinkField field = command.Field;
            field.LinkedVariable = variable;
            using (LinkFieldTypeChangeEvent changeEvent = LinkFieldTypeChangeEvent.GetPooled(field, variable.Type))
            {
                field.SendEvent(changeEvent);
            }
            return true;
        }
    }
}
#endif