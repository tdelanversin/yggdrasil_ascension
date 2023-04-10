using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Assimp;
using System.Data;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using SharpFont.Cache;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using SharpDX;

namespace YGR
{
    public enum X_DoorDirection
    {
        Horizontal = 0,
        Vertical
    }

    public class Y_Door : IWalkable
    {
        public Rectangle Rect { get; set; }
        public string Name { get; }
        public X_CollisionModel_Room Collision { get; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }

        private int[][] _collision;
        private X_DoorDirection _direction;
        private Texture2D _floor;
        private float _scale;
        const int _numTilesDoorWidth = 5;

        //Texture2D _TL_3x3;
        //Texture2D _TR_3x3;
        //Texture2D _BL_3x3;
        //Texture2D _BR_3x3;
        //Texture2D _FR_1x1;
        //Texture2D _FS_L_1x1;
        //Texture2D _FS_T_1x1;
        //Texture2D _FS_R_1x1;
        //Texture2D _WS_1x1;
        //Texture2D _WT_H_1x1;
        //Texture2D _WT_V_1x1;

        public Y_Door(
            X_DoorDirection direction, 
            int numTilesLength, 
            int tileWidth, 
            int tileHeight, 
            int tileOffset,
            GraphicsDevice graphicsDevice
            )
        {
            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);

            collision = flipToPosition(collision, direction, tileOffset);

            _collision = getDoorPoints(collision, tileWidth, tileHeight);

            _direction = direction;

            Collision = new X_CollisionModel_Room(_collision, tileWidth, tileHeight);

            Rect = new Rectangle(0, 0, tileWidth * _collision[0].Length, tileHeight * _collision.Length);

            Texture2D T_TL_1x1;
            Texture2D T_TR_1x1;
            Texture2D T_BL_1x1;
            Texture2D T_BR_1x1;
            Texture2D T_FR_1x1;
            Texture2D T_FS_L_1x1;
            Texture2D T_FS_T_1x1;
            Texture2D T_FS_R_1x1;
            Texture2D T_FS_TL_1x1;
            Texture2D T_FS_TR_1x1;
            Texture2D T_WS_1x1;
            Texture2D T_WT_H_1x1;
            Texture2D T_WT_V_1x1;
            Texture2D T_FS_BL_1x1;
            Texture2D T_FS_BR_1x1;

            using (FileStream fileStream = new FileStream("./Doors/TL-1x1.png", FileMode.Open))
                T_TL_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/TR-1x1.png", FileMode.Open))
                T_TR_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/BL-1x1.png", FileMode.Open))
                T_BL_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/BR-1x1.png", FileMode.Open))
                T_BR_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FR-1x1.png", FileMode.Open))
                T_FR_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-L-1x1.png", FileMode.Open))
                T_FS_L_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-T-1x1.png", FileMode.Open))
                T_FS_T_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-R-1x1.png", FileMode.Open))
                T_FS_R_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-TR-1x1.png", FileMode.Open))
                T_FS_TR_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-TL-1x1.png", FileMode.Open))
                T_FS_TL_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/WS-1x1.png", FileMode.Open))
                T_WS_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/WT-H-1x1.png", FileMode.Open))
                T_WT_H_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/WT-V-1x1.png", FileMode.Open))
                T_WT_V_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-BR-1x1.png", FileMode.Open))
                T_FS_BR_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/FS-BL-1x1.png", FileMode.Open))
                T_FS_BL_1x1 = Texture2D.FromStream(graphicsDevice, fileStream);

            Color[] TL_1x1;
            Color[] TR_1x1;
            Color[] BL_1x1;
            Color[] BR_1x1;
            Color[] FR_1x1;
            Color[] FS_L_1x1;
            Color[] FS_T_1x1;
            Color[] FS_R_1x1;
            Color[] FS_TL_1x1;
            Color[] FS_TR_1x1;
            Color[] WS_1x1;
            Color[] WT_H_1x1;
            Color[] WT_V_1x1;
            Color[] FS_BR_1x1;
            Color[] FS_BL_1x1;

            List<Color[]> textels = new List<Color[]>();

