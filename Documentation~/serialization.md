---
uid: serial
---

# Save and load running graph state

Serialization saves the state of a behavior graph and loads it back from the same state the next time you start the game. This helps to preserve game states, user progress, and configurations across game sessions.

Unity Behavior utilizes the `com.unity.serialization` and `com.unity.properties` packages to create property bags for each node you want to serialize. A property bag is a serializable definition of another class, generated automatically either from the source code or through reflection. These property bags are updated whenever the nodes are modified. For more information, refer to [Introduction to Unity Serialization](https://docs.unity3d.com/Packages/com.unity.serialization@3.1/manual/index.html).
 
## Pre-defined nodes

Serialization is enabled by default in all the [predefined nodes](predefined-node.md) in Unity Behavior. These nodes save and load their state automatically without requiring additional implementation. 

If you're using a version of Unity Behavior older than 1.0.0 that didn't support serialization, update your custom nodes to make them serializable. Follow the steps in the next section to enable serialization.
 
## Custom nodes

To save and restore the states of [custom nodes](create-custom-node.md) in Unity Behavior, you need to implement serialization. Follow these steps:

1. For each custom node, make sure the node classes include the `[Serializable]` and `[Unity.Properties.GeneratePropertyBag]` attributes.
 
    For example, if your behavior graph uses a custom **Wait** node, check that the `WaitAction.cs` file includes the following attribute: 

    ```
    [Serializable, Unity.Properties.GeneratePropertyBag]
    ```
    
    The `[Serializable]` attribute indicates that the class can be serialized, while the `Unity.Properties.GeneratePropertyBag` attribute tells the serialization system to generate property bags for this class.

2. Instruct the serialization system to generate property bags for all types within the assembly by adding the following attribute to the `AssemblyInfo.cs` file. 

    ```
    [assembly:GeneratePropertyBagsForAssembly]
    ```

    This attribute ensures that property bags are created for all types marked with `[GeneratePropertyBag]` within the assembly. Note that this can impact code compilation time. To mitigate this, it's advised to move your custom nodes to their own assembly definitions (`.asmdef`). This way, you only affect the `.asmdef` containing the `[assembly:GeneratePropertyBagsForAssembly]` tag, thereby improving compilation efficiency.

3. For any private non-`BlackboardVariable` members, include the `[CreateProperty]` attribute.

   For example:

   ```
   [CreateProperty] private float m_Progress = 0.0f;
   ```

   > [!NOTE]
   > **Advanced use case**: If you need to serialized a non-`BlackboardVariable` field to the graph asset (like a reference to another node in the graph) and ensure it's serialized at runtime, create a property using `[CreateProperty]` and add `[DontGenerateProperty]` to the field. This method avoids runtime reflection, which can lead to performance issue.
   
    For example:
   
      ```
      [CreateProperty] public Node Child => m_Child;

      [SerializeReference, DontCreateProperty] private Node m_Child;
      ```

4. To successfully save and load behavior graph runtime data, provide a resolver for GameObject references. The resolver restores these references upon loading. To achieve this, implement the `IUnityObjectResolver` interface. For example, `Map` returns a unique identifier for the GameObject and `Resolve` converts that unique identifier back to the GameObject, restoring the original reference.

   You can find an example implementation in the `SceneController.cs` file in the Serialization sample.

5. Any class that is referenced by a custom node and is intended to be serialized needs to have a parameterless constructor available.

## Additional resources

* [Create a behavior graph with Unity Behavior's generative AI](about-genai.md)
* [Get started with Unity Behavior](get-started.md)