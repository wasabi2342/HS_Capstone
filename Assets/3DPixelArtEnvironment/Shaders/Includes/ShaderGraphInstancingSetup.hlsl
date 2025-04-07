/*
Copyright (c) 2016 Unity Technologies
Copyright (c) Voyage (Edits for local matrices, instanceData struct and access function for normal)

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

// https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/CGIncludes/UnityStandardParticleInstancing.cginc used for reference
#ifndef SHADER_GRAPH_INSTANCING
#define SHADER_GRAPH_INSTANCING

#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
struct InstanceData
{
    float4x4 TRS;
    float3 normal;
};

StructuredBuffer<InstanceData> _InstanceData;

#if UNITY_ANY_INSTANCING_ENABLED

void vertInstancingMatrices(inout float4x4 objectToWorld, inout float4x4 worldToObject)
{
    InstanceData data = _InstanceData[unity_InstanceID];

    // transform matrix    
    objectToWorld = mul(objectToWorld, mul(_LocalToWorld, data.TRS));
    
    // inverse transform matrix
    float3x3 w2oRotation;
    w2oRotation[0] = objectToWorld[1].yzx * objectToWorld[2].zxy - objectToWorld[1].zxy * objectToWorld[2].yzx;
    w2oRotation[1] = objectToWorld[0].zxy * objectToWorld[2].yzx - objectToWorld[0].yzx * objectToWorld[2].zxy;
    w2oRotation[2] = objectToWorld[0].yzx * objectToWorld[1].zxy - objectToWorld[0].zxy * objectToWorld[1].yzx;

    float det = dot(objectToWorld[0].xyz, w2oRotation[0]);

    w2oRotation = transpose(w2oRotation);

    w2oRotation *= rcp(det);

    float3 w2oPosition = mul(w2oRotation, -objectToWorld._14_24_34);

    worldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
    worldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
    worldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
    worldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);
}

void vertInstancingSetup()
{
    vertInstancingMatrices(unity_ObjectToWorld, unity_WorldToObject);
}

#endif
#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
#include "UnityIndirect.cginc"
void instanceIDShaderGraph_float(float3 In, out float3 Out, out float InstanceID)
{
    InitIndirectDrawArgs(0);
    InstanceID = 0;
    #ifndef SHADERGRAPH_PREVIEW
    #if UNITY_ANY_INSTANCING_ENABLED
	InstanceID = unity_InstanceID;
    #endif
    #endif
    Out = In;
}

void instanceShaderGraphSetup_float(float3 In, out float3 Out)
{
    Out = In;
}

void instanceNormal_float(float InstanceID, out float3 Normal)
{
    Normal = float3(0, 0, 0);
    #if UNITY_ANY_INSTANCING_ENABLED
	    InstanceData instanceData = _InstanceData[InstanceID];
        Normal = instanceData.normal;
    #endif
}
#endif
