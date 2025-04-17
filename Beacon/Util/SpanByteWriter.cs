using System.Diagnostics;
using System.Text;
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
    private int _position = 0;
        
    public int BytesWritten => _position;

    public bool TryWriteVarInt(int value)
    {
        // Let VarInt.TryWrite handle the case where the position is out of bounds
        if (_position > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!VarInt.TryWrite(currentSpan, value, out var written))
        {
            return false;
        }
        _position += written;
        return true;
    }
        
    public bool TryWriteLong(long value)
    {
        // Longs are always 8 bytes long
        if (_position + 8 > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!ProtocolHelper.TryWriteLong(value, currentSpan))
            return false;
            
        _position += 8;
        return true;
    }
    
    public bool TryWriteVarLong(long value)
    {
        // Let VarLong.TryWrite handle the case where the position is out of bounds
        if (_position > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!VarLong.TryWrite(currentSpan, value, out var written))
            return false;
            
        _position += written;
        return true;
    }
        
    public bool TryWriteVarString(ReadOnlySpan<char> value)
    {
        var utf8ByteLength = Encoding.UTF8.GetByteCount(value);
        return TryWriteVarString(value, utf8ByteLength); 
    }

    public bool TryWriteVarString(ReadOnlySpan<char> value, int utf8ByteLength)
    {
        // Fail fast if there is not enough space for the string.
        // Another VarInt will be written before the string, so we let VarInt.TryWrite handle that case.
        if (_position + utf8ByteLength > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!VarInt.TryWrite(currentSpan, utf8ByteLength, out var written))
            return false;
            
        _position += written;
        return TryWriteUtf8String(value);
    }
        
    public bool TryWriteUtf8String(ReadOnlySpan<char> value)
    {
        // Encoding.UTF8.GetByteCount will fail if the string is too long, no need to check here.
        if (_position > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!Encoding.UTF8.TryGetBytes(value, currentSpan, out var written))
            return false;
            
        _position += written;
        return true;
    }

    public bool TryWriteUuid(Guid guid)
    {
        // A guid is always 16 bytes, so we can fail fast if not enough space.
        if (_position + 16 > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!guid.TryWriteBytes(currentSpan, true, out var written))
            return false;
        
        Debug.Assert(written == 16);
        _position += 16;

        return true;
    }

    public bool TryWrite(Span<byte> buffer)
    {
        if (_position + buffer.Length > _span.Length)
            return false;
        
        var currentSpan = _span[_position..];
        if (!buffer.TryCopyTo(currentSpan))
            return false;
        
        _position += buffer.Length;
        return true;
    }
}