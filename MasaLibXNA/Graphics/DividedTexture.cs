using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Masa.Lib;
using System.Diagnostics.Contracts;

namespace Masa.Lib.XNA
{
	/// <summary>
	/// コマ分割対応の汎用テクスチャ
	/// </summary>
	public class DividedTexture
	{
		readonly protected Texture2D texture;

		//少なくとも長さ1
		readonly public Rectangle[] Rect;
		
		/// <summary>
		/// コマ当たりの幅 高さ
		/// </summary>
		readonly public int Width, Height;
		
		/// <summary>
		/// 中心指定描画に使う、{Width/2, Height/2}のベクトル
		/// </summary>
		readonly protected Vector2 HalfVector;


		public Texture2D Texture
		{
			get { return texture; }
		}

		/// <summary>
		/// 分割数を指定して生成する。左上1段目から右上1段目、左2段目から右2段目、の順に 0, 1, ...と番号がふられる
		/// </summary>
		/// <param name="tex"></param>
		/// <param name="divx">横方向のコマ分割数</param>
		/// <param name="divy">縦方向のコマ分割数</param>
		public DividedTexture(Texture2D tex, int divx, int divy)
		{
			Contract.Requires(divx >= 0 && divy >= 0);
			if (divx == 0)
			{
				divx = 1;
			}
			if (divy == 0)
			{
				divy = 1;
			}
			Width = tex.Width / divx;
			Height = tex.Height / divy;
			texture = tex;
			Rect = new Rectangle[divx * divy];
			for (int i = 0; i < divy; i++)
			{
				for (int k = 0; k < divx; k++)
				{
					Rect[i * divx + k] = new Rectangle(k * Width, i * Height, Width, Height);
				}
			}
			HalfVector = new Vector2(Width / 2f, Height / 2f);
		}

		/// <summary>
		/// テクスチャの回転中心
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected Vector2 Center(int index)
		{
			return new Vector2(Rect[index].X + HalfVector.X, Rect[index].Y + HalfVector.Y);
		}

		protected Rectangle MakeDestRect(Vector2 center)
		{
			return new Rectangle((int)(center.X), (int)(center.Y), Width, Height);
			//return new Rectangle((int)(center.X - HalfVector.X), (int)(center.Y - HalfVector.Y), Width, Height);
		}

		/// <summary>
		/// 左上座標と番号を指定してスプライト描画
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="index"></param>
		public virtual void Draw(SpriteBatch sprite, Vector2 pos, int index)
		{
			sprite.Draw(Texture, pos, Rect[index], Color.White);
		}

		/// <summary>
		/// 中心座標と番号を指定してスプライト描画
		/// </summary>
		/// <param name="center"></param>
		/// <param name="index"></param>
		public void DrawCenter(SpriteBatch sprite, Vector2 center, int index)
		{
			sprite.Draw(Texture, center - HalfVector, Rect[index], Color.White);
		}

		public void DrawCenterRotate(SpriteBatch sprite, Vector2 center, int index, float angle)
		{
			sprite.Draw(Texture, MakeDestRect(center), Rect[index], Color.White, angle, HalfVector, SpriteEffects.None, 0);
		}

		
		
	}
}
