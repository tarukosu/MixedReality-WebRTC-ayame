using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Microsoft.MixedReality.WebRTC.Unity.ThirdParty.Ayame
{
    [Preserve]
    public class RegisterMessage
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("roomId")]
        public string RoomId { set; get; }

        [JsonProperty("clientId")]
        public string ClientId { set; get; }

        [JsonProperty("authnMetaData")]
        public string AuthnMetaData { set; get; }

        [JsonProperty("signalingKey")]
        public string SignalingKey { set; get; }
    }

    [Preserve]
    public class PongMessage
    {
        [JsonProperty("type")]
        public string Type { set; get; }
    }

    [Preserve]
    public class SdpMessage
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("sdp")]
        public string Sdp { set; get; }
    }

    [Preserve]
    public class IceMessage
    {
        [JsonProperty("type")]
        public string Type { get; } = "candidate";

        [JsonProperty("ice")]
        public Ice Ice { set; get; }
    }

    [Preserve]
    public class IceServerMessage
    {
        [JsonProperty("urls")]
        public List<string> Urls { set; get; }

        [JsonProperty("credential")]
        public string Credential { set; get; }
    }

    [Preserve]
    public class ReceivedMessage
    {
        [JsonProperty("type")]
        public string Type { set; get; }

        [JsonProperty("authzMetadata")]
        public string AuthzMetadata { set; get; }

        [JsonProperty("iceServers")]
        public List<IceServerMessage> IceServers { set; get; }

        [JsonProperty("isExistClient")]
        public bool IsExistClient { set; get; }

        [JsonProperty("sdp")]
        public string Sdp { set; get; }

        [JsonProperty("ice")]
        public Ice Ice { set; get; }
    }

    [Preserve]
    public class Ice
    {
        [JsonProperty("candidate")]
        public string Candidate { set; get; }

        [JsonProperty("sdpMid")]
        public string SdpMid { set; get; }

        [JsonProperty("sdpMLineIndex")]
        public int SdpMLineIndex { set; get; }
    }
}
