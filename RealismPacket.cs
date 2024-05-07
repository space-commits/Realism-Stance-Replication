using Fika.Core.Networking;
using LiteNetLib.Utils;
using UnityEngine;

namespace StanceReplication
{
    public struct RealismPacket : INetSerializable
    {
        public float SprintAnimationVarient;
        public string ProfileId;
        public bool IsPatrol;
        public Vector3 WeapPosition;
        public Quaternion Rotation;

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            WeapPosition = reader.GetVector3();
            Rotation = reader.GetQuaternion();
            IsPatrol = reader.GetBool();
            SprintAnimationVarient = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(WeapPosition);
            writer.Put(Rotation);
            writer.Put(IsPatrol);
            writer.Put(SprintAnimationVarient);
        }
    }
}
