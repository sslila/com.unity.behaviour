---
uid: node-types
---

# Behavior graph node types

The logic flow of the behavior graph begins at the start node at the top. You can then add further nodes to develop and expand your behavior graph.

The tables in this section provide information on the different types of nodes available in Unity Behavior along with their description.

## Action node types

To use the **Action** nodes, select **Add** > **Action**.

| Node | Description |
| ---- | ----------- |
| Create new Action | Creates a new action node. To use this node, select **Add** > **Action** > **Create new Action**. <br>For more information, refer to [Create a custom node](create-custom-node.md).|
| **Animation** | To use the **Animation** options, select **Add** > **Action** > **Animation**. |
| Set Animator Boolean | Sets a Boolean parameter on an animator to a specific value. |
| Set Animator Float | Sets a float parameter on an animator to a specific value. |
| Set Animator Integer | Sets an integer parameter on an animator to a specific value. |
| Set Animator Trigger | Sets a trigger on an animator. |
| **Blackboard** | To use the **Blackboard** options, select **Add** > **Action** > **Blackboard**. |
| Set Variable Value | Sets the value of a given variable. |
| Conditional Guard | Returns `success` if the condition evaluates to true and `failure` if the condition evaluates to false. |
| **Debug** | To use the **Debug** options, select **Add** > **Action** > **Debug**. |
| Log Message | Logs a message to the console. |
| Log Variable | Logs the value of a variable to the console. |
| Log Variable Change | Logs the value of a variable to the console when it changes. |
| **Delay** | To use the **Delay** options, select **Add** > **Action** > **Delay**. |
| Wait | Waits for a specified number of seconds. |
| Wait (Range) | Waits for the duration specified between the **Min** and **Max** values. |
| **Find** | To use the **Find** options, select **Add** > **Action** > **Find**. |
| Find Closest With Tag | Finds the closest GameObject with the given tag. |
| Find With Tag | Finds a GameObject with the given tag. |
| **GameObject** | To use the **GameObject** options, select **Add** > **Action** > **GameObject**. |
| Attach Object | Sets the transform parent of a GameObject to another GameObject with an offset. |
| Destroy Object | Destroys a GameObject. |
| Don't Destroy On Load | Prevents a GameObject from being destroyed on load. |
| Instantiate Object | Instantiates a GameObject to the graph owner scene. |
| Set Object Active State | Sets the active state of a GameObject. |
| Set Object List Active State | Sets the active state of all the GameObjects on the list. |
| **Navigation** | To use the **Navigation** options, select **Add** > **Action** > **Navigation**. |
| Navigate To Location | Navigates a GameObject to a specified position using `NavMeshAgent`. Note that if `NavMeshAgent` isn't available, the node uses `Transform`.|
| Navigate To Target | Navigates a GameObject towards another GameObject using `NavMeshAgent`. Note that if `NavMeshAgent` isn't available, the node uses `Transform`. |
| Patrol | Moves a GameObject along way points using `NavMeshAgent`. Note that if `NavMeshAgent` isn't available, the node uses `Transform`. |
| **Physics** | To use the **Physics** options, select **Add** > **Action** > **Physics**. |
| Add Force | Applies physics force to the target Rigidbody. |
| Add Torque | Applies torque to the target Rigidbody. |
| Check Collisions In Radius | Checks for collisions in a specified radius around the agent. If a collision is found, the collided object is stored in `[CollidedObject]`. |
| Set Velocity | Sets the linear velocity of the target Rigidbody to a specific value. |
| Wait For Collision | Waits for an `OnCollision` event on the specified agent. |
| Wait For Collision 2D | Waits for a 2D `OnCollision` event on the specified agent. |
| Wait For Trigger | Waits for an `OnTrigger` event on the specified agent. |
| Wait for Trigger 2D | Waits for a 2D `OnTrigger` event on the specified agent. |
| **Resource** | To use the **Resource** options, select **Add** > **Action** > **Resource**. |
| Play Audio | Plays an AudioResource at the target location. |
| Play Particle System | Plays a ParticleSystem at the target location. |
| **Scene** | To use the **Scene** options, select **Add** > **Action** > **Scene**. |
| Load Scene | Loads a Unity scene. |
| Unload Scene | Unloads a Unity scene. |
| **Transform** | To use the **Transform** options, select **Add** > **Action** > **Transform**. |
| Look At | Rotates the transform to look at the target. |
| Rotate | Rotates the transform by an Euler rotation.  |
| Scale | Scales the transform by a value. |
| Set Position | Sets the target's position to a specific location. |
| Set Position To Target | Sets the transform's position to a specific target position. |
| Set Rotation | Sets the transform's rotation to a specific Euler rotation. |
| Set Scale | Sets the transform's scale to a specific value. |
| Translate | Translates the target's position by a specific amount. |

