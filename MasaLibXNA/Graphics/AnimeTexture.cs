using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Masa.Lib.XNA
{
	/// <summary>
	/// テクスチャアニメーションの1コマ。GameTextureのインデックスと、そのコマが持続するフレーム数を持つ
	/// </summary>
	public struct AnimeFrame
	{
		public readonly int Index;
		public readonly int Count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index">コマ番号</param>
		/// <param name="count">持続するフレーム数</param>
		public AnimeFrame(int index, int count)
		{
			Index = index;
			Count = count;
		}
	}



	/// <summary>
	/// アニメーション情報を保有するテクスチャ
	/// </summary>
	public class GameTexture : DividedTexture
	{
		public AnimeFrame[] Frame;
		static readonly AnimeFrame[] NoAnime = new AnimeFrame[0];

		public GameTexture(Texture2D tex, int divx, int divy, AnimeFrame[] anime)
			: base(tex, divx, divy)
		{
			Frame = (AnimeFrame[])(anime.Clone());
		}

		/// <summary>
		/// アニメーション無効
		/// </summary>
		/// <param name="tex"></param>
		/// <param name="divx"></param>
		/// <param name="divy"></param>
		public GameTexture(Texture2D tex, int divx, int divy)
			:base(tex, divx, divy)
		{
			Frame = NoAnime;
		}

		/// <summary>
		/// ゲームキャラクタ用スプライト描画メソッド
		/// </summary>
		/// <param name="center">中心座標</param>
		/// <param name="index">インデックス</param>
		/// <param name="angle">回転角。デフォルトだと無回転</param>
		/// <param name="rate">拡大率。デフォルトだと無拡大</param>
		public void Draw(SpriteBatch sprite, Vector2 center, int index, Color col, float angle = float.NaN, float rate = 1f)
		{
			if (float.IsNaN(angle) && rate == 1)
			{
				Vector2 pos = center - HalfVector;
				sprite.Draw(Texture, pos, Rect[index], col);//無回転・無拡大
			}
			else
			{
				//Vector2 pos = center - HalfVector;
				Vector2 pos = center;
				if (float.IsNaN(angle))
				{
					angle = 0;
				}
				//Reference.Sprite.Draw(Texture, pos, Rect[index], Color.White, angle,
				//				Center(index), rate, SpriteEffects.None, 0);
				sprite.Draw(Texture, pos, Rect[index], col, angle,
					HalfVector, rate, SpriteEffects.None, 0);
			}
		}

		public void DrawZ(SpriteBatch sprite, Vector2 center, int index, Color col, float z, float angle = float.NaN, float rate = 1f)
		{

			Vector2 pos = center;
			if (float.IsNaN(angle))
			{
				angle = 0;
			}
			sprite.Draw(Texture, pos, Rect[index], col, angle,
				HalfVector, rate, SpriteEffects.None, z);

		}


		/// <summary>
		/// ゲームキャラクタ用スプライト描画メソッド
		/// </summary>
		/// <param name="center">中心座標</param>
		/// <param name="index">インデックス</param>
		/// <param name="angle">回転角。デフォルトだと無回転</param>
		/// <param name="rate">拡大率。デフォルトだと無拡大</param>
		public void Draw(SpriteBatch sprite, Vector2 center, int index, float angle = float.NaN, float rate = 1f)
		{
			Draw(sprite, center, index, Color.White, angle, rate);
		}

	}

	//TODO structにしたい
	/// <summary>
	/// 実際にアニメーションを行い、テクスチャを保持する
	/// キャラ一つ一つが別々に保有するもの。
	/// </summary>
	public class AnimeTexture
	{
		GameTexture Texture;
		SpriteBatch sprite;

		int Counter;
		/// <summary>
		/// 現在のフレーム番号
		/// </summary>
		public int AnimeIndex;

		/// <summary>
		/// デフォルトは-1。正数に設定するとそのインデックスを使って描画
		/// </summary>
		public int CharaIndex;

		public GameTexture BaseTexture
		{
			get { return Texture; }
		}

		public AnimeTexture(GameTexture tex, SpriteBatch sprite)
		{
			this.sprite = sprite;
			Texture = tex;
			Counter = 0;
			AnimeIndex = 0;
			CharaIndex = -1;
		}

		/// <summary>
		/// AnimeFrameに従ってアニメーションを実行する
		/// </summary>
		public void Update()
		{
			if (Texture.Frame.Length > 1)
			{
				Counter++;
				if (Texture.Frame[AnimeIndex].Count == Counter)
				{
					FrameShift();
				}
			}
		}

		/// <summary>
		/// Updateとは関係なく強制的に次のフレームへ遷移させる。アニメーション無効なら何もしない
		/// 実行すると現在のフレームの再生時間はリセットされる
		/// </summary>
		public void Next()
		{
			if (Texture.Frame.Length > 1)
			{
				FrameShift();
			}
		}

		void FrameShift()
		{
			Counter = 0;
			AnimeIndex++;
			if (AnimeIndex >= Texture.Frame.Length)
			{
				AnimeIndex -= Texture.Frame.Length;
			}
		}

		public void Draw(Vector2 center, float angle = float.NaN, float rate = 1)
		{
			Draw(center, Color.White, angle, rate);
		}

		public void Draw(Vector2 center, Color col, float angle = float.NaN, float rate = 1)
		{
			if (CharaIndex == -1)
			{
				if (Texture.Frame.Length == 0)
				{
					Texture.Draw(sprite, center, 0, col, angle, rate);
				}
				else
				{
					Texture.Draw(sprite, center, Texture.Frame[AnimeIndex].Index, col, angle, rate);
				}
			}
			else
			{
				Texture.Draw(sprite, center, CharaIndex, col, angle, rate);
			}
		}

		public void Draw(float x, float y, float angle = float.NaN, float rate = 1f)
		{
			Draw(new Vector2(x, y), angle, rate);
		}

		/// <summary>
		/// テクスチャ取得元の座標Rect
		/// </summary>
		public Rectangle SourceRect
		{
			get
			{
				if (Texture.Frame.Length > 0)
				{
					return Texture.Rect[Texture.Frame[AnimeIndex].Index];
				}
				else
				{
					return Texture.Rect[0];
				}
			}
		}
	}
}
