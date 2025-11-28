
using System.Buffers;
using Beacon.Api;
using Beacon.Net;

namespace Beacon.Util;

/// <summary>
/// A utility struct for writing common Minecraft Protocol data structures to a byte buffer.
/// You can keep writing data to the buffer until you are done or the buffer is full.
/// Afterward, you can check if the write was successful and how many bytes were written.
/// </summary>
/// <remarks>
/// Once the buffer is full, the writer will stop writing, mark itself as faulted and any subsequent write operations will be ignored
/// At that point, the data that is already written in the buffer is basically useless.
/// </remarks>
public ref struct PayloadWriter
{
    private SpanByteWriter _writer;
    private bool _isFaulted;

    public PayloadWriter(Span<byte> buffer, int packetId)
    {
        _writer = new SpanByteWriter(buffer);
        WriteVarInt(packetId);
    }

    public void WriteVarInt(int packetId)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteVarInt(packetId);
    }
    
    public void WriteLong(long value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteLong(value);
    }
    
    public void WriteVarLong(long value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteVarLong(value);
    }
    
    public void WriteVarString(ReadOnlySpan<char> value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteVarString(value);
    }

    public void WriteUuid(Guid guid)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteUuid(guid);
    }
    
    /// <summary>
    /// Signed 32-bit integer, two's complement
    /// </summary>
    /// <param name="entityId"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void WriteInt(int entityId)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteInt(entityId);
    }

    public void Write(Span<byte> buffer)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWrite(buffer);
    }

    public bool IsSuccess(out int bytesWritten)
    {
        bytesWritten = _isFaulted ? 0 : _writer.BytesWritten;
        return !_isFaulted;
    }


    public void WriteBool(bool isHardcore)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteBool(isHardcore);
    }

    /// <summary>
    /// Writes a list of VarStrings to the buffer, prefixed with the total length of the list as a VarInt.
    /// </summary>
    /// <param name="strings"></param>
    public void WritePrefixedStringArray(List<string> strings)
    {
        if (_isFaulted) return;
        WriteVarInt(strings.Count);
        foreach (var s in strings)
        {
            WriteVarString(s);
        }
    }
    
    public void WritePrefixedIdentifierArray(List<string> identifiers)
    {   
        if (_isFaulted) return;
        WriteVarInt(identifiers.Count);
        foreach (var identifier in identifiers)
        {
            WriteIdentifier(identifier);
        }
    }

    public void WriteIdentifier(string dimensionName)
    {
        // Identifiers are just strings limited in length.
        WriteVarString(dimensionName);
    }

    public void WriteUnsignedByte(byte value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteUnsignedByte(value);
    }
    
    public void WriteSignedByte(sbyte value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteSignedByte(value);
    }
    
    public void WriteSignedShort(short value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteSignedShort(value);
    }
    
    public void WriteUnsignedShort(ushort value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteUnsignedShort(value);
    }

    public void WritePosition(Position deathLocation)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWritePosition(deathLocation);
    }
}