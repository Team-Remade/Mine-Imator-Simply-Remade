using System.Collections.Generic;
using System.Linq;
using Godot;
using ImGuiNET;
using SimplyRemadeMI.core;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SimplyRemadeMI.ui;

public class SceneTreePanel
{
    public SceneWorld World { get; set; }
    
    public readonly List<SceneObject> SceneObjects = [];
    public int SelectedObjectIndex = -1;

    private const string PAYLOAD_TYPE = "SCENE_OBJECT_INDEX";
    
    #nullable enable
    public SceneObject? SelectedObject => SelectedObjectIndex >= 0 && SelectedObjectIndex < SceneObjects.Count
        ? SceneObjects[SelectedObjectIndex]
        : null;
    #nullable disable

    public void Render()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.2f, 0.2f, 0.25f, 1.0f));

        if (ImGui.Begin("Scene Tree",
                ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("Scene Objects:");
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.18f, 0.18f, 0.22f, 1.0f));
            ImGui.BeginChild("##RootDropTarget", new Vector2(0, 28), ImGuiChildFlags.None, ImGuiWindowFlags.None);
            ImGui.Text("Drop here to unparent");
            HandleRootDropTarget();
            ImGui.EndChild();
            ImGui.PopStyleColor();

            // Build and render hierarchy: top-level nodes are objects with no parent
            int renderedCount = 0;
            foreach (var obj in SceneObjects.Where(obj => obj.GetParent() is SceneWorld))
            {
                RenderObjectNode(obj);
                renderedCount++;
            }

            if (SceneObjects.Count == 0 || renderedCount == 0)
            {
                ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), "No objects in scene");
                ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), "Press F7 to spawn an empty object");
            }
        }

        ImGui.End();
        ImGui.PopStyleColor();
    }
    
    private unsafe void HandleRootDropTarget()
    {
        if (!ImGui.BeginDragDropTarget()) return;
        var payload = ImGui.AcceptDragDropPayload(PAYLOAD_TYPE);
        if (payload.NativePtr != null && payload.Data != System.IntPtr.Zero && payload.DataSize == sizeof(int))
        {
            int index = *(int*)payload.Data;
            if (index >= 0 && index < SceneObjects.Count)
            {
                var obj = SceneObjects[index];
                // Unparent
                obj.GetParent().RemoveChild(obj);
                World.AddChild(obj);
            }
        }

        ImGui.EndDragDropTarget();
    }

    private unsafe void RenderObjectNode(SceneObject obj)
    {
        int index = SceneObjects.IndexOf(obj);
        bool isSelected = index == SelectedObjectIndex;
        bool hasChildren = obj.GetChildren().Count > 0;

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                    ImGuiTreeNodeFlags.SpanFullWidth;
        if (!hasChildren)
        {
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        }

        if (isSelected)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        ImGui.PushID(index);
        bool open = ImGui.TreeNodeEx(obj.Name, flags);
        if (ImGui.IsItemClicked())
        {
            SelectedObjectIndex = index;
            SelectionManager.Selection.Add(SceneObjects[index]);
            SelectionManager.QuerySelection();
        }

        // Begin drag source for this node
        if (ImGui.BeginDragDropSource())
        {
            // Payload: index of the dragged object
            ImGui.SetDragDropPayload(PAYLOAD_TYPE, (System.IntPtr)(&index), (uint)sizeof(int));
            ImGui.Text($"Move '{obj.Name}'");
            ImGui.EndDragDropSource();
        }

        // Accept drop to reparent here
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(PAYLOAD_TYPE);
            if (payload.NativePtr != null && payload.Data != System.IntPtr.Zero && payload.DataSize == sizeof(int))
            {
                int draggedIndex = *(int*)payload.Data;
                if (draggedIndex >= 0 && draggedIndex < SceneObjects.Count)
                {
                    var draggedObj = SceneObjects[draggedIndex];
                    // Prevent invalid parenting: to itself or descendant
                    if (draggedObj != obj && !obj.IsDescendantOf(draggedObj))
                    {
                        draggedObj.GetParent().RemoveChild(draggedObj);
                        obj.AddChild(draggedObj);
                    }
                }
            }

            ImGui.EndDragDropTarget();
        }

        ImGui.SameLine();
        switch (obj.ObjectType)
        {
            case SceneObject.Type.Cube:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[Cube]");
                break;
            case SceneObject.Type.Block:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[Block]");
                break;
            case SceneObject.Type.Empty:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[Empty]");
                break;
            case SceneObject.Type.Item:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[Item]");
                break;
            case SceneObject.Type.ModelPart:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[ModelPart]");
                break;
            case SceneObject.Type.Camera:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[Camera]");
                break;
            case SceneObject.Type.PointLight:
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1.0f), "[PointLight]");
                break;
            default:
                Main.ShowErrorDialog("Invalid object type: " + obj.ObjectType, "Object type not supported");
                System.Environment.Exit(1);
                return;
        }

        if (open && hasChildren)
        {
            foreach (var child in obj.GetChildren())
            {
                if (child is SceneObject sceneObject)
                {
                    RenderObjectNode(sceneObject);
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
    public void DeleteSelectedObject()
    {
        if (SelectedObjectIndex < 0 || SelectedObjectIndex >= SceneObjects.Count)
            return;

        var selectedObject = SceneObjects[SelectedObjectIndex];
        
        // Recursively collect all SceneObject children to delete
        var objectsToDelete = new List<SceneObject>();
        CollectChildrenRecursive(selectedObject, objectsToDelete);
        
        // Add the selected object itself to the deletion list
        objectsToDelete.Add(selectedObject);
        
        // Delete all collected objects in reverse order (children first)
        for (int i = objectsToDelete.Count - 1; i >= 0; i--)
        {
            var objToDelete = objectsToDelete[i];
            
            // Remove from the SceneObjects list if it's there
            int indexInList = SceneObjects.IndexOf(objToDelete);
            if (indexInList != -1)
            {
                SceneObjects.RemoveAt(indexInList);
                
                // Adjust selected index if necessary
                if (indexInList < SelectedObjectIndex)
                {
                    SelectedObjectIndex--;
                }
            }
            
            // Remove from the scene tree
            objToDelete.QueueFree();
            
            // Remove the picking node for this object and update the viewport picking system
            Main.GetInstance().MainViewport.RemovePickingNode(objToDelete.ID);
        }
        
        // Clear selection if the selected object was deleted
        if (objectsToDelete.Contains(selectedObject))
        {
            SelectedObjectIndex = -1;
            SelectionManager.ClearSelection();
        }
    }
    
    private static void CollectChildrenRecursive(SceneObject parent, List<SceneObject> children)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is SceneObject sceneObject)
            {
                // Add this child to the list
                children.Add(sceneObject);
                
                // Recursively collect its children
                CollectChildrenRecursive(sceneObject, children);
            }
        }
    }
}