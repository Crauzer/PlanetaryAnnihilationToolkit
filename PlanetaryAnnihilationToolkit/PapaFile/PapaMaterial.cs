using PlanetaryAnnihilationToolkit.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace PlanetaryAnnihilationToolkit.PapaFile
{
    public class PapaMaterial
    {
        public string ShaderName { get; private set; }

        public List<PapaVectorParameter> VectorParameters { get; private set; } = new();
        public List<PapaTextureParameter> TextureParameters { get; private set; } = new();
        public List<PapaMatrixParameter> MatrixParameters { get; private set; } = new();

        internal PapaMaterial(ICollection<string> strings, PapaEncodingMaterial material, ICollection<PapaTexture> textures)
        {
            this.ShaderName = strings.ElementAtOrDefault(material.ShaderNameIndex);

            this.VectorParameters = material.VectorParameters.Select(x => new PapaVectorParameter(strings, x)).ToList();
            this.TextureParameters = material.TextureParameters.Select(x => new PapaTextureParameter(strings, x, textures)).ToList();
            this.MatrixParameters = material.MatrixParameters.Select(x => new PapaMatrixParameter(strings, x)).ToList();
        }
    }

    public struct PapaVectorParameter
    {
        public string Name { get; private set; }
        public Vector4 Value { get; private set; }

        internal PapaVectorParameter(ICollection<string> strings, PapaEncodingVectorParameter parameter)
        {
            this.Name = strings.ElementAtOrDefault(parameter.NameIndex);
            this.Value = parameter.Value;
        }
    }
    public struct PapaTextureParameter
    {
        public string Name { get; private set; }
        public PapaTexture Texture { get; private set; }

        internal PapaTextureParameter(ICollection<string> strings, PapaEncodingTextureParameter parameter, ICollection<PapaTexture> textures)
        {
            this.Name = strings.ElementAtOrDefault(parameter.NameIndex);
            this.Texture = textures.ElementAt(parameter.TextureIndex);
        }
    }
    public struct PapaMatrixParameter
    {
        public string Name { get; private set; }
        public Matrix4x4 Value { get; private set; }

        internal PapaMatrixParameter(ICollection<string> strings, PapaEncodingMatrixParameter parameter)
        {
            this.Name = strings.ElementAtOrDefault(parameter.NameIndex);
            this.Value = parameter.Value;
        }
    }


    internal struct PapaEncodingMaterial
    {
        internal ushort ShaderNameIndex { get; private set; }

        internal List<PapaEncodingVectorParameter> VectorParameters { get; private set; }
        internal List<PapaEncodingTextureParameter> TextureParameters { get; private set; }
        internal List<PapaEncodingMatrixParameter> MatrixParameters { get; private set; }

        internal PapaEncodingMaterial(BinaryReader br)
        {
            this.ShaderNameIndex = br.ReadUInt16();

            ushort vectorParameterCount = br.ReadUInt16();
            ushort textureParameterCount = br.ReadUInt16();
            ushort matrixParameterCount = br.ReadUInt16();

            this.VectorParameters = new List<PapaEncodingVectorParameter>(vectorParameterCount);
            this.TextureParameters = new List<PapaEncodingTextureParameter>(textureParameterCount);
            this.MatrixParameters = new List<PapaEncodingMatrixParameter>(matrixParameterCount);

            long vectorParametersOffset = br.ReadInt64();
            long textureParametersOffset = br.ReadInt64();
            long matrixParametersOffset = br.ReadInt64();

            long returnOffset = br.BaseStream.Position;

            if (vectorParametersOffset > 0)
            {
                br.BaseStream.Seek(vectorParametersOffset, SeekOrigin.Begin);
                for (int i = 0; i < vectorParameterCount; i++)
                {
                    this.VectorParameters.Add(new PapaEncodingVectorParameter(br));
                }
            }
            if (textureParametersOffset > 0)
            {
                br.BaseStream.Seek(textureParametersOffset, SeekOrigin.Begin);
                for (int i = 0; i < textureParameterCount; i++)
                {
                    this.TextureParameters.Add(new PapaEncodingTextureParameter(br));
                }
            }
            if (matrixParametersOffset > 0)
            {
                br.BaseStream.Seek(matrixParametersOffset, SeekOrigin.Begin);
                for (int i = 0; i < matrixParameterCount; i++)
                {
                    this.MatrixParameters.Add(new PapaEncodingMatrixParameter(br));
                }
            }

            br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
        }
    }

    internal struct PapaEncodingVectorParameter
    {
        internal short NameIndex { get; private set; }
        internal Vector4 Value { get; private set; }

        internal PapaEncodingVectorParameter(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            ushort padding = br.ReadUInt16();
            this.Value = br.ReadVector4();
        }
    }
    internal struct PapaEncodingTextureParameter
    {
        internal short NameIndex { get; private set; }
        internal ushort TextureIndex { get; private set; }

        internal PapaEncodingTextureParameter(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            this.TextureIndex = br.ReadUInt16();
        }
    }
    internal struct PapaEncodingMatrixParameter
    {
        internal short NameIndex { get; private set; }
        internal Matrix4x4 Value { get; private set; }

        internal PapaEncodingMatrixParameter(BinaryReader br)
        {
            this.NameIndex = br.ReadInt16();
            ushort padding = br.ReadUInt16();
            this.Value = br.ReadMatrix4x4();
        }
    }
}
