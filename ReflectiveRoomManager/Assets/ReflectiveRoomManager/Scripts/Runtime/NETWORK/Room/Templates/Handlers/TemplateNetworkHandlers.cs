using System.Linq;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Handlers
{
    using Messages;

    /// <summary>
    /// Handles template-related network messages.
    /// </summary>
    public static class TemplateNetworkHandlers
    {
#if REFLECTIVE_SERVER
        private static bool _serverHandlersRegistered;

        public static void RegisterServerHandlers()
        {
            if (_serverHandlersRegistered)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] Server handlers already registered");
                return;
            }

            NetworkServer.RegisterHandler<RequestTemplateListMessage>(OnServerRequestTemplateList);
            NetworkServer.RegisterHandler<CreateRoomFromTemplateMessage>(OnServerCreateRoomFromTemplate);

            _serverHandlersRegistered = true;
            Debug.Log("[TemplateNetworkHandlers] Server handlers registered");
        }

        public static void UnregisterServerHandlers()
        {
            if (!_serverHandlersRegistered) return;

            NetworkServer.UnregisterHandler<RequestTemplateListMessage>();
            NetworkServer.UnregisterHandler<CreateRoomFromTemplateMessage>();

            _serverHandlersRegistered = false;
            Debug.Log("[TemplateNetworkHandlers] Server handlers unregistered");
        }

        private static void OnServerRequestTemplateList(NetworkConnectionToClient conn, RequestTemplateListMessage msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] RoomManagerBase instance not found");
                return;
            }

            if (!roomManager.EnableTemplates)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] Template system not enabled");
                conn.Send(new TemplateListMessage(System.Array.Empty<TemplateData>()));
                return;
            }

            var manager = roomManager.TemplateManager;
            if (manager == null)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] TemplateManager is null");
                conn.Send(new TemplateListMessage(System.Array.Empty<TemplateData>()));
                return;
            }

            var templates = msg.HasCategoryFilter
                ? manager.GetTemplatesByCategory(msg.GetCategoryFilter().Value)
                : manager.GetAllTemplates();

            var templateDataList = templates.Select(TemplateData.FromTemplate).ToArray();

            conn.Send(new TemplateListMessage(templateDataList));

            Debug.Log($"[TemplateNetworkHandlers] Sent {templateDataList.Length} templates to {conn.connectionId}");
        }

        private static void OnServerCreateRoomFromTemplate(NetworkConnectionToClient conn, CreateRoomFromTemplateMessage msg)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] RoomManagerBase instance not found");
                conn.Send(RoomCreationErrorMessage.SystemDisabled());
                return;
            }

            if (!roomManager.EnableTemplates)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] Template system not enabled");
                conn.Send(RoomCreationErrorMessage.SystemDisabled());
                return;
            }

            var templateOverride = msg.Override.ToRoomTemplateOverride();
            roomManager.CreateRoomFromTemplate(msg.TemplateID, templateOverride, conn);
        }
#endif

#if REFLECTIVE_CLIENT
        private static bool _clientHandlersRegistered;

        public static void RegisterClientHandlers()
        {
            if (_clientHandlersRegistered)
            {
                Debug.LogWarning("[TemplateNetworkHandlers] Client handlers already registered");
                return;
            }

            NetworkClient.RegisterHandler<TemplateListMessage>(OnClientTemplateList);
            NetworkClient.RegisterHandler<RoomCreationErrorMessage>(OnClientCreationError);

            _clientHandlersRegistered = true;
            Debug.Log("[TemplateNetworkHandlers] Client handlers registered");
        }

        public static void UnregisterClientHandlers()
        {
            if (!_clientHandlersRegistered) return;

            NetworkClient.UnregisterHandler<TemplateListMessage>();
            NetworkClient.UnregisterHandler<RoomCreationErrorMessage>();

            _clientHandlersRegistered = false;
            Debug.Log("[TemplateNetworkHandlers] Client handlers unregistered");
        }

        private static void OnClientTemplateList(TemplateListMessage msg)
        {
            Debug.Log($"[TemplateNetworkHandlers] Received {msg.Count} templates from server");
            TemplateEvents.InvokeTemplateListReceived(msg.Templates);
        }

        private static void OnClientCreationError(RoomCreationErrorMessage msg)
        {
            Debug.LogWarning($"[TemplateNetworkHandlers] Room creation error: {msg.ErrorMessage}");
            TemplateEvents.InvokeRoomCreationError(msg.ErrorMessage);
        }

        public static void ClearClientEvents()
        {
            TemplateEvents.ClearEvents();
        }
#endif
    }
}
