/*
 * packter sender から送られるデータ一行分のデータを保管するためのclass
 *
 * srcAddress, srcPort
 * dstAddress, dstPort という名前の4つの float の値
 * (それぞれ 0.0 から 1.0 までの値であることが期待されている)と、
 * パケット画像番号(packetImageNumber) を持っている。
 * 
 * packter senderから送られた文字列をそれらしく解析する機能もこのclassが受け持っている
 *
 * 拡張として、時間を遡れるように、登録された GameTime を記録しておく機能も持たせた。
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PackterViewer;

namespace PackterViewer
{
    class FlyPacketBallistic : FlyPacket
    {
        double srcAddress = 0;
        double dstAddress = 0;
        double srcPort = 0;
        double dstPort = 0;
        byte packetImageNumber = 0;
        string packetImageString = null;
        string packetDescription = null;

        GameTime gameTime = null;
        string originalString = "undefined";
        string originalResult = "undefined";

        public FlyPacketBallistic() {
            gameTime = new GameTime();
        }

        public FlyPacketBallistic(double sourceAddress, double sourcePort, double destinationAddress, double destinationPort
            , byte imageNumber, string description, GameTime createdGameTime)
        {
            srcAddress = sourceAddress;
            srcPort = sourcePort;
            dstAddress = destinationPort;
            dstPort = destinationPort;
            packetImageNumber = imageNumber;
            packetDescription = description;
            gameTime = createdGameTime;
            originalString = String.Format("{0}, {1}, {2}, {3}, {4}, {5}"
                , sourceAddress, sourcePort, destinationAddress, destinationPort, description, imageNumber);
        }
        
        public double SrcAddress
        {
            get { return srcAddress; }
        }
        
        public double DstAddress
        {
            get { return dstAddress; }
        }
        
        public double SrcPort
        {
            get { return srcPort; }
        }
        
        public double DstPort
        {
            get { return dstPort; }
        }

        public byte PacketImageNumber
        {
            get { return packetImageNumber; }
        }

        public string PacketImageString
        {
            get { return packetImageString; }
        }

        public GameTime CreatedGameTime
        {
            get { return gameTime; }
            //set { gameTime = value; }
        }
        public GameTime StartGameTime
        {
            set { }
        }

        double String2Double(string str)
        {
            try
            {
                if (str == null || str.Length <= 0)
                    return -1;
                double d;
                if (double.TryParse(str, out d) == false)
                    return -1;
                if (d >= 1 && d <= 65536 && str.IndexOf('.') < 0)
                    return d / 65536.0;
                if (d >= 0 && d <= 1)
                    return d;
                return -1;
            }
            catch { return -1; }
        }

        public bool SetFromData(StreamReader reader)
        {
            string line = reader.ReadLine();
            if (line == null)
                return false;
            char [] delimiters = { ',', '\n' };
            string[] words = line.Split(delimiters);
            if (words.Length < 6)
                return false;
            if ((srcAddress = IPv4Tools.IPString2Double(words[0])) < 0)
                return false;
            if ((dstAddress = IPv4Tools.IPString2Double(words[1])) < 0)
                return false;
            if ((srcPort = String2Double(words[2])) < 0)
                return false;
            if ((dstPort = String2Double(words[3])) < 0)
                return false;

            srcAddress = ConfigReader.Instance.RevisionXAxis(srcAddress);
            dstAddress = ConfigReader.Instance.RevisionXAxis(dstAddress);
            srcPort = ConfigReader.Instance.RevisionYAxis(srcPort);
            dstPort = ConfigReader.Instance.RevisionYAxis(dstPort);

            System.Diagnostics.Debug.WriteLine(srcAddress.ToString() + ", " + srcPort.ToString() + " => " + dstAddress.ToString() + ", " + dstPort.ToString());
            packetImageString = words[4];
            if (byte.TryParse(words[4], out packetImageNumber) == false)
                packetImageNumber = 0;
            packetDescription = words[5];
            originalString = line;
            
            return true;
        }

        public bool setResult(StreamReader reader)
        {
            String result = reader.ReadToEnd();
            originalResult = result;
            return true;
        }

        public string OriginalResult
        {
            get { return originalResult; }
        }


        public string OriginalString
        {
            get { return originalString; }
        }
        public PacketBoard CreatePacketBoard(Model model, Texture2D texture, float defaultScale, Vector3 min, Vector3 max, float flyMillisecond)
        {
            float srcX = (float)this.SrcAddress;
            float srcZ = (float)this.SrcPort;
            float dstX = (float)this.DstAddress;
            float dstZ = (float)this.DstPort;
            byte imageNumber = this.PacketImageNumber;
            string fileName = this.PacketImageString;
            GameTime nowGameTime = this.CreatedGameTime;
            Vector3 diff = max - min;

            Vector3 startPoint = new Vector3(srcX * diff.X + min.X, 200.0f, srcZ * diff.Z + min.Z);
            Vector3 endPoint = new Vector3(dstX * diff.X + min.X, 200.0f, dstZ * diff.Z + min.Z);

            return new PacketBoardBallistic(model, texture, startPoint, endPoint, nowGameTime, flyMillisecond, this.OriginalString);
        }
    }
}
