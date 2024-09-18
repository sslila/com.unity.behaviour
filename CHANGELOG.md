# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.1] - 2024-09-11

### Added
- BlackboardVariable support for various Resource types.
- BlackboardVariables of type UnityEngine.Object can now have an embedded value in their field (previously needed to set it in the GameObject's inspector).
- Added new behaviors to StartOnEvent node (Default, Restart, Once).  
- Blackboard assets can now be created through the Project view Create menu.
- Added serialization/deserializatioon support for graph, blackboard and node values to JSON. Added demonstation sample.
- Allow using `TooltipAttribute` on BlackboardVariables to display a tooltip.
- Can now right click on a Blackboard Variable to copy its GUID.
- Can now right click on a Blackboard Variable to copy its GUID.
- Inspector will now display an inspection of the behavior graph, when nothing else is being selected.
- The Story button on the editor toolbar has been removed, and the graph's subgraph representation can now be edited through the Inspector.
- Transform Blackboard Variable type to the default blackboard options.
- Can add Blackboard references to graphs.
- Shared variables that are shared between instances of the same graph.
- Run Subgraph node combines now two nodes, a static Run Subgraph and the new Run Subgraph Dynamic, which can be used to run graphs dynamically on runtime.
- Subgraph variables can now be added on the Blackboard.
- Adds new built-in nodes
- Transform Blackboard Variable type to the default blackboard options.
- Can add Blackboard references to graphs.
- Shared variables that are shared between instances of the same graph.
- Run Subgraph node combines now two nodes, a static Run Subgraph and the new Run Subgraph Dynamic, which can be used to run graphs dynamically on runtime.
- Subgraph variables can now be added on the Blackboard.

### Changed

- AppUI dependency updated to 2.0.0-pre.6.
- Blackboard, Node Wizard and Condition Wizard now use a search window for variables.
- Removed variable icons.
- Creation wizards fields will be automatically focused when shown.
- Replaced icons for behavior graph agent and the graph asset with new designs.
- Updated node filenames and class names to have appropriate postfixes.
- Replaced all mouse events & usages to pointer events.
- Updated MoveToLocation and MoveToTarget logic for slowing down as target reached to use a distance from target instead of Min(Speed, Distance);
- Updated blackboard variable field assignment to support implicit casting between GameObject and components, components and GameObjects as well as components of different types.
- Moved existing built-in nodes to package namespace
- Updated built-in nodes categories
- Merged Repeat nodes into a single node.
- Merged Run in Parallel nodes into a single node.
- Updated all tutorial images to be JPEG intead of PNG for a significant memory saving.
- Start node repeat field is no longer a link field.

### Fixed

- Disabled inspector buttons for editing the definitions of built-in nodes.
- Inspector showing buttons for editing the definitions of built-in nodes.
- Fixed custom node definition editing not cleaning up existing properties.
- Crashes on Domain reload caused by OnAssetsSave().
- Performance when dragging nodes around a graph.
- Fixed newly created Conditions not being automatically added to the selected Conditional node.
- Fixed RepeatWhile node behaving like a do while.
- Conditions not getting copied properly when a conditional node was duplicated.
- Fixed navigation nodes sometimes stalling after being called successively.
- Fixed click events going through to elements underneath search menu items.
- Sequence insertion indicator didn't render at the correct width when dragging a node on top of a node then moving the mouse ontop of another node.
- Multiple instances of graphs being kept in memory when processed on save.
- Fixed pressing delete on an empty graph adding unnecessary command.
- Inheriting from another Node will correctly display the inherited Blackboard Variables.
- Fixed Graph Editor title blocking interaction with Open and Debug buttons.
- Crashes on Domain reload caused by OnAssetsSave().
- Fixed asset name in graph editor not updating when asset name changed on disc.
- Fixed newly created Conditions not being automatically added to the selected Conditional node.
- Fixed RepeatWhile node behaving like a do while.
- Conditions not getting copied properly when a conditional node was duplicated.
- Fixed navigation nodes sometimes stalling after being called successively.
- Fixed click events going through to elements underneath search menu items.

### Known Issues
- Clearing a node field, saving and reverting the asset in source control does not restore the field value.
- PlayParticleSystem internal pooling does not support runtime deserialization.

## [0.10.1] - 2024-06-14

### Fixed

- Enum fields in a condition don't save user's choice of enum value.

### Known Issues

- Graph Editor performance can be slow at the start of an action when there are a lot of nodes.
- Sometimes after dragging a node in and out of a sequence it won't be sequenceable and edges won't connect to it. Re-opening the asset seems to fix it.
- Undo/Redo often causes exceptions.
- Edge rendering has a bit of a snap when moving the lower node above the top node. Needs to be smoothed out.
- Dragging multiple nodes out of a sequence sometimes causes them to be on top of each other.
- Editor window does not react to asset name changes until the window is reopened.
- When creating a new node (i.e. Action, Modifier, Sequencing/Flow), the dialog does not immediately focus the “Name” input field, nor does it automatically focus the “Story” input field.
- Graph Asset gets serialized with minor changes even when no changes were made.

## [0.10.0] - 2024-06-13

### Added

- Documentation. Documentation!!! Rejoice!

### Fixed

- BlackboardVariable will only invoke its OnValueChanged callback if the value has changed. This also fixes situatiosn where the Variable Value Changed Condition was called when the same value was set.
- BehaviorGraphAgent is now public instead of internal when Netcode For GameObjects is included.
- Abort/Restart nodes inspector label is fixed to correctly say "Restarts if"/"Aborts if" depending on the node type.
- Dragging a node within a sequence won't move it if the insertion is where the node already is.
- Improve edge rendering when the end point is above the start point.
- Warnings with regard to UI Toolkit attributes in Unity 6.

### Changed

- Conditional nodes (Conditional Guard, Conditional Branch & Repeat While Condition) will now accept custom conditions, and multiple conditions can be added on the nodes.
- Reduced impact on build size by over ~100 MB by removing assets from Resource folders.
- Add Node window title will be "Add Node" instead of Root when at the root option.
- Dragging items into a sequence will sort them by order on the graph, not selection order.
- Rename the Node Inspector title from "Inspector" to "Node Inspector".
- Story text fields on Modifier and Sequencing node wizards are now optional.
- Added more documentation.

### Known Issues

- Sometimes after dragging a node in and out of a sequence it won't be sequenceable and edges won't connect to it. Re-opening the asset seems to fix it.

## [0.9.1] - 2024-05-24

### Fixed

- Move an Editor only method call that leaked into the runtime in #if UNITY_EDITOR.

## [0.9.0] - 2024-05-23

### Added

- Added new Abort and Restart nodes for interrupting branch execution.
- Condition wizard that enables custom condition creation for Abort and Restart nodes, which can be found through the 'Create new Condition' option when assigning a condition through the inspector.

### Fixed

- Fixed null reference errors when ending nodes with no child assigned.
- Improved the visual appearance of nodes by removing extra margins on LinkFields.
- Changes to subgraph assets will now trigger referencing graph assets to be rebuilt.
- Domain reloads will no longer create duplicate runtime assets.
- Deleting custom node scripts will no longer cause the graph to be corrupt.
- Fixed an issue where dialog windows would collapse in size when the graph editor window was resized.
- Fixed an issue where setting the Start node Repeat value from the inspector did not update on the graph node.

### Changed

- Nodes in a graph which had their scripts deleted will be replaced with placeholder nodes. These nodes are skipped at runtime.
- Moved the BehaviorGraphAgent component to AI/Muse Behavior Agent.
- Added an icon to the BehaviorGraphAgent component.
- Added info icon to Placeholder Toast message.
- Moved Placeholder Toast to the top.
- Moved the close button to be always on the right side of dialog elements.
- Link button now tints on mouse hover, showing you can click it.

## [0.8.0] - 2024-05-02

### Fixed

- Fixed WebGL build failures due to compilation errors stemming from reference to the Unity.Muse.Chat namespace.
- Link fields for enum types are now preserved on IL2CPP platforms.
- Enums should no longer be populated with the wrong members. Old enum variables with the error should be deleted and re-added.

### Changed

- Nodes no longer need to end their child nodes in OnEnd(). The OnEnd method will serve purely for managing the wrap-up of the node's execution.

## [0.7.1] - 2024-04-05

### Added

- Debug nodes for logging variable values to the console.
- Added Muse dropdown to the graph editor toolbar.
- Using Generative AI features now consumes Muse points.
- Added 'R' hotkey to frame to the root node.

### Fixed

- Applied correct minimum sizes for the Blackboard and Inspector floating panel content.
- Fixed flex style values on branch correction widget panel elements.
- Fixed cast exceptions upon aligning nodes while edges are selected.
- Ensured that list variable item input fields are full width on Blackboard.
- Fixed mismatched field type warnings on conditional nodes and set variable value nodes.
- Event nodes will now correctly update which channel they are listening on when the channel variable is reassigned.

### Changed

- Moved OnValueChanged callback to base BlackboardVariable class.
- Generative AI features now use the Muse Chat backend.

### Changed

- Disabled behavior graph agents are no longer selectable in the debug menu.

## [0.6.4] - 2024-03-22

### Fixed

- An issue preventing certain nodes from being added to a sequence.
- Enum creation wizard wouldn't create the fields correctly.
- Node creation wizard stopped updating the preview node.
- Debug warning when assigning a new type of enum to a switch node.

### Known Issues

- Enums which share a name with a class from another assembly don't always show correctly in the Blackboard.
- An exception relating to SetName after domain reload.

## [0.6.3] - 2024-03-20

### Fixed

- Correctly modify event node fields after drag and drop an event channel blackboard variable.

## [0.6.2] - 2024-03-20

### Added

- Allow drag and drop of variables from the Blackboard onto compatible Node fields (Unity versions 2023.2+ only).

### Fixed

- MoveTo actions: Improved stopping distance to take colliders into account and avoid situations navigation can't end.

### Changed

- Selecting a new node to be added after pressing the space key will add the node to the end of a sequence if one is selected.

## [0.6.1] - 2024-03-18

### Fixed

- SearchView: Returning in navigation will correctly select the previous node you entered.
- Move To Target node should correctly update the Nav Mesh's destination if the target moved.
- Fixed an exception related to event nodes.

## [0.6.0] - 2024-03-15

### Added

- Added PlaceholderActionNodes
- Added creating Placeholder nodes for missing actions when generating a branch with AI
- Added PlaceholderActionNodes widget and navigation
- List variable support.
- Added a OnValueChanged callback for Blackboard variable value changes.
- When connecting an edge, escape key will cancel the edge drag.
- Node creation wizards will now show Vector4 in the dropdowns options.
- Behavior Agent Inspector: Right click context menu to reset variables with an override.
- Allow drag and drop asset to the Muse Behavior Graph field in the inspector.
- Allow drag and drop asset on a GameObject in the hierarchy, adding the BehaviorAgent if needed.
- Allow drag and drop asset on a GameObject inspector space, adding the BehaviorAgent if needed.

### Fixed

- Sticky node changes should register for serialisation correctly.
- Dragging a node onto another to create a sequence should correctly maintain output connections.
- Dragging a node or sequence onto an existing sequence, connections should be maintained where possible.
- Search window keyboard controls and arrow navigation should work as expected.
- Blackboard variables on agent that haven't been overwritten should update to new value.
- After deleting an edge, undo correctly restores the link between the UI and asset data, preventing phantom edges.
- Fixed LinkFields not updating their fields values reliably on undo-redo.

### Changed

- Area selection will now select edges.
- Duplicating and pasting nodes will select the duplicated nodes.
- Overwritten variables on the agent will show an '(Override)' label.

## [0.5.11] - 2024-03-06

### Fixed

- Support with latest AppUI package (1.0.2).

## [0.5.10] - 2024-03-05

### Added

- Added a tooltip for node input fields which displays the field variable type.
- Added a cooldown node.

### Fixed

- Fixed the placement of duplicated nodes. When using the duplicate hotkey (CTRL/CMD + D), nodes are now correctly placed at mouse position.
- Fixed assignment of the graph owner object when switching graphs at runtime.
- Fixed stripping of GUID properties in WebGL builds.
- Fixed stripping node types in IL2CPP builds.
- Fixed access to generative features for Muse subscribers.
- Modifying fields in the blackboard now triggers the asset to save.
- Fixed succeeder nodes indefinitely waiting after a child node has completed.
- Fixed modifier nodes not resetting child branch statuses.
- Patrol no longer gets stuck when there are only 2 points.
- Including Netcode for GameObjects caused errors.

### Changed

- Reduced the visual size of sequences.
- Increased the hit box for sequences.
- Graphs now add an instance of a newly created type after generation.

## [0.5.8] - 2024-01-10

### Fixed

- Fixed the alignment of the wizard panel title text.
- Fixed the Vector field widths on inspector.
- Fixed node generation issues when editing the definition of a built-in node.
- Removed unnecessary user prompts when editing a node with hidden variables that are not included in the node story.
- Fixed incorrect method access in BehaviorGraphAgent, causing Netcode for GameObjects errors.

## [0.5.7] - 2023-12-08

### Fixed

- Max serialisation depth reached errors.
- Vector styling in the inspector and nodes UI inside the graph.
- Color and Vector fields not showing correctly in the GameObject inspector.

## [0.5.6] - 2023-12-06

### Fixed

- Fixed VariableModel and TypedVariableModel related errors when building with IL2CPP.
- Fixed exceptions in collision nodes.
- UI: EnumLinkField text wrapping shouldn't happen anymore.
- Error with Input System for Unity 2021.
- Correctly diffrentiate InputSystem deprecated APIs across Unity versions.
- LinkField label color wasn't right in some nodes.
- Various Node UI fixes.
- Certain nodes OnEnd weren't called.

### Changed

- Collision Nodes: If the tag is empty, detect all collisions.
- Removed scroll bars from the graph canvas.

## [0.5.5] - 2023-11-16

### Added

- Minimum window size for Muse Behavior Window.
- Move collision nodes from Sample to Package

### Fixed

- Fixed UnityEditor call preventing compilation.
- Fixed certain type assignments in subgraph fields.
- Missing meta file warnings.
- Error with Input System for Unity 2021.

### Changed

- Updated documentation.

## [0.5.4] - 2023-11-16

### Fixed

- Error after installing the package caused by the Domain Reload not finding test dependencies.

## [0.5.3] - 2023-11-15

### Added

- Orange UI stripe on conditional action nodes.
- Debug status for active subgraphs.
- Allow opening the authoring graph by double clicking the runtime sub-asset.
- Add inspectors for Conditional Guard, Branch and Repeat While nodes.

### Fixed

- Dependency on unit tests caused an exception on some versions of Unity.
- BlacboardEnum attribute was internal, causing an error when used.
- Fix loss of local value on Set Variable Value node.
- An issue with subgraphs fields not allowing you to set certain types.
- Incorrect null check in FindClosestWithTagAction and FindObjectWithTagAction, preventing them from functioning.

## [0.5.2] - 2023-11-14

### Added

- New UI designs implemented.
- Hotkeys: A and Shift+A to align selected nodess' immediate children (A) and all nodes under selected nodes (Shift+A).
- Suggested variable types in node wizards.
- Variable equality evaluator node types.
- Auto-linking of variables by name and recent match.
- Switch serialization of graph instances to use JSON to support 2021 LTS and 2022 LTS.
