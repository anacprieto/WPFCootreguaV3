using WPFCootreguaV2.Presentation.Shared;
using Domain.UIServices.Audio;
using NAudio.Wave;
using System.IO;
using System.Speech.Synthesis;
using UI.Audios;
using System.Threading.Tasks;
using System.Threading;
using ArisWPF.Presentation.Shared;
using System;
using WPFCootreguaV2.Domain.Variables;

namespace Domain.UIServices.Audio;

public class TextToAudioManager: IAudioManager
{
    private CancellationTokenSource _cts = new();
    private SpeechSynthesizer _speechSynth = new();
    private bool _isAudioOn;
    private Task? _voiceTask = null;

    public TextToAudioManager()
    {
        _speechSynth.SelectVoice("Microsoft Sabina Desktop");
    }
    public async Task PlayLoop(string audioName)
    {
        if (_voiceTask != null && !_voiceTask.IsCompleted)
        {
            await Stop();
        }
        _voiceTask = VoiceLoop(audioName);
        _isAudioOn = true;
    }

    public async Task Play(string audioName)
    {
        if (_voiceTask != null && !_voiceTask.IsCompleted)
        {
            await Stop();
        }
        _isAudioOn = true;
        _voiceTask = Voice(audioName);
    }

    public async Task Stop()
    {
        _speechSynth.SpeakAsyncCancelAll();
        _cts.Cancel();
        if (_voiceTask != null)
        {
            await _voiceTask;
            _voiceTask.Dispose();
        }
        _isAudioOn = false;
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    private async Task VoiceLoop(string audioName)
    {
        _speechSynth.SpeakAsyncCancelAll();
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                _speechSynth.SpeakAsync((string?)ConstantRetriever.Get(typeof(AudioTexts),audioName) ?? "");
                await Task.Delay(10000, _cts.Token);
            }
        }
        catch { }
        
    }

    private Task Voice(string audioName)
    {
        _speechSynth.SpeakAsyncCancelAll();
        _speechSynth.SpeakAsync((string?)ConstantRetriever.Get(typeof(AudioTexts), audioName) ?? "");
        _isAudioOn = false;
        return Task.CompletedTask;
    }
}

public class FileToAudioManager : IAudioManager, IDisposable
{
    private CancellationTokenSource _cts = new();
    private AudioFileReader? _audioFile;
    private WaveOutEvent? _outputDevice;
    private bool _isAudioOn;
    private Task? _voiceTask = null;

    public FileToAudioManager()
    {
        _audioFile = null;
        _outputDevice = null;
    }
    public async Task PlayLoop(string audioName)
    {
        if (_voiceTask != null && !_voiceTask.IsCompleted)
        {
            await Stop();
        }
        _isAudioOn = true;
        _voiceTask = VoiceLoop(audioName);
    }

    public async Task Play(string audioName)
    {
        if (_voiceTask != null && !_voiceTask.IsCompleted)
        {
            await Stop();
        }
        _isAudioOn = true;
        _voiceTask = Voice(audioName);
    }

    public async Task Stop()
    {
        _cts.Cancel();
        if (_voiceTask != null)
        {
            await _voiceTask;
            _voiceTask.Dispose();
            _audioFile?.Dispose();
            _outputDevice?.Dispose();
        }
        _isAudioOn = false;
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    private async Task VoiceLoop(string audioName)
    {
        var mp3Path = Path.Combine(AppInfo.APP_DIR, "Assets/Audios/"+audioName.ToLower() + ".mp3");
        _audioFile = new AudioFileReader(mp3Path);
        _outputDevice = new WaveOutEvent();
        _outputDevice.Init(_audioFile);

        //_outputDevice.PlaybackStopped += (sender, eventargs) =>
        //{
        //    EventLogger.SaveLog(EventType.Info, "El audio ha sido parado.", eventargs);
        //    if (_audioFile != null && _audioFile.Position != null) _audioFile.Position = 0;
        //};

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                _outputDevice.Stop();
                _outputDevice.Play();
                await Task.Delay(10000, _cts.Token);
                
            }
        }
        catch { }

    }

    private Task Voice(string audioName)
    {
        var mp3Path = Path.Combine(AppInfo.APP_DIR, "Assets/Audios/" + audioName.ToLower() + ".mp3");
        _audioFile = new AudioFileReader(mp3Path);
        _outputDevice = new WaveOutEvent();
        _outputDevice.Init(_audioFile);
        _outputDevice.Stop();
        _outputDevice.Play();
        _isAudioOn = false;
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

