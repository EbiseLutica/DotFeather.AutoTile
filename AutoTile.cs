/*
AutoTile.cs

The MIT License (MIT)

Copyright (c) 2019 Xeltica

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Drawing;
using System.IO;

namespace DotFeather
{
    /// <summary>
    /// WOLF RPG Editor と互換性のあるオートタイルを提供します。
    /// </summary>
    public class AutoTile : ITile
    {
        /// <summary>
        /// 描画されるテクスチャを取得します。
        /// </summary>
        public Texture2D[] Texture => Textures[animationState];

        /// <summary>
        /// アニメーションに使われるテクスチャの配列を取得します。
        /// </summary>
        public Texture2D[][] Textures { get; private set; }

        /// <summary>
        /// アニメーションにおけるテクスチャ1枚あたりの描画時間を取得します。
        /// </summary>
        public double Interval { get; private set; }

        public AutoTile(Texture2D[][] textures, double interval)
        {
            if (textures.Length < 1)
                throw new ArgumentException(nameof(textures));
            Textures = textures;
            Interval = interval;
        }

        public void Draw(GameBase game, Tilemap map, Vector location, Color? color)
        {
            if (prevFrameCount != game.TotalFrame)
            {
                if (timer > Interval)
                {
                    animationState = animationState < Textures.Length - 1 ? animationState + 1 : 0;
                    timer = 0;
                }
                timer += Time.DeltaTime;
            }
            prevFrameCount = game.TotalFrame;

            const int outside = 4 * 0;
            const int vertical = 4 * 1;
            const int horizontal = 4 * 2;
            const int inside = 4 * 3;
            const int fill = 4 * 4;
            var l = (int)Texture[0].Size.X;
            var topLeft = new Vector(0, 0);
            var topRight = new Vector(l, 0);
            var bottomLeft = new Vector(0, l);
            var bottomRight = new Vector(l, l);
            void Draw(Texture2D texture, Vector rel) => TextureDrawer.Draw(game, texture, location + rel * map.Scale, map.Scale, map.Angle, color);
            var (x, y) = map.CurrentDrawingPosition ?? VectorInt.Zero;

            //top left
            {
                var texture =
                    map[x - 1, y] != this && map[x, y - 1] != this
                        ? outside :
                    map[x - 1, y] != this
                        ? vertical :
                    map[x, y - 1] != this
                        ? horizontal :
                    map[x - 1, y - 1] != this
                        ? inside : fill;
                Draw(Texture[texture + 0], topLeft);
            }
            //top right
            {
                var texture =
                    map[x + 1, y] != this && map[x, y - 1] != this
                        ? outside :
                    map[x + 1, y] != this
                        ? vertical :
                    map[x, y - 1] != this
                        ? horizontal :
                    map[x + 1, y - 1] != this
                        ? inside : fill;
                Draw(Texture[texture + 1], topRight);
            }
            //bottom left
            {
                var texture =
                    map[x - 1, y] != this && map[x, y + 1] != this
                        ? outside :
                    map[x - 1, y] != this
                        ? vertical :
                    map[x, y + 1] != this
                        ? horizontal :
                    map[x - 1, y + 1] != this
                        ? inside : fill;
                Draw(Texture[texture + 2], bottomLeft);
            }
            //bottom right
            {
                var texture =
                    map[x + 1, y] != this && map[x, y + 1] != this
                        ? outside :
                    map[x + 1, y] != this
                        ? vertical :
                    map[x, y + 1] != this
                        ? horizontal :
                    map[x + 1, y + 1] != this
                        ? inside : fill;
                Draw(Texture[texture + 3], bottomRight);
            }
        }

        /// <summary>
        /// 画像ファイルを指定して、タイルを生成します。
        /// </summary>
        /// <param name="path">ファイルパス。</param>
        /// <param name="frameLength">アニメーションするフレームの枚数。</param>
        /// <param name="size">画像1枚あたりのサイズ。</param>
        /// <param name="interval">画像1枚あたりのインターバル。秒単位。</param>
        /// <returns>生成されたタイル。</returns>
        public static AutoTile LoadFrom(string path, int frameLength, VectorInt size, double interval = 0) => new AutoTile(LoadAndSplitFrom(path, frameLength, size), interval);

        /// <summary>
        /// 画像ファイルを指定して、タイルを生成します。
        /// </summary>
        /// <param name="stream">ファイルを示すストリーム。</param>
        /// <param name="frameLength">アニメーションするフレームの枚数。</param>
        /// <param name="size">画像1枚あたりのサイズ。</param>
        /// <param name="interval">画像1枚あたりのインターバル。秒単位。</param>
        /// <returns>生成されたタイル。</returns>
        public static AutoTile LoadFrom(Stream stream, int frameLength, VectorInt size, double interval = 0) => new AutoTile(LoadAndSplitFrom(stream, frameLength, size), interval);

        private static Texture2D[][] LoadAndSplitFrom(string path, int frameLength, VectorInt size)
        {
            return LoadAndSplitFrom(File.OpenRead(path), frameLength, size);
        }

        private static Texture2D[][] LoadAndSplitFrom(Stream stream, int frameLength, VectorInt size)
        {
            var width = frameLength * 2;
            var height = 10;
            var tex = new Texture2D[frameLength][];
            var temp = Texture2D.LoadAndSplitFrom(stream, width, height, size / 2);
            for (var ix = 0; ix < width; ix += 2)
            {
                var buf = new Texture2D[height * 2];
                var i = 0;
                for (var iy = 0; iy < height; iy++)
                {
                    var offset = iy * width + ix;
                    buf[i + 0] = temp[offset + 0];
                    buf[i + 1] = temp[offset + 1];
                    i += 2;
                }
                tex[ix / 2] = buf;
            }
            return tex;
        }

        /// <summary>
        /// この <see cref="AutoTile"/> を削除します。
        /// </summary>
        public void Destroy()
        {
            foreach (var a1 in Textures)
                foreach (var a2 in a1)
                    a2.Dispose();
        }

        private int animationState = 0;
        private double timer = 0;
        private long prevFrameCount = -1;
    }
}
