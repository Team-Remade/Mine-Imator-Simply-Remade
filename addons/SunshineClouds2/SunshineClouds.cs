using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
[GlobalClass]
public partial class SunshineClouds : CompositorEffect
{
    [ExportGroup("Basic Settings")]
    [Export(PropertyHint.Range, "0,1")] public float CloudsCoverage { get { return _cloudsCoverage; } set { _cloudsCoverage = value;  } }
    private float _cloudsCoverage = 0.6f;
    [Export(PropertyHint.Range, "0,20")] public float CloudsDensity { get { return _cloudsDensity; } set { _cloudsDensity = value;  } }
    private float _cloudsDensity = 1.0f;
    [Export(PropertyHint.Range, "0,1")] public float AtmosphericDensity { get { return _atmosphericDensity; } set { _atmosphericDensity = value;  } }
    private float _atmosphericDensity = 0.5f;

    [ExportSubgroup("Colors")]
    [Export(PropertyHint.Range, "0,0.5")] public float CloudsAnisotropy { get { return _cloudsAnisotropy; } set { _cloudsAnisotropy = value;  } }
    private float _cloudsAnisotropy = 0.3f;
    [Export] public Color CloudAmbientColor { get; set; } = new Color(0.352f, 0.624f, 0.784f, 1.0f);
    [Export] public Color CloudAmbientTint { get; set; } = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [Export] public Color AtmosphereColor { get; set; } = new Color(0.801f, 0.893f, 0.962f, 1.0f);
    [Export] public Color AmbientOcclusionColor { get; set; } = new Color(0.17f, 0.044f, 0.027f, 0.549f);

    [ExportSubgroup("Structure")]
    [Export(PropertyHint.Range, "0,1")] public float AccumilationDecay { get { return _accumilationDecay; } set { _accumilationDecay = value; } }
    private float _accumilationDecay = 0.5f;
    [Export(PropertyHint.Range, "100,1000000")] public float ExtraLargeNoiseScale { get { return _extraLargeNoiseScale; } set { _extraLargeNoiseScale = value;  } }
    private float _extraLargeNoiseScale = 320000.0f;
    [Export(PropertyHint.Range, "100,500000")] public float LargeNoiseScale { get { return _largeNoiseScale; } set { _largeNoiseScale = value;  } }
    private float _largeNoiseScale = 50000.0f;
    [Export(PropertyHint.Range, "100,100000")] public float MediumNoiseScale { get { return _mediumNoiseScale; } set { _mediumNoiseScale = value;  } }
    private float _mediumNoiseScale = 6000.0f;
    [Export(PropertyHint.Range, "100,10000")] public float SmallNoiseScale { get { return _smallNoiseScale; } set { _smallNoiseScale = value;  } }
    private float _smallNoiseScale = 2500.0f;

    [Export(PropertyHint.Range, "0.001,1.0")] public float CloudsSharpness { get { return _cloudsSharpness; } set { _cloudsSharpness = value;  } }
    private float _cloudsSharpness = 1.0f;
    [Export(PropertyHint.Range, "0,3")] public float CloudsDetailPower { get { return _cloudsDetailPower; } set { _cloudsDetailPower = value;  } }
    private float _cloudsDetailPower = 0.9f;
    [Export(PropertyHint.Range, "0,50000")] public float CurlNoiseStrength { get { return _curlNoiseStrength; } set { _curlNoiseStrength = value;  } }
    private float _curlNoiseStrength = 5000.0f;

    [Export(PropertyHint.Range, "0,2")] public float LightingSharpness { get { return _lightingSharpness; } set { _lightingSharpness = value;  } }
    private float _lightingSharpness = 0.05f;
    [Export(PropertyHint.Range, "0,10")] public float LightingDensity { get { return _lightingDensity; } set { _lightingDensity = value;  } }
    private float _lightingDensity = 0.55f;

    [Export] public float CloudFloor { get { return _cloudFloor; } set { _cloudFloor = value;  } }
    private float _cloudFloor = 1500.0f;
    [Export] public float CloudCeiling { get { return _cloudCeiling; } set { _cloudCeiling = value;  } }
    private float _cloudCeiling = 25000.0f;



    [ExportSubgroup("Performance")]
    [Export] public int MaxStepCount { get { return _maxStepCount; } set { _maxStepCount = value;  } }
    private int _maxStepCount = 50;
    [Export] public int MaxLightingSteps { get { return _maxLightingSteps; } set { _maxLightingSteps = value;  } }
    private int _maxLightingSteps = 32;

    [Export(PropertyHint.Enum, "Native,Quarter,Eighth,Sixteenth")] public int ResolutionScale { get { return _resolutionScale; } set { _resolutionScale = value; _lastSize = Vector2I.Zero; } }
    private int _resolutionScale = 0;

    [Export(PropertyHint.Range, "0,2")] public float LODBias { get { return _LODBias; } set { _LODBias = value;  } }
    private float _LODBias = 1.0f;

    [ExportSubgroup("Noise Textures")]
    [Export] public Texture3D DitherNoise { get { return _DitherNoise; } set { _DitherNoise = value;  } }
    private Texture3D _DitherNoise;
    [Export] public Texture2D HeightGradient { get { return _HeightGradient; } set { _HeightGradient = value;  } }
    private Texture2D _HeightGradient;
    [Export] public Texture2D ExtraLargeNoisePatterns { get { return _ExtraLargeNoisePatterns; } set { _ExtraLargeNoisePatterns = value;  } }
    private Texture2D _ExtraLargeNoisePatterns;
    [Export] public Texture3D LargeScaleNoise { get { return _LargeScaleNoise; } set { _LargeScaleNoise = value;  } }
    private Texture3D _LargeScaleNoise;
    [Export] public Texture3D MediumScaleNoise { get { return _MediumScaleNoise; } set { _MediumScaleNoise = value;  } }
    private Texture3D _MediumScaleNoise;
    [Export] public Texture3D SmallScaleNoise { get { return _SmallScaleNoise; } set { _SmallScaleNoise = value;  } }
    private Texture3D _SmallScaleNoise;
    [Export] public Texture3D CurlNoise { get { return _CurlNoise; } set { _CurlNoise = value;  } }
    private Texture3D _CurlNoise;
    

