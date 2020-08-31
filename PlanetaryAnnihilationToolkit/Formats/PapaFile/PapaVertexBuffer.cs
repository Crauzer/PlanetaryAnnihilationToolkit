using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetaryAnnihilationToolkit.Formats.PapaFile
{
    internal struct PapaEncodingVertexBuffer
    {
        internal PapaVertexFormat Format { get; private set; }
        internal uint VertexCount { get; private set; }
        internal ulong DataSize { get; private set; }
        internal ulong DataOffset { get; private set; }

        internal List<PapaVertex> Vertices { get; private set; }

        internal PapaEncodingVertexBuffer(BinaryReader br)
        {
            this.Format = (PapaVertexFormat)br.ReadByte();
            if (this.Format == PapaVertexFormat.TexCoord4 ||
                this.Format == PapaVertexFormat.Matrix)
            {
                throw new Exception("Unsupported Vertex Format: " + this.Format.ToString());
            }

            byte[] padding = new byte[] { br.ReadByte(), br.ReadByte(), br.ReadByte() };
            this.VertexCount = br.ReadUInt32();
            this.DataSize = br.ReadUInt64();
            this.DataOffset = br.ReadUInt64();

            this.Vertices = new List<PapaVertex>((int)this.VertexCount);

            ReadVertices(br);
        }

        private void ReadVertices(BinaryReader br)
        {
            long returnPosition = br.BaseStream.Position;
            br.BaseStream.Seek((long)this.DataOffset, SeekOrigin.Begin);

            for (int i = 0; i < this.VertexCount; i++)
            {
                this.Vertices.Add(new PapaVertex(br, this.Format));
            }

            br.BaseStream.Seek(returnPosition, SeekOrigin.Begin);
        }
    }

    public enum PapaVertexFormat : byte
    {
        Position3 = 0x0,
        Position3Color4bTexCoord2 = 0x1,
        Position3Color4bTexCoord4 = 0x2,
        Position3Color4bTexCoord6 = 0x3,
        Position3Normal3 = 0x4,
        Position3Normal3TexCoord2 = 0x5,
        Position3Normal3Color4TexCoord2 = 0x6,
        Position3Normal3Color4TexCoord4 = 0x7,
        Position3Weights4bBones4bNormal3TexCoord2 = 0x8,
        Position3Normal3Tan3Bin3TexCoord2 = 0x9,
        Position3Normal3Tan3Bin3TexCoord4 = 0xA,
        Position3Normal3Tan3Bin3Color4TexCoord4 = 0xB,
        TexCoord4 = 0xC,
        Position3Color8fTexCoord6 = 0xD,
        Matrix = 0xE,
    }
}
