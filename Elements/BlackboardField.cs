using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class BlackboardField<TValueType> : BaseField<TValueType>
    {

        public BlackboardField(string label)
            : base(label, null)
        {
            //visualInput.focusable = false;
            labelElement.focusable = false;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        }
    }
}