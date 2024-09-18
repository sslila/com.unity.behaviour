#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ConditionWizardWindow : NodeWizardWindow
    {
        internal static ConditionWizard GetAndShowWindow(VisualElement targetView,
            Dictionary<string, Type> variableSuggestions = null, Action<string> onComplete = null)
        {
            ConditionWizard wizard = new ConditionWizard(onComplete);
            Modal modal = WizardUtils.CreateAndShowStepperModal("New Custom Condition", targetView, wizard);

            wizard.SetVariableSuggestions(variableSuggestions);
            return wizard;
        }
    }
}
#endif