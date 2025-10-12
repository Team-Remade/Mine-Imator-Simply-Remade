#[vertex]
#version 450

#define MSAA_ENABLED 1

#include "./CloudsInc.txt"

#define STAGE_VERTEX
#include "./SunshineCloudsDisplay.rast"
#undef STAGE_VERTEX

#[fragment]
#version 450

#define MSAA_ENABLED 1

#include "./CloudsInc.txt"
#include "./SunshineCloudsDisplay.rast"
