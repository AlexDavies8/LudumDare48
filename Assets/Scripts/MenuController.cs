using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject _leaderboardEntryPrefab = null;
    [SerializeField] private Transform _leaderboardContainer = null;
    [SerializeField] private LeaderboardHandler _leaderboardHandler = null;

    [SerializeField] private Slider _sfxSlider = null;
    [SerializeField] private Slider _musicSlider = null;

    [SerializeField] private AudioMixer _master = null;

    private void Awake()
    {
        _leaderboardHandler.DownloadHighscores(InitializeLeaderboard);

        if (!PlayerPrefs.HasKey("SFXVolume"))
            SetSFXVolume(1f);
        else
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
        if (!PlayerPrefs.HasKey("MusicVolume"))
            SetMusicVolume(1f);
        else
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));

        _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        _master.SetFloat("MusicVolume", 20f * Mathf.Log10(volume + 0.001f));
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        _master.SetFloat("SFXVolume", 20f * Mathf.Log10(volume + 0.001f));
    }

    public void InitializeLeaderboard(LeaderboardHandler.LeaderboardEntry[] entries)
    {
        entries = entries.OrderBy(e => -e.score).ToArray();
        for (int i = 0; i < entries.Length; i++)
        {
            GameObject entryGO = Instantiate(_leaderboardEntryPrefab, _leaderboardContainer);
            var entry = entryGO.GetComponent<LeaderboardEntryObj>();
            entry.NameText.text = entries[i].name;
            entry.ScoreText.text = entries[i].score.ToString();
            //entry.DateText.text = entries[i].date;
        }
    }
}
