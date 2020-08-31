using Pfim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaryAnnihilationToolkit.PapaFile
{
    public class PapaTexture
    {

    }

    internal struct PapaEncodingTexture
    {
        internal short NameIndex { get; private set; }
        internal PapaTextureFormat Format { get; private set; }
        internal byte MipCount { get; private set; }
        internal bool IsSrgb { get; private set; }
        internal ushort Width { get; private set; }
        internal ushort Height { get; private set; }
        internal ulong DataSize { get; private set; }
        internal ulong DataOffset { get; private set; }

        internal IImage Image { get; private set; }

        internal PapaEncodingTexture(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            this.Format = (PapaTextureFormat)br.ReadByte();

            byte bits = br.ReadByte();

            this.MipCount = (byte)(bits & 0b01111111);
            this.IsSrgb = ((bits >> 7) & 1) == 1;

            this.Width = br.ReadUInt16();
            this.Height = br.ReadUInt16();
            this.DataSize = br.ReadUInt64();
            this.DataOffset = br.ReadUInt64();
            this.Image = null;

            long returnOffset = br.BaseStream.Position;
            br.BaseStream.Seek((long)this.DataOffset, SeekOrigin.Begin);

            byte[] imageData = br.ReadBytes((int)this.DataSize);
            MemoryStream simulatedDds = SimulateDdsFile(imageData);
            this.Image = Pfim.Pfim.FromStream(simulatedDds);

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }

        private MemoryStream SimulateDdsFile(byte[] imageData)
        {
            byte[] dds = new byte[imageData.Length + 124 + 4];
            MemoryStream ddsStream = new MemoryStream(dds);

            using (BinaryWriter bw = new BinaryWriter(ddsStream, Encoding.UTF8, true))
            {
                bw.Write(Encoding.ASCII.GetBytes("DDS "));
                bw.Write((uint)124); // dwSize
                bw.Write((uint)(0x1 | 0x2 | 0x4 | 0x8 | 0x1000 | 0x20000 | 0x80000)); // dwFlags
                bw.Write((uint)this.Height);
                bw.Write((uint)this.Width);
                bw.Write(ComputePitch()); // dwPitchOrLinearSize
                bw.Write(GetFormatBitsPerPixel()); //dwDepth
                bw.Write((uint)this.MipCount);

                // dwReserved1[11]
                for (int i = 0; i < 11; i++)
                {
                    bw.Write((uint)0);
                }

                WritePixelFormat(bw);

                bw.Write((uint)(0x400000 | 0x1000)); // dwCaps
                bw.Write((uint)0); // dwCaps2
                bw.Write((uint)0); // dwCaps3
                bw.Write((uint)0); // dwCaps4
                bw.Write((uint)0); // dwReserved2

                bw.Write(imageData);
            }

            ddsStream.Position = 0;

            return ddsStream;
        }
        private void WritePixelFormat(BinaryWriter bw)
        {
            bw.Write((uint)32); // dwSize

            if (this.Format == PapaTextureFormat.DXT1)
            {
                bw.Write((uint)0x4); // dwFlags
                bw.Write(Encoding.ASCII.GetBytes("DXT1")); // dwFourCC
                bw.Write((uint)0); // dwRGBBitCount
                bw.Write((uint)0); // dwRBitMask
                bw.Write((uint)0); // dwGBitMask
                bw.Write((uint)0); // dwBBitMask
                bw.Write((uint)0); // dwABitMask
            }
            else if (this.Format == PapaTextureFormat.DXT5)
            {
                bw.Write((uint)0x4); // dwFlags
                bw.Write(Encoding.ASCII.GetBytes("DXT5")); // dwFourCC
                bw.Write((uint)0); // dwRGBBitCount
                bw.Write((uint)0); // dwRBitMask
                bw.Write((uint)0); // dwGBitMask
                bw.Write((uint)0); // dwBBitMask
                bw.Write((uint)0); // dwABitMask
            }
            else if (this.Format == PapaTextureFormat.R8G8B8A8)
            {
                bw.Write((uint)(0x1 | 0x40)); // dwFlags
                bw.Write((uint)0); // dwFourCC
                bw.Write(GetFormatBitsPerPixel()); // dwRGBBitCount
                bw.Write(0xFF000000); // dwRBitMask
                bw.Write(0x00FF0000); // dwGBitMask
                bw.Write(0x0000FF00); // dwBBitMask
                bw.Write(0x000000FF); // dwABitMask
            }
            else if (this.Format == PapaTextureFormat.R8G8B8X8)
            {
                bw.Write((uint)(0x40)); // dwFlags
                bw.Write((uint)0); // dwFourCC
                bw.Write(GetFormatBitsPerPixel()); // dwRGBBitCount
                bw.Write(0xFF000000); // dwRBitMask
                bw.Write(0x00FF0000); // dwGBitMask
                bw.Write(0x0000FF00); // dwBBitMask
                bw.Write((uint)0); // dwABitMask
            }
            else if (this.Format == PapaTextureFormat.B8G8R8A8)
            {
                bw.Write((uint)(0x1 | 0x40)); // dwFlags
                bw.Write((uint)0); // dwFourCC
                bw.Write(GetFormatBitsPerPixel()); // dwRGBBitCount
                bw.Write(0x0000FF00); // dwRBitMask
                bw.Write(0x00FF0000); // dwGBitMask
                bw.Write(0xFF000000); // dwBBitMask
                bw.Write(0x000000FF); // dwABitMask
            }
        }

        private uint ComputePitch()
        {
            if (this.Format == PapaTextureFormat.DXT1 ||
                this.Format == PapaTextureFormat.DXT5)
            {
                uint blockSize = GetFormatBlockSize();

                return Math.Max(1, (uint)(this.Width + 3) / 4) * blockSize;
            }
            else
            {
                uint bitsPerPixel = GetFormatBitsPerPixel();

                return (this.Width * bitsPerPixel + 7) / 8;
            }
        }

        private uint GetFormatBitsPerPixel()
        {
            switch (this.Format)
            {
                case PapaTextureFormat.R8G8B8A8: return 32;
                case PapaTextureFormat.R8G8B8X8: return 32;
                case PapaTextureFormat.B8G8R8A8: return 32;
                case PapaTextureFormat.DXT1: return 4;
                case PapaTextureFormat.DXT5: return 8;
                case PapaTextureFormat.R8: return 8;
                default: throw new Exception("Unsupported format");
            }
        }
        private uint GetFormatBlockSize()
        {
            switch (this.Format)
            {
                case PapaTextureFormat.DXT1: return 8;
                case PapaTextureFormat.DXT3: return 16;
                case PapaTextureFormat.DXT5: return 16;
                default: throw new Exception("Unsupported format");
            }
        }
    }

    public enum PapaTextureFormat : byte
    {
        Invalid = 0x0,
        R8G8B8A8 = 0x1,
        R8G8B8X8 = 0x2,
        B8G8R8A8 = 0x3,
        DXT1 = 0x4,
        DXT3 = 0x5,
        DXT5 = 0x6,
        R32F = 0x7,
        RG32F = 0x8,
        RGBA32F = 0x9,
        R16F = 0xA,
        RG16F = 0xB,
        RGBA16F = 0xC,
        R8 = 0xD,
        RG8 = 0xE,
        D0 = 0xF,
        D16 = 0x10,
        D24 = 0x11,
        D24S8 = 0x12,
        D32 = 0x13,
        R8I = 0x14,
        R8UI = 0x15,
        R16I = 0x16,
        R16UI = 0x17,
        RG8I = 0x18,
        RG8UI = 0x19,
        RG16I = 0x1A,
        RG16UI = 0x1B,
        R32I = 0x1C,
        R32UI = 0x1D,
        Shadow16 = 0x1E,
        Shadow24 = 0x1F,
        Shadow32 = 0x20
    };
}
