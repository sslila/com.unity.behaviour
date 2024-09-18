---
uid: blackboard-var
---

# Create and manage variables and Blackboards

Each behavior graph has its own **Blackboard** that you can use to create and manage variables specific to that graph. Additionally, you can create and use shared **Blackboard** assets, which can be referenced by multiple behavior graphs.

The following table compares the Unity Behavior graph's **Blackboard** and the **Blackboard** asset.

| Graph-specific Blackboard | Shared Blackboard assets |
| ---------- | ------------ |
| Each behavior graph has its own unique **Blackboard**. | If multiple behavior graphs need to access a set of variables, you can create standalone **Blackboard** assets. |
| You can create variables directly within this **Blackboard** to be used exclusively by that behavior graph. | These shared **Blackboard** assets act as a common repository of variables, enabling different graphs to read from and write to the same set of variables. |

Use this step-by-step guide to use a **Blackboard** asset in a behavior graph.

## Step 1: Create a Blackboard asset

1. Right-click an empty area of the **Assets** window.
2. Select **Create** > **Behavior** > **Blackboard**.
 
   The **Blackboard** asset displays in the **Assets** window.
3. Select the **Backboard** asset and specify a name that reflects its purpose.
4. Double-click the **Blackboard** asset to open the **Blackboard** editor.

## Step 2: Add variables to the Blackboard asset

Use the **Blackboard** asset to create different variables on the **Blackboard**. You can then add and use the new **Blackboard** across multiple behavior graphs.

1. Click the **+** icon on the **Blackboard** editor. 

   The **Add Variable** window displays. It lists the following variables:

   * **GameObject**
   * **Basic Types**
   * **Behavior**
   * **Enumeration**
   * **Events**
   * **List**
   * **Resources**
   * **Vector Types**
   * **Other**

2. Select a variable to add it to the **Blackboard**.

3. Rename the variable and assign a value to it.

## Step 3: Use the Blackboard asset in a behavior graph

After you have created a **Blackboard** asset and have added variables to it, you can use the **Blackboard** asset in a different behavior graph. 

Perform the following steps:

1. Open the behavior graph where you want to use the **Blackboard** asset.

2. In the behavior graph **Blackboard**, select the **+** icon to open the **Add Variable** window.

   The behavior graph **Blackboard** has the same variables as the **Blackboard** editor.

3. Select **Blackboards**.

4. Select the **Blackboard** asset you created in [Step 1: Create a Blackboard asset](#step-1-create-a-blackboard-asset).

   The **Blackboard** asset displays on the Unity Behavior graph's **Blackboard**. 

   > [!NOTE]
   > If you don't create **Blackboard** assets, the option to select a **Blackboard** variable does not display in the behavior graph's **Blackboard** list.
   
   To add new variables on the behavior graph **Blackboard**, select the **+** icon. To add new variables or edit any variable added on the behavior graph's **Blackboard** from the **Blackboard** asset, use the **Blackboard** editor.

### Expose and Shared variables

In Unity's behavior graph, the **Expose** and **Shared** options manage how variables are accessed and shared within and across different behavior graphs.

The **Expose** option makes a **Blackboard** variable available to external systems such as the Unity **Inspector**. The variable displays in the **Inspector** window under **Behavior Agent**. This lets you modify the variable directly through the Unity **Inspector**.

The **Shared** option marks a variable in the **Blackboard** as globally shared across different instances of the behavior graph. When multiple behavior graphs need to use the same data, marking the variable as **Shared** creates a centralized state that all graphs can reference. For example, mark a variable as **Shared** if several Agents need to react to the same environment state.

> [!NOTE]
> You can't pass **Shared** variables directly to subgraphs. This means variables set as **Shared** in one graph aren’t automatically accessible in its subgraphs. To use a **Shared** variable between subgraphs, add a **Blackboard** asset on the subgraph's **Blackboard** and create a **Shared** variable there. If you create a **Shared** variable in the graph's own **Blackboard**, it's only shared within that graph and its own instances, not any subsequent subgraphs.

To use these options, do the following:

1. Right-click on a variable in the **Blackboard**.
2. Select **Expose** to expose and access a variable externally.
3. Select **Shared** to share the variable with other behavior graphs that reference the same **Blackboard**.