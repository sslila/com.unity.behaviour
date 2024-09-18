#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class EventChannelWizardWindow : NodeWizardWindow
    {
        internal static EventChannelWizard GetAndShowWindow(VisualElement targetView, Dictionary<string, Type> variableSuggestions = null)
        {
            EventChannelWizard wizard = new EventChannelWizard();
            Modal modal = WizardUtils.CreateAndShowStepperModal("New Event Channel", targetView, wizard);
   
            wizard.SetVariableSuggestions(variableSuggestions);
            return wizard;
        }
    }
}
#endif
