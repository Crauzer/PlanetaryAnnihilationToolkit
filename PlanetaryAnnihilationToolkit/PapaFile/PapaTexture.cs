using System;
using System.Collections.Generic;
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
