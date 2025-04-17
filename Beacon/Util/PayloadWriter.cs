
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
    
    public void WriteString(ReadOnlySpan<char> value)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteVarString(value);
    }

    public void WriteUuid(Guid guid)
    {
        if (_isFaulted) return;
        _isFaulted = !_writer.TryWriteUuid(guid);
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
}