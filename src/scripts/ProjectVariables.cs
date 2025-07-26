using System.Collections.Generic;
using Godot;

namespace MineImatorSimplyRemade.scripts;

public partial class ProjectVariables : Resource
{
    public int CamPoseAmount = 0;
    public List<int> CamPosePos = [0];
    public List<int> CamPoseRotate = [0];
    public List<int> CamPoseShake = [0];
    public List<int> CamPoseTransition = [0];
    public List<List<int>> CamPoseData = [[0]];
    public int CamRotate = 0;
    public int CamShake = 0;
    public int CamTransition = 0;
    public int CamNextMin = -1;
    
    public int Chars = 0;
    public string[] CharName = [""];
    public int[] CharSkin = [0];
    public int[] CharVis = [0];
    public int[] CharCol = [-1];
    public int[] CharModel = [0];
    public int[] CharLock = [0];
    public int[] CharLockParent = [-1];
    public int[] CharLockPart = [0];
    public List<List<int>> CharNext = [[-1]];
    public List<List<int>> CharPrev = [[-1]];
    public List<int> CharNextMin = [-1];
    public List<int> CharFlash = [0];
    public List<int> PoseAmount = [0];
    public List<List<int>> PosePos = [[0]];
    public List<List<int>> PoseVis = [[1]];
    public List<List<int>> PoseLock = [[1]];
    public List<List<int>> PoseTransition = [[0]];

    public int Skins = 1;
    public List<string> SkinNames = ["Default"];
    //TODO: SkinDef???
    public List<Texture2D> SkinSprite = [ResourceLoader.Load<Texture2D>("res://sprites/steve.png")];
    
    public int Objs = 0;
    public string[] ObjNames = [""];
    public List<List<int>> ObjTex = [[-1]];
    public List<int> ObjVis = [0];
    public List<int> ObjCol = [-1];
    public List<int> ObjKind = [0]; // 0=item, 1=block, 2=schematic
    public List<int> ObjModel = [-1];
    public List<int> ObjBlock = [0];
    public List<int> ObjData = [0];
    public List<int> ObjItemKind = [1];
    public List<int> ObjItemX = [0];
    public List<int> ObjItemY = [0];
    public List<int> ObjItemSprite = [-1];
    public List<string> ObjSchSource = [""];
    public List<int> ObjSchLength = [0];
    public List<int> ObjSchWidth = [0];
    public List<int> ObjSchHeight = [0];
    public List<int> ObjSchSprite = [-1];
    public List<int> ObjLock = [-1];
    public List<int> ObjLockParent = [0];
    public List<int> ObjLockPart = [0];
    public List<List<int>> ObjRotX = [[0]];
    public List<List<int>> ObjRotY = [[0]];
    public List<List<int>> ObjRotZ = [[0]];
    public List<List<int>> ObjNext = [[-1]];
    public List<List<int>> ObjPrev = [[-1]];
    public List<List<int>> ObjNextMin = [[-1]];
    public List<int> ObjFlash = [0];
    public List<int> ObjPoseAmount = [0];
    public List<List<int>> ObjPosePos = [[0]];
    public List<List<int>> ObjPoseVis = [[1]];
    public List<List<int>> ObjPoseTransition = [[0]];

    public int Textures = 2;
    public List<string> TextureName = ["DefaultTerrain", "DefaultItems"];
    public List<Resource> TexturesSprite = [new Atlas(), new Atlas()];

    public int Bgs = 0;
    public int BgSelect = 0;
    public int BgShow = 1;
    public int BgStretch = 1;
    public Color BgColor = Color.FromString("#16749459", Colors.DeepSkyBlue);
    public string[] BgNames = ["None"];
    public int GroundShow = 1;
    public int TimelineNameW = 100;
    
    public int LightsEnabled = 1;
    public int LightsAmount = 0;
    public List<Light3D> Lights = new();

    public int Temp = 30;
    public int Loop = 1;

    public int CameraFocus = 0;

    public int PrevX = Mathf.FloorToInt(App.Instance.GetViewport().GetMousePosition().X);
    public int PrevY = Mathf.FloorToInt(App.Instance.GetViewport().GetMousePosition().Y);
    
    public int Changes = 0;
}