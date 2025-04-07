// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TidalFlask/Doublesided Lit No Normal"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_BaseTexture("Base Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGPROGRAM
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half ASEVFace : VFACE;
			float2 uv_texcoord;
		};

		uniform sampler2D _BaseTexture;
		uniform float4 _BaseTexture_ST;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 switchResult11 = (((i.ASEVFace>0)?(float3(0,0,1)):(float3(0,0,-1))));
			o.Normal = switchResult11;
			float2 uv_BaseTexture = i.uv_texcoord * _BaseTexture_ST.xy + _BaseTexture_ST.zw;
			float4 tex2DNode1 = tex2D( _BaseTexture, uv_BaseTexture );
			o.Albedo = tex2DNode1.rgb;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18912
977;176;1694;1176;2311.753;1063.355;1.709505;True;False
Node;AmplifyShaderEditor.Vector3Node;20;-996.6805,-17.18365;Inherit;False;Constant;_Vector2;Vector 2;5;0;Create;True;0;0;0;False;0;False;0,0,-1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;21;-999.2805,-187.7836;Inherit;False;Constant;_Vector0;Vector 0;5;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;1;-742.145,96.28124;Inherit;True;Property;_BaseTexture;Base Texture;1;0;Create;True;0;0;0;False;0;False;-1;71e786286eed7414e9580af36b608761;71e786286eed7414e9580af36b608761;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwitchByFaceNode;11;-646.8792,-185.3164;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-194.488,-136.1415;Float;False;True;-1;6;;0;0;Standard;TidalFlask/Doublesided Lit No Normal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;0;21;0
WireConnection;11;1;20;0
WireConnection;0;0;1;0
WireConnection;0;1;11;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=5212D90E830DEEC8040151CD20F416CE39DA262D