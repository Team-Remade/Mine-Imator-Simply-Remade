using System;
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
    
    public readonly Dictionary<Guid, SceneObject> SceneObjects = new();
    public Guid? SelectedObjectGuid = null;

    private const string PAYLOAD_TYPE = "SCENE_OBJECT_GUID";
    
    #nullable enable
    public SceneObject? SelectedObject => SelectedObjectGuid.HasValue && SceneObjects.ContainsKey(SelectedObjectGuid.Value)
        ? SceneObjects[SelectedObjectGuid.Value]
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
            foreach (var obj in SceneObjects.Values.Where(obj => obj.GetParent() is SceneWorld))
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
        if (payload.NativePtr != null && payload.Data != System.IntPtr.Zero && payload.DataSize == sizeof(Guid))
        {
            Guid draggedGuid = *(Guid*)payload.Data;
            if (SceneObjects.ContainsKey(draggedGuid))
            {
                var obj = SceneObjects[draggedGuid];
                // Unparent
                obj.GetParent().RemoveChild(obj);
                World.AddChild(obj);
            }
        }

        ImGui.EndDragDropTarget();
    }

    private unsafe void RenderObjectNode(SceneObject obj)
    {
        Guid objGuid = obj.ObjectGuid;
        bool isSelected = SelectedObjectGuid == objGuid;
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

        ImGui.PushID(objGuid.ToString());
        bool open = ImGui.TreeNodeEx(obj.Name, flags);
        if (ImGui.IsItemClicked())
        {
            SelectedObjectGuid = objGuid;
            SelectionManager.Selection.Clear();
            SelectionManager.Selection.Add(obj);
            SelectionManager.QuerySelection();
        }

        // Begin drag source for this node
        if (ImGui.BeginDragDropSource())
        {
            // Payload: GUID of the dragged object
            ImGui.SetDragDropPayload(PAYLOAD_TYPE, (System.IntPtr)(&objGuid), (uint)sizeof(Guid));
            ImGui.Text($"Move '{obj.Name}'");
            ImGui.EndDragDropSource();
        }

        // Accept drop to reparent here
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(PAYLOAD_TYPE);
            if (payload.NativePtr != null && payload.Data != System.IntPtr.Zero && payload.DataSize == sizeof(Guid))
            {
                Guid draggedGuid = *(Guid*)payload.Data;
                if (SceneObjects.ContainsKey(draggedGuid))
                {
                    var draggedObj = SceneObjects[draggedGuid];
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
    public void UpdateAllObjectIDs()
    {
        // Convert dictionary values to a list to get consistent indexing
        // Filter out any objects that are no longer valid (disposed/queued for deletion)
        var objectsList = SceneObjects.Values
            .Where(obj => GodotObject.IsInstanceValid(obj))
            .ToList();
        
        // Update each object's ID based on its index in the list (starting at 1)
        for (int i = 0; i < objectsList.Count; i++)
        {
            objectsList[i].ID = i + 1;
        }
        
        // Update the picking system to reflect new IDs
        Main.GetInstance().MainViewport.UpdatePicking();
    }
    
    public void DeleteSelectedObject()
    {
        if (SelectedObjectGuid == null || !SceneObjects.ContainsKey(SelectedObjectGuid.Value))
            return;

        var selectedObject = SceneObjects[SelectedObjectGuid.Value];
        
        // Recursively collect all SceneObject children to delete
        var objectsToDelete = new List<SceneObject>();
        CollectChildrenRecursive(selectedObject, objectsToDelete);
        
        // Add the selected object itself to the deletion list
        objectsToDelete.Add(selectedObject);
        
        // Delete all collected objects
        foreach (var objToDelete in objectsToDelete)
        {
            // Remove from the SceneObjects dictionary
            if (SceneObjects.ContainsValue(objToDelete))
            {
                var guid = objToDelete.ObjectGuid;
                SceneObjects.Remove(guid);
            }
            
            // Remove from the scene tree
            objToDelete.QueueFree();
            
            // Remove the picking node for this object and update the viewport picking system
            Main.GetInstance().MainViewport.RemovePickingNode(objToDelete.ID);
        }
        
        // Clear selection if the selected object was deleted
        if (objectsToDelete.Contains(selectedObject))
        {
            SelectedObjectGuid = null;
            SelectionManager.ClearSelection();
        }
        
        // Update all object IDs based on their new indices
        UpdateAllObjectIDs();
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