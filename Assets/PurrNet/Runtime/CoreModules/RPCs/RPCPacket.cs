using System;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet
{
    public struct RPCPacket : IPackedAuto, IRpc
    {
        public NetworkID networkId;
        public SceneID sceneId;
        public PlayerID senderId;
        public PlayerID? targetId;
        public Size rpcId;

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

    internal readonly struct RPC_ID : IEquatable<RPC_ID>
    {
        public readonly PackedUInt typeHash;
        public readonly SceneID sceneId;
        public readonly NetworkID networkId;
        private readonly Size rpcId;
        private readonly Size childId;

        public RPC_ID(RPCPacket packet)
        {
            sceneId = packet.sceneId;
            networkId = packet.networkId;
            rpcId = packet.rpcId;
            typeHash = default;
            childId = default;
        }

        public RPC_ID(StaticRPCPacket packet)
        {
            sceneId = default;
            networkId = default;
            rpcId = packet.rpcId;
            typeHash = packet.typeHash;
            childId = default;
        }

        public RPC_ID(ChildRPCPacket packet)
        {
            sceneId = packet.sceneId;
            networkId = packet.networkId;
            rpcId = packet.rpcId;
            typeHash = default;
            childId = packet.childId;
        }

        public override int GetHashCode()
        {
            return sceneId.GetHashCode() ^
                   networkId.GetHashCode() ^
                   rpcId.GetHashCode() ^
                   typeHash.GetHashCode() ^
                   childId.GetHashCode();
        }

        public bool Equals(RPC_ID other)
        {
            return typeHash == other.typeHash &&
                   sceneId.Equals(other.sceneId) &&
                   networkId.Equals(other.networkId) &&
                   rpcId == other.rpcId &&
                   childId == other.childId;
        }

        public override bool Equals(object obj)
        {
            return obj is RPC_ID other && Equals(other);
        }
    }

    internal class RPC_DATA
    {
        public RPC_ID rpcid;
        public RPCPacket packet;
        public RPCSignature sig;
        public BitPacker stream;
    }

    internal class CHILD_RPC_DATA
    {
        public RPC_ID rpcid;
        public ChildRPCPacket packet;
        public RPCSignature sig;
        public BitPacker stream;
    }

    internal class STATIC_RPC_DATA
    {
        public RPC_ID rpcid;
        public StaticRPCPacket packet;
        public RPCSignature sig;
        public BitPacker stream;
    }
}
