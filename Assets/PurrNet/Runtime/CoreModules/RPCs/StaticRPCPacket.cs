using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet
{
    public struct StaticRPCPacket : IPackedAuto, IRpc
    {
        public PackedUInt typeHash;
        public Size rpcId;
        public PlayerID senderId;
        public PlayerID? targetId;
        [DontDeltaCompress] public ByteData data;

        public ByteData rpcData
        {
            get { return data; }
            set { data = value; }
        }

        public PlayerID senderPlayerId => senderId;
        public PlayerID targetPlayerId
        {
            get => targetId ?? default;
            set => targetId = value;
        }
    }
}
