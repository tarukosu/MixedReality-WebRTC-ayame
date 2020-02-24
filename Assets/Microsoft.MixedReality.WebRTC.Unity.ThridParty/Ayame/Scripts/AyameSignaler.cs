using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

using Newtonsoft.Json;
using WebSocket4Net;
// using SuperSocket.ClientEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.WebRTC.Unity.ThridParty.Ayame
{
    public class AyameSignaler : Signaler
    {
        [SerializeField]
        string serverUrl = "wss://ayame-lite.shiguredo.jp/signaling";

        [SerializeField]
        string signalingKey = "";

        [SerializeField]
        string roomId = "";

        [SerializeField]
        bool autoConnection = true;

        [SerializeField]
        bool printDebugLog = false;

        WebSocket ws;
        bool tryToConnect = false;

        readonly object messageQueueLock = new object();
        ConcurrentQueue<ReceivedMessage> receivedMessageQueue = new ConcurrentQueue<ReceivedMessage>();

        async void Start()
        {
            ws = new WebSocket(serverUrl);

            ws.Opened += Websocket_Opened;
            ws.MessageReceived += Websocket_MessageReceived;
            ws.Closed += Websocket_Closed;
            // ws.Error += Websocket_Error;

            ws.AutoSendPingInterval = 30;
            ws.EnableAutoSendPing = true;

            if (autoConnection)
            {
                await Task.Delay(3000);
                tryToConnect = true;
            }

            // _ = WaitAndConnect();
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
                Connect();
            }
        }

        private void Connect()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif
            tryToConnect = false;
            ws.Open();
        }

        private async Task WaitAndConnect()
        {
            await Task.Delay(3000);
            tryToConnect = true;
            // ws.Open();
        }

        private void OnDisable()
        {
            if (ws != null && ws.State == WebSocketState.Open)
            {
                ws.Close();
            }
        }

        #region Websocket Events
        private void Websocket_Opened(object sender, EventArgs e)
        {
            SendRegisterMessage();
        }

        /*
        private void Websocket_Error(object sender, ErrorEventArgs e)
        {
            Debug.LogError(e.Exception);
        }
        */

        private async void Websocket_Closed(object sender, EventArgs e)
        {
            if (printDebugLog)
            {
                Debug.Log("Websocket closed");
            }

            if (autoConnection)
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
                        PeerConnection.Peer.CreateOffer();
                    }
                    break;
                case "offer":
                    _nativePeer.SetRemoteDescription("offer", message.Sdp);
                    _nativePeer.CreateAnswer();
                    break;
                case "answer":
                    _nativePeer.SetRemoteDescription("answer", message.Sdp);
                    break;
                case "candidate":
                    _nativePeer.AddIceCandidate(message.Ice.SdpMid, message.Ice.SdpMLineIndex, message.Ice.Candidate);
                    break;
            }
        }

        private void SendRegisterMessage()
        {
            var clientId = Guid.NewGuid().ToString("N").Substring(0, 10);

            var message = new RegisterMessage()
            {
                Type = "register",
                Key = signalingKey,
                RoomId = roomId,
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

        #region ISignaler interface

        public override Task SendMessageAsync(Message message)
        {
            var type = "";
            switch (message.MessageType)
            {
                case Message.WireMessageType.Offer:
                    type = "offer";
                    break;
                case Message.WireMessageType.Answer:
                    type = "answer";
                    break;
                case Message.WireMessageType.Ice:
                    type = "candidate";
                    break;
            }

            if (message.MessageType == Message.WireMessageType.Ice)
            {
                var iceParts = message.Data.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var iceMessage = new IceMessage()
                {
                    Type = type,
                    Ice = new Ice()
                    {
                        Candidate = iceParts[0],
                        SdpMLineIndex = int.Parse(iceParts[1]),
                        SdpMid = iceParts[2],
                    }
                };
                SendWsMessage(iceMessage);
            }
            else
            {
                var sdpMessage = new SdpMessage()
                {
                    Type = type,
                    Sdp = message.Data
                };
                SendWsMessage(sdpMessage);
            }

            return Task.CompletedTask;
        }

        #endregion


        protected override void OnIceCandiateReadyToSend(string candidate, int sdpMlineIndex, string sdpMid)
        {
        }

        protected override void OnSdpOfferReadyToSend(string offer)
        {
        }

        protected override void OnSdpAnswerReadyToSend(string answer)
        {
        }
    }
}