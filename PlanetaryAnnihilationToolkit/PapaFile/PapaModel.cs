using PlanetaryAnnihilationToolkit.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PlanetaryAnnihilationToolkit.PapaFile
{
    public class PapaModel
    {
        public string Name { get; private set; }
        public PapaSkeleton Skeleton { get; private set; }
        public List<PapaMeshBinding> MeshBindings { get; private set; } = new();
        public Matrix4x4 Model2SceneTransform { get; private set; }

        internal PapaModel(ICollection<string> strings, PapaEncodingModel model, ICollection<PapaSkeleton> skeletons, ICollection<PapaMesh> meshes)
        {
            this.Name = strings.ElementAtOrDefault(model.NameIndex);
            this.Skeleton = skeletons.ElementAtOrDefault(model.SkeletonIndex);

            foreach (PapaEncodingMeshBinding encodingMeshBinding in model.MeshBindings)
            {
                this.MeshBindings.Add(new PapaMeshBinding(strings, encodingMeshBinding, meshes));
            }

            this.Model2SceneTransform = model.Model2SceneTransform;
        }
    }

    public class PapaMeshBinding
    {
        public string Name { get; private set; }
        public PapaMesh Mesh { get; private set; }
        public Matrix4x4 Mesh2ModelTransform { get; private set; }
        public List<ushort> BoneMappings { get; private set; } = new();

        internal PapaMeshBinding(ICollection<string> strings, PapaEncodingMeshBinding meshBinding, ICollection<PapaMesh> meshes)
        {
            this.Name = strings.ElementAtOrDefault(meshBinding.NameIndex);
            this.Mesh = meshes.ElementAt(meshBinding.MeshIndex);
            this.Mesh2ModelTransform = meshBinding.Mesh2ModelTransform;
            this.BoneMappings = meshBinding.BoneMappings;
        }
    }

    internal struct PapaEncodingModel
    {
        internal short NameIndex { get; private set; }
        internal short SkeletonIndex { get; private set; }
        internal List<PapaEncodingMeshBinding> MeshBindings { get; private set; }
        internal Matrix4x4 Model2SceneTransform { get; private set; }

        internal PapaEncodingModel(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            this.SkeletonIndex = br.ReadInt16();

            ushort meshBindingCount = br.ReadUInt16();
            ushort padding = br.ReadUInt16();
            this.MeshBindings = new List<PapaEncodingMeshBinding>(meshBindingCount);

            this.Model2SceneTransform = br.ReadMatrix4x4();

            long meshBindingTableOffset = br.ReadInt64();

            long returnOffset = br.BaseStream.Position;
            if (meshBindingTableOffset > 0)
            {
                br.BaseStream.Seek(meshBindingTableOffset, SeekOrigin.Begin);
                for (int i = 0; i < meshBindingCount; i++)
                {
                    this.MeshBindings.Add(new PapaEncodingMeshBinding(br));
                }
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }

    internal struct PapaEncodingMeshBinding
    {
        internal short NameIndex { get; private set; }
        internal ushort MeshIndex { get; private set; }
        internal List<ushort> BoneMappings { get; private set; }
        internal Matrix4x4 Mesh2ModelTransform { get; private set; }

        internal PapaEncodingMeshBinding(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            this.MeshIndex = br.ReadUInt16();

            ushort boneMappingCount = br.ReadUInt16();
            ushort padding = br.ReadUInt16();
            this.BoneMappings = new List<ushort>(boneMappingCount);

            this.Mesh2ModelTransform = br.ReadMatrix4x4();

            long boneMappingsOffset = br.ReadInt64();

            long returnOffset = br.BaseStream.Position;
            if (boneMappingsOffset > 0)
            {
                for (int i = 0; i < boneMappingCount; i++)
                {
                    this.BoneMappings.Add(br.ReadUInt16());
                }
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }
}
