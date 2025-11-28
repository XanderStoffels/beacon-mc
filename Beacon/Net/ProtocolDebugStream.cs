namespace Beacon.Net;

/// <summary>
/// A debug stream appends all content to a file.
/// </summary>
public class ProtocolDebugStream : Stream
{
    private readonly Stream _origin;
    private readonly DirectoryInfo _outputDir;
    private readonly FileInfo _logFile;

    public ProtocolDebugStream(Stream origin, DirectoryInfo outputDir)
    {
        _origin = origin;
        _outputDir = outputDir;
        
        if (!outputDir.Exists)
        {
            outputDir.Create();
        }
    }


    public override void Flush()
    {
        _origin.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // Read from the client (server-bound).
        return _origin.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _origin.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _origin.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        // Write to the client (client-bound).
        _origin.Write(buffer, offset, count);
    }

    public override bool CanRead => _origin.CanRead;
    public override bool CanSeek => _origin.CanSeek;
    public override bool CanWrite => _origin.CanWrite;
    public override long Length => _origin.Length;
    public override long Position {get => _origin.Position; set => _origin.Position = value;}
}