#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ActionNodeWizardWindow : NodeWizardWindow
    {
        internal static ActionNodeWizard GetAndShowWindow(VisualElement targetView,
            Dictionary<string, Type> variableSuggestions = null)
        {
            ActionNodeWizard wizard = new ActionNodeWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal("New Action", targetView, wizard);

            wizard.SetVariableSuggestions(variableSuggestions);
#if UNITY_EDITOR
            wizard.SetupGenAiButton();
            wizard.SetupGenAiLayout();
#endif
            return wizard;
        }

        internal static ActionNodeWizard GetAndShowEditWindow(VisualElement targetView, NodeInfo info,
            Dictionary<string, Type> variableSuggestions = null)
        {
            ActionNodeWizard wizard = new ActionNodeWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal(info.Name, targetView, wizard);
            wizard.SetVariableSuggestions(variableSuggestions);
            wizard.SetupEditWizard(info);
            return wizard;
        }
    }
}
#endif