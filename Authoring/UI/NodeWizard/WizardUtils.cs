#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.Behavior
{
    internal static class WizardUtils
    {
        internal static Modal CreateAndShowStepperModal(string modalTitle, VisualElement targetView, BaseNodeWizard wizard)
        {
            WizardStepper stepper = new WizardStepper();
            Modal modal = Modal.Build(targetView, stepper);
            stepper.WizardAppBar.title = modalTitle;
            stepper.CloseButton.clicked += modal.Dismiss;
            stepper.Add(wizard);
            wizard.SetupWizardStepperModal(stepper, modal);
            modal.Show();
            modal.shown += (modal) =>
            {
                wizard.OnShow();
            };

            return modal;
        }
        
        internal static string GetCategoryFieldValue(TextField categoryField, Dropdown categoryDropdown)
        {
            if (categoryField.style.display == DisplayStyle.None && !categoryDropdown.value.Any())
            {
                return string.Empty;
            }
            return categoryField.style.display == DisplayStyle.Flex ? categoryField.value : categoryDropdown.sourceItems[categoryDropdown.value.First()].ToString();
        }

    }
}
#endif