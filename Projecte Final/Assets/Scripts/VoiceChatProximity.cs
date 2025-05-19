using UnityEngine;
using Mirror;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VoiceChatProximity : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxVoiceDistance = 10f; // Distancia máxima para escuchar voz
    [SerializeField] private int sampleRate = 16000; // Frecuencia de muestreo (16 kHz para optimizar)
    [SerializeField] private float transmissionInterval = 0.1f; // Cada cuánto se envía audio (en segundos)

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
        // Espera inicialización de Mirror/Steam
        yield return new WaitForSeconds(1f);

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected!");
            yield break;
        }

        string micDevice = Microphone.devices[0];
        audioSource.clip = Microphone.Start(micDevice, true, 1, sampleRate);
        audioSource.loop = true;

        // Espera hasta que el micrófono esté activo
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

            // Captura los datos del micrófono
            microphoneBuffer = new float[audioSource.clip.samples];
            audioSource.clip.GetData(microphoneBuffer, 0);

            // Convierte a bytes (simulando compresión básica)
            compressedBuffer = new byte[microphoneBuffer.Length * sizeof(float)];
            System.Buffer.BlockCopy(microphoneBuffer, 0, compressedBuffer, 0, compressedBuffer.Length);

            // Envía a la red
            CmdSendVoiceData(compressedBuffer);
        }
    }

    [Command]
    private void CmdSendVoiceData(byte[] voiceData)
    {
        // Filtro de proximidad: reenvía solo a jugadores cercanos
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
        if (!isLocalPlayer) // Evita auto-escucha
        {
            // Convierte bytes a audio
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