## Events node types

| Node | Description |
| ---- | ----------- |
| Start On Event Message | Starts the subgraph upon receiving an event message. |
| On Start | This is the root of the behavior graph. You can use multiple **On Start** nodes in your graph.|
| Send Event Message | Sends an event message on the assigned channel. |
| Start On Event Message | Starts the subgraph upon receiving an event message. |
| Wait for Event Message | Use this node when you want the game character to wait to receive an event message on the assigned channel. |

## Flow node types

| Node | Description |
| ---- | ----------- |
| Create new Modifier | Creates a new modifier.<br> To use this node, select **Add** > **Flow** > **Create new Modifier**.<br>Modifiers affect how their connected subgraphs are run. <br>For example, a repeat modifier causes its branch to perform its operation more than once. For more information, refer to [Create a custom node](create-custom-node.md).|
| Create new Sequencing | Use this node to create a new sequencing node. To use this node, select **Add** > **Flow** > **Create New Sequencing**.<br>Sequencing nodes control how their connected branches flow. They control how sequences run and define the conditions for a sequence's completion. For more information, refer to [Create a custom node](create-custom-node.md). |
| **Abort** | To use the **Conditional** options, select **Add** > **Flow** > **Abort**. |
| Abort | Aborts the branch when the assigned conditions are true. |
| Restart | Restarts the branch when the assigned conditions are true. |
| **Conditional** | To use the **Conditional** options, select **Add** > **Flow** > **Conditional**. |
| Conditional Branch | Selects a branch based on whether the condition evaluates to true or false. |
| Switch | Branches off based on the `Enumeration` value. |
| **Parallel Execution** | To use the **Parallel Execution** options, select **Add** > **Flow** > **Parallel Execution**. |
| Run In Parallel | Runs all the branches simultaneously. You can set different execution modes in this node to handle parallel branches. |
| Wait For All | Activates a child when all parents have started this node. It can't restart until the child's subgraph has ended. |
| Wait For Any | Activates a child when any parent starts this node. It can't restart until the child's subgraph has ended. |
| **Cooldown** | Imposes a mandatory wait time between iterations to regulate action frequency. |
| **Inverter** | Inverts the result of the child action. A value of success becomes failure and a value of failure becomes success. |
| **Random** | Runs a random branch. |
| **Repeat** | Repeats the operation of a node. You can set different execution modes that define how and when the repeated task executes.|
| **Sequence** | Runs branches in order until one fails or all succeed. |
| **Succeeder** | Forces success for the child node. These nodes are useful in cases where you want to process a branch of a tree where a failure is expected or anticipated, without interrupting the processing of the sequence to which that branch belongs. |
| **Time Out** | Terminates the execution of its branch after a specified number of seconds. |
| **Try In Order** | Runs branches in order until one succeeds. |

## Subgraphs node types

| Node | Description |
| ---- | ----------- |
| Run Subgraph | Runs the assigned subgraph and return the graph's final status. |

## Sticky note

Use the **Sticky Note** node to add a sticky note to the behavior graph. To add a **Sticky Note** to your behavior graph, follow these steps:

1. Right-click an empty area of the Unity Behavior graph editor.
2. Click **Add** from the context menu.
3. Select **Sticky Note**.

This adds a **Sticky Note** to your behavior graph, allowing you to annotate it as needed.

## Additional resources

* [Unity Behavior editor user interface](user-interface.md)
* [Create a behavior graph](create-behavior-graph.md)