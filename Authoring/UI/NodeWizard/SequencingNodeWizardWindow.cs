#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class SequencingNodeWizardWindow : NodeWizardWindow
    {
        internal static SequencingNodeWizard GetAndShowWindow(VisualElement targetView, Dictionary<string, Type> variableSuggestions = null)
        {
            SequencingNodeWizard wizard = new SequencingNodeWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal("New Sequencing", targetView, wizard);
   
            wizard.SetVariableSuggestions(variableSuggestions);
            return wizard;
        }

        internal static SequencingNodeWizard GetAndShowEditWindow(VisualElement targetView, NodeInfo info, NodeModel model, Dictionary<string, Type> variableSuggestions = null)
        {
            SequencingNodeWizard wizard = new SequencingNodeWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal(info.Name, targetView, wizard);
            wizard.SetVariableSuggestions(variableSuggestions);
            wizard.SetupEditWizard(info, model);
         
            return wizard;
        }
    }
}
#endif