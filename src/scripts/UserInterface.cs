using System;
using System.Collections.Generic;
using Godot;

namespace MineImatorSimplyRemade.scripts;

public partial class UserInterface : Node
{
    [Export] public TheView Viewport;
    [Export] public Control Timeline;
    [Export] public Control Properties;
    
    public ProjectVariables Project;
    
    public int TimelineY = 0;
    public int TimelineH = 150;
    public float TimelineYP = 1f;
    public int TimelineToggle = 0;
    public bool TimelineShow = true;
    public int TimelinePos = 0;
    public int TimelineLocked = 1;
    public int TimelineAmount = 0;
    
    public int PropertiesX = 0;
    public int PropertiesW = 280;
    public int PropertiesXP = 1;
    public int PropertiesToggle = 0;
    public int PropertiesShow = 1;
    public int PropertiesLocked = 1;
    
    public string WindowDrag = "";
    public string HelpText = "";
    public int ShowHelp = 1;
    
    public int PropertiesStartY = 0;
    public Scrollbar PropertiesScroll = new Scrollbar(1);
    public int PropertiesH = 0;
    
    public int PropertiesTlSel = -2;
    public Scrollbar PropertiesCharScroll = new Scrollbar(1);
    public int PropertiesCharFirst = 0;
    
    public Scrollbar PropertiesObjScroll = new Scrollbar(1);
    public int PropertiesObjFirst = 0;
    
    public int PropertiesCharsShow = 1;
    public int PropertiesBgShow = 0;
    public int PropertiesLightShow = 0;
    public int PropertiesObjShow = 0;
    
    public int TimelineCamera = 0;
    public int TimelineLastPos = 0;
    public int TimelinePlay = 0;
    public Scrollbar TimelineScrollH = new Scrollbar(0);
    public Scrollbar TimelineScrollV = new Scrollbar(1);
    public int TimelineCharFirst = 0;
    public int TimelinePoseFirst = 0;
    public int TimelineClickPosFirst = -1;
    public int TimelineCopyPosFirst = 0;
    
    public int SkinZoom = 3;
    
    public int LightsSx = -3500;
    public int LightsSy = -3500;
    public int LightsZoom = 32;
    public int LightsDrag = -1;
    public float LightsMouseX = 0;
    public float LightsMouseY = 0;
    public int LightsMcsX = 0;
    public int LightsMcsY = 0;
    public Color LightsColor = Colors.White;
    public int LightsRange = 10000;
    public int LightsSelect = -1;
    public int YZ = 0;
    
    public List<float> PosSX = new();
    public List<float> PosSY = new();
    public List<float> PosSZoom = new();
    public List<int> PosSDrag = new();
    public List<float> PosSMouseX = new();
    public List<float> PosSMouseY = new();
    public List<int> PosSMcsX = new();
    public List<int> PosSMcsY = new();
    public List<int> PosSSelect = new();
    public List<int> PosSYZ = new();
    public List<int> PosSSnap = new();
    
    public int MeterMX = 0;
    public int CircleMeterD = 0;
    public int MeterSel = -1;
    public int Step = 0;
    
    public int PosePartSel = 0;
    public int PoseSelect = -1;
    public int PoseStartY = 0;
    public int PoseAlphaShow = 0;
    public int PoseScaleShow = 0;
    public int PosePosShow = 0;
    public int PoseRotShow = 0;
    public int PoseTransShow = 0;
    public int PoseShakeShow = 0;
    public int PoseH = 0;
    public Scrollbar PoseScroll = new Scrollbar(1);
    public int PoseLastPos = 0;
    public int Playing = 0;
    public int Recording = 0;
    public string RecordFileName = "";
    
    public string FileName = "UnsavedAnimation";
    public int CurrentModel = -1;

    public int SchWidth = 0;
    public int SchLength = 0;
    public int SchHeight = 0;
    
    public int ModelLength = 10;
    public int ModelWidth = 10;
    public int ModelHeight = 10;
    
    public override void _Ready()
    {
        App.Instance.UserInterface = this;
        
        LightsMouseX = App.Instance.GetViewport().GetMousePosition().X;
        LightsMouseY = App.Instance.GetViewport().GetMousePosition().Y;
        
        var h = DisplayServer.ScreenGetSize().Y - 200;
        var w = DisplayServer.ScreenGetSize().X - 160;
        
        DisplayServer.WindowSetSize(new Vector2I(w, h));
        App.Instance.CenterScreen();
        
        ExternalLibrary.Start();

        for (int a = 0; a < 3; a++)
        {
            PosSX.Add(-18.724375f);
            PosSY.Add(-18.724375f);
            PosSZoom.Add(0.16875f);
            PosSDrag.Add(-1);
            PosSMouseX.Add(GetViewport().GetMousePosition().X);
            PosSMouseY.Add(GetViewport().GetMousePosition().Y);
            PosSMcsX.Add(0);
            PosSMcsY.Add(0);
            PosSSelect.Add(-1);
            PosSYZ.Add(0);
            PosSSnap.Add(0);
        }

        Project = new ProjectVariables();
        Atlas atlas1 = (Atlas)Project.TexturesSprite[0];
        atlas1.Name = "DefaultTerrain";
        atlas1.ResourceLocation = "res://sprites/defaultTerrain/terrain/";
        atlas1.Init();
        Atlas atlas2 = (Atlas)Project.TexturesSprite[1];
        atlas2.Name = "DefaultItems";
        atlas2.ResourceLocation = "res://sprites/defaultItems/item/";
        atlas2.Init();
        
        Viewport.Environment.Environment.BackgroundColor = Project.BgColor;
        
        Timeline.CustomMinimumSize = new Vector2I(0, TimelineH);
        Properties.CustomMinimumSize = new Vector2I(PropertiesW, 0);
        
        Camera.Reset();
    }

    public override void _Process(double delta)
    {
        return;
        
        var a = 0;
        var w = GetWindow().Size.X;
        var h = GetWindow().Size.Y;
        
        //Timeline
        if (TimelineLocked == 1)
        {
            TimelineYP = 1;
        }
        else
        {
            if (TimelineToggle == 0)
            {
                if ((Mathf.FloorToInt(GetViewport().GetMousePosition().Y) > h - 100 || (Mathf.FloorToInt(GetViewport().GetMousePosition().Y) < 50 && PropertiesX == 0)) && !TimelineShow && !Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    TimelineToggle = 1;
                }

                if (Mathf.FloorToInt(GetViewport().GetMousePosition().Y) < h - TimelineH - 50 && TimelineShow && WindowDrag == "")
                {
                    TimelineToggle = 2;
                }
            } else 
            {
                if (TimelineToggle == 1)
                {
                    TimelineYP += 0.2f;
                }

                if (TimelineToggle == 2)
                {
                    TimelineYP -= 0.2f;
                }

                if (TimelineYP == 0 || Math.Abs(TimelineYP - 1) < 0.01)
                {
                    TimelineToggle = 0;
                    TimelineShow = !TimelineShow;
                }
            }
        }

        a = 0;
        if (TimelineToggle > 0)
        {
            a = 3 - TimelineToggle;
        }
    }
}