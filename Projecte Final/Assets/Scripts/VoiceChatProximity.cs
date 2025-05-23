using UnityEngine;
using Mirror;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VoiceChatProximity : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxVoiceDistance = 10f;
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private float transmissionInterval = 0.1f;

    private AudioSource audioSource;
    private bool isRecording = false;
    private float[] microphoneBuffer;
    private byte[] compressedBuffer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (isLocalPlayer)
        {
            StartCoroutine(InitMicrophone());
        }
    }

    IEnumerator InitMicrophone()
    {
        yield return new WaitForSeconds(1f);

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected!");
            yield break;
        }

        string micDevice = Microphone.devices[0];
        audioSource.clip = Microphone.Start(micDevice, true, 1, sampleRate);
        audioSource.loop = true;

        while (!(Microphone.GetPosition(micDevice) > 0)) { }

        audioSource.Play();
        isRecording = true;
        StartCoroutine(TransmitVoice());
    }

    IEnumerator TransmitVoice()
    {
        while (isRecording)
        {
            yield return new WaitForSeconds(transmissionInterval);

            microphoneBuffer = new float[audioSource.clip.samples];
            audioSource.clip.GetData(microphoneBuffer, 0);

            compressedBuffer = new byte[microphoneBuffer.Length * sizeof(float)];
            System.Buffer.BlockCopy(microphoneBuffer, 0, compressedBuffer, 0, compressedBuffer.Length);

            CmdSendVoiceData(compressedBuffer);
        }
    }

    [Command]
    private void CmdSendVoiceData(byte[] voiceData)
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.identity != null && conn.identity != netIdentity)
            {
                float distance = Vector3.Distance(transform.position, conn.identity.transform.position);
                if (distance <= maxVoiceDistance)
                {
                    TargetReceiveVoiceData(conn, voiceData);
                }
            }
        }
    }

    [TargetRpc]
    private void TargetReceiveVoiceData(NetworkConnection target, byte[] voiceData)
    {
        if (!isLocalPlayer)
        {
            float[] receivedData = new float[voiceData.Length / sizeof(float)];
            System.Buffer.BlockCopy(voiceData, 0, receivedData, 0, voiceData.Length);

            AudioClip clip = AudioClip.Create("Voice", receivedData.Length, 1, sampleRate, false);
            clip.SetData(receivedData, 0);
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDestroy()
    {
        if (isRecording)
        {
            Microphone.End(null);
        }
    }
}