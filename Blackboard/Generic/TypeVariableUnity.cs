using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior.GraphFramework.Generic
{
    public class TypedVariableModelColor : TypedVariableModel<Color>
    {
    }

    public class TypedVariableModelColorList : TypedVariableModel<List<Color>>
    {
    }
    
    public class TypedVariableModelScriptableObject : TypedVariableModel<ScriptableObject>{}
    public class TypedVariableModelScriptableObjectList : TypedVariableModel<List<ScriptableObject>>{}
    public class TypedVariableModelGameObject : TypedVariableModel<GameObject>{}
    public class TypedVariableModelGameObjectList : TypedVariableModel<List<GameObject>>{}
    public class TypedVariableModelTransform : TypedVariableModel<Transform>{}
    public class TypedVariableModelTransformList : TypedVariableModel<List<Transform>>{}
    public class TypedVariableModelTexture2D : TypedVariableModel<Texture2D>{}
    public class TypedVariableModelTexture2DList : TypedVariableModel<List<Texture2D>>{}
}