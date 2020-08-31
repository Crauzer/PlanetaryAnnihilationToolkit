using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetaryAnnihilationToolkit.Formats.PapaFile
{
    internal struct PapaEncodingIndexBuffer
    {
        internal PapaIndexFormat Format { get; private set; }
        internal uint IndexCount { get; private set; }
        internal ulong DataSize { get; private set; }
        internal ulong DataOffset { get; private set; }

        internal List<uint> Indices { get; private set; }

        internal PapaEncodingIndexBuffer(BinaryReader br)
        {
            this.Format = (PapaIndexFormat)br.ReadByte();
            byte[] padding = new byte[] { br.ReadByte(), br.ReadByte(), br.ReadByte() };
            this.IndexCount = br.ReadUInt32();
            this.DataSize = br.ReadUInt64();
            this.DataOffset = br.ReadUInt64();

            this.Indices = new List<uint>();

            ReadIndices(br);
        }

        private void ReadIndices(BinaryReader br)
        {
            long returnOffset = br.BaseStream.Position;
            br.BaseStream.Seek((long)this.DataOffset, SeekOrigin.Begin);

            if (this.Format == PapaIndexFormat.UInt16)
            {
                for (int i = 0; i < this.IndexCount; i++)
                {
                    this.Indices.Add(br.ReadUInt16());
                }
            }
            else if (this.Format == PapaIndexFormat.UInt32)
            {
                for (int i = 0; i < this.IndexCount; i++)
                {
                    this.Indices.Add(br.ReadUInt32());
                }
            }
            else
            {
                throw new Exception("Invalid Index Buffer format: " + this.Format);
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }

    public enum PapaIndexFormat : byte
    {
        UInt16 = 0,
        UInt32 = 1
    }
}
