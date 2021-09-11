using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH_WEST = 0,
    NORTH = 1,
    NORTH_EAST = 2,
    WEST = 3,
    CENTER = 4,
    EAST = 5,
    SOUTH_WEST = 6,
    SOUTH = 7,
    SOUTH_EAST = 8
}

public enum NodeFields
{
    X = 0,
    Y = 1,
    VALUE = 2,
    G = 3,
    H = 4,
    F = 5,
    PARENT = 6,
    POINTER = 7,
    MARK = 8,
    CHILDREN = 9,
    LIMIT = 10,
}

public enum Constant
{
    NULL = 0xFFFFFF,

    KEEP = 0x800000,    //连通点

    VV = 3,
    HV = 4,
    SV = 5,
    VH = 3,
    HH = 4
}

public class Point
{
    public int x = 0;
    public int y = 0;
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class AStar
{
    private static int[] FourOffset = new int[]{
        //-1, -1, (int)Constant.SV, (int)Direction.NORTH_WEST, 
        0, -1, (int)Constant.VV, (int)Direction.NORTH, 
        //1, -1, (int)Constant.SV, (int)Direction.NORTH_EAST,
        -1, 0, (int)Constant.HV, (int)Direction.WEST,
        1, 0, (int)Constant.HV, (int)Direction.EAST,
        //-1, 1, (int)Constant.SV, (int)Direction.SOUTH_WEST, 
        0, 1, (int)Constant.VV, (int)Direction.SOUTH,
        //1, 1, (int)Constant.SV, (int)Direction.SOUTH_EAST
    };

    private static int[] EightOffset = new int[]{
        -1, -1, (int)Constant.SV, (int)Direction.NORTH_WEST,
        0, -1, (int)Constant.VV, (int)Direction.NORTH,
        1, -1, (int)Constant.SV, (int)Direction.NORTH_EAST,
        -1, 0, (int)Constant.HV, (int)Direction.WEST,
        1, 0, (int)Constant.HV, (int)Direction.EAST,
        -1, 1, (int)Constant.SV, (int)Direction.SOUTH_WEST,
        0, 1, (int)Constant.VV, (int)Direction.SOUTH,
        1, 1, (int)Constant.SV, (int)Direction.SOUTH_EAST
    };

    private List<int> open = null;
    private int[] map = null;
    private int closeId = 0;
    private int openId = 0;

    public AStar()
    {
        closeId = 0;
        openId = 0;
    }


    public short Width { get; set; }
    public short Height { get; set; }

    public short CellWidth { get; set; }
    public short CellHeight { get; set; }

    public short LimitX { get; set; }
    public short LimitY { get; set; }

    public void Init(Buffer buffer)
    {
        Width = buffer.ReadInt16();
        Height = buffer.ReadInt16();
        CellWidth = buffer.ReadInt16();
        CellHeight = buffer.ReadInt16();
        LimitX = buffer.ReadInt16();
        LimitY = buffer.ReadInt16();

        map = new int[LimitX * LimitY * (int)NodeFields.LIMIT];
        openId = 0;
        closeId = 0;
        int cursor = 0;
        for (int y = 0; y < LimitY; ++y)
        {
            int index = LimitX * y * (ushort)NodeFields.LIMIT;
            int x = 0;
            for (int i = 0; i < LimitX; ++i)
            {
                map[(int)(index + (int)NodeFields.X)] = x++;
                map[(int)(index + (int)NodeFields.Y)] = y;
                map[(int)(index + (int)NodeFields.VALUE)] = (int)(buffer.ReadInt32() & 0x00FF);
                map[(int)(index + (int)NodeFields.MARK)] = 0;
                map[(int)(index + (int)NodeFields.CHILDREN)] = (int)Constant.NULL;
                index += (int)NodeFields.LIMIT;
                cursor++;
            }
        }
        //读取连通点
        //TODO
    }

    private int FindReplacer(int startX, int startY, int endX, int endY, int auth)
    {
        int index = (LimitX * endY + endX) * (int)NodeFields.LIMIT;
        int value = map[index + (int)NodeFields.VALUE];
        if ((value & 0x0F) <= auth)
        {
            return index;
        }
        int minHNode = (int)Constant.NULL;
        int minH = (int)Constant.NULL;
        int[] rangeX = new int[2];
        int[] rangeY = new int[2];
        int radius = 1;
        while ((int)Constant.NULL == minHNode)
        {
            rangeX[0] = endX - radius;
            rangeX[1] = endX + radius;
            rangeY[0] = endY - radius;
            rangeY[1] = endY + radius;

            int y;
            int x;
            int h;
            int limit;
            for (int i = 0; i < rangeY.Length; ++i)
            {
                y = rangeY[i];
                if (y < 0 || y >= LimitY)
                {
                    continue;
                }

                limit = rangeX[1] < LimitX ? rangeX[1] : LimitX;
                for (x = rangeX[0] > 0 ? rangeX[0] : 0; x < limit; ++x)
                {
                    index = (LimitX * y + x) * (int)NodeFields.LIMIT;
                    value = map[index + (int)NodeFields.VALUE];
                    if ((value & 0x0F) > auth)
                    {
                        continue;
                    }

                    h = Mathf.Abs(startY - y) * (int)Constant.VH + Mathf.Abs(startX - x) * (int)Constant.HH;
                    if (h < minH)
                    {
                        minH = h;
                        minHNode = index;
                    }
                }
            }

            rangeY[0] += 1;
            rangeY[1] -= 1;
            for (int i = 0; i < rangeX.Length; ++i)
            {
                x = rangeX[i];
                if (x < 0 || x >= LimitX)
                {
                    continue;
                }

                limit = rangeY[1] < LimitY ? rangeY[1] : LimitY;
                for (y = rangeY[0] > 0 ? rangeY[0] : 0; y < limit; ++y)
                {
                    index = (LimitX * y + x) * (int)NodeFields.LIMIT;
                    value = map[index + (int)NodeFields.VALUE];
                    if ((value & 0x0F) > auth)
                    {
                        continue;
                    }

                    h = Mathf.Abs(startY - y) * (int)Constant.VH + Mathf.Abs(startX - x) * (int)Constant.HH;
                    if (h < minH)
                    {
                        minH = h;
                        minHNode = index;
                    }
                }
            }
            ++radius;
        }
        return minHNode;
    }

    public List<Point> Find(int startX, int startY, int endX, int endY, int auth, bool eightDir = true, List<Point> backdata = null)
    {
        if (LimitX < startX || LimitY < startY || LimitX < endX || LimitY < endY)
        {
            return null;
        }
        if (startX == endX && startY == endY)
        {
            return null;
        }
        open = new List<int>();
        closeId = closeId + 1;
        openId = openId + 2;

        int counter = 0;
        int node = FindReplacer(startX, startY, startX, startY, auth);
        startX = map[node + (int)NodeFields.X];
        startY = map[node + (int)NodeFields.Y];
        node = FindReplacer(endX, endY, endX, endY, auth);
        endX = map[node + (int)NodeFields.X];
        endY = map[node + (int)NodeFields.Y];

        int end = (LimitX * endY + endX) * (int)NodeFields.LIMIT;
        int minHNode = PushToOpen(startX, startY, 0, Mathf.Abs(endY - startY) * (int)Constant.VH + Mathf.Abs(endX - startX) * (int)Constant.HH, (int)Constant.NULL);

        int child;
        int parent;
        int value;
        int g;
        int i;
        int[] OffsetList = null;
        if (eightDir)
        {
            OffsetList = EightOffset;
        }
        else
        {
            OffsetList = FourOffset; ;
        }
        int len = OffsetList.Length;

        int col = endX;
        int row = endY;

        int x = startX;
        int y = startY;
        int direction = (x < col ? 0 : (x == col ? 1 : 2)) + (y < row ? 0 : (y == row ? 3 : 6));

        while (0 != open.Count)
        {
            if (++counter >= 30000)
            {
                return BuildPath(minHNode);
            }

            node = PopFromOpen();
            if (node == end)
            {
                return BuildPath(node);
            }

            col = map[node + (int)NodeFields.X];
            row = map[node + (int)NodeFields.Y];

            parent = map[node + (int)NodeFields.PARENT];
            if ((int)Constant.NULL != parent)
            {
                x = map[parent + (int)NodeFields.X];
                y = map[parent + (int)NodeFields.Y];
                direction = (x < col ? 0 : (x == col ? 1 : 2)) + (y < row ? 0 : (y == row ? 3 : 6));
            }

            //检测连通点
            for (i = 0; i < len; i += 4)
            {
                x = col + OffsetList[i];
                if (x < 0 || x >= LimitX)
                {
                    continue;
                }
                y = row + OffsetList[i + 1];
                if (y < 0 || y >= LimitY)
                {
                    continue;
                }
                child = (LimitX * y + x) * (int)NodeFields.LIMIT;
                value = map[child + (int)NodeFields.VALUE];
                if ((value & 0x0F) > auth)
                {
                    continue;
                }
                if (map[child + (int)NodeFields.MARK] == closeId)
                {
                    continue;
                }
                if (OffsetList[i + 3] == direction)
                {
                    g = map[node + (int)NodeFields.G];
                }
                else
                {
                    g = map[node + (int)NodeFields.G] + OffsetList[i + 2];
                }
                if (map[child + (int)NodeFields.MARK] == openId)
                {
                    if (g < map[child + (int)NodeFields.G])
                    {
                        map[child + (int)NodeFields.PARENT] = node;
                        map[child + (int)NodeFields.G] = g;
                        map[child + (int)NodeFields.F] = g + map[child + (int)NodeFields.H];
                        ShiftUp(map[child + (int)NodeFields.POINTER], child);
                    }
                }
                else
                {
                    if (null != backdata)
                    {
                        backdata.Add(new Point(map[child + (int)NodeFields.X], map[child + (int)NodeFields.Y]));
                    }
                    child = PushToOpen(x, y, g, Mathf.Abs(endY - y) * (int)Constant.VH + Mathf.Abs(endX - x) * (int)Constant.HH, node);

                    if (map[child + (int)NodeFields.H] < map[minHNode + (int)NodeFields.H])
                    {
                        minHNode = child;
                    }
                }
            }
            //检测跳跃点
            child = map[node + (int)NodeFields.CHILDREN];
            if ((int)Constant.NULL == child)
            {
                continue;
            }
            value = map[child + (int)NodeFields.VALUE];
            if ((value & 0x0F) > auth)
            {
                continue;
            }
            if (map[child + (int)NodeFields.MARK] == closeId)
            {
                continue;
            }
            g = map[node + (int)NodeFields.G] + 0;
            if (map[child + (int)NodeFields.MARK] == openId)
            {
                if (g < map[child + (int)NodeFields.G])
                {
                    map[child + (int)NodeFields.PARENT] = node;
                    map[child + (int)NodeFields.G] = g;
                    map[child + (int)NodeFields.F] = g + map[child + (int)NodeFields.H];
                    ShiftUp(map[child + (int)NodeFields.POINTER], child);
                }
            }
            else
            {
                x = map[child + (int)NodeFields.X];
                y = map[child + (int)NodeFields.Y];
                child = PushToOpen(x, y, g, Mathf.Abs(endY - y) * (int)Constant.VH + Mathf.Abs(endX - x) * (int)Constant.HH, node);
                if (map[child + (int)NodeFields.H] < map[minHNode + (int)NodeFields.H])
                {
                    minHNode = child;
                }
            }
        }
        return BuildPath(minHNode);
    }

    private int PushToOpen(int x, int y, int g, int h, int parent)
    {
        int index = (LimitX * y + x) * (int)NodeFields.LIMIT;
        map[index + (int)NodeFields.PARENT] = parent;
        map[index + (int)NodeFields.G] = g;
        map[index + (int)NodeFields.H] = h;
        map[index + (int)NodeFields.F] = (ushort)(g + h);
        map[index + (int)NodeFields.MARK] = openId;
        open.Add(index);
        ShiftUp(open.Count - 1, index);
        return index;
    }

    private int PopFromOpen()
    {
        int result = open[0];
        int value = open[open.Count - 1];
        open.RemoveAt(open.Count - 1);
        if (0 != open.Count)
        {
            ShiftDown(0, value);
        }
        map[result + (int)NodeFields.MARK] = closeId;
        return result;
    }

    private void ShiftDown(int i, int e)
    {
        int l = (i << 1) + 1;
        int r = l + 1;
        int count = open.Count;
        int v;
        while (l < count)
        {
            v = open[l];
            if (r < count && map[v + (int)NodeFields.F] > map[open[r] + (int)NodeFields.F])
            {
                v = open[r];
                l = r;
            }
            if (map[e + (int)NodeFields.F] < map[v + (int)NodeFields.F])
            {
                break;
            }
            map[v + (int)NodeFields.POINTER] = i;
            open[i] = v;
            i = l;
            l = (i << 1) + 1;
            r = l + 1;
        }
        map[e + (int)NodeFields.POINTER] = i;
        open[i] = e;
    }

    private void ShiftUp(int i, int e)
    {
        int p = (i - 1) >> 1;
        int v;
        while (0 != i)
        {
            v = open[p];
            if (map[v + (int)NodeFields.F] <= map[e + (int)NodeFields.F])
            {
                break;
            }

            map[v + (int)NodeFields.POINTER] = i;
            open[i] = v;
            i = p;
            p = (i - 1) >> 1;
        }
        map[e + (int)NodeFields.POINTER] = i;
        open[i] = e;
    }

    public bool IsNeedKeep(Point pos)
    {
        int index = (LimitX * (int)pos.y + (int)pos.x) * (int)NodeFields.LIMIT;
        return (map[index + (int)NodeFields.VALUE] & (int)Constant.KEEP) != 0;
    }

    private List<Point> BuildPath(int node)
    {
        if ((int)Constant.NULL == node || (int)Constant.NULL == map[node + (int)NodeFields.PARENT])
        {
            return null;
        }
        List<Point> result = new List<Point>();
        Point start = new Point(map[node + (int)NodeFields.X], map[node + (int)NodeFields.Y]);
        node = map[node + (int)NodeFields.PARENT];
        result.Add(start);

        Point end = new Point(map[node + (int)NodeFields.X], map[node + (int)NodeFields.Y]);
        int old = node;
        node = map[node + (int)NodeFields.PARENT];

        Point target;
        while ((int)Constant.NULL != node)
        {
            target = new Point(map[node + (int)NodeFields.X], map[node + (int)NodeFields.Y]);
            var keep = map[old + (int)NodeFields.VALUE] & (int)Constant.KEEP;
            var dir = this.direction(start, end, target);
            if ((map[old + (int)NodeFields.VALUE] & (int)Constant.KEEP) == 0 || 0 != this.direction(start, end, target))
            {
                result.Add(end);
                start = end;
            }
            end = target;
            old = node;
            node = map[node + (int)NodeFields.PARENT];
        }
        return result;
    }

    private int direction(Point start, Point end, Point target)
    {
        return (start.x - target.x) * (end.y - target.y) - (end.x - target.x) * (start.y - target.y);
    }
}