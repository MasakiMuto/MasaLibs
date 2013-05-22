using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using TextureFunc = System.Func<string, Microsoft.Xna.Framework.Graphics.Texture2D>;

namespace Masa.ParticleEngine
{
	public class PMIData
	{
		public string TextureName { get; set; }
		public string Name { get; set; }
		public ushort Mass { get; set; }
		public Vector4 Color { get; set; }
		public ParticleBlendMode Blend { get; set; }
		public int Layer { get; set; }

		public PMIData(string texture, string name, ushort mass, Vector4 color, ParticleBlendMode blend, int layer)
		{
			TextureName = texture;
			Name = name;
			Mass = mass;
			Color = color;
			Blend = blend;
			Layer = layer;
		}

		public PMIData Clone()
		{
			return new PMIData(String.Copy(TextureName), String.Copy(Name), Mass, Color, Blend, Layer);
		}

		public ParticleManagerInitializer CreatePMI(TextureFunc texFunc)
		{
			return new ParticleManagerInitializer(texFunc(TextureName), Name, Mass, Color, Blend, Layer);
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			var a = obj as PMIData;
			if (a == null)
			{
				return false;
			}
			return a.TextureName == TextureName 
				&& a.Name == Name
				&& a.Mass == Mass
				&& a.Color == Color
				&& a.Blend == Blend
				&& a.Layer == Layer;
		}
	}

	public class ParticleManagerInitializerManager
	{
		/// <summary>
		/// XMLのタグ名
		/// </summary>
		public static readonly string PMITag = "particle_manager_initializer";

		/// <summary>
		/// ParticleManagerInitializerをそのまま作る
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="texFunc">テクスチャのファイル名を引数にテクスチャを返す関数</param>
		/// <returns></returns>
		public static IEnumerable<ParticleManagerInitializer> LoadPMIFromFile(string fileName, TextureFunc texFunc)
		{
			return LoadPMIDatas(fileName).Select(i => i.CreatePMI(texFunc));
		}

		/// <summary>
		/// ParticleManagerの生データを読み取る。応用向け
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static IEnumerable<PMIData> LoadPMIDatas(string fileName)
		{
#if WINDOWS_PHONE
			using (var stream = TitleContainer.OpenStream(fileName))
#else
			using (var stream = File.OpenRead(fileName))
#endif
			{
				return XDocument.Load(stream).Root.Elements("particle_manager_initializer").Select(i =>
				{
					Func<string, float> getColorElement = (s) => float.Parse(i.Element("color").Element(s).Value);
					var layer = i.Element("layer");
					return new PMIData(
						i.Element("texture").Value,
						i.Element("name").Value,
						ushort.Parse(i.Element("mass").Value),
						new Vector4(getColorElement("r"), getColorElement("g"), getColorElement("b"), getColorElement("a")),
						(ParticleBlendMode)Enum.Parse(typeof(ParticleBlendMode), i.Element("blend").Value, false),
						layer == null ? 0 : int.Parse(layer.Value)
						);
				});
			}
		}
	}
}
