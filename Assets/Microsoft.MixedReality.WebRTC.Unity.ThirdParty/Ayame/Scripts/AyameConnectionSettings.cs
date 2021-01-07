using System;
using UnityEngine;

namespace Microsoft.MixedReality.WebRTC.Unity.ThirdParty.Ayame
{
    public class AyameConnectionSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string serverUrl = null;

        [NonSerialized]
        public string ServerUrl;

        [SerializeField]
        private string signalingKey = null;

        [NonSerialized]
        public string SignalingKey;

        [SerializeField]
        private string roomId = null;

        [NonSerialized]
        public string RoomId;

        [SerializeField]
        private bool autoConnection = false;

        [NonSerialized]
        public bool AutoConnection;


        public void OnAfterDeserialize()
        {
            ServerUrl = serverUrl;
            SignalingKey = signalingKey;
            RoomId = roomId;
            AutoConnection = autoConnection;
        }

        public void OnBeforeSerialize() { }
    }
}
