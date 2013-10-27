using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit.Graphics;
/*
namespace Masa.Lib.XNA
{
	/// <summary>
	/// Alpha, Additive, Multiply, Subtract
	/// </summary>
	public enum BlendMode
	{
		Alpha,
		Add,
		Mul,
		Sub
	}

	public static class BlendStateManager
	{
		static readonly BlendState[] blendStates;

		static BlendStateManager()
		{
			blendStates = new BlendState[Enum.GetValues(typeof(BlendMode)).Length];
			blendStates[(int)BlendMode.Alpha] = BlendState.AlphaBlend;
			blendStates[(int)BlendMode.Add] = new BlendState()
			{
				AlphaBlendFunction = BlendFunction.Add,
				ColorBlendFunction = BlendFunction.Add,
				AlphaDestinationBlend = Blend.One,
				ColorDestinationBlend = Blend.One,
				AlphaSourceBlend = Blend.SourceAlpha,
				ColorSourceBlend = Blend.SourceAlpha,
			};
			blendStates[(int)BlendMode.Mul] = new BlendState()
			{
				ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
				ColorBlendFunction = BlendFunction.Add,
				AlphaBlendFunction = BlendFunction.Add,
				ColorDestinationBlend = Blend.SourceColor,
				AlphaDestinationBlend = Blend.SourceColor,
				ColorSourceBlend = Blend.Zero,
				AlphaSourceBlend = Blend.Zero
			};
			blendStates[(int)BlendMode.Sub] = new BlendState()
			{
				ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
				AlphaBlendFunction = BlendFunction.ReverseSubtract,
				ColorBlendFunction = BlendFunction.ReverseSubtract,
				AlphaDestinationBlend = Blend.One,
				ColorDestinationBlend = Blend.One,
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
			};
		}

		public static BlendState GetBlendState(BlendMode mode)
		{
			return blendStates[(int)mode];
		}
	}
}
*/