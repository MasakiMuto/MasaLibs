
float4x4 Projection;
float4x4 ViewProjection;
float4x4 World;
float Time;
float2 TargetSize;//描画先のwidth,height
float4 Color;
float2 Offset;//スクリーン座標で加えるオフセット

Texture2D<float4> Texture;

SamplerState texsampler;

struct VertexShaderInput
{
	float3 Pos : POSITION0;
	float3 Vel : POSITION1;
	float3 Acc : POSITION2;
	float3 Alpha : COLOR0;
	float2 R : PSIZE0;
	float Time : BLENDWEIGHT0;
	float2 Angle : POSITION3;
	float3 Color : COLOR1;
};

struct GSInput
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
	float Angle : POSITION1;
	float2 Radius : PSIZE0;
};

struct VertexShaderOutput
{
	float4 Pos : SV_POSITION;
	float4 Color : COLOR0;
	float2 tex : TEXCOORD0;

};

GSInput VS2D(VertexShaderInput input)
{
	GSInput output;
	float dtime = Time - input.Time;
	float2 p;
	output.Color = float4(input.Color, input.Alpha.x + input.Alpha.y * dtime + input.Alpha.z * (0.5 * dtime * dtime));
	p = input.Pos.xy + input.Vel.xy * dtime + input.Acc.xy * (0.5 * dtime * dtime) + Offset;//中心点のスクリーン座標演算
	output.Pos = float4(p.x, p.y, 0, output.Color.a > 0);//Alpha <= 0ならwを0にして表示しない
	output.Radius = input.R;
	output.Angle = input.Angle.x + input.Angle.y * dtime;
	return output;
}

//VertexShaderOutput VS3D(VertexShaderInput input)
//{
//	VertexShaderOutput output;
//	float dtime = Time - input.Time;
//	float sn, cs;
//	sincos(input.Angle.x + input.Angle.y * dtime, sn, cs);
//	//output.tex = input.Tex;
//	output.Color = float4(input.Color, input.Alpha.x + input.Alpha.y * dtime + input.Alpha.z * (0.5 * dtime * dtime));
//	//input.R *= output.Alpha > 0;
//	input.Pos = input.Pos + input.Vel * dtime + input.Acc * (0.5 * dtime * dtime);
//	//input.Pos.x -= TargetSize.x * 0.5;
//	//input.Pos.y -= TargetSize.y * 0.5;
//	//input.Pos.y *= -1;
//	//output.Pos.xy += (output.tex.xy * 2 - 1) * input.R * 0.5;
//	output.Pos = mul(float4(input.Pos, 1), ViewProjection);
//	output.Pos.xy += mul((output.tex.xy * 2 - 1) * input.R * float2( Projection._m00, Projection._m11) * 0.5,  float2x2(cs, sn, -sn, cs));	
//	
//	return output;
//}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET
{
	float4 t = Texture.Sample(texsampler, input.tex);
	t.rgb *= input.Color.rgb;
	return  t * (Color * input.Color.a);
}

float4 ScreenToWorld(float4 v){
	v.xy /= TargetSize * 0.5;
	v.xy -= 1;
	v.y *= -1;
	return v;
}


[maxvertexcount(6)]
void GS(point GSInput input[1], inout TriangleStream<VertexShaderOutput> stream)
{
	VertexShaderOutput p;
	float2 tex[4] = { float2(0, 0), float2(1, 0), float2(1, 1), float2(0, 1) };
	int index[6] = { 0, 1, 2, 0, 2, 3 };
	float sn, cs;
	sincos(input[0].Angle, sn, cs);
	p.Color = input[0].Color;
	for (int i = 0; i < 6; i++)
	{
		p.tex = tex[index[i]];
		p.Pos = input[0].Pos + float4(mul((p.tex * 2 - 1) * input[0].Radius, float2x2(cs, sn, -sn, cs)), 0, 0);
		p.Pos = mul(p.Pos, World);
		p.Pos = ScreenToWorld(p.Pos);


		stream.Append(p);
		if (i == 2 || i == 5){
			stream.RestartStrip();
		}
	}
}


float4 MulPixel(float4 color : COLOR0, float2 tex : TEXCOORD0) : SV_TARGET
{
	float4 t = Texture.Sample(texsampler, tex) * color;
	return t + (1 - t.a);
}

technique Technique1
{
	pass Game2D
	{
		Profile = 10;
		VertexShader = VS2D;
		GeometryShader = GS;
		PixelShader = PixelShaderFunction;
	}

	/*pass Back3D
	{
	Profile = 11;
	VertexShader = VS3D;
	PixelShader = PixelShaderFunction;
	}*/
}