            int len = T_BL_1x1.Width * T_BL_1x1.Height;
            /* 2 */
            TL_1x1 = new Color[len];
            T_TL_1x1.GetData<Color>(TL_1x1);
            textels.Add(TL_1x1);

            /* 3 */
            TR_1x1 = new Color[len];
            T_TR_1x1.GetData<Color>(TR_1x1);
            textels.Add(TR_1x1);

            /* 4 */
            BL_1x1 = new Color[len];
            T_BL_1x1.GetData<Color>(BL_1x1);
            textels.Add(BL_1x1);

            /* 5 */
            BR_1x1 = new Color[len];
            T_BR_1x1.GetData<Color>(BR_1x1);
            textels.Add(BR_1x1);

            /* 6 */
            FR_1x1 = new Color[len];
            T_FR_1x1.GetData<Color>(FR_1x1);
            textels.Add(FR_1x1);

            /* 7 */
            FS_L_1x1 = new Color[len];
            T_FS_L_1x1.GetData<Color>(FS_L_1x1);
            textels.Add(FS_L_1x1);

            /* 8 */
            FS_R_1x1 = new Color[len];
            T_FS_R_1x1.GetData<Color>(FS_R_1x1);
            textels.Add(FS_R_1x1);

            /* 9 */
            FS_T_1x1 = new Color[len];
            T_FS_T_1x1.GetData<Color>(FS_T_1x1);
            textels.Add(FS_T_1x1);

            /* 10 */
            FS_TL_1x1 = new Color[len];
            T_FS_TL_1x1.GetData<Color>(FS_TL_1x1);
            textels.Add(FS_TL_1x1);

            /* 11 */
            FS_TR_1x1 = new Color[len];
            T_FS_TR_1x1.GetData<Color>(FS_TR_1x1);
            textels.Add(FS_TR_1x1);

            /* 12 */
            WS_1x1 = new Color[len];
            T_WS_1x1.GetData<Color>(WS_1x1);
            textels.Add(WS_1x1);

            /* 13 */
            WT_H_1x1 = new Color[len];
            T_WT_H_1x1.GetData<Color>(WT_H_1x1);
            textels.Add(WT_H_1x1);

            /* 14 */
            WT_V_1x1 = new Color[len];
            T_WT_V_1x1.GetData<Color>(WT_V_1x1);
            textels.Add(WT_V_1x1);

            /* 15 */
            FS_BR_1x1 = new Color[len];
            T_FS_BR_1x1.GetData<Color>(FS_BR_1x1);
            textels.Add(FS_BR_1x1);

            /* 16 */
            FS_BL_1x1 = new Color[len];
            T_FS_BL_1x1.GetData<Color>(FS_BL_1x1);
            textels.Add(FS_BL_1x1);

            //Color[] floorData = new Color[len];
            //_floor.GetData<Color>(floorData);
            //Color[] newData = new Color[len];
            //for (int i = 0; i < len; ++i)
            //{
            //    if (groundData[i].A == 0)
            //    {
            //        newData[i] = floorData[i];
            //    }
            //    else
            //    {
            //        newData[i] = Color.Transparent;
            //    }
            //}
            //_floor.SetData<Color>(newData);

            int[][] pattern = _collision.Clone() as int[][];
            for(int i=0; i < pattern.Length; i++)
            {
                for(int j=0; j<pattern[0].Length; ++j)
                {
                    if (pattern[i][j] > 0) pattern[i][j] = 1;
                }
            }

            int[,] cornerTL = new int[,] { 
                { 1, 1 }, 
                { 1, 0 } };
            int[,] cornerTR = new int[,] {
                { 1, 1 },
                { 0, 1 } };
            int[,] cornerBL = new int[,] {
                { 1, 0 },
                { 1, 1 } };
            int[,] cornerBR = new int[,] {
                { 0, 1 },
                { 1, 1 } };

            var tl = findMatch(cornerTL, pattern, 0, 0);
            var tr = findMatch(cornerTR, pattern, 0, 1);
            var bl = findMatch(cornerBL, pattern, 1, 0);
            var br = findMatch(cornerBR, pattern, 1, 1);
            

