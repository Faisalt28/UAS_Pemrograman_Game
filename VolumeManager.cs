using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    private AudioSource[] audioSources;

    void Start()
    {
        // Cari semua AudioSource tanpa pengurutan (lebih cepat)
        audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        // Atur slider ke nilai awal volume
        volumeSlider.value = PlayerPrefs.GetFloat("volume", 1f);
        SetVolume(volumeSlider.value);

        // Pasang event listener ke slider
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        foreach (AudioSource source in audioSources)
        {
            source.volume = volume;
        }

        PlayerPrefs.SetFloat("volume", volume);
    }
}