    [ExportGroup("Advanced Settings")]
    [ExportSubgroup("Visuals")]
    [Export(PropertyHint.Range, "0,1000")] public float DitherSpeed { get { return _ditherSpeed; } set { _ditherSpeed = value;  } }
    private float _ditherSpeed = 100.8254f;
    [Export(PropertyHint.Range, "0,20")] public float BlurPower { get { return _blurPower; } set { _blurPower = value;  } }
    private float _blurPower = 2.0f;
    [Export(PropertyHint.Range, "0,6")] public float BlurQuality { get { return _blurQuality; } set { _blurQuality = value;  } }
    private float _blurQuality = 1.0f;

    [ExportSubgroup("Performance")]
    [Export] public float MinStepDistance { get { return _minStepDistance; } set { _minStepDistance = value;  } }
    private float _minStepDistance = 100.0f;
    [Export] public float MaxStepDistance { get { return _maxStepDistance; } set { _maxStepDistance = value;  } }
    private float _maxStepDistance = 600.0f;

    [Export] public float LightingTravelDistance { get { return _lightingTravelDistance; } set { _lightingTravelDistance = value;  } }
    private float _lightingTravelDistance = 5000.0f;

    [ExportGroup("Compute Shaders")]
    [Export(PropertyHint.File, "*.glsl")] public RDShaderFile PrePassComputeShader;
    [Export(PropertyHint.File, "*.glsl")] public RDShaderFile ComputeShader;
    [Export(PropertyHint.File, "*.glsl")] public RDShaderFile PostPassComputeShader;

    [ExportGroup("Internal Use")]
    [ExportSubgroup("Positions")]
    [Export] public Vector3 WindDirection { get; set; } = Vector3.Zero;
    [Export] public Vector3 ExtraLargeScaleCloudsPosition { get; set; } = Vector3.Zero;
    [Export] public Vector3 LargeScaleCloudsPosition { get; set; } = Vector3.Zero;
    [Export] public Vector3 MediumScaleCloudsPosition { get; set; } = Vector3.Zero;
    [Export] public Vector3 DetailCloudsPosition { get; set; } = Vector3.Zero;
    [Export] public float CurrentTime { get; set; } = 0.0f;
    [ExportSubgroup("Light Data")]
    [Export] public Godot.Collections.Array<Vector4> DirectionalLightsData { get; set; } = new Godot.Collections.Array<Vector4>();
    [Export] public Godot.Collections.Array<Vector4> PointLightsData { get; set; } = new Godot.Collections.Array<Vector4>();

    public bool LightsUpdated { get; set; } = false;

    private RenderingDevice _rd;
    private Rid _shader = new Rid();
    private Rid _pipeline = new Rid();

    private Rid _prepass_shader = new Rid();
    private Rid _prepass_pipeline = new Rid();

    private Rid _postpass_shader = new Rid();
    private Rid _postpass_pipeline = new Rid();

    private Rid _nearestSampler = new Rid();
    private Rid _linearSampler = new Rid();
    private Rid _linearSamplerNoRepeat = new Rid();

    private Rid _generalDataBuffer = new Rid();
    private Rid _lightDataBuffer = new Rid();
    private Rid[] _accumulationTextures;
    private Rid _resizedDepth = new Rid();
    private byte[] _pushConstants;
    private byte[] _prepasspushConstants;
    private byte[] _postpasspushConstants;  
    private Vector2I _lastSize = new Vector2I(0,0);

    private RenderSceneBuffersRD _buffers;


    private Rid[] _uniformSets;
    private float[] _generalDataFloats = new float[112];
    private byte[] _generalData = new byte[448];

    private float[] _lightDataFloats = new float[96];
    private byte[] _lightData = new byte[384];

    private bool _accumulationisA = false;

    private Transform3D? _lastViewMat;
    private Projection? _lastProjectionMat;

    private int _filterIndex = 0;

