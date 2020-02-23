using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

using Microsoft.MixedReality.WebRTC.Unity;

using Newtonsoft.Json;
using WebSocket4Net;
using SuperSocket.ClientEngine;

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

        WebSocket ws;

        bool registered = false;

        readonly object messageQueueLock = new object();
        ConcurrentQueue<ReceivedMessage> receivedMessageQueue = new ConcurrentQueue<ReceivedMessage>();

        void Start()
        {
            ws = new WebSocket(serverUrl);

            ws.Opened += Websocket_Opened;
            ws.MessageReceived += Websocket_MessageReceived;
            ws.Closed += Websocket_Closed;
            ws.DataReceived += Websocket_DataReceived;
            ws.Error += Websocket_Error;

            ws.AutoSendPingInterval = 30;
            ws.EnableAutoSendPing = true;

            _ = WaitAndConnect();
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
        }

        private async Task WaitAndConnect()
        {
            await Task.Delay(3000);
            ws.Open();
        }

        private void OnDisable()
        {
            if (ws != null && ws.State == WebSocketState.Open)
            {
                ws.Close();
            }
        }

        

        private void Websocket_Error(object sender, ErrorEventArgs e)
        {
            Debug.Log("error");

        }

        private void Websocket_Closed(object sender, EventArgs e)
        {
            Debug.Log("closed");

        }

        private void Websocket_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.Log("data received");

        }

        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log($"message received: {e.Message}");
            // var message = e.Message;
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

        private void ProcessMessage(ReceivedMessage message)
        {
            if (message.IceServers != null)
            {
                foreach (var ice in message.IceServers)
                {
                    // Debug.Log(ice.Credential);
                }
            }

            Debug.Log($"Received SDP message: type={message.Type} data={message.Sdp}");

            switch (message.Type)
            {
                case "ping":
                    SendPong();
                    break;
                case "accept":
                    Debug.Log(message.IsExistUser);
                    /*
                    if (message.IsExistUser == null)
                    {
                        PeerConnection.Peer.CreateOffer();
                    }
                    else
                    {
                        if (message.IsExistUser == "true")
                        {
                            PeerConnection.Peer.CreateOffer();
                        }
                    }
                    */
                    break;
                case "offer":
                    Debug.Log("offer");
                    Debug.Log(message.Sdp);
                    _nativePeer.SetRemoteDescription("offer", message.Sdp);
                    var result = _nativePeer.CreateAnswer();
                    Debug.Log("create answer" + result);
                    break;
                case "answer":
                    Debug.Log("answer");
                    _nativePeer.SetRemoteDescription("answer", message.Sdp);
                    break;
                case "candidate":
                    Debug.Log("candidate");
                    var parts = message.Candidate.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    // Note the inverted arguments; candidate is last here, but first in OnIceCandiateReadyToSend
                    _nativePeer.AddIceCandidate(parts[2], int.Parse(parts[1]), parts[0]);
                    break;
            }
        }

        private void SendPong()
        {
            var message = new PongMessage()
            {
                Type = "pong"
            };
            SendWsMessage(message);
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            Debug.Log("opened");

            if (registered)
            {
                return;
            }
            registered = true;

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

        private void SendWsMessage(object message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);
            SendWsMessage(serializedMessage);
        }

        private void SendWsMessage(string message)
        {
            Debug.Log($"send: {message}");
            ws.Send(message);
        }


        #region ISignaler interface

        public override Task SendMessageAsync(Message message)
        {
            //Debug.Log("SendMessageAsync");
            //Debug.Log(message);
            Debug.Log(message.MessageType);

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
                var iceMessage = new IceMessage()
                {
                    Type = type,
                    Candidate = message.Data
                    /*
                    Ice = new Dictionary<string, string>
                    {
                        {"candidate", message.Data}
                    }
                    */
                };

                SendWsMessage(JsonConvert.SerializeObject(iceMessage));
            }
            else
            {
                var sdpMessage = new SdpMessage()
                {
                    Type = type,
                    Sdp = message.Data
                };

                SendWsMessage(JsonConvert.SerializeObject(sdpMessage));
            }

            return Task.CompletedTask;
            //if(message.MessageType == Message.WireMessageType.Answer)
            //throw new NotImplementedException();
        }

        #endregion


        protected override void OnIceCandiateReadyToSend(string candidate, int sdpMlineIndex, string sdpMid)
        {
            Debug.Log("OnIce");
            Debug.LogWarning("OnIce");
            //throw new NotImplementedException();
        }

        protected override void OnSdpOfferReadyToSend(string offer)
        {
            Debug.LogWarning("OnSdpOffer");
            //throw new NotImplementedException();
        }

        protected override void OnSdpAnswerReadyToSend(string answer)
        {
            Debug.LogWarning("OnSdpAnswer");
            //throw new NotImplementedException();
        }
    }
}