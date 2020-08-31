using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlanetaryAnnihilationToolkit.PapaFile
{
    public class PapaMesh
    {
        public List<PapaVertex> Vertices { get; private set; } = new();
        public List<uint> Indices { get; private set; } = new();

        public List<PapaMaterialGroup> MaterialGroups { get; private set; } = new();

        internal PapaMesh(
            ICollection<string> strings,
            PapaEncodingMesh mesh,
            ICollection<PapaMaterial> materials,
            ICollection<PapaEncodingVertexBuffer> vertexBufferTuble,
            ICollection<PapaEncodingIndexBuffer> indexBufferTable)
        {
            this.Vertices = vertexBufferTuble.ElementAt(mesh.VertexBufferIndex).Vertices;
            this.Indices = indexBufferTable.ElementAt(mesh.IndexBufferIndex).Indices;

            foreach (PapaEncodingMaterialGroup encodingMaterialGroup in mesh.MaterialGroups)
            {
                this.MaterialGroups.Add(new PapaMaterialGroup(strings, encodingMaterialGroup, materials));
            }
        }
    }

    public class PapaMaterialGroup
    {
        public string Name { get; private set; }
        public PapaMaterial Material { get; private set; }

        public uint FirstIndex { get; private set; }
        public uint PrimitiveCount { get; private set; }

        internal PapaMaterialGroup(ICollection<string> strings, PapaEncodingMaterialGroup materialGroup, ICollection<PapaMaterial> materials)
        {
            this.Name = materialGroup.NameIndex < 0 ? string.Empty : strings.ElementAt(materialGroup.NameIndex);
            this.Material = materials.ElementAt(materialGroup.MaterialIndex);
            this.FirstIndex = materialGroup.FirstIndex;
            this.PrimitiveCount = materialGroup.PrimitiveCount;
        }
    }

    internal struct PapaEncodingMesh
    {
        internal ushort VertexBufferIndex { get; private set; }
        internal ushort IndexBufferIndex { get; private set; }

        internal List<PapaEncodingMaterialGroup> MaterialGroups { get; private set; }

        internal PapaEncodingMesh(BinaryReader br)
        {
            this.VertexBufferIndex = br.ReadUInt16();
            this.IndexBufferIndex = br.ReadUInt16();

            ushort materialGroupCount = br.ReadUInt16();
            ushort padding = br.ReadUInt16();
            long materialGroupsOffset = br.ReadInt64();

            this.MaterialGroups = new List<PapaEncodingMaterialGroup>(materialGroupCount);

            long returnOffset = br.BaseStream.Position;
            if (materialGroupsOffset > 0)
            {
                br.BaseStream.Seek(materialGroupsOffset, SeekOrigin.Begin);
                for (int i = 0; i < materialGroupCount; i++)
                {
                    this.MaterialGroups.Add(new PapaEncodingMaterialGroup(br));
                }
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }

    internal struct PapaEncodingMaterialGroup
    {
        internal short NameIndex { get; private set; }
        internal ushort MaterialIndex { get; private set; }
        internal uint FirstIndex { get; private set; }
        internal uint PrimitiveCount { get; private set; }
        internal PapaPrimitiveType PrimitiveType { get; private set; }

        internal PapaEncodingMaterialGroup(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            this.MaterialIndex = br.ReadUInt16();
            this.FirstIndex = br.ReadUInt32();
            this.PrimitiveCount = br.ReadUInt32();
            this.PrimitiveType = (PapaPrimitiveType)br.ReadByte();
            byte[] padding = new byte[] { br.ReadByte(), br.ReadByte(), br.ReadByte() };

            if (this.PrimitiveType != PapaPrimitiveType.Triangles)
            {
                throw new Exception("Encountered a MaterialGroup with PrimitiveType." + this.PrimitiveType.ToString());
            }
        }
    }

    public enum PapaPrimitiveType : byte
    {
        Points = 0,
        Lines = 1,
        Triangles = 2
    }
}
