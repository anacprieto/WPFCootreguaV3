using System.Threading.Tasks;
using UI.Audios;

namespace Domain.UIServices.Audio;

public interface IAudioManager
{
    Task Play(string audioName);
    Task PlayLoop(string audioName);
    Task Stop();
}
