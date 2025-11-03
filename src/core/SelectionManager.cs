using System.Collections.Generic;
using Gizmo3DPlugin;

namespace SimplyRemadeMI.core;

public static class SelectionManager
{
    public static readonly List<SceneObject> Selection = [];
    
    public static readonly Gizmo3D TransformGizmo = new();

    public static void QuerySelection(bool multiSelect = false)
    {
        var obj = Selection[^1];
        TransformGizmo.Visible = true;
        TransformGizmo.Position = obj.Position;
        if (!multiSelect)
        {
            TransformGizmo.ClearSelection();
        }
        
        TransformGizmo.Select(obj);
    }

    public static void ClearSelection()
    {
        Selection.Clear();
        TransformGizmo.Visible = false;
        TransformGizmo.ClearSelection();
    }
}