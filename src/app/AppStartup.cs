using System;
using System.IO;
using MineImatorSimplyRemade.app.dialogues;
using MineImatorSimplyRemade.app.libraries;
using MineImatorSimplyRemade.project;
using MineImatorSimplyRemade.utility.vertex;

namespace MineImatorSimplyRemade.app;

public static class AppStartup
{
    public static bool Start()
    {
        var startupError = true;

        if (!Lib.Start())
            return false;

        if (!File.Exists(Macro.LegacyFile))
            return MissingFile.ShowMessage(Macro.LegacyFile);

        if (!File.Exists(Macro.LanguageFile))
            return MissingFile.ShowMessage(Macro.LanguageFile);

        VertexFormat.Start();

        //TODO: Shader start

        Legacy.Start();

        Lists();

        startupError = false;

        return startupError;
    }

    private static void Lists()
    {
        string[] values =
        [
            "POS_X",
            "POS_Y",
            "POS_Z",
            "ROT_X",
            "ROT_Y",
            "ROT_Z",
            "SCA_X",
            "SCA_Y",
            "SCA_Z",
            "BEND_ANGLE",
            "BEND_ANGLE_X",
            "BEND_ANGLE_Y",
            "BEND_ANGLE_Z",
            "ALPHA",
            "RGB_ADD",
            "RGB_SUB",
            "RGB_MUL",
            "HSB_ADD",
            "HSB_SUB",
            "HSB_MUL",
            "MIX_COLOR",
            "GLOW_COLOR",
            "MIX_PERCENT",
            "EMISSIVE",
            "METALLIC",
            "ROUGHNESS",
            "SUBSURFACE",
            "SUBSURFACE_RADIUS_RED",
            "SUBSURFACE_RADIUS_GREEN",
            "SUBSURFACE_RADIUS_BLUE",
            "SUBSURFACE_COLOR",
            "WIND_INFLUENCE",
            "SPAWN",
            "FREEZE",
            "CLEAR",
            "CUSTOM_SEED",
            "SEED",
            "ATTRACTOR",
            "FORCE",
            "FORCE_DIRECTIONAL",
            "FORCE_VORTEX",
            "LIGHT_COLOR",
            "LIGHT_STRENGTH",
            "LIGHT_SPECULAR_STRENGTH",
            "LIGHT_SIZE",
            "LIGHT_RANGE",
            "LIGHT_FADE_SIZE",
            "LIGHT_SPOT_RADIUS",
            "LIGHT_SPOT_SHARPNESS",
            "CAM_FOV",
            "CAM_BLADE_AMOUNT",
            "CAM_BLADE_ANGLE",
            "CAM_LIGHT_MANAGEMENT",
            "CAM_TONEMAPPER",
            "CAM_EXPOSURE",
            "CAM_GAMMA",
            "CAM_ROTATE",
            "CAM_ROTATE_DISTANCE",
            "CAM_ROTATE_ANGLE_XY",
            "CAM_ROTATE_ANGLE_Z",
            "CAM_SHAKE",
            "CAM_SHAKE_MODE",
            "CAM_SHAKE_STRENGTH_X",
            "CAM_SHAKE_STRENGTH_Y",
            "CAM_SHAKE_STRENGTH_Z",
            "CAM_SHAKE_SPEED_X",
            "CAM_SHAKE_SPEED_Y",
            "CAM_SHAKE_SPEED_Z",
            "CAM_DOF",
            "CAM_DOF_DEPTH",
            "CAM_DOF_RANGE",
            "CAM_DOF_FADE_SIZE",
            "CAM_DOF_BLUR_SIZE",
            "CAM_DOF_BLUR_RATIO",
            "CAM_DOF_BIAS",
            "CAM_DOF_THRESHOLD",
            "CAM_DOF_GAIN",
            "CAM_DOF_FRINGE",
            "CAM_DOF_FRINGE_ANGLE_RED",
            "CAM_DOF_FRINGE_ANGLE_GREEN",
            "CAM_DOF_FRINGE_ANGLE_BLUE",
            "CAM_DOF_FRINGE_RED",
            "CAM_DOF_FRINGE_GREEN",
            "CAM_DOF_FRINGE_BLUE",
            "CAM_BLOOM",
            "CAM_BLOOM_THRESHOLD",
            "CAM_BLOOM_INTENSITY",
            "CAM_BLOOM_RADIUS",
            "CAM_BLOOM_RATIO",
            "CAM_BLOOM_BLEND",
            "CAM_LENS_DIRT",
            "CAM_LENS_DIRT_BLOOM",
            "CAM_LENS_DIRT_GLOW",
            "CAM_LENS_DIRT_RADIUS",
            "CAM_LENS_DIRT_INTENSITY",
            "CAM_LENS_DIRT_POWER",
            "CAM_COLOR_CORRECTION",
            "CAM_CONTRAST",
            "CAM_BRIGHTNESS",
            "CAM_SATURATION",
            "CAM_VIBRANCE",
            "CAM_COLOR_BURN",
            "CAM_GRAIN",
            "CAM_GRAIN_STRENGTH",
            "CAM_GRAIN_SATURATION",
            "CAM_GRAIN_SIZE",
            "CAM_VIGNETTE",
            "CAM_VIGNETTE_RADIUS",
            "CAM_VIGNETTE_SOFTNESS",
            "CAM_VIGNETTE_STRENGTH",
            "CAM_VIGNETTE_COLOR",
            "CAM_CA",
            "CAM_CA_BLUR_AMOUNT",
            "CAM_CA_DISTORT_CHANNELS",
            "CAM_CA_RED_OFFSET",
            "CAM_CA_GREEN_OFFSET",
            "CAM_CA_BLUE_OFFSET",
            "CAM_DISTORT",
            "CAM_DISTORT_REPEAT",
            "CAM_DISTORT_ZOOM_AMOUNT",
            "CAM_DISTORT_AMOUNT",
            "CAM_SIZE_USE_PROJECT",
            "CAM_SIZE_KEEP_ASPECT_RATIO",
            "CAM_WIDTH",
            "CAM_HEIGHT",
            "BG_IMAGE_SHOW",
            "BG_IMAGE_ROTATION",
            "BG_SKY_MOON_PHASE",
            "BG_SKY_TIME",
            "BG_SKY_ROTATION",
            "BG_SUNLIGHT_STRENGTH",
            "BG_SUNLIGHT_ANGLE",
            "BG_SKY_SUN_ANGLE",
            "BG_SKY_SUN_SCALE",
            "BG_SKY_MOON_ANGLE",
            "BG_SKY_MOON_SCALE",
            "BG_TWILIGHT",
            "BG_SKY_CLOUDS_SHOW",
            "BG_SKY_CLOUDS_SPEED",
            "BG_SKY_CLOUDS_HEIGHT",
            "BG_SKY_CLOUDS_OFFSET",
            "BG_GROUND_SHOW",
            "BG_GROUND_SLOT",
            "BG_BIOME",
            "BG_SKY_COLOR",
            "BG_SKY_CLOUDS_COLOR",
            "BG_SUNLIGHT_COLOR",
            "BG_AMBIENT_COLOR",
            "BG_NIGHT_COLOR",
            "BG_GRASS_COLOR",
            "BG_FOLIAGE_COLOR",
            "BG_WATER_COLOR",
            "BG_LEAVES_OAK_COLOR",
            "BG_LEAVES_SPRUCE_COLOR",
            "BG_LEAVES_BIRCH_COLOR",
            "BG_LEAVES_JUNGLE_COLOR",
            "BG_LEAVES_ACACIA_COLOR",
            "BG_LEAVES_DARK_OAK_COLOR",
            "BG_LEAVES_MANGROVE_COLOR",
            "BG_FOG_SHOW",
            "BG_FOG_SKY",
            "BG_FOG_CUSTOM_COLOR",
            "BG_FOG_COLOR",
            "BG_FOG_CUSTOM_OBJECT_COLOR",
            "BG_FOG_OBJECT_COLOR",
            "BG_FOG_DISTANCE",
            "BG_FOG_SIZE",
            "BG_FOG_HEIGHT",
            "BG_WIND",
            "BG_WIND_SPEED",
            "BG_WIND_STRENGTH",
            "BG_WIND_DIRECTION",
            "BG_WIND_DIRECTIONAL_SPEED",
            "BG_WIND_DIRECTIONAL_STRENGTH",
            "BG_TEXTURE_ANI_SPEED",
            "TEXTURE_OBJ",
            "TEXTURE_MATERIAL_OBJ",
            "TEXTURE_NORMAL_OBJ",
            "SOUND_OBJ",
            "SOUND_VOLUME",
            "SOUND_PITCH",
            "SOUND_START",
            "SOUND_END",
            "TEXT",
            "TEXT_FONT",
            "TEXT_HALIGN",
            "TEXT_VALIGN",
            "TEXT_AA",
            "TEXT_OUTLINE",
            "TEXT_OUTLINE_COLOR",
            "CUSTOM_ITEM_SLOT",
            "ITEM_SLOT",
            "ITEM_NAME",
            "PATH_OBJ",
            "PATH_OFFSET",
            "PATH_POINT_ANGLE",
            "PATH_POINT_SCALE",
            "IK_TARGET",
            "IK_BLEND",
            "IK_TARGET_ANGLE",
            "IK_ANGLE_OFFSET",
            "VISIBLE",
            "TRANSITION",
            "EASE_IN_X",
            "EASE_IN_Y",
            "EASE_OUT_X",
            "EASE_OUT_Y"
        ];

        foreach (var s in values)
        {
            GlobalVar.ValueNames.Add(s);
        }

        for (var i = Array.IndexOf(values, "CAM_FOV"); i <= Array.IndexOf(values, "CAM_HEIGHT"); i++)
        {
            GlobalVar.CameraValues.Add(values[i]);
        }
    }
}