            int[][] result = pattern.Clone() as int[][];
            foreach (var t in tl)
            {
                result[t.Item1][t.Item2] = 2;
                if (t.Item1 + 2 < result.Length && t.Item2+1 < result[0].Length && result[t.Item1 + 2][t.Item2 + 1] == 0) result[t.Item1 + 2][t.Item2 + 1] = 10;
                if (t.Item2 - 1 >= 0 && result[t.Item1][t.Item2 - 1] == 0) result[t.Item1][t.Item2 - 1] = 8;
                if (t.Item2 + 1 < result[0].Length && result[t.Item1][t.Item2 + 1] == 0) result[t.Item1][t.Item2 + 1] = 7;
            }
            foreach (var t in tr)
            {
                result[t.Item1][t.Item2] = 3;
                if (t.Item1 + 1 < result.Length && t.Item2 - 1 >= 0 && result[t.Item1 + 1][t.Item2 - 1] == 0) result[t.Item1 + 1][t.Item2 - 1] = 12;
                if (t.Item1 + 2 < result.Length && t.Item2 - 1 >= 0 && result[t.Item1 + 2][t.Item2 - 1] >= 0) result[t.Item1 + 2][t.Item2 - 1] = 11;
                if (t.Item2 - 1 >= 0 && result[t.Item1][t.Item2 - 1] == 0) result[t.Item1][t.Item2 - 1] = 8;
                if (t.Item2 + 1 < result[0].Length && result[t.Item1][t.Item2 + 1] == 0) result[t.Item1][t.Item2 + 1] = 7;
            }
            foreach (var t in bl)
            {
                result[t.Item1][t.Item2] = 4;
                if (t.Item1 + 1 < result.Length && result[t.Item1 + 1][t.Item2] <= 0) result[t.Item1 + 1][t.Item2] = 12;
                if (t.Item1 + 2 < result.Length && result[t.Item1 + 2][t.Item2] == 0) result[t.Item1 + 2][t.Item2] = 9;
                if (t.Item2 - 1 >= 0 && result[t.Item1][t.Item2 - 1] == 0) result[t.Item1][t.Item2 - 1] = 8;
                if (t.Item1 + 1 < result.Length && t.Item2 - 1 >= 0 && result[t.Item1 + 1][t.Item2 - 1] == 0) result[t.Item1 + 1][t.Item2 - 1] = 8;
                if (t.Item1 + 2 < result.Length && t.Item2 - 1 >= 0 && result[t.Item1 + 2][t.Item2 - 1] == 0) result[t.Item1 + 2][t.Item2 - 1] = 16;
            }
            foreach (var t in br)
            {
                result[t.Item1][t.Item2] = 5;
                if (t.Item1 + 1 < result.Length && result[t.Item1 + 1][t.Item2] <= 0) result[t.Item1 + 1][t.Item2] = 12;
                if (t.Item1 + 2 < result.Length && result[t.Item1 + 2][t.Item2] == 0) result[t.Item1 + 2][t.Item2] = 9;
                if (t.Item2 + 1 < result[0].Length && result[t.Item1][t.Item2+1] == 0) result[t.Item1][t.Item2+1] = 7;
                if (t.Item1 + 1 < result.Length && t.Item2 + 1 < result[0].Length && result[t.Item1 + 1][t.Item2+1] == 0) result[t.Item1+1][t.Item2 + 1] = 7;
                if (t.Item1 + 2 < result.Length && t.Item2 + 1 < result[0].Length && result[t.Item1 + 2][t.Item2 + 1] == 0) result[t.Item1 + 2][t.Item2 + 1] = 15;
            }

