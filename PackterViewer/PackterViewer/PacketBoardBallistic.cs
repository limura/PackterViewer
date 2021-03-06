/*
 * Board class を使って板状のテクスチャが始点から終点まで飛んでいくのを表示するためのclass
 * 
 * コンストラクタで指定された始点から終点まで、StartTime から StartTime+FlyTimeMillisecond の間でまっすぐ飛んでいく。
 */
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PackterViewer;

namespace PackterViewer
{
    public class PacketBoardBallistic : PacketBoard
    {
        Vector3 startPoint;
        Vector3 endPoint;
        double startTimeMillisecond;
        float flyTimeMillisecond;
        string description;
        bool noSelection = false;

        Board board;
        
        public PacketBoardBallistic(Model model, Texture2D texture
            , Vector3 StartPoint, Vector3 EndPoint, GameTime StartTime, float FlyTimeMillisecond
            , string Description)
        {
            board = new Board(model);
            board.Texture = texture;

            startPoint = StartPoint;
            endPoint = EndPoint;
            startTimeMillisecond = StartTime.TotalGameTime.TotalMilliseconds;
            flyTimeMillisecond = FlyTimeMillisecond;
            board.Position = StartPoint;
            description = Description;
        }

        public float Scale
        {
            set { if (board != null) board.Scale = value; }
            get { if (board != null) return board.Scale; return 0; }
        }

        /// <summary>
        /// マウスに感知されるかどうかを指定します
        /// </summary>
        public bool NoSelection
        {
            get { return noSelection; }
            set { noSelection = value; }
        }

        /// <summary>
        /// 指示された時間に表示されるべきかどうかを調べ、表示されるべきであれば自分の座標を更新します
        /// </summary>
        /// <param name="nowGameTimeMilliseconds">表示される時間</param>
        /// <returns>表示されるべきであった(0)、表示される時間よりも後の時間を指定された(1)、表示される時間よりも前の時間を指定された(-1)</returns>
        public int Update(double nowGameTimeMilliseconds)
        {
            double diffTime = nowGameTimeMilliseconds - startTimeMillisecond;
            if (diffTime > flyTimeMillisecond)
                return 1;
            if(diffTime < 0)
                return -1;
            double diffTimePlusDelta = diffTime + startTimeMillisecond / 1000;

            float sin = (float)Math.Sin(Math.PI * diffTime / flyTimeMillisecond);
            float f = (float)(diffTime / flyTimeMillisecond);
            board.Position = startPoint + (endPoint - startPoint) * f;
            board.Position = new Vector3(board.Position.X, board.Position.Y * sin - 100.0f, board.Position.Z);
            
            float sinDelta = (float)Math.Sin(Math.PI * diffTimePlusDelta / flyTimeMillisecond);
            float fDelta = (float)(diffTimePlusDelta / flyTimeMillisecond);
            Vector3 delta = startPoint + (endPoint - startPoint) * fDelta;
            delta = new Vector3(delta.X, delta.Y * sinDelta - 100.0f, delta.Z);
            delta = (delta - board.Position) * 100;
            // 向きベクトルを回転行列に変換するのに怪しくビルボードの計算式を使う
            board.RotationMatrix = Matrix.CreateBillboard(Vector3.Zero, -delta, Vector3.UnitY, delta);
            
            //System.Diagnostics.Debug.WriteLine(f + " = " + diffTime + " / " + flyTimeMillisecond
            //    + " gameTime: " + nowGameTime.TotalGameTime.TotalMilliseconds + " - " + startTimeMillisecond);
            return 0;
        }

        public void Draw(Matrix view, Viewport viewport, Matrix projection, float scale, Vector3 cameraPosition, Vector3 cameraTarget)
        {
            board.Draw(view, viewport, projection, scale, cameraPosition, cameraTarget);
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                return new BoundingSphere(board.Position, board.Scale);
            }
        }

        public string Description
        {
            get { return description; }
        }

        public bool LightingEnabled
        {
            get { return board.LightingEnabled; }
            set { board.LightingEnabled = value; }
        }

        public double CreatedTimeMillisecond
        {
            get { return startTimeMillisecond; }
        }

        public float Alpha
        {
            get { return board.Alpha; }
            set { board.Alpha = value; }
        }

        public bool BillboardEnabled
        {
            get { return board.BillboardEnabled; }
            set { board.BillboardEnabled = value; }
        }

        public float PositionZ
        {
            get { return board.Position.Z; }
        }

        public int CompareTo(PacketBoard x)
        {
            return PacketBoardBallistic.Compare(this, x);
        }
        public static int Compare(Object x, Object y)
        {
            try
            {
                PacketBoard a = (PacketBoardBallistic)x;
                PacketBoard b = (PacketBoardBallistic)y;
                if (a.PositionZ == b.PositionZ)
                {
                    return 0;
                }
                if (a.PositionZ > b.PositionZ)
                {
                    return 1;
                }
                return -1;
            }
            catch
            {
                return 0;
            }
        }
    }
}
