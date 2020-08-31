using PlanetaryAnnihilationToolkit.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PlanetaryAnnihilationToolkit.Formats.PapaFile
{
    public class PapaAnimation
    {
        public string Name { get; private set; }
        public float FPS { get; private set; }

        public Dictionary<ushort, List<PapaAnimationTransform>> BoneFrameTransforms { get; private set; } = new();

        internal PapaAnimation(ICollection<string> strings, PapaEncodingAnimation animation)
        {
            this.Name = strings.ElementAtOrDefault(animation.NameIndex);
            this.FPS = animation.FPS;

            foreach (ushort boneIndex in animation.Bones)
            {
                this.BoneFrameTransforms.Add(boneIndex, new List<PapaAnimationTransform>(animation.FrameCount));

                for (int i = 0; i < animation.FrameCount; i++)
                {
                    this.BoneFrameTransforms[boneIndex].Add(new PapaAnimationTransform(animation.Transforms[boneIndex * animation.FrameCount + i]));
                }
            }
        }
    }

    public struct PapaAnimationTransform
    {
        public Vector3 Translation { get; private set; }
        public Quaternion Rotation { get; private set; }

        internal PapaAnimationTransform(PapaEncodingAnimationTransform transform)
        {
            this.Translation = transform.Translation;
            this.Rotation = transform.Rotation;
        }
    }

    internal struct PapaEncodingAnimation
    {
        internal short NameIndex { get; private set; }
        internal float FPS { get; private set; }
        internal int FrameCount { get; private set; }

        internal List<ushort> Bones { get; private set; }
        internal List<PapaEncodingAnimationTransform> Transforms { get; private set; }

        internal PapaEncodingAnimation(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();

            ushort boneCount = br.ReadUInt16();
            this.Bones = new(boneCount);

            this.FrameCount = br.ReadInt32();
            this.Transforms = new(boneCount * this.FrameCount);

            uint fpsNumerator = br.ReadUInt32();
            uint fpsDenominator = br.ReadUInt32();
            this.FPS = fpsNumerator / fpsDenominator;

            long boneTableOffset = br.ReadInt64();
            long transformsOffset = br.ReadInt64();

            long returnOffset = br.BaseStream.Position;
            if (boneTableOffset > 0)
            {
                br.BaseStream.Seek(boneTableOffset, SeekOrigin.Begin);
                for (int i = 0; i < boneCount; i++)
                {
                    this.Bones.Add(br.ReadUInt16());
                }
            }
            if (transformsOffset > 0)
            {
                br.BaseStream.Seek(transformsOffset, SeekOrigin.Begin);
                for (int i = 0; i < boneCount * this.FrameCount; i++)
                {
                    this.Transforms.Add(new PapaEncodingAnimationTransform(br));
                }
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }

    internal struct PapaEncodingAnimationTransform
    {
        internal Vector3 Translation { get; private set; }
        internal Quaternion Rotation { get; private set; }

        internal PapaEncodingAnimationTransform(BinaryReader br)
        {
            this.Translation = br.ReadVector3();
            this.Rotation = br.ReadQuaternion();
        }
    }
}