            if(direction == X_DoorDirection.Horizontal)
            {
                // h: 3 - 4
                // h: 2 - 5
                for (int i = 0; i < result.Length; i++)
                {
                    int j = 2;
                    while (j < result[0].Length && result[i][j] == 1)
                    {
                        result[i][j] = 13;
                        if (i + 1 < result.Length && result[i + 1][j] <= 0) result[i + 1][j] = 12;
                        if (i + 2 < result.Length && result[i + 2][j] == 0) result[i + 2][j] = 9;
                        j++;
                    }

                    j = result[0].Length - 3;
                    while (j > 0 && result[i][j] == 1)
                    {
                        result[i][j] = 13;
                        if (i + 1 < result.Length && result[i + 1][j] <= 0) result[i + 1][j] = 12;
                        if (i + 2 < result.Length && result[i + 2][j] == 0) result[i + 2][j] = 9;
                        j--;
                    }
                }

                for (int i = 0; i < result[0].Length; ++i)
                {
                    if (result[0][i] == 2 || result[0][i] == 3)
                    {
                        int j = 1;
                        while (result[j][i] == 1)
                        {
                            result[j][i] = 14;
                            if (i - 1 >= 0 && result[j][i - 1] == 0) result[j][i - 1] = 8;
                            if (i + 1 < result[0].Length && result[j][i + 1] == 0) result[j][i + 1] = 7;
                            j++;
                        }
                    }
                    else if (result[result.Length - 1][i] == 4 || result[result.Length - 1][i] == 5)
                    {
                        int j = result.Length - 2;
                        while (result[j][i] == 1)
                        {
                            result[j][i] = 14;
                            if (i - 1 >= 0 && result[j][i - 1] == 0) result[j][i - 1] = 8;
                            if (i + 1 < result[0].Length && result[j][i + 1] == 0) result[j][i + 1] = 7;
                            j--;
                        }
                    }
                }
                //output(result, "./logs/pattern.csv");
            }
            else
            {
                // v: 2 - 5
                // v: 4 - 3
                for (int i = 0; i < result[0].Length; i++)
                {
                    int j = 2;
                    while (j < result.Length && result[j][i] == 1)
                    {
                        result[j][i] = 14;
                        if (i - 1 >= 0 && result[j][i - 1] == 0) result[j][i - 1] = 8;
                        if (i + 1 < result[0].Length && result[j][i + 1] == 0) result[j][i + 1] = 7;
                        j++;
                    }

                    j = result.Length - 3;
                    while (j > 0 && result[j][i] == 1)
                    {
                        //if(j == result.Length - 2) result[j+1][i] = 12;
                        result[j][i] = 14;
                        if (i - 1 >= 0 && result[j][i - 1] == 0) result[j][i - 1] = 8;
                        if (i + 1 < result[0].Length && result[j][i + 1] == 0) result[j][i + 1] = 7;
                        j--;
                    }
                }

                for (int i = 0; i < result.Length; ++i)
                {
                    if (result[i][0] == 2 || result[i][0] == 4)
                    {
                        int j = 1;
                        while (result[i][j] == 1)
                        {
                            result[i][j] = 13;
                            if (i + 1 < result.Length && result[i + 1][j] <= 0) result[i + 1][j] = 12;
                            if (i + 2 < result.Length && result[i + 2][j] == 0) result[i + 2][j] = 9;
                            j++;
                        }
                    }
                    else if(result[i][result[0].Length-1] == 5 || result[i][result[0].Length - 1] == 3)
                    {
                        int j = result[0].Length-2;
                        while (result[i][j] == 1)
                        {
                            result[i][j] = 13;
                            if (i + 1 < result.Length && result[i + 1][j] <= 0) result[i + 1][j] = 12;
                            if (i + 2 < result.Length && result[i + 2][j] == 0) result[i + 2][j] = 9;
                            j--;
                        }
                    }
                }
            }

            for(int x=0; x<result.Length; ++x)
            {
                for(int y=0; y < result[0].Length; ++y)
                {
                    if(result[x][y] == 0) result[x][y] = 6;
                }
            }

            output(result, "./logs/pattern.csv");

            int width = T_BL_1x1.Width;
            int height = T_BL_1x1.Height;
            _floor = new Texture2D(graphicsDevice, tileWidth * result[0].Length, tileHeight * result.Length);
            _scale = (float)tileHeight / height;

            for (int x = 0; x < result[0].Length; ++x)
            {
                for (int y = 0; y < result.Length; ++y)
                {
                    var index = result[y][x] - 2;
                    Color[] elem;
                    if (index >= 0)
                    {
                        elem = textels.ElementAt(index);
                    }
                    else
                    {
                        elem = Enumerable.Repeat<Color>(Color.Transparent, width * height).ToArray();
                    }
                    //var elem = textels.ElementAt(1);
                    _floor.SetData(0, new Rectangle(x * width, y * height, width, height), elem, 0, elem.Length);
                }
            }
        }

