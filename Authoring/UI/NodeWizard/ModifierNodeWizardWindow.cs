#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ModifierNodeWizardWindow : NodeWizardWindow
    {
        internal static ModifierNodeWizard GetAndShowWindow(VisualElement targetView, Dictionary<string, Type> variableSuggestions = null)
        {
            ModifierNodeWizard wizard = new ModifierNodeWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal("New Modifier", targetView, wizard);
   
            wizard.SetVariableSuggestions(variableSuggestions);
            return wizard;
        }
        
        internal static ModifierNodeWizard GetAndShowEditWindow(VisualElement targetView, NodeInfo info, Dictionary<string, Type> variableSuggestions = null)
        {
            ModifierNodeWizard wizard = new ModifierNodeWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal(info.Name, targetView, wizard);
            wizard.SetVariableSuggestions(variableSuggestions);
            wizard.SetupEditWizard(info);
            return wizard;
        }
    }
}
#endif