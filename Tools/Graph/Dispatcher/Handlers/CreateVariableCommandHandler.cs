#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using UnityEngine;

internal class CreateVariableCommandHandler : CommandHandler<CreateVariableCommand>
{
    public override bool Process(CreateVariableCommand command)
    {
        CreateBlackboardVariable(command.VariableType, command.Name, command.Args);
        return true;
    }
    
    private void CreateBlackboardVariable(Type type, string name, params object[] args)
    {
        // Debug.LogError($"CreateBlackboardVariable: {type}");
        VariableModel variable = Activator.CreateInstance(type, args) as VariableModel;
        variable.Name = name;
        DispatcherContext.BlackboardAsset.Variables.Add(variable);
        BlackboardAsset.InvokeBlackboardChanged();
        BlackboardView.FocusOnVariableNameField(variable);
    }
}
#endif