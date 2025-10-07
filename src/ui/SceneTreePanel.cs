using System.Collections.Generic;
using Godot;
using ImGuiNET;
using SimplyRemadeMI.core;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SimplyRemadeMI.ui;

public class SceneTreePanel
{
    public SceneWorld World { get; set; }
    
    public List<SceneObject> SceneObjects = new List<SceneObject>();
    public int SelectedObjectIndex = -1;

    private const string PayloadType = "SCENE_OBJECT_INDEX";

    public SceneObject? SelectedObject => SelectedObjectIndex >= 0 && SelectedObjectIndex < SceneObjects.Count
        ? SceneObjects[SelectedObjectIndex]
        : null;

    public void Render(Vector2I position, Vector2I size)
    {
        //ImGui.SetNextWindowPos(new Vector2(position.X, position.Y));
        //ImGui.SetNextWindowSize(new Vector2(size.X, size.Y));
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
            for (int i = 0; i < SceneObjects.Count; i++)
            {
                var obj = SceneObjects[i];
                if (obj.GetParent() is not SceneWorld) continue; // only roots here
                RenderObjectNode(obj);
                renderedCount++;
            }

            if (SceneObjects.Count == 0 || renderedCount == 0)
            {
                ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), "No objects in scene");
                ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), "Press F7 to spawn a cube");
            }
        }

        ImGui.End();
        ImGui.PopStyleColor();
    }
    
    private unsafe void HandleRootDropTarget()
    {
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(PayloadType);
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
            ImGui.SetDragDropPayload(PayloadType, (System.IntPtr)(&index), (uint)sizeof(int));
            ImGui.Text($"Move '{obj.Name}'");
            ImGui.EndDragDropSource();
        }

        // Accept drop to reparent here
        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(PayloadType);
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
            default:
                Main.GetInstance().ShowErrorDialog("Invalid object type: " + obj.ObjectType, "Object type not supported");
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
}