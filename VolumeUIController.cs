using UnityEngine;
using UnityEngine.UI;

public class VolumeUIController : MonoBehaviour
{
    public GameObject volumeCanvas;  // Panel UI volume
    public Slider volumeSlider;      // Slider volume

    void Start()
    {
        volumeCanvas.SetActive(false); // Sembunyikan canvas saat awal
        volumeSlider.value = AudioListener.volume; // Set slider ke volume saat ini
        volumeSlider.onValueChanged.AddListener(SetVolume); // Event listener
    }

    public void ToggleVolumeCanvas()
    {
        volumeCanvas.SetActive(!volumeCanvas.activeSelf); // Munculkan/sembunyikan panel
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value; // Langsung set volume global
    }
}
