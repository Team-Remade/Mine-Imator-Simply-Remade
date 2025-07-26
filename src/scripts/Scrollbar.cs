namespace MineImatorSimplyRemade.scripts;

public class Scrollbar
{
    public int ScrollValue = 0;
    public int ScrollDir;
    public int ScrollPress = 0;
    
    public Scrollbar(int direction)
    {
        ScrollDir = direction;
        GlobalVar.Scrollbars.Add(this);
    }
}