using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Verifica si la conexión ya tiene un personaje (esto evita que el host cree un personaje adicional)
        if (conn.identity == null)
        {
            base.OnServerAddPlayer(conn);
        }
    }
}