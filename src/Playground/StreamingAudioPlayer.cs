using NAudio.Wave;

sealed class StreamingAudioPlayer : IDisposable
{
    private readonly BufferedWaveProvider _bufferedWaveProvider;
    private readonly IWavePlayer _player;
    private readonly object _syncRoot = new();

    public StreamingAudioPlayer(WaveFormat waveFormat)
    {
        _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
        {
            BufferDuration = TimeSpan.FromSeconds(20),
            DiscardOnBufferOverflow = true,
        };

        _player = new WaveOutEvent();
        _player.Init(_bufferedWaveProvider);
        _player.Play();
    }

    public void Enqueue(byte[] audioBytes)
    {
        if (audioBytes.Length == 0)
        {
            return;
        }

        lock (_syncRoot)
        {
            _bufferedWaveProvider.AddSamples(audioBytes, 0, audioBytes.Length);
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            _bufferedWaveProvider.ClearBuffer();
        }
    }

    public void Dispose()
    {
        _player.Dispose();
    }
}