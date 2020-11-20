using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

using Newtonsoft.Json;
using WebSocket4Net;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.WebRTC.Unity.ThirdParty.Ayame
{
    public class AyameSignaler : Signaler
    {
        [SerializeField]
        private AyameConnectionSettings connectionSettings = null;

        public AyameConnectionSettings ConnectionSettings => connectionSettings;

        [SerializeField]
        private bool printDebugLog = false;

        private WebSocket ws;
        private bool tryToConnect = false;

        private readonly object messageQueueLock = new object();
        private readonly ConcurrentQueue<ReceivedMessage> receivedMessageQueue = new ConcurrentQueue<ReceivedMessage>();

        async void Start()
        {
            ws = new WebSocket(connectionSettings.ServerUrl);

            ws.Opened += Websocket_Opened;
            ws.MessageReceived += Websocket_MessageReceived;
            ws.Closed += Websocket_Closed;

            ws.EnableAutoSendPing = false;

            if (connectionSettings.AutoConnection)
            {
                await Task.Delay(3000);
                tryToConnect = true;
            }
        }

        protected override void Update()
        {
            base.Update();

            lock (messageQueueLock)
            {
                while (receivedMessageQueue.TryDequeue(out var message))
                {
                    ProcessMessage(message);
                }
            }

            if (tryToConnect)
            {
                ConnectToServer();
            }
        }

        protected override void OnDisable()
        {
            if (ws != null && ws.State == WebSocketState.Open)
            {
                ws.Close();
            }
            base.OnDisable();
        }

        public void Connect()
        {
            tryToConnect = true;
        }

        private void ConnectToServer()
        {
            tryToConnect = false;
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif
            ws.Open();
        }


        #region Websocket Events
        private void Websocket_Opened(object sender, EventArgs e)
        {
            SendRegisterMessage();
        }

        private async void Websocket_Closed(object sender, EventArgs e)
        {
            if (printDebugLog)
            {
                Debug.Log("Websocket closed");
                Debug.Log(e);
            }

            if (connectionSettings.AutoConnection)
            {
                await Task.Delay(1000);

                if (printDebugLog)
                {
                    Debug.Log("Reconnect to server");
                }
                tryToConnect = true;
            }
        }

        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (printDebugLog)
            {
                Debug.Log($"Message received: {e.Message}");
            }

            try
            {
                var message = JsonConvert.DeserializeObject<ReceivedMessage>(e.Message);
                lock (messageQueueLock)
                {
                    receivedMessageQueue.Enqueue(message);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }
        #endregion

        private void ProcessMessage(ReceivedMessage message)
        {
            switch (message.Type)
            {
                case "ping":
                    SendPongMessage();
                    break;
                case "accept":
                    if (message.IsExistClient)
                    {
                        PeerConnection.StartConnection();
                    }
                    break;
                case "offer":
                    PeerConnection.HandleConnectionMessageAsync(message.ToWebRTCMessage()).ContinueWith(_ =>
                    {
                        PeerConnection.Peer.CreateAnswer();
                    }, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.RunContinuationsAsynchronously);
                    break;
                case "answer":
                    _ = PeerConnection.HandleConnectionMessageAsync(message.ToWebRTCMessage());
                    break;
                case "candidate":
                    PeerConnection.Peer.AddIceCandidate(message.ToIceCandidate());
                    break;
            }
        }
        
        private void SendRegisterMessage()
        {
            var clientId = Guid.NewGuid().ToString("N").Substring(0, 10);

            var message = new RegisterMessage()
            {
                Type = "register",
                SignalingKey = connectionSettings.SignalingKey,
                RoomId = connectionSettings.RoomId,
                ClientId = clientId,
            };

            SendWsMessage(message);
        }

        private void SendPongMessage()
        {
            var message = new PongMessage()
            {
                Type = "pong"
            };

            SendWsMessage(message);
        }

        private void SendWsMessage(object message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);
            SendWsMessage(serializedMessage);
        }

        private void SendWsMessage(string message)
        {
            if (printDebugLog)
            {
                Debug.Log($"Send message: {message}");
            }
            ws.Send(message);
        }

        public override Task SendMessageAsync(WebRTC.SdpMessage message)
        {
            var sdpMessage = new SdpMessage()
            {
                Type = message.Type.ToString().ToLower(),
                Sdp = message.Content
            };
            SendWsMessage(sdpMessage);
            return Task.CompletedTask;
        }

        public override Task SendMessageAsync(IceCandidate candidate)
        {
            var iceMessage = new IceMessage()
            {
                Ice = new Ice()
                {
                    Candidate = candidate.Content,
                    SdpMLineIndex = candidate.SdpMlineIndex,
                    SdpMid = candidate.SdpMid,
                }
            };
            SendWsMessage(iceMessage);
            return Task.CompletedTask;
        }
    }
}

