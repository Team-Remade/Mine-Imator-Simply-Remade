using System.Collections.Generic;
using Gizmo3DPlugin;

namespace SimplyRemadeMI.core;

public static class SelectionManager
{
    public static List<SceneObject> Selection = new List<SceneObject>();
    
    public static Gizmo3D TransformGizmo = new Gizmo3D();

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