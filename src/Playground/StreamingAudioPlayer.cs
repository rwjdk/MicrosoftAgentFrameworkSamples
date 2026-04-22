using NAudio.Wave;

sealed class StreamingAudioPlayer : IDisposable
{
    private static readonly TimeSpan MinimumPreroll = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan StreamStartPadding = TimeSpan.FromMilliseconds(450);
    private readonly BufferedWaveProvider _bufferedWaveProvider;
    private readonly WaveOutEvent _player;
    private readonly object _syncRoot = new();
    private readonly int _minimumPrerollBytes;
    private readonly byte[] _streamStartPaddingBytes;
    private string? _activeItemId;
    private bool _isPlaybackStarted;

    public StreamingAudioPlayer(WaveFormat waveFormat)
    {
        _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
        {
            BufferDuration = TimeSpan.FromSeconds(20),
            DiscardOnBufferOverflow = true,
            ReadFully = true,
        };

        _minimumPrerollBytes = (int)(waveFormat.AverageBytesPerSecond * MinimumPreroll.TotalSeconds);
        _streamStartPaddingBytes = new byte[(int)(waveFormat.AverageBytesPerSecond * StreamStartPadding.TotalSeconds)];

        _player = new WaveOutEvent
        {
            DesiredLatency = 120,
        };
        _player.Init(_bufferedWaveProvider);
    }

    public void Enqueue(string itemId, byte[] audioBytes)
    {
        if (audioBytes.Length == 0)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (!string.Equals(_activeItemId, itemId, StringComparison.Ordinal))
            {
                _activeItemId = itemId;

                if (_streamStartPaddingBytes.Length > 0)
                {
                    _bufferedWaveProvider.AddSamples(_streamStartPaddingBytes, 0, _streamStartPaddingBytes.Length);
                }
            }

            _bufferedWaveProvider.AddSamples(audioBytes, 0, audioBytes.Length);
            TryStartPlayback(force: false);
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _player.Stop();
            _bufferedWaveProvider.ClearBuffer();
            _activeItemId = null;
            _isPlaybackStarted = false;
        }
    }

    public void FlushPendingAudio()
    {
        lock (_syncRoot)
        {
            TryStartPlayback(force: true);
        }
    }

    public void Dispose()
    {
        _player.Dispose();
    }

    private void TryStartPlayback(bool force)
    {
        if (_isPlaybackStarted)
        {
            return;
        }

        if (!force && _bufferedWaveProvider.BufferedBytes < _minimumPrerollBytes)
        {
            return;
        }

        if (_bufferedWaveProvider.BufferedBytes == 0)
        {
            return;
        }

        _player.Play();
        _isPlaybackStarted = true;
    }
}
