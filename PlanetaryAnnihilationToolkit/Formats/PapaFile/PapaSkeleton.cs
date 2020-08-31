using PlanetaryAnnihilationToolkit.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PlanetaryAnnihilationToolkit.Formats.PapaFile
{
    public class PapaSkeleton
    {
        public List<PapaBone> Bones { get; private set; } = new();

        internal PapaSkeleton(ICollection<string> strings, PapaEncodingSkeleton skeleton)
        {
            foreach (PapaEncodingBone encodingBone in skeleton.Bones)
            {
                this.Bones.Add(new PapaBone(strings, encodingBone));
            }
        }
    }

    public class PapaBone
    {
        public string Name { get; private set; }
        public short Id { get; private set; }
        public short ParentId { get; private set; }

        public Vector3 Translation { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Matrix4x4 ShearAndScale { get; private set; }
        public Matrix4x4 Bind2BoneTransform { get; private set; }

        internal PapaBone(ICollection<string> strings, PapaEncodingBone bone)
        {
            this.Name = strings.ElementAt(bone.NameIndex);
            this.ParentId = bone.ParentIndex;
            this.Translation = bone.Translation;
            this.Rotation = bone.Rotation;
            this.ShearAndScale = bone.ShearAndScale;
            this.Bind2BoneTransform = bone.Bind2BoneTransform;
        }
    }

    internal struct PapaEncodingSkeleton
    {
        internal List<PapaEncodingBone> Bones { get; private set; }

        internal PapaEncodingSkeleton(BinaryReader br)
        {
            ushort boneCount = br.ReadUInt16();
            ushort[] padding = new ushort[] { br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16() };
            long bonesOffset = br.ReadInt64();

            this.Bones = new List<PapaEncodingBone>(boneCount);

            long returnOffset = br.BaseStream.Position;
            if (bonesOffset > 0)
            {
                for (int i = 0; i < boneCount; i++)
                {
                    this.Bones.Add(new PapaEncodingBone((short)i, br));
                }
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }

    internal struct PapaEncodingBone
    {
        internal short Id { get; private set; }
        internal short NameIndex { get; private set; }
        internal short ParentIndex { get; private set; }
        internal Vector3 Translation { get; private set; }
        internal Quaternion Rotation { get; private set; }
        internal Matrix4x4 ShearAndScale { get; private set; }
        internal Matrix4x4 Bind2BoneTransform { get; private set; }

        internal PapaEncodingBone(short id, BinaryReader br)
        {
            this.Id = id;
            this.NameIndex = br.ReadInt16();
            this.ParentIndex = br.ReadInt16();
            this.Translation = br.ReadVector3();
            this.Rotation = br.ReadQuaternion();
            this.ShearAndScale = br.ReadMatrix3x3();
            this.Bind2BoneTransform = br.ReadMatrix4x4();

            if (this.NameIndex < 0)
            {
                throw new Exception("Found a bone with no Name");
            }
        }
    }
}
