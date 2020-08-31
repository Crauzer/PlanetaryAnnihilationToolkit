using PlanetaryAnnihilationToolkit.Extensions;
using PlanetaryAnnihilationToolkit.Helpers.Structures;
using System.IO;
using System.Numerics;

namespace PlanetaryAnnihilationToolkit.PapaFile
{
    public struct PapaVertex
    {
        public Vector3? Position { get; private set; }
        public Vector3? Normal { get; private set; }
        public Vector3? Tangent { get; private set; }
        public Vector3? Binormal { get; private set; }
        public Color? Color1 { get; private set; }
        public Color? Color2 { get; private set; }
        public Vector2? TexCoord1 { get; private set; }
        public Vector2? TexCoord2 { get; private set; }
        public Vector2? TexCoord3 { get; private set; }
        public float[] Weights { get; private set; }
        public byte[] Bones { get; private set; }

        internal PapaVertex(BinaryReader br, PapaVertexFormat format)
        {
            this.Position = null;
            this.Normal = null;
            this.Tangent = null;
            this.Binormal = null;
            this.Color1 = null;
            this.Color2 = null;
            this.TexCoord1 = null;
            this.TexCoord2 = null;
            this.TexCoord3 = null;
            this.Weights = null;
            this.Bones = null;

            if (format == PapaVertexFormat.Position3)
            {
                this.Position = br.ReadVector3();
            }
            else if (format == PapaVertexFormat.Position3Color4bTexCoord2)
            {
                this.Position = br.ReadVector3();
                this.Color1 = br.ReadColor(ColorFormat.RgbaU8);
                this.TexCoord1 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Color4bTexCoord4)
            {
                this.Position = br.ReadVector3();
                this.Color1 = br.ReadColor(ColorFormat.RgbaU8);
                this.TexCoord1 = br.ReadVector2();
                this.TexCoord2 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Color4bTexCoord6)
            {
                this.Position = br.ReadVector3();
                this.Color1 = br.ReadColor(ColorFormat.RgbaU8);
                this.TexCoord1 = br.ReadVector2();
                this.TexCoord2 = br.ReadVector2();
                this.TexCoord3 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Normal3)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
            }
            else if (format == PapaVertexFormat.Position3Normal3TexCoord2)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.TexCoord1 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Normal3Color4TexCoord2)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.Color1 = br.ReadColor(ColorFormat.RgbaU8);
                this.TexCoord1 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Normal3Color4TexCoord4)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.Color1 = br.ReadColor(ColorFormat.RgbaU8);
                this.TexCoord1 = br.ReadVector2();
                this.TexCoord2 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Weights4bBones4bNormal3TexCoord2)
            {
                this.Position = br.ReadVector3();

                byte[] weights = new byte[] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                this.Weights = new float[]
                {
                    weights[0] / 255,
                    weights[1] / 255,
                    weights[2] / 255,
                    weights[3] / 255
                };

                this.Bones = new byte[] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
                this.Normal = br.ReadVector3();
                this.TexCoord1 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Normal3Tan3Bin3TexCoord2)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.Tangent = br.ReadVector3();
                this.Binormal = br.ReadVector3();
                this.TexCoord1 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Normal3Tan3Bin3TexCoord4)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.Tangent = br.ReadVector3();
                this.Binormal = br.ReadVector3();
                this.TexCoord1 = br.ReadVector2();
                this.TexCoord2 = br.ReadVector2();
            }
            else if (format == PapaVertexFormat.Position3Normal3Tan3Bin3Color4TexCoord4)
            {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.Tangent = br.ReadVector3();
                this.Binormal = br.ReadVector3();
                this.Color1 = br.ReadColor(ColorFormat.RgbaU8);
                this.TexCoord1 = br.ReadVector2();
                this.TexCoord2 = br.ReadVector2();
            }
        }
    }
}
