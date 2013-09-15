
float4x4 Projection;
float4x4 ViewProjection;
float Time;
float2 TargetSize;//�`����width,height
float4 Color;
float2 Offset;//�X�N���[�����W�ŉ�����I�t�Z�b�g
sampler texsampler : register(s0);

struct VertexShaderInput
{
	float3 Pos : POSITION0;
	float3 Vel : POSITION1;
	float3 Acc : POSITION2;
	float3 Alpha : COLOR0;
	float2 R : PSIZE0;
	float Time : BLENDWEIGHT0;
	//float Index : TEXCOORD0;
	float2 Tex : TEXCOORD0;
	float2 Angle : POSITION3;
	float3 Color : COLOR1;
};

struct VertexShaderOutput
{
	float4 Pos : POSITION0;
	float4 Color : COLOR0;
	float2 tex : TEXCOORD0;

};

VertexShaderOutput VS2D(VertexShaderInput input)
{
	VertexShaderOutput output;
	float dtime = Time - input.Time;
	float2 p;
	float sn, cs;
	sincos(input.Angle.x + input.Angle.y * dtime, sn, cs);
	output.Color = float4(input.Color, input.Alpha.x + input.Alpha.y * dtime + input.Alpha.z * (0.5 * dtime * dtime));
	output.tex = input.Tex;
	p = input.Pos.xy + input.Vel.xy * dtime + input.Acc.xy * (0.5 * dtime * dtime) + Offset;//���S�_�̃X�N���[�����W���Z
	p += mul((output.tex * 2 - 1) * input.R, float2x2(cs, sn, -sn, cs));//�����`�̊e���_�̍��W
	p /= TargetSize * 0.5;//3D���W 0~2
	p -= 1;//-1~+1
	output.Pos = float4(p.x, -p.y, 0, output.Color.a > 0);//Alpha <= 0�Ȃ�w��0�ɂ��ĕ\�����Ȃ�
	
	return output;
}

VertexShaderOutput VS3D(VertexShaderInput input)
{
	VertexShaderOutput output;
	float dtime = Time - input.Time;
	float sn, cs;
	sincos(input.Angle.x + input.Angle.y * dtime, sn, cs);
	output.tex = input.Tex;
	output.Color = float4(input.Color, input.Alpha.x + input.Alpha.y * dtime + input.Alpha.z * (0.5 * dtime * dtime));
	//input.R *= output.Alpha > 0;
	input.Pos = input.Pos + input.Vel * dtime + input.Acc * (0.5 * dtime * dtime);
	//input.Pos.x -= TargetSize.x * 0.5;
	//input.Pos.y -= TargetSize.y * 0.5;
	//input.Pos.y *= -1;
	//output.Pos.xy += (output.tex.xy * 2 - 1) * input.R * 0.5;
	output.Pos = mul(float4(input.Pos, 1), ViewProjection);
	output.Pos.xy += mul((output.tex.xy * 2 - 1) * input.R * float2( Projection._m00, Projection._m11) * 0.5,  float2x2(cs, sn, -sn, cs));	
	
	return output;
}

float4 PixelShaderFunction(float4 color : COLOR0, float2 tex : TEXCOORD0) : COLOR0
{
	float4 t = tex2D(texsampler, tex);
	t.rgb *= color.rgb;
	//t *= alpha;
	return  t * (Color * color.a);
}

float4 MulPixel(float4 color : COLOR0, float2 tex : TEXCOORD0) : COLOR0
{
	float4 t = tex2D(texsampler, tex) * color;
	return t + (1 - t.a);
}

technique Technique1
{
	pass Game2D
	{
		VertexShader = compile vs_3_0 VS2D();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}

	pass Back3D
	{
		VertexShader = compile vs_3_0 VS3D();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