        private void output(int[][] pattern, string name)
        {
            string s = "";
            using (StreamReader reader = new StreamReader(name))
            {
                s = reader.ReadToEnd();
            }
            s += "\n\n";
            using (StreamWriter writer = new StreamWriter(name))
            {
                for (int x = 0; x < pattern.GetLength(0); ++x)
                {
                    s += string.Join("\t", pattern[x]) + "\n";
                }
                writer.WriteLine(s);
            }
        }

        private IList<Tuple<int, int>> findMatch(int[,] match, int[][] pattern, int ox, int oy)
        {
            int lx = match.GetLength(0);
            int ly = match.GetLength(1);
            int px = pattern.Length - lx + 1;
            int py = pattern[0].Length - ly + 1;
            IList<Tuple<int, int>> matchPos = new List<Tuple<int, int>>();

            for (int x = 0; x < px; ++x)
            {
                for (int y = 0; y < py; ++y)
                {
                    for (int mx = 0; mx < lx; ++mx)
                    {
                        for (int my = 0; my < ly; ++my)
                        {
                            var p = pattern[x + mx][y + my];
                            var m = match[mx, my];
                            if (p != m && !(p==-1 && m==0)) goto no_match;
                        }
                    }
                    matchPos.Add(new Tuple<int, int>(x+ox, y+oy));
                    no_match: continue;
                }
            }

            return matchPos;
        }

