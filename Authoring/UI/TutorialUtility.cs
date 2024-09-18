using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;
using Object = UnityEngine.Object;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal static class TutorialUtility
    {
        internal static void CreateAndShowEdgeConnectTutorial(VisualElement view)
        {
            List<Texture2D> frames = ResourceLoadAPI.LoadAll<Texture2D>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/Images/EdgeConnectTutorial/");
            
            Dialog dialog = new Dialog();
            dialog.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/TutorialStylesheet.uss"));
            dialog.dismissable = true;
            dialog.description = "Congratulations, you've added your first node! To connect nodes, drag a line from the Edge of one node into another.";
            
            TutorialAnimation tutorialAnimation = new TutorialAnimation(frames);
            dialog.hierarchy.Insert(1, tutorialAnimation);
            dialog.Q<Divider>().style.display = DisplayStyle.None;

            Button continueButton = CreateContinueButton(dialog, "Continue");
            
            Modal modal = Modal.Build(view, dialog);
            continueButton.clicked += modal.Dismiss;
            modal.Show();
        }

        internal static void CreateAndShowSequencingTutorial(VisualElement view)
        {
            List<Texture2D> frames = ResourceLoadAPI.LoadAll<Texture2D>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/Images/SequenceTutorial/");
         
            Dialog dialog = new Dialog();
            dialog.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/TutorialStylesheet.uss"));
            dialog.dismissable = true;
            dialog.description = "Great, you've added your first two Action nodes! To sequence Actions, drag and drop the Action nodes together into a group.";
            
            TutorialAnimation tutorialAnimation = new TutorialAnimation(frames);
            dialog.hierarchy.Insert(1, tutorialAnimation);
            dialog.Q<Divider>().style.display = DisplayStyle.None;
            
            Button continueButton = CreateContinueButton(dialog, "Continue");
            
            Modal modal = Modal.Build(view, dialog);
            continueButton.clicked += modal.Dismiss;
            modal.Show();
        }

        private static Button CreateContinueButton(Dialog dialog, string buttonText)
        {
            Button button = new Button();
            button.title = buttonText;
            button.variant = ButtonVariant.Accent;
            
            VisualElement buttonGroup = dialog.Q<VisualElement>("appui-dialog__buttongroup");
            buttonGroup.Add(button);

            return button;
        }
    }

    internal class TutorialAnimation : VisualElement
    {
        private int m_CurrentFrameIndex;
        private List<Texture2D> m_Frames;

        internal TutorialAnimation(List<Texture2D> frames)
        {
            m_Frames = frames.OrderBy(texture => texture.name).ToList();
            if (m_Frames.Count > 1)
            {
                schedule.Execute(LoopTutorialFrames).Every(50);
            }
            else if (m_Frames.Count == 1)
            {
                Texture2D frame = (Texture2D)m_Frames.First();
                style.backgroundImage = frame;
            }
        }

        private void LoopTutorialFrames()
        {
            if (m_CurrentFrameIndex < m_Frames.Count)
            {
                Texture2D frame = (Texture2D)m_Frames[m_CurrentFrameIndex];
                style.backgroundImage = frame;
                m_CurrentFrameIndex++;
            }
            else m_CurrentFrameIndex = 0;
        }
    }
}