    public SunshineClouds()
    {
        EffectCallbackType = EffectCallbackTypeEnum.PostSky;
        AccessResolvedDepth = true;
        AccessResolvedColor = true;
        NeedsMotionVectors = true;

        RenderingServer.CallOnRenderThread(Callable.From(InitializeCompute));
    }


    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            RenderingServer.CallOnRenderThread(Callable.From(ClearCompute));
        }
    }

    private void ClearCompute()
    {
        //GD.Print("clearing compute");
        if (_rd != null)
        {
            if (_shader.IsValid) _rd.FreeRid(_shader);
            _shader = new Rid();

            if (_prepass_shader.IsValid) _rd.FreeRid(_prepass_shader);
            _prepass_shader = new Rid();

            if (_postpass_shader.IsValid) _rd.FreeRid(_postpass_shader);
            _postpass_shader = new Rid();

            if (_nearestSampler.IsValid) _rd.FreeRid(_nearestSampler);
            _nearestSampler = new Rid();

            if (_linearSampler.IsValid) _rd.FreeRid(_linearSampler);
            _linearSampler = new Rid();

            if (_linearSamplerNoRepeat.IsValid) _rd.FreeRid(_linearSamplerNoRepeat);
            _linearSamplerNoRepeat = new Rid();
            
            if (_generalDataBuffer.IsValid) _rd.FreeRid(_generalDataBuffer);
            _generalDataBuffer = new Rid();

            if (_lightDataBuffer.IsValid) _rd.FreeRid(_lightDataBuffer);
            _lightDataBuffer = new Rid();

            if (_resizedDepth.IsValid) _rd.FreeRid(_resizedDepth);
            _resizedDepth = new Rid();
            


            if (_accumulationTextures != null)
            {
                foreach (var item in _accumulationTextures)
                {
                    if (item.IsValid)
                    {
                        _rd.FreeRid(item);
                    }
                }
                _accumulationTextures = null;
            }


        }
    }

    public void InitializeCompute()
    {
        if (_rd == null)
        {
            _rd = RenderingServer.GetRenderingDevice();

            if (_rd == null)
            {
                Enabled = false;
                GD.PrintErr("No rendering device on load.");
                return;
            }
        }

        ClearCompute();

        RDSamplerState samplerState = new RDSamplerState();
        samplerState.MinFilter = RenderingDevice.SamplerFilter.Nearest;
        samplerState.MagFilter = RenderingDevice.SamplerFilter.Nearest;
        samplerState.RepeatU = RenderingDevice.SamplerRepeatMode.Repeat;
        samplerState.RepeatV = RenderingDevice.SamplerRepeatMode.Repeat;
        samplerState.RepeatW = RenderingDevice.SamplerRepeatMode.Repeat;
        _nearestSampler = _rd.SamplerCreate(samplerState);

        RDSamplerState linearsamplerState = new RDSamplerState();
        linearsamplerState.MinFilter = RenderingDevice.SamplerFilter.Linear;
        linearsamplerState.MagFilter = RenderingDevice.SamplerFilter.Linear;
        linearsamplerState.RepeatU = RenderingDevice.SamplerRepeatMode.Repeat;
        linearsamplerState.RepeatV = RenderingDevice.SamplerRepeatMode.Repeat;
        linearsamplerState.RepeatW = RenderingDevice.SamplerRepeatMode.Repeat;
        _linearSampler = _rd.SamplerCreate(linearsamplerState);


        RDSamplerState linearsamplerStateNoRepeat = new RDSamplerState();
        linearsamplerStateNoRepeat.MinFilter = RenderingDevice.SamplerFilter.Linear;
        linearsamplerStateNoRepeat.MagFilter = RenderingDevice.SamplerFilter.Linear;
        linearsamplerStateNoRepeat.RepeatU = RenderingDevice.SamplerRepeatMode.ClampToEdge;
        linearsamplerStateNoRepeat.RepeatV = RenderingDevice.SamplerRepeatMode.ClampToEdge;
        linearsamplerStateNoRepeat.RepeatW = RenderingDevice.SamplerRepeatMode.ClampToEdge;
        _linearSamplerNoRepeat = _rd.SamplerCreate(linearsamplerStateNoRepeat);

        if (DitherNoise == null) {
            DitherNoise = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/bluenoise_Dither.png") as Texture3D;
        }
        if (HeightGradient == null){
            HeightGradient = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/HeightGradient.tres") as Texture2D;
        }
        if (ExtraLargeNoisePatterns == null){
            ExtraLargeNoisePatterns = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/ExtraLargeScaleNoise.tres") as Texture2D;
        }
        if (LargeScaleNoise == null){
            LargeScaleNoise = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/LargeScaleNoise.tres") as Texture3D;
        }
        if (MediumScaleNoise == null){
            MediumScaleNoise = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/MediumScaleNoise.tres") as Texture3D;
        }
        if (SmallScaleNoise == null){
            SmallScaleNoise = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/SmallScaleNoise.tres") as Texture3D;
        }
        if (CurlNoise == null){
            CurlNoise = ResourceLoader.Load("res://addons/SunshineClouds2/NoiseTextures/curl_noise_varied.tga") as Texture3D;
        }


        if (ComputeShader == null)
        {
            ComputeShader = ResourceLoader.Load<RDShaderFile>("res://addons/SunshineClouds2/SunshineCloudsCompute.glsl");
        }
        if (PrePassComputeShader == null)
        {
            PrePassComputeShader = ResourceLoader.Load<RDShaderFile>("res://addons/SunshineClouds2/SunshineCloudsPreCompute.glsl");
        }

        if (PostPassComputeShader == null)
        {
            PostPassComputeShader = ResourceLoader.Load<RDShaderFile>("res://addons/SunshineClouds2/SunshineCloudsPostCompute.glsl");
        }

        if (ComputeShader == null || PrePassComputeShader == null || PostPassComputeShader == null)
        {
            Enabled = false;
            GD.PrintErr("No Shader found on load.");
            ClearCompute();
            return;
        }

        var prepassshaderSpirv = PrePassComputeShader.GetSpirV();
        _prepass_shader = _rd.ShaderCreateFromSpirV(prepassshaderSpirv);
        if (_prepass_shader.IsValid)
        {
            _prepass_pipeline = _rd.ComputePipelineCreate(_prepass_shader);
        }
        else
        {
            Enabled = false;
            GD.PrintErr("Prepass Shader failed to compile.");
            ClearCompute();
            return;
        }

        var shaderSpirv = ComputeShader.GetSpirV();
        _shader = _rd.ShaderCreateFromSpirV(shaderSpirv);
        if (_shader.IsValid)
        {
            _pipeline = _rd.ComputePipelineCreate(_shader);
        }
        else
        {
            Enabled = false;
            GD.PrintErr("Shader failed to compile.");
            ClearCompute();
            return;
        }

        var postpassshaderSpirv = PostPassComputeShader.GetSpirV();
        _postpass_shader = _rd.ShaderCreateFromSpirV(postpassshaderSpirv);
        if (_postpass_shader.IsValid)
        {
            _postpass_pipeline = _rd.ComputePipelineCreate(_postpass_shader);
        }
        else
        {
            Enabled = false;
            GD.PrintErr("Post pass Shader failed to compile.");
            ClearCompute();
            return;
        }
    }

    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        if (_rd == null)
        {
            InitializeCompute();
        }
        else if (_pipeline.IsValid && HeightGradient != null && ExtraLargeNoisePatterns != null && LargeScaleNoise != null && MediumScaleNoise != null && SmallScaleNoise != null && DitherNoise != null && CurlNoise != null)
        {
            _buffers = renderData.GetRenderSceneBuffers() as RenderSceneBuffersRD;
            if (_buffers != null)
            {
                Vector2I size = _buffers.GetInternalSize();
                if (size.X == 0 && size.Y == 0)
                {
                    return;
                }

                uint resscale = 1;

                switch (ResolutionScale)
                {
                    case 0: //1:1
                        resscale = 1;
                        break;
                    case 1: //4:1
                        resscale = 2;
                        break;
                    case 2: //8:1
                        resscale = 4;
                        break;
                    case 3: //16:1
                        resscale = 8;
                        break;
                }

                Vector2I newSize = size;
                newSize.X = newSize.X / (int)resscale;
                newSize.Y = newSize.Y / (int)resscale;

                uint viewCount = _buffers.GetViewCount();

                if (size != _lastSize || _uniformSets == null || _uniformSets.Length != viewCount * 3)
                {
                    InitializeCompute();

                    _accumulationTextures = new Rid[viewCount * 6];
                    _uniformSets = new Rid[viewCount * 3];

                    //prepass push constants:
                    float[] prepassData = new float[4];
                    _prepasspushConstants = new byte[16];

                    prepassData[0] = size.X;
                    prepassData[1] = size.Y;
                    prepassData[2] = resscale;
                    prepassData[3] = 0.0f;

                    Buffer.BlockCopy(prepassData, 0, _prepasspushConstants, 0, 16);

                    float[] postpassData = new float[4];
                    _postpasspushConstants = new byte[16];

                    postpassData[0] = newSize.X;
                    postpassData[1] = newSize.Y;
                    postpassData[2] = resscale;
                    postpassData[3] = 0.0f;

                    Buffer.BlockCopy(postpassData, 0, _postpasspushConstants, 0, 16);


                    for (uint view = 0; view < viewCount; view++)
                    {
                        Rid colorImage = _buffers.GetColorLayer(view);
                        Rid depthImage = _buffers.GetDepthLayer(view);

                        var baseColorformat = _rd.TextureGetFormat(colorImage);
                        baseColorformat.Format = RenderingDevice.DataFormat.R32G32B32A32Sfloat;
                        baseColorformat.Width = (uint)newSize.X;
                        baseColorformat.Height = (uint)newSize.Y;

                        //then make my data accumilation images.
                        _accumulationTextures[view * 3] = _rd.TextureCreate(baseColorformat, new RDTextureView(), null);
                        _accumulationTextures[view * 3 + 1] = _rd.TextureCreate(baseColorformat, new RDTextureView(), null);
                        _accumulationTextures[view * 3 + 2] = _rd.TextureCreate(baseColorformat, new RDTextureView(), null);
                        _accumulationTextures[view * 3 + 3] = _rd.TextureCreate(baseColorformat, new RDTextureView(), null);
                        _accumulationTextures[view * 3 + 4] = _rd.TextureCreate(baseColorformat, new RDTextureView(), null);
                        _accumulationTextures[view * 3 + 5] = _rd.TextureCreate(baseColorformat, new RDTextureView(), null);


                        var depthformat = _rd.TextureGetFormat(depthImage);
                        depthformat.Width = (uint)newSize.X;
                        depthformat.Height = (uint)newSize.Y;
                        depthformat.Format = RenderingDevice.DataFormat.R32Sfloat;
                        depthformat.UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.SamplingBit;
                        _resizedDepth = _rd.TextureCreate(depthformat, new RDTextureView(), null);


                        var _prepassuniformsArray = new Godot.Collections.Array<RDUniform>();
                        var prepassDepthUniform = new RDUniform();
                        prepassDepthUniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        prepassDepthUniform.Binding = 0;
                        prepassDepthUniform.AddId(_nearestSampler);
                        prepassDepthUniform.AddId(depthImage);
                        _prepassuniformsArray.Add(prepassDepthUniform);

                        var prepassDepthOutputUniform = new RDUniform();
                        prepassDepthOutputUniform.UniformType = RenderingDevice.UniformType.Image;
                        prepassDepthOutputUniform.Binding = 1;
                        prepassDepthOutputUniform.AddId(_resizedDepth);
                        _prepassuniformsArray.Add(prepassDepthOutputUniform);

                        _uniformSets[view * 3] = _rd.UniformSetCreate(_prepassuniformsArray, _prepass_shader, 0);


                        var _uniformsArray = new Godot.Collections.Array<RDUniform>();

                        var outputDataUniform = new RDUniform();
                        outputDataUniform.UniformType = RenderingDevice.UniformType.Image;
                        outputDataUniform.Binding = 0;
                        outputDataUniform.AddId(_accumulationTextures[view * 3]);
                        _uniformsArray.Add(outputDataUniform);

                        var outputColorUniform = new RDUniform();
                        outputColorUniform.UniformType = RenderingDevice.UniformType.Image;
                        outputColorUniform.Binding = 1;
                        outputColorUniform.AddId(_accumulationTextures[view * 3 + 1]);
                        _uniformsArray.Add(outputColorUniform);


                        var accum1Auniform = new RDUniform();
                        accum1Auniform.UniformType = RenderingDevice.UniformType.Image;
                        accum1Auniform.Binding = 2;
                        accum1Auniform.AddId(_accumulationTextures[view * 3 + 2]);
                        _uniformsArray.Add(accum1Auniform);

                        var accum1Buniform = new RDUniform();
                        accum1Buniform.UniformType = RenderingDevice.UniformType.Image;
                        accum1Buniform.Binding = 3;
                        accum1Buniform.AddId(_accumulationTextures[view * 3 + 3]);
                        _uniformsArray.Add(accum1Buniform);

                        var accum2Auniform = new RDUniform();
                        accum2Auniform.UniformType = RenderingDevice.UniformType.Image;
                        accum2Auniform.Binding = 4;
                        accum2Auniform.AddId(_accumulationTextures[view * 3 + 4]);
                        _uniformsArray.Add(accum2Auniform);

                        var accum2Buniform = new RDUniform();
                        accum2Buniform.UniformType = RenderingDevice.UniformType.Image;
                        accum2Buniform.Binding = 5;
                        accum2Buniform.AddId(_accumulationTextures[view * 3 + 5]);
                        _uniformsArray.Add(accum2Buniform);



                        var depthuniform = new RDUniform();
                        depthuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        depthuniform.Binding = 6;
                        depthuniform.AddId(_nearestSampler);
                        depthuniform.AddId(_resizedDepth);
                        _uniformsArray.Add(depthuniform);


                        var extraNoiseuniform = new RDUniform();
                        extraNoiseuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        extraNoiseuniform.Binding = 7;
                        extraNoiseuniform.AddId(_linearSampler);
                        extraNoiseuniform.AddId(RenderingServer.TextureGetRdTexture(ExtraLargeNoisePatterns.GetRid()));
                        _uniformsArray.Add(extraNoiseuniform);

                        var noiseuniform = new RDUniform();
                        noiseuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        noiseuniform.Binding = 8;
                        noiseuniform.AddId(_linearSampler);
                        noiseuniform.AddId(RenderingServer.TextureGetRdTexture(LargeScaleNoise.GetRid()));
                        _uniformsArray.Add(noiseuniform);
                        
                        var mediumnoiseuniform = new RDUniform();
                        mediumnoiseuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        mediumnoiseuniform.Binding = 9;
                        mediumnoiseuniform.AddId(_linearSampler);
                        mediumnoiseuniform.AddId(RenderingServer.TextureGetRdTexture(MediumScaleNoise.GetRid()));
                        _uniformsArray.Add(mediumnoiseuniform);

                        var smallnoiseuniform = new RDUniform();
                        smallnoiseuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        smallnoiseuniform.Binding = 10;
                        smallnoiseuniform.AddId(_linearSampler);
                        smallnoiseuniform.AddId(RenderingServer.TextureGetRdTexture(SmallScaleNoise.GetRid()));
                        _uniformsArray.Add(smallnoiseuniform);
                        
                        var curlnoiseuniform = new RDUniform();
                        curlnoiseuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        curlnoiseuniform.Binding = 11;
                        curlnoiseuniform.AddId(_linearSampler);
                        curlnoiseuniform.AddId(RenderingServer.TextureGetRdTexture(CurlNoise.GetRid()));
                        _uniformsArray.Add(curlnoiseuniform);

                        var dithernoiseuniform = new RDUniform();
                        dithernoiseuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        dithernoiseuniform.Binding = 12;
                        dithernoiseuniform.AddId(_nearestSampler);
                        dithernoiseuniform.AddId(RenderingServer.TextureGetRdTexture(DitherNoise.GetRid()));
                        _uniformsArray.Add(dithernoiseuniform);


                        var heightgradientuniform = new RDUniform();
                        heightgradientuniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        heightgradientuniform.Binding = 13;
                        heightgradientuniform.AddId(_linearSamplerNoRepeat);
                        heightgradientuniform.AddId(RenderingServer.TextureGetRdTexture(HeightGradient.GetRid()));
                        _uniformsArray.Add(heightgradientuniform);


                        _generalDataBuffer = _rd.UniformBufferCreate(448);
                        var camerauniform = new RDUniform();
                        camerauniform.UniformType = RenderingDevice.UniformType.UniformBuffer;
                        camerauniform.Binding = 14;
                        camerauniform.AddId(_generalDataBuffer);
                        _uniformsArray.Add(camerauniform);

                        _lightDataBuffer = _rd.UniformBufferCreate(384);
                        var lightdatauniform = new RDUniform();
                        lightdatauniform.UniformType = RenderingDevice.UniformType.UniformBuffer;
                        lightdatauniform.Binding = 15;
                        lightdatauniform.AddId(_lightDataBuffer);
                        _uniformsArray.Add(lightdatauniform);

                        _uniformSets[view * 3 + 1] = _rd.UniformSetCreate(_uniformsArray, _shader, 0);


                        var _postpassuniformsArray = new Godot.Collections.Array<RDUniform>();
                        var prepassColorDataUniform = new RDUniform();
                        prepassColorDataUniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        prepassColorDataUniform.Binding = 0;
                        prepassColorDataUniform.AddId(_linearSamplerNoRepeat);
                        prepassColorDataUniform.AddId(_accumulationTextures[view * 3]);
                        _postpassuniformsArray.Add(prepassColorDataUniform);

                        var prepassColorUniform = new RDUniform();
                        prepassColorUniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        prepassColorUniform.Binding = 1;
                        prepassColorUniform.AddId(_linearSamplerNoRepeat);
                        prepassColorUniform.AddId(_accumulationTextures[view * 3 + 1]);
                        _postpassuniformsArray.Add(prepassColorUniform);

                        var postpassColorUniform = new RDUniform();
                        postpassColorUniform.UniformType = RenderingDevice.UniformType.Image;
                        postpassColorUniform.Binding = 2;
                        postpassColorUniform.AddId(colorImage);
                        _postpassuniformsArray.Add(postpassColorUniform);

                        var postpassDepthUniform = new RDUniform();
                        postpassDepthUniform.UniformType = RenderingDevice.UniformType.SamplerWithTexture;
                        postpassDepthUniform.Binding = 3;
                        postpassDepthUniform.AddId(_nearestSampler);
                        postpassDepthUniform.AddId(depthImage);
                        _postpassuniformsArray.Add(postpassDepthUniform);

                        var postpasscamerauniform = new RDUniform();
                        postpasscamerauniform.UniformType = RenderingDevice.UniformType.UniformBuffer;
                        postpasscamerauniform.Binding = 4;
                        postpasscamerauniform.AddId(_generalDataBuffer);
                        _postpassuniformsArray.Add(postpasscamerauniform);

                        var postpasslightdatauniform = new RDUniform();
                        postpasslightdatauniform.UniformType = RenderingDevice.UniformType.UniformBuffer;
                        postpasslightdatauniform.Binding = 5;
                        postpasslightdatauniform.AddId(_lightDataBuffer);
                        _postpassuniformsArray.Add(postpasslightdatauniform);

                        _uniformSets[view * 3 + 2] = _rd.UniformSetCreate(_postpassuniformsArray, _postpass_shader, 0);

                    }
                    LightsUpdated = true;
                }

                using var ms = new System.IO.MemoryStream();
                using var bw = new System.IO.BinaryWriter(ms);

                bw.Write((float)newSize.X);
                bw.Write((float)newSize.Y);
                bw.Write(_largeNoiseScale);
                bw.Write(_mediumNoiseScale);

                bw.Write(CurrentTime);
                bw.Write(_cloudsCoverage);
                bw.Write(_cloudsDensity);
                bw.Write(_cloudsDetailPower);

                bw.Write(_lightingDensity);
                bw.Write(_accumilationDecay);

                var rendersceneData = renderData.GetRenderSceneData();
                var cameraTR = rendersceneData.GetCamTransform();
                var viewProj = rendersceneData.GetCamProjection();


                //_accumulationisA = !_accumulationisA;
                bw.Write(_accumulationisA ? 1.0f : 0.0f);
                bw.Write(0.0f);

                _pushConstants = ms.ToArray();
                _lastSize = size;

                UpdateMatricies(cameraTR, viewProj);
                if (LightsUpdated || DirectionalLightsData.Count == 0)
                {
                    UpdateLights();
                }


                uint prepassxGroups = ((uint)size.X - 1) / 32 + 1;
                uint prepassyGroups = ((uint)size.Y - 1) / 32 + 1;

                uint xGroups = ((uint)size.X - 1) / 32 / resscale + 1;
                uint yGroups = ((uint)size.Y - 1) / 32 / resscale + 1;

                for (uint view = 0; view < viewCount; view++)
                {
                    //GD.Print((uint)_prepasspushConstants.Length);
                    var prepasscomputeList = _rd.ComputeListBegin();
                    _rd.ComputeListBindComputePipeline(prepasscomputeList, _prepass_pipeline);
                    _rd.ComputeListBindUniformSet(prepasscomputeList, _uniformSets[view * 3], 0);
                    _rd.ComputeListSetPushConstant(prepasscomputeList, _prepasspushConstants, (uint)_prepasspushConstants.Length);
                    _rd.ComputeListDispatch(prepasscomputeList, xGroups, yGroups, 1);
                    //_rd.ComputeListAddBarrier(prepasscomputeList);
                    _rd.ComputeListEnd();



                    var computeList = _rd.ComputeListBegin();
                    _rd.ComputeListBindComputePipeline(computeList, _pipeline);
                    _rd.ComputeListBindUniformSet(computeList, _uniformSets[view * 3 + 1], 0);
                    _rd.ComputeListSetPushConstant(computeList, _pushConstants, (uint)_pushConstants.Length);
                    _rd.ComputeListDispatch(computeList, xGroups, yGroups, 1);
                    //_rd.ComputeListAddBarrier(computeList);
                    _rd.ComputeListEnd();


                    var postpasscomputeList = _rd.ComputeListBegin();
                    _rd.ComputeListBindComputePipeline(postpasscomputeList, _postpass_pipeline);
                    _rd.ComputeListBindUniformSet(postpasscomputeList, _uniformSets[view * 3 + 2], 0);
                    _rd.ComputeListSetPushConstant(postpasscomputeList, _postpasspushConstants, (uint)_postpasspushConstants.Length);
                    _rd.ComputeListDispatch(postpasscomputeList, prepassxGroups, prepassyGroups, 1);
                    _rd.ComputeListEnd();
                }
            }
        }
    }


    private Vector2 GetRotationDifferences(Transform3D transformA, Transform3D transformB)
    {
        // Get the forward vectors of each transform
        Vector3 forwardA = -transformA.Basis.Z;
        Vector3 forwardB = -transformB.Basis.Z;

        // Normalize the vectors
        forwardA = forwardA.Normalized();
        forwardB = forwardB.Normalized();

        // Calculate the horizontal (yaw) difference
        Vector3 adjustedforwardA = new Vector3(forwardA.X, 0.0f, forwardA.Z).Normalized();
        Vector3 adjustedforwardB = new Vector3(forwardB.X, 0.0f, forwardB.Z).Normalized();
        Vector3 perp = adjustedforwardA.Cross(adjustedforwardB);

        float yawDifference = adjustedforwardA.AngleTo(adjustedforwardB);
        if (perp.Dot(Vector3.Up) < 0.0f)
        {
            yawDifference *= -1.0f;
        }

        float pitchDifference = new Vector3(0.0f, forwardA.Y, 1.0f).Normalized().AngleTo(new Vector3(0.0f, forwardB.Y, 1.0f).Normalized());

        if (forwardA.Y > forwardB.Y)
        {
            pitchDifference *= -1.0f;
        }

        return new Vector2(yawDifference, pitchDifference);
    }


    private void UpdateMatricies(Transform3D cameraTR, Projection viewProj)
    {
        int idx = 0;

        _filterIndex += 1; //Switch to 4 if your testing the 2x2 bayer pattern.
        if (_filterIndex >= 16)
        {
            _filterIndex = 0;
        }

        // Camera matrix (16 floats)
        _generalDataFloats[idx++] = cameraTR.Basis.X.X;
        _generalDataFloats[idx++] = cameraTR.Basis.X.Y;
        _generalDataFloats[idx++] = cameraTR.Basis.X.Z;
        _generalDataFloats[idx++] = 0;

        _generalDataFloats[idx++] = cameraTR.Basis.Y.X;
        _generalDataFloats[idx++] = cameraTR.Basis.Y.Y;
        _generalDataFloats[idx++] = cameraTR.Basis.Y.Z;
        _generalDataFloats[idx++] = 0;

        _generalDataFloats[idx++] = cameraTR.Basis.Z.X;
        _generalDataFloats[idx++] = cameraTR.Basis.Z.Y;
        _generalDataFloats[idx++] = cameraTR.Basis.Z.Z;
        _generalDataFloats[idx++] = 0;

        _generalDataFloats[idx++] = cameraTR.Origin.X;
        _generalDataFloats[idx++] = cameraTR.Origin.Y;
        _generalDataFloats[idx++] = cameraTR.Origin.Z;
        _generalDataFloats[idx++] = 1.0f;

        // Camera matrix (previous frame or current if not available)
        if (_lastViewMat.HasValue)
        {
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.X.X;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.X.Y;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.X.Z;
            _generalDataFloats[idx++] = 0;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.Y.X;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.Y.Y;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.Y.Z;
            _generalDataFloats[idx++] = 0;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.Z.X;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.Z.Y;
            _generalDataFloats[idx++] = _lastViewMat.Value.Basis.Z.Z;
            _generalDataFloats[idx++] = 0;
            _generalDataFloats[idx++] = _lastViewMat.Value.Origin.X;
            _generalDataFloats[idx++] = _lastViewMat.Value.Origin.Y;
            _generalDataFloats[idx++] = _lastViewMat.Value.Origin.Z;
            _generalDataFloats[idx++] = 1.0f;
        }
        else
        {
            _generalDataFloats[idx++] = cameraTR.Basis.X.X;
            _generalDataFloats[idx++] = cameraTR.Basis.X.Y;
            _generalDataFloats[idx++] = cameraTR.Basis.X.Z;
            _generalDataFloats[idx++] = 0;
            _generalDataFloats[idx++] = cameraTR.Basis.Y.X;
            _generalDataFloats[idx++] = cameraTR.Basis.Y.Y;
            _generalDataFloats[idx++] = cameraTR.Basis.Y.Z;
            _generalDataFloats[idx++] = 0;
            _generalDataFloats[idx++] = cameraTR.Basis.Z.X;
            _generalDataFloats[idx++] = cameraTR.Basis.Z.Y;
            _generalDataFloats[idx++] = cameraTR.Basis.Z.Z;
            _generalDataFloats[idx++] = 0;
            _generalDataFloats[idx++] = cameraTR.Origin.X;
            _generalDataFloats[idx++] = cameraTR.Origin.Y;
            _generalDataFloats[idx++] = cameraTR.Origin.Z;
            _generalDataFloats[idx++] = 1.0f;
        }

        // Projection matrix (16 floats)
        _generalDataFloats[idx++] = viewProj.X.X;
        _generalDataFloats[idx++] = viewProj.X.Y;
        _generalDataFloats[idx++] = viewProj.X.Z;
        _generalDataFloats[idx++] = viewProj.X.W;

        _generalDataFloats[idx++] = viewProj.Y.X;
        _generalDataFloats[idx++] = viewProj.Y.Y;
        _generalDataFloats[idx++] = viewProj.Y.Z;
        _generalDataFloats[idx++] = viewProj.Y.W;

        _generalDataFloats[idx++] = viewProj.Z.X;
        _generalDataFloats[idx++] = viewProj.Z.Y;
        _generalDataFloats[idx++] = viewProj.Z.Z;
        _generalDataFloats[idx++] = viewProj.Z.W;

        _generalDataFloats[idx++] = viewProj.W.X;
        _generalDataFloats[idx++] = viewProj.W.Y;
        _generalDataFloats[idx++] = viewProj.W.Z;
        _generalDataFloats[idx++] = viewProj.W.W;

        // Projection matrix (previous frame or current if not available)
        if (_lastProjectionMat.HasValue)
        {
            _generalDataFloats[idx++] = _lastProjectionMat.Value.X.X;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.X.Y;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.X.Z;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.X.W;

            _generalDataFloats[idx++] = _lastProjectionMat.Value.Y.X;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.Y.Y;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.Y.Z;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.Y.W;

            _generalDataFloats[idx++] = _lastProjectionMat.Value.Z.X;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.Z.Y;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.Z.Z;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.Z.W;

            _generalDataFloats[idx++] = _lastProjectionMat.Value.W.X;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.W.Y;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.W.Z;
            _generalDataFloats[idx++] = _lastProjectionMat.Value.W.W;
        }
        else
        {
            _generalDataFloats[idx++] = viewProj.X.X;
            _generalDataFloats[idx++] = viewProj.X.Y;
            _generalDataFloats[idx++] = viewProj.X.Z;
            _generalDataFloats[idx++] = viewProj.X.W;

            _generalDataFloats[idx++] = viewProj.Y.X;
            _generalDataFloats[idx++] = viewProj.Y.Y;
            _generalDataFloats[idx++] = viewProj.Y.Z;
            _generalDataFloats[idx++] = viewProj.Y.W;

            _generalDataFloats[idx++] = viewProj.Z.X;
            _generalDataFloats[idx++] = viewProj.Z.Y;
            _generalDataFloats[idx++] = viewProj.Z.Z;
            _generalDataFloats[idx++] = viewProj.Z.W;

            _generalDataFloats[idx++] = viewProj.W.X;
            _generalDataFloats[idx++] = viewProj.W.Y;
            _generalDataFloats[idx++] = viewProj.W.Z;
            _generalDataFloats[idx++] = viewProj.W.W;
        }

        _lastProjectionMat = viewProj;
        _lastViewMat = cameraTR;
        _accumulationisA = !_accumulationisA;

        // Simple data (44 floats)
        _generalDataFloats[idx++] = ExtraLargeScaleCloudsPosition.X;
        _generalDataFloats[idx++] = ExtraLargeScaleCloudsPosition.Y;
        _generalDataFloats[idx++] = ExtraLargeScaleCloudsPosition.Z;
        _generalDataFloats[idx++] = ExtraLargeNoiseScale;

        _generalDataFloats[idx++] = LargeScaleCloudsPosition.X;
        _generalDataFloats[idx++] = LargeScaleCloudsPosition.Y;
        _generalDataFloats[idx++] = LargeScaleCloudsPosition.Z;
        _generalDataFloats[idx++] = LightingSharpness;

        _generalDataFloats[idx++] = MediumScaleCloudsPosition.X;
        _generalDataFloats[idx++] = MediumScaleCloudsPosition.Y;
        _generalDataFloats[idx++] = MediumScaleCloudsPosition.Z;
        _generalDataFloats[idx++] = LightingTravelDistance;

        _generalDataFloats[idx++] = DetailCloudsPosition.X;
        _generalDataFloats[idx++] = DetailCloudsPosition.Y;
        _generalDataFloats[idx++] = DetailCloudsPosition.Z;
        _generalDataFloats[idx++] = AtmosphericDensity;

        _generalDataFloats[idx++] = CloudAmbientColor.R * CloudAmbientTint.R;
        _generalDataFloats[idx++] = CloudAmbientColor.G * CloudAmbientTint.G;
        _generalDataFloats[idx++] = CloudAmbientColor.B * CloudAmbientTint.B;
        _generalDataFloats[idx++] = CloudAmbientColor.A * CloudAmbientTint.A;

        _generalDataFloats[idx++] = AmbientOcclusionColor.R;
        _generalDataFloats[idx++] = AmbientOcclusionColor.G;
        _generalDataFloats[idx++] = AmbientOcclusionColor.B;
        _generalDataFloats[idx++] = AmbientOcclusionColor.A;

        _generalDataFloats[idx++] = AtmosphereColor.R;
        _generalDataFloats[idx++] = AtmosphereColor.G;
        _generalDataFloats[idx++] = AtmosphereColor.B;
        _generalDataFloats[idx++] = AtmosphereColor.A;

        _generalDataFloats[idx++] = _smallNoiseScale;
        _generalDataFloats[idx++] = _minStepDistance;
        _generalDataFloats[idx++] = _maxStepDistance;
        _generalDataFloats[idx++] = _LODBias;

        _generalDataFloats[idx++] = _cloudsSharpness;
        _generalDataFloats[idx++] = (float)DirectionalLightsData.Count / 2;
        _generalDataFloats[idx++] = (float)PointLightsData.Count / 2;
        _generalDataFloats[idx++] = _cloudsAnisotropy;

        _generalDataFloats[idx++] = _cloudFloor;
        _generalDataFloats[idx++] = _cloudCeiling;
        _generalDataFloats[idx++] = (float)_maxStepCount;
        _generalDataFloats[idx++] = (float)_maxLightingSteps;

        _generalDataFloats[idx++] = (float)_filterIndex;
        _generalDataFloats[idx++] = (float)_blurPower;
        _generalDataFloats[idx++] = (float)_blurQuality;
        _generalDataFloats[idx++] = (float)_curlNoiseStrength;

        _generalDataFloats[idx++] = WindDirection.X;
        _generalDataFloats[idx++] = WindDirection.Z;
        _generalDataFloats[idx++] = 0.0f;
        _generalDataFloats[idx++] = 0.0f;

        // Copy all floats to the byte buffer
        Buffer.BlockCopy(_generalDataFloats, 0, _generalData, 0, _generalDataFloats.Length * 4);

        _rd.BufferUpdate(_generalDataBuffer, 0, (uint)_generalData.Length, _generalData);
    }

    private void UpdateLights()
    {
        LightsUpdated = false;

        if (DirectionalLightsData.Count == 0)
        {
            DirectionalLightsData.Add(new Vector4(0.5f, 1.0f, 0.5f, 16.0f));
            DirectionalLightsData.Add(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
        }

        int idx = 0;
        int directionalLightCount = Mathf.Min(DirectionalLightsData.Count, 8);
        for (int i = 0; i < directionalLightCount; i++)
        {
            _lightDataFloats[idx++] = DirectionalLightsData[i].X;
            _lightDataFloats[idx++] = DirectionalLightsData[i].Y;
            _lightDataFloats[idx++] = DirectionalLightsData[i].Z;
            _lightDataFloats[idx++] = DirectionalLightsData[i].W;
        }
        idx = 32;
        int pointLightCount = Mathf.Min(PointLightsData.Count, 16);
        for (int i = 0; i < pointLightCount; i++)
        {
            _lightDataFloats[idx++] = PointLightsData[i].X;
            _lightDataFloats[idx++] = PointLightsData[i].Y;
            _lightDataFloats[idx++] = PointLightsData[i].Z;
            _lightDataFloats[idx++] = PointLightsData[i].W;
        }

        // Copy all floats to the byte buffer
        Buffer.BlockCopy(_lightDataFloats, 0, _lightData, 0, _lightDataFloats.Length * 4);

        _rd.BufferUpdate(_lightDataBuffer, 0, (uint)_lightData.Length, _lightData);
    }
}
