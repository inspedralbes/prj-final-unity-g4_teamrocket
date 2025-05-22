using UnityEngine;

public class MicrophoneListenerManager : MonoBehaviour
{
    public float micLoudness;
    private AudioClip micClip;
    private string micDevice;
    private int sampleWindow = 128;

    void Start()
    {
        micDevice = Microphone.devices[0];
        micClip = Microphone.Start(micDevice, true, 1, AudioSettings.outputSampleRate);
        Debug.Log("Micrófono iniciado: " + micDevice);
    }

    void Update()
    {
        micLoudness = GetMaxVolume();
        Debug.Log("Volumen actual: " + micLoudness);
    }

    float GetMaxVolume()
    {
        float maxLevel = 0;
        float[] waveData = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(micDevice) - sampleWindow + 1;
        if (micPosition < 0) return 0;

        micClip.GetData(waveData, micPosition);
        for (int i = 0; i < sampleWindow; ++i)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (maxLevel < wavePeak)
                maxLevel = wavePeak;
        }
        return maxLevel;
    }
}