        private int[][] getDoorPoints(int[][] collision, int tileWidth, int tileHeight)
        {
            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            for (int x = 0; x < collision.Length; ++x)
            {
                for (int y = 0; y < collision[0].Length; ++y)
                {
                    var val = collision[x][y];
                    if (val == 2 || val == 4)
                    {
                        var side = (val == 2) ? X_ConnectorSide.Bottom : X_ConnectorSide.Left;
                        Doors.Add(side, new List<X_ConnectorPoint>{ new X_ConnectorPoint(side, new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2))});
                        collision[x][y] = 0;
                    }
                    if (val == 3 || val == 5)
                    {
                        var side = (val == 3) ? X_ConnectorSide.Top : X_ConnectorSide.Right;
                        Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2)) });
                        collision[x][y] = 0;
                    }
                }
            }

            return collision;
        }

        private int[][] flipToPosition(int[][] collision, X_DoorDirection direction, int tileOffset)
        {
            int[][] newCol = null;
            if (direction == X_DoorDirection.Vertical)
            {
                if(tileOffset < 0)
                {
                    // No need to do anything. This is the generation case
                    newCol = collision;
                }
                else
                {
                    // flip horizontally
                    newCol = new int[collision.Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision[0].Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            newCol[x][newCol[0].Length - 1 - y] = collision[x][y];
                        }
                    }
                }
            }
            else
            {
                if(tileOffset < 0)
                {
                    // rotate by 90 degrees clock-wise
                    newCol = new int[collision[0].Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision.Length];
                    }
                    for (int x = 0; x < newCol[0].Length; ++x)
                    {
                        for (int y = 0; y < newCol.Length; ++y)
                        {
                            var val = collision[x][y];
                            if (val == 2) val = 4;
                            else if (val == 3) val = 5;
                            newCol[y][newCol[0].Length - 1 - x] = val;
                        }
                    }
                }
                else
                {
                    // rotate by 90 degrees clock-wise
                    // then flip vertically
                    newCol = new int[collision[0].Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision.Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            var val = collision[y][x];
                            if (val == 2) val = 4;
                            else if (val == 3) val = 5;
                            newCol[newCol.Length - 1 - x][newCol[0].Length - 1 - y] = val;
                        }
                    }
                }
            }

            return newCol;
        }

        private int[][] createDoorTemplate(int numTilesLength, int tileOffset)
        {
            int width = numTilesLength;
            int doorWidth = _numTilesDoorWidth;
            int height = doorWidth + Math.Abs(tileOffset);

            int[][] collision = new int[width][];
            for (int x = 0; x < width; ++x)
            {
                collision[x] = new int[height];
                for (int y = 0; y < height; ++y)
                {
                    collision[x][y] = 0;
                }
            }

            int half1 = (int)Math.Floor((float)width / 2.0f) + 2;

            for (int x = 2; x < half1; ++x)
            {
                collision[x][0] = 1;
            }
            //output(collision, "./logs/pattern.csv");
            for (int y = doorWidth; y < collision[0].Length; ++y)
            {
                collision[0][y] = -1;
                collision[1][y] = -1;
            }
            for (int x = 2; x < half1 - doorWidth; ++x)
            {
                collision[x][doorWidth - 1] = 1;
                for(int y=doorWidth; y < collision[0].Length; ++y) collision[x][y] = -1;
            }
            //output(collision, "./logs/pattern.csv");
            for (int x = half1-doorWidth; x < width-2; ++x)
            {
                collision[x][height - 1] = 1;
            }
            //output(collision, "./logs/pattern.csv");
            for (int x = half1; x<width-2; ++x)
            {
                collision[x][Math.Abs(tileOffset)] = 1;
                for (int y = 0; y <= Math.Abs(tileOffset)-1; ++y) collision[x][y] = -1;
            }
            for (int y = 0; y <= Math.Abs(tileOffset) - 1; ++y) {
                collision[width - 1][y] = -1;
                collision[width - 2][y] = -1;
            } 
            //output(collision, "./logs/pattern.csv");
            for (int y = 0; y < Math.Abs(tileOffset); ++y)
            {
                collision[half1 - 1][y + 1] = 1;
                collision[half1 - doorWidth][height - y - 2] = 1;
            }
            //output(collision, "./logs/pattern.csv");
            collision[width-3][height-1-doorWidth/2] = 2;
            collision[2][doorWidth/2] = 3;

            return collision;
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!Doors.ContainsKey(side)) return null;
            return Doors[side].First();
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms</para>
        /// <para>The connector is in the middle</para>
        /// 
        /// <br>-------------------- </br>
        /// <br>left |||XXX||| right </br>
        /// <br>-------------------- </br>
        ///  
        /// <para>OR</para>
        /// 
        /// <br>  top   </br>
        /// <br> |||||  </br>
        /// <br> |XXX|  </br>
        /// <br> |||||  </br>
        /// <br> bottom </br>
        /// 
        /// <para>The method acts out of the PERSPECTIVE OF THE CONNECTOR!</para>
        /// 
        /// <para>side1 = X_ConnectorSide.Left means, that room1 is on the left side of the connector.</para>
        /// </summary>
        /// <param name="side1">The origin side: the connector will place the other side with respect to this side</param>
        /// <param name="from">The Y_Room on side1 (the origin side)</param>
        /// <param name="side2">The adjusted side: the connector will place this side with respect to the other side</param>
        /// <param name="to">The Y_Room on side 2 (the adjusted side)</param>
        /// <param name="random">Random generator for the random connector point selection</param>
        /// <returns></returns>
        public IWalkable Connect(
            X_ConnectorSide side1,
            IWalkable from,
            X_ConnectorSide side2,
            IWalkable to
        )
        {
            /*
             * If we use 
             *    connect(Left, room1, Right, room2)
             * we have to FLIP the sides for the next call because for the left side of the connector
             * we need a point on the right side of room 1 and for the right side of the connector, we need
             * a point on the left side of room 2!!!!!!!
             */

            if (side1 == X_ConnectorSide.Left) side1 = X_ConnectorSide.Right;
            else if (side1 == X_ConnectorSide.Right) side1 = X_ConnectorSide.Left;
            else if (side1 == X_ConnectorSide.Top) side1 = X_ConnectorSide.Bottom;
            else if (side1 == X_ConnectorSide.Bottom) side1 = X_ConnectorSide.Top;

            if (side2 == X_ConnectorSide.Left) side2 = X_ConnectorSide.Right;
            else if (side2 == X_ConnectorSide.Right) side2 = X_ConnectorSide.Left;
            else if (side2 == X_ConnectorSide.Top) side2 = X_ConnectorSide.Bottom;
            else if (side2 == X_ConnectorSide.Bottom) side2 = X_ConnectorSide.Top;

            return connect(from, from.GetConnectorPoint(side1), to, to.GetConnectorPoint(side2));
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms</para>
        /// <para>The connector is in the middle</para>
        /// 
        /// <br>-------------------- </br>
        /// <br>left |||XXX||| right </br>
        /// <br>-------------------- </br>
        ///  
        /// <para>OR</para>
        /// 
        /// <br>  top   </br>
        /// <br> |||||  </br>
        /// <br> |XXX|  </br>
        /// <br> |||||  </br>
        /// <br> bottom </br>
        /// 
        /// <para>The method acts out of the PERSPECTIVE OF THE ROOMS!</para>
        /// 
        /// <para>If room1 is on the LEFT side of the connector, then the connectorPoint1 must be on the RIGHT side of room1.</para>
        /// </summary>
        /// <param name="room1">Y_Room with connectorPoint1</param>
        /// <param name="connectorPoint1">The connector point on Y_Room 1</param>
        /// <param name="room2">Y_Room with connectorPoint2</param>
        /// <param name="connectorPoint2">The connector point on Y_Room 2</param>
        /// <returns></returns>
        private IWalkable connect(
            IWalkable room1,
            X_ConnectorPoint connectorPoint1,
            IWalkable room2,
            X_ConnectorPoint connectorPoint2
        )
        {
            // check possibilities
            // horizontal: left and right or right and left
            // vertical: top and bottom or bottom and top
            bool hCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Left && connectorPoint2.ConnectorSide == X_ConnectorSide.Right;
            bool hCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Right && connectorPoint2.ConnectorSide == X_ConnectorSide.Left;
            bool vCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Bottom && connectorPoint2.ConnectorSide == X_ConnectorSide.Top;
            bool vCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Top && connectorPoint2.ConnectorSide == X_ConnectorSide.Bottom;

            // horizontal connectors only with left and right or right and left
            // vertical connectors only with top and bottom or bottom and top
            if (!(
                ((hCheck1 || hCheck2) && _direction == X_DoorDirection.Horizontal) ||
                ((vCheck1 || vCheck2) && _direction == X_DoorDirection.Vertical))
                )
            {
                Logger.Error("Invalid connector combination");
            }

            /* left to right */
            if (hCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Right].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Left].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* right to left */
            else if (hCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Left].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Right].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* bottom to top */
            else if (vCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Top].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Bottom].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* top to bottom */
            else if (vCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Bottom].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Top].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }

            // the following flip of connectorPoint2 and connectorPoint1 is NOT a bug
            DoorRooms.Add(connectorPoint2.ConnectorSide, new List<IWalkable> { room1 });
            DoorRooms.Add(connectorPoint1.ConnectorSide, new List<IWalkable> { room2 });
            room1.DoorRooms.Add(connectorPoint2.ConnectorSide, new List<IWalkable> { this });
            room2.DoorRooms.Add(connectorPoint1.ConnectorSide, new List<IWalkable> { this });

            return this;
        }

        public void SplitConnectedCollisionModels()
        {
            foreach(var room in DoorRooms)
            {
                if (room.Key == X_ConnectorSide.Left || room.Key == X_ConnectorSide.Right)
                {
                    foreach (var r in room.Value)
                    {
                        // need vertical split
                        r.Collision.SplitCollisionVerticallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                    }
                }
                else
                {
                    foreach (var r in room.Value)
                    {
                        // need horizontal split                    
                        r.Collision.SplitCollisionHorizontallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.Orange, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            foreach(var door in Doors)
            {
                foreach(var d in door.Value)
                {
                    d.DrawOutline(gameTime, globalOffset, spriteBatch);
                }
            }

            //Factory_Debug.DrawPoint(_leftOrBottomConnector.Point.X, _leftOrBottomConnector.Point.Y, 11, Color.Red, spriteBatch);
            //Factory_Debug.DrawPoint(_rightOrTopConnector.Point.X, _rightOrTopConnector.Point.Y, 11, Color.Orange, spriteBatch);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _floor, Rect.Location.ToVector2(),
                new Rectangle(0, 0, _floor.Width, _floor.Height),
                Color.White, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
        }

        public void MoveTo(Point position)
        {
            var p = position - Rect.Location;
            Rect = new Rectangle(p.X, p.Y, Rect.Width, Rect.Height);
            foreach(var door in Doors)
            {
                foreach(var d in door.Value)
                {
                    d.MoveBy(p);
                }
            }
            Collision.MoveBy(p);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Door;
        }
    }
}
