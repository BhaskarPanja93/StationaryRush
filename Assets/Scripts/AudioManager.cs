using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject playerEye;
    [SerializeField] private AudioClip _bgMusic;
    [SerializeField] private AudioClip _gameOver;
    [SerializeField] private AudioClip _dayLoss;
    [SerializeField] private AudioClip _dayProfit;
    [SerializeField] private AudioClip _invalid;
    [SerializeField] private AudioClip _rackItemClick;
    [SerializeField] private AudioClip _tableItemClick;
    [SerializeField] private AudioClip _restock;
    private Dictionary<string, AudioSource> _audioSources;

    private AudioSource AttachClip(AudioClip audioClip)
    {
        var source = playerEye.AddComponent<AudioSource>();
        source.clip = audioClip;
        source.transform.SetParent(playerEye.transform);
        _audioSources.Add(audioClip.name, source);
        return source;
    }
    
    private void Awake()
    {
        _audioSources = new Dictionary<string, AudioSource>();
        var oneTimers = new List<AudioClip>
        {
            _gameOver,
            _dayLoss,
            _dayProfit,
            _invalid,
            _rackItemClick,
            _tableItemClick,
            _restock
        };

        foreach (var audioClip in oneTimers)
            AttachClip(audioClip);
        var bgSource = AttachClip(_bgMusic);
        bgSource.playOnAwake = true;
        bgSource.loop = true;
        bgSource.volume = 0.25f;
        bgSource.Play();
    }

    private void Play(string audioName)
    {
        _audioSources[audioName].Stop();
        _audioSources[audioName].Play();
    }

    public void PlayGameOver()
    {
        Play(_gameOver.name);
    }

    public void PlayDayLoss()
    {
        Play(_dayLoss.name);
    }

    public void PlayDayProfit()
    {
        Play(_dayProfit.name);
    }

    public void PlayInvalid()
    {
        Play(_invalid.name);
    }

    public void PlayRackItemClick()
    {
        Play(_rackItemClick.name);
    }

    public void PlayTableItemClick()
    {
        Play(_tableItemClick.name);
    }

    public void PlayRestock()
    {
        Play(_restock.name);
    }
}
