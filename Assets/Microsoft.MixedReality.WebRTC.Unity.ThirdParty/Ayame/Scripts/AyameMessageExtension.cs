using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Microsoft.MixedReality.WebRTC.Unity.ThirdParty.Ayame
{
    static class ReceivedMessageExtensions
    {
        public static WebRTC.SdpMessage ToWebRTCMessage(this ReceivedMessage message)
        {
            switch (message.Type)
            {
                case "offer":
                    return new WebRTC.SdpMessage()
                    {
                        Type = SdpMessageType.Offer,
                        Content = message.Sdp
                    };
                case "answer":
                    return new WebRTC.SdpMessage()
                    {
                        Type = SdpMessageType.Answer,
                        Content = message.Sdp
                    };
                default:
                    return null;
            }
        }
        public static WebRTC.IceCandidate ToIceCandidate(this ReceivedMessage message)
        {
            switch (message.Type)
            {
                case "candidate":
                    return new WebRTC.IceCandidate()
                    {
                        Content = message.Ice.Candidate,
                        SdpMid = message.Ice.SdpMid,
                        SdpMlineIndex = message.Ice.SdpMLineIndex
                    };
                default:
                    return null;
            }
        }
    }
}

