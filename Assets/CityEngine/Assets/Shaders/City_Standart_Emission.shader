// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "City/Standart_Emission"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,0)
		_EmissionColor("EmissionColor", Color) = (1,1,1,0)
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			half filler;
		};

		uniform half4 _Color;
		uniform half4 _EmissionColor;
		uniform half _NightEmission;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Color.rgb;
			o.Emission = ( _EmissionColor * _NightEmission ).rgb;
			o.Smoothness = 0.3;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
7;11;1906;1000;1256.778;454.9837;1.324988;True;True
Node;AmplifyShaderEditor.RangedFloatNode;5;-592.6783,0.8534203;Inherit;False;Global;_NightEmission;_NightEmission;1;0;Create;True;0;0;False;0;1;0.05257852;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;-651.67,-167.7249;Inherit;False;Property;_EmissionColor;EmissionColor;1;0;Create;True;0;0;False;0;1,1,1,0;0.5294118,0.5204546,0.1215686,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-373.5905,85.50002;Inherit;False;Constant;_Smoothness;Smoothness;1;0;Create;True;0;0;False;0;0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-471.6374,-340.4105;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,0;0,0.1058824,0.2627451,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-106.7025,-251.3894;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-232.67,-11.72485;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;City/Standart_Emission;False;False;False;False;False;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;1;5;0
WireConnection;6;0;7;0
WireConnection;6;1;5;0
WireConnection;0;0;2;0
WireConnection;0;2;6;0
WireConnection;0;4;3;0
ASEEND*/
//CHKSM=A31A0426789010006496E2967C9748B68FD7659B