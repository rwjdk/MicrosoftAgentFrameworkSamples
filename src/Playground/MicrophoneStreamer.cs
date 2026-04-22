using System.Threading.Channels;
using NAudio.Wave;

namespace Playground;

sealed class MicrophoneStreamer : IAsyncDisposable
{
    private readonly WaveInEvent _waveIn;
    private readonly Channel<byte[]> _audioChannel;
    private readonly CancellationTokenRegistration _cancellationRegistration;
    private bool _isDisposed;

    public MicrophoneStreamer(CancellationToken cancellationToken)
    {
        WaveFormat waveFormat = new(24_000, 16, 1);
        _audioChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

        _waveIn = new WaveInEvent
        {
            BufferMilliseconds = 100,
            NumberOfBuffers = 3,
            WaveFormat = waveFormat,
        };

        _waveIn.DataAvailable += HandleDataAvailable;
        _waveIn.RecordingStopped += (_, args) =>
        {
            if (args.Exception is not null)
            {
                _audioChannel.Writer.TryComplete(args.Exception);
                return;
            }

            _audioChannel.Writer.TryComplete();
        };

        _cancellationRegistration = cancellationToken.Register(Stop);
    }

    public void Start() => _waveIn.StartRecording();

    public void Stop()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            _waveIn.StopRecording();
        }
        catch
        {
            _audioChannel.Writer.TryComplete();
        }
    }

    public IAsyncEnumerable<byte[]> ReadAllAsync(CancellationToken cancellationToken)
        => _audioChannel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return ValueTask.CompletedTask;
        }

        _isDisposed = true;
        _cancellationRegistration.Dispose();
        _waveIn.DataAvailable -= HandleDataAvailable;
        _waveIn.Dispose();
        _audioChannel.Writer.TryComplete();
        return ValueTask.CompletedTask;
    }

    private void HandleDataAvailable(object? sender, WaveInEventArgs args)
    {
        if (args.BytesRecorded <= 0)
        {
            return;
        }

        byte[] chunk = new byte[args.BytesRecorded];
        Buffer.BlockCopy(args.Buffer, 0, chunk, 0, args.BytesRecorded);
        _audioChannel.Writer.TryWrite(chunk);
    }
}