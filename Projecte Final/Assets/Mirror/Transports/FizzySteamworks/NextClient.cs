#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public class NextClient : NextCommon, IClient
    {
        public bool Connected { get; private set; }
        public bool Error { get; private set; }

        private TimeSpan ConnectionTimeout;
        private event Action<byte[], int> OnReceivedData;
        private event Action OnConnected;
        private event Action OnDisconnected;
        private Callback<SteamNetConnectionStatusChangedCallback_t> c_onConnectionChange = null;
        private CancellationTokenSource cancelToken;
        private TaskCompletionSource<Task> connectedComplete;
        private CSteamID hostSteamID = CSteamID.Nil;
        private HSteamNetConnection HostConnection;
        private List<Action> BufferedData;

        private NextClient(FizzySteamworks transport)
        {
            ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
            BufferedData = new List<Action>();
        }

        public static NextClient CreateClient(FizzySteamworks transport, string host)
        {
            NextClient c = new NextClient(transport);

            c.OnConnected += () => transport.OnClientConnected.Invoke();
            c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
            c.OnReceivedData += (data, ch) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), ch);

            try
            {
                SteamNetworkingUtils.InitRelayNetworkAccess();
                c.Connect(host);
            }
            catch (FormatException)
            {
                Debug.LogError("Formato de SteamID inválido");
                c.Error = true;
                c.OnConnectionFailed();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al conectar: {ex.Message}");
                c.Error = true;
                c.OnConnectionFailed();
            }

            return c;
        }

        private async void Connect(string host)
        {
            cancelToken = new CancellationTokenSource();
            c_onConnectionChange = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);

            try
            {
                hostSteamID = new CSteamID(UInt64.Parse(host));
                connectedComplete = new TaskCompletionSource<Task>();
                OnConnected += SetConnectedComplete;

                SteamNetworkingIdentity smi = new SteamNetworkingIdentity();
                smi.SetSteamID(hostSteamID);

                SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[]
                {
                    new SteamNetworkingConfigValue_t {
                        m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial,
                        m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                        m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 10000 }
                    },
                    new SteamNetworkingConfigValue_t {
                        m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected,
                        m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                        m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 30000 }
                    },
                    new SteamNetworkingConfigValue_t {
                        m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize,
                        m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                        m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1024 * 1024 }
                    }
                };

                HostConnection = SteamNetworkingSockets.ConnectP2P(ref smi, 0, options.Length, options);
                Debug.Log($"Iniciando conexión P2P con {hostSteamID}");

                Task connectedCompleteTask = connectedComplete.Task;
                Task timeOutTask = Task.Delay(ConnectionTimeout, cancelToken.Token);

                if (await Task.WhenAny(connectedCompleteTask, timeOutTask) != connectedCompleteTask)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        Debug.LogError("Conexión cancelada por el usuario");
                    }
                    else
                    {
                        Debug.LogError($"Timeout de conexión después de {ConnectionTimeout.TotalSeconds} segundos");
                    }
                    OnConnectionFailed();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error durante la conexión: {ex}");
                Error = true;
                OnConnectionFailed();
            }
            finally
            {
                OnConnected -= SetConnectedComplete;
            }
        }

        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
        {
            Debug.Log($"Estado cambiado: {param.m_info.m_eState} - {param.m_info.m_szEndDebug}");

            if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
            {
                Connected = true;
                OnConnected?.Invoke();
                
                if (BufferedData.Count > 0)
                {
                    Debug.Log($"Procesando {BufferedData.Count} mensajes en buffer");
                    foreach (Action a in BufferedData)
                    {
                        a();
                    }
                    BufferedData.Clear();
                }
            }
            else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || 
                     param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                Debug.Log($"Conexión cerrada: {param.m_info.m_szEndDebug}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (cancelToken != null && !cancelToken.IsCancellationRequested)
            {
                cancelToken.Cancel();
            }

            if (Connected)
            {
                InternalDisconnect();
            }

            if (HostConnection.m_HSteamNetConnection != 0)
            {
                Debug.Log("Cerrando conexión Steam");
                SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Desconexión normal", false);
                HostConnection.m_HSteamNetConnection = 0;
            }

            Dispose();
        }

        private void Dispose()
        {
            if (c_onConnectionChange != null)
            {
                c_onConnectionChange.Dispose();
                c_onConnectionChange = null;
            }
        }

        private void InternalDisconnect()
        {
            Connected = false;
            OnDisconnected?.Invoke();
            Debug.Log("Desconectado internamente");
        }

        public void ReceiveData()
        {
            IntPtr[] ptrs = new IntPtr[MAX_MESSAGES];
            int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(HostConnection, ptrs, MAX_MESSAGES);

            if (messageCount > 0)
            {
                for (int i = 0; i < messageCount; i++)
                {
                    (byte[] data, int ch) = ProcessMessage(ptrs[i]);
                    if (Connected)
                    {
                        OnReceivedData?.Invoke(data, ch);
                    }
                    else
                    {
                        BufferedData.Add(() => OnReceivedData?.Invoke(data, ch));
                    }
                }
            }
        }

        public void Send(byte[] data, int channelId)
        {
            try
            {
                EResult res = SendSocket(HostConnection, data, channelId);

                if (res == EResult.k_EResultNoConnection || res == EResult.k_EResultInvalidParam)
                {
                    Debug.LogError("Conexión perdida durante el envío");
                    InternalDisconnect();
                }
                else if (res != EResult.k_EResultOK)
                {
                    Debug.LogError($"Error al enviar: {res}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Excepción al enviar: {ex.Message}");
                InternalDisconnect();
            }
        }

        public void FlushData()
        {
            SteamNetworkingSockets.FlushMessagesOnConnection(HostConnection);
        }

        private void SetConnectedComplete() => connectedComplete?.SetResult(connectedComplete.Task);
        private void OnConnectionFailed() => OnDisconnected?.Invoke();
    }
}
#endif