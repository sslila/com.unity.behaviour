#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Behavior
{
    internal class WizardStepper : VisualElement
    {
        public override VisualElement contentContainer => Content;
        internal AppBar WizardAppBar => this.Q<AppBar>("WizardAppBar");

        internal ActionButton BackButton => this.Q<ActionButton>("appui-appbar__back-button");

        internal readonly ActionButton CloseButton;
        
        internal readonly Button ConfirmButton;
        internal Button NextButton => this.Q<Button>("NextButton");
        internal VisualElement StepperContainer => this.Q<VisualElement>("WizardStepperContainer");
        private VisualElement Content => this.Q<VisualElement>("WizardContentContainer");
        internal readonly List<VisualElement> WizardSteps = new List<VisualElement>();
        
        internal readonly VisualElement PreviewElement;

        private List<System.Action> m_OnShowActions = new List<System.Action>();
        private List<System.Action> m_OnHideActions = new List<System.Action>();

        private int m_CurrentStep;

        internal int CurrentStep
        {
            get => m_CurrentStep;
            set
            {
                if (m_OnHideActions.Count > m_CurrentStep)
                {
                    m_OnHideActions[m_CurrentStep]?.Invoke();
                }
                m_CurrentStep = value;
                OnCurrentStepChanged();
            }
        }

        internal void AddStep(VisualElement visualElement, System.Action onShow = null, System.Action onHide = null)
        {
            WizardSteps.Add(visualElement);
            m_OnShowActions.Add(onShow);
            m_OnHideActions.Add(onHide);
        }

        internal void RemoveStep(VisualElement visualElement)
        {
            int index = WizardSteps.IndexOf(visualElement);
            if (index == -1)
            {
                return;
            }
            WizardSteps.RemoveAt(index);
            m_OnShowActions.RemoveAt(index);
            m_OnHideActions.RemoveAt(index);
        }

        internal bool ContainsStep(VisualElement visualElement)
        {
            return WizardSteps.Contains(visualElement);
        }

        private void OnCurrentStepChanged()
        {
            WizardAppBar.showBackButton = m_CurrentStep > 0;
            if (m_OnShowActions.Count > m_CurrentStep)
            {
                m_OnShowActions[m_CurrentStep]?.Invoke();
            }
        }

        protected internal WizardStepper()
        {
            AddToClassList("WizardStepper");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/WizardStepperStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/WizardStepperLayout.uxml").CloneTree(this);
            
            // Node preview styles.
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/GraphNodeStylesheet.uss"));
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/BehaviorNodeStylesheet.uss"));
            
            AddToClassList("BehaviorModal");

            PreviewElement = this.Q<VisualElement>("PreviewElement");

            CloseButton = new ActionButton
            {
                quiet = true,
                icon = "x"
            };
            WizardAppBar.Add(CloseButton);

            ConfirmButton = this.Q<Button>("ConfirmButton");
            ConfirmButton.variant = ButtonVariant.Accent;
            ConfirmButton.title = "Create";
            ConfirmButton.Hide();

            NextButton.clicked += OnNextButtonClicked;
            BackButton.clicked += OnBackButtonClicked;
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }
        
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (WizardSteps.Count != 0)
            {
                // Make sure that only the first step VisualElement is visible.
                foreach (VisualElement stepView in WizardSteps.Where(stepView => stepView != WizardSteps.First()))
                {
                    stepView.Hide();
                }

                return;
            }

            NextButton.Hide();
            ConfirmButton.Show();
        }

        internal void OnNextButtonClicked()
        {
            if (WizardSteps.Count == 0)
            {
                return;
            }

            if (CurrentStep >= WizardSteps.Count - 1)
            {
                return;
            }

            WizardSteps[CurrentStep].Hide();
            CurrentStep++;
            WizardSteps[CurrentStep].Show();
            if (CurrentStep == WizardSteps.Count - 1)
            {
                NextButton.style.display = DisplayStyle.None;
                ConfirmButton.Show();
            }
        }
        
        private void OnBackButtonClicked()
        {
            if (WizardSteps.Count == 0)
            {
                return;
            }
            NextButton.style.display = DisplayStyle.Flex;
            if (CurrentStep > 0 && CurrentStep < WizardSteps.Count)
            {
                WizardSteps[CurrentStep].Hide();
                CurrentStep--;
                WizardSteps[CurrentStep].Show();
            }
            else
            {
                CurrentStep = 0;
            }

            ConfirmButton.Hide();
        }
    }
}
#endif