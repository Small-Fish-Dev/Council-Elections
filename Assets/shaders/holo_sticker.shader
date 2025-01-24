FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    VrForward();
    Depth();
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"
	CreateInputTexture2D( HoloGradient, Srgb, 8, "", "_color", "Holographic,10/10", Default3( 1.0, 1.0, 1.0 ) );
	Texture2D g_tHoloGradient < Channel(RGB, Box(HoloGradient), Srgb); OutputFormat(BC7); SrgbRead(true); > ;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );
		float normalDotView = dot(m.Normal, g_vCameraDirWs);
		normalDotView = (normalDotView * 0.5) + 1.0;
		normalDotView = pow(normalDotView, 1.2);
		float3 gradientColor = g_tHoloGradient.Sample(g_sAniso, normalDotView.xx);

		m.Albedo *= gradientColor;

		return ShadingModelStandard::Shade( i, m );
	}
}
