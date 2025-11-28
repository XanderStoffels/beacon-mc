using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Beacon.Api;
using Beacon.Net;

namespace Beacon.Util;

/// <summary>
/// A struct that allows writing common Minecraft data types to a span of bytes.
/// Keeps track of how many bytes have been written.
/// </summary>
/// <param name="span"></param>
public ref struct SpanByteWriter(Span<byte> span)
{
    private readonly Span<byte> _span = span;
    public int BytesWritten { get; private set; } = 0;

    public bool TryWriteVarInt(int value)
    {
        // Let VarInt.TryWrite handle the case where the position is out of bounds
        if (BytesWritten > _span.Length)
            return false;

        var currentSpan = _span[BytesWritten..];
        if (!VarInt.TryWrite(currentSpan, value, out var written))
        {
            return false;
        }

        BytesWritten += written;
        return true;
    }

    public bool TryWriteLong(long value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!BinaryPrimitives.TryWriteInt64BigEndian(currentSpan, value))
            return false;

        BytesWritten += 8;
        return true;
    }

    public bool TryWriteVarLong(long value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!VarLong.TryWrite(currentSpan, value, out var written))
            return false;

        BytesWritten += written;
        return true;
    }

    public bool TryWriteVarString(ReadOnlySpan<char> value)
    {
        var utf8ByteLength = Encoding.UTF8.GetByteCount(value);
        return TryWriteVarString(value, utf8ByteLength);
    }

    private bool TryWriteVarString(ReadOnlySpan<char> value, int utf8ByteLength)
    {
        var currentSpan = _span[BytesWritten..];
        if (!VarInt.TryWrite(currentSpan, utf8ByteLength, out var written))
            return false;

        BytesWritten += written;
        return TryWriteUtf8String(value);
    }

    private bool TryWriteUtf8String(ReadOnlySpan<char> value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!Encoding.UTF8.TryGetBytes(value, currentSpan, out var written))
            return false;

        BytesWritten += written;
        return true;
    }

    public bool TryWriteUuid(Guid guid)
    {
        var currentSpan = _span[BytesWritten..];
        if (!guid.TryWriteBytes(currentSpan, true, out var written))
            return false;

        Debug.Assert(written == 16);
        BytesWritten += 16;

        return true;
    }
    
    public bool TryWriteInt(int entityId)
    {
        var currentSpan = _span[BytesWritten..];
        if (!BitConverter.TryWriteBytes(currentSpan, entityId))
            return false;

        BytesWritten += 4;
        return true;
    }

    public bool TryWriteBool(bool value)
    {
        if (!MemoryMarshal.TryWrite(_span[BytesWritten..], value))
            return false;

        BytesWritten += 1;
        return true;
    }

    public bool TryWrite(Span<byte> buffer)
    {
        if (BytesWritten + buffer.Length > _span.Length)
            return false;

        var currentSpan = _span[BytesWritten..];
        if (!buffer.TryCopyTo(currentSpan))
            return false;

        BytesWritten += buffer.Length;
        return true;
    }
    
    public bool TryWriteUnsignedByte(byte value)
    {
        if (!MemoryMarshal.TryWrite(_span[BytesWritten..], value))
            return false;

        BytesWritten += 1;
        return true;
    }

    public bool TryWriteSignedByte(sbyte value)
    {
        if (!MemoryMarshal.TryWrite(_span[BytesWritten..], value))
            return false;

        BytesWritten += 1;
        return true;
    }

    public bool TryWriteSignedShort(short value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!BitConverter.TryWriteBytes(currentSpan, value))
            return false;

        BytesWritten += 2;
        return true;
    }

    public bool TryWriteUnsignedShort(ushort value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!BitConverter.TryWriteBytes(currentSpan, value))
            return false;

        BytesWritten += 2;
        return true;
    }

    public bool TryWriteFloat(float value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!BitConverter.TryWriteBytes(currentSpan, value))
            return false;

        BytesWritten += 4;
        return true;
    }
    
    public bool TryWriteDouble(double value)
    {
        var currentSpan = _span[BytesWritten..];
        if (!BitConverter.TryWriteBytes(currentSpan, value))
            return false;

        BytesWritten += 8;
        return true;
    }

    public bool TryWriteTextComponent(object value)
    {
        throw new NotImplementedException();
    }
    
    public bool TryWriteJsonTextComponent(object value)
    {
        throw new NotImplementedException();
    }
    
    public bool TryWriteIdentifier(ReadOnlySpan<char> value)
    {
        return value.Length <= 32767 && TryWriteVarString(value);
    }
    
    public bool TryWriteEntityMetadata(object value)
    {
        throw new NotImplementedException();
    }
    
    public bool TryWriteSlot(object value)
    {
        throw new NotImplementedException();
    }
    
    public bool TryWriteHashedSlot(object value)
    {
        throw new NotImplementedException();
    }
    
    public bool TryWriteNbt(object value)
    {
        throw new NotImplementedException();
    }
    
    public bool TryWritePosition(long value)
    {
        return TryWriteLong(value);
    }
    
    public bool TryWritePosition(Position value)
    {
        return TryWriteLong(value.ToLong());
    }
    
    public bool TryWriteAngle(byte value)
    {
        if (!MemoryMarshal.TryWrite(_span[BytesWritten..], value))
            return false;

        BytesWritten += 1;
        return true;
    }
    
    /// <summary>
    /// Writes the following to the Span: <br/>
    /// - VarInt: Number of longs in the following array. May be 0 (if no bits are set). <br/>
    /// - Array of Long: A packed representation of the bit set.
    /// </summary>
    /// <param name="bitArray"></param>
    /// <returns></returns>
    public bool TryWriteBitSet(BitArray bitArray)
    {
        var amountOfBits = bitArray.Length;
        var amountOfLongs = (amountOfBits + 63) / 64;
    
        // First write the number of longs
        if (!TryWriteVarInt(amountOfLongs))
            return false;
    
        // Process each long.
        for (var i = 0; i < amountOfLongs; i++)
        {
            var value = 0L;
            var startBit = i * 64;
            var endBit = Math.Min(startBit + 64, amountOfBits);
    
            for (var bitIndex = startBit; bitIndex < endBit; bitIndex++)
            {
                if (bitArray[bitIndex])
                {
                    value |= 1L << (bitIndex - startBit);
                }
            }
    
            // Write the long to the span.
            if (!TryWriteLong(value))
                return false;
        }
        return true;
    }

    public bool TryWriteFixedBitset(BitArray bitArray)
    {
        var amountOfBytes = (bitArray.Length + 7) / 8;
        if (_span.Length - BytesWritten < amountOfBytes)
            return false;
        
        var buffer = _span[BytesWritten..];
        for (var i = 0; i < bitArray.Length; i++)
        {
            if (bitArray[i])
            {
                buffer[i / 8] |= (byte)(1 << (i % 8));
            }
        }
        
        BytesWritten += amountOfBytes;
        return true;
    }
    

}