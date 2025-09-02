using Godot;

namespace SimplyRemadeMI.core;

public partial class SceneObject : Node3D
{
    public enum Type
    {
        Empty,
        Cube,
        Block,
        Item,
        ModelPart,
        TestInvalidType,
    }
    
    public Type ObjectType;
    
    public bool IsDescendantOf(SceneObject ancestor)
    {
        var current = GetParent();
        while (current != null)
        {
            if (current == ancestor)
                return true;
            current = current.GetParent();
        }

        return false;
    }
}