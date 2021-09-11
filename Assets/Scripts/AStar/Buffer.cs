using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buffer
{
    private byte[] buffer = null;
    public int Position { get; set; }

    public byte[] Data { get { return buffer; } }
    public Buffer(byte[] data)
    {
        Position = 0;
        buffer = new byte[data.Length];
        Array.Copy(data, buffer, data.Length);
    }

    public Buffer(long len)
    {
        Position = 0;
        buffer = new byte[len];
    }

    public int Lenght => buffer.Length;

    public void Write(byte[] data)
    {
        int len = data.Length;
        buffer = new byte[len];
        Array.Copy(data, buffer, len);
    }

    public char ReadUInt8()
    {
        int size = sizeof(char);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToChar(buf, 0);
    }


    public ushort ReadUInt16()
    {
        int size = sizeof(ushort);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToUInt16(buf, 0);
    }

    public short ReadInt16()
    {
        int size = sizeof(short);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToInt16(buf, 0);
    }

    public uint ReadUInt32()
    {
        int size = sizeof(uint);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToUInt32(buf, 0);
    }

    public int ReadInt32()
    {
        int size = sizeof(int);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToInt32(buf, 0);
    }

    public UInt64 ReadUInt64()
    {
        int size = sizeof(UInt64);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToUInt64(buf, 0);
    }

    public Int64 ReadInt64()
    {
        int size = sizeof(Int64);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToInt64(buf, 0);
    }

    public bool ReadBoolean()
    {
        int size = sizeof(bool);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToBoolean(buf, 0);
    }

    public float ReadFloat()
    {
        int size = sizeof(float);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToSingle(buf, 0);
    }

    public double ReadDouble()
    {
        int size = sizeof(double);
        byte[] buf = new byte[size];
        Array.Copy(buffer, Position, buf, 0, size);
        Position += size;
        return BitConverter.ToDouble(buf, 0);
    }
}