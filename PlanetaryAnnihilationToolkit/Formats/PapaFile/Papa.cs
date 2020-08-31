using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PlanetaryAnnihilationToolkit.Formats.PapaFile
{
    public class Papa
    {
        public List<PapaTexture> Textures { get; private set; } = new();
        public List<PapaModel> Models { get; private set; } = new();
        public List<PapaAnimation> Animations { get; private set; } = new();

        public Papa(string fileLocation) : this(File.OpenRead(fileLocation)) { }
        public Papa(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                string magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (magic != "apaP")
                {
                    throw new Exception("Invalid file signature: " + magic);
                }

                uint version = br.ReadUInt32();
                if (version != 0x00030000) // Version: 3
                {
                    throw new Exception("Invalid version: " + version);
                }

                short stringCount = br.ReadInt16();
                short textureCount = br.ReadInt16();
                short vertexBufferCount = br.ReadInt16();
                short indexBufferCount = br.ReadInt16();
                short materialCount = br.ReadInt16();
                short meshCount = br.ReadInt16();
                short skeletonCount = br.ReadInt16();
                short modelCount = br.ReadInt16();
                short animationCount = br.ReadInt16();

                short[] padding = new short[] { br.ReadInt16(), br.ReadInt16(), br.ReadInt16() };

                long stringTableOffset = br.ReadInt64();
                long textureTableOffset = br.ReadInt64();
                long vertexBufferTableOffset = br.ReadInt64();
                long indexBufferTableOffset = br.ReadInt64();
                long materialTableOffset = br.ReadInt64();
                long meshTableOffset = br.ReadInt64();
                long skeletonTableOffset = br.ReadInt64();
                long modelTableOffset = br.ReadInt64();
                long animationTableOffset = br.ReadInt64();

                // Read all tables from file
                string[] stringTable = ReadStringTable(br, stringCount, stringTableOffset);
                PapaEncodingTexture[] textureTable = ReadTextureTable(br, textureCount, textureTableOffset);
                PapaEncodingVertexBuffer[] vertexBufferTable = ReadVertexBufferTable(br, vertexBufferCount, vertexBufferTableOffset);
                PapaEncodingIndexBuffer[] indexBufferTable = ReadIndexBufferTable(br, indexBufferCount, indexBufferTableOffset);
                PapaEncodingMaterial[] materialTable = ReadMaterialTable(br, materialCount, materialTableOffset);
                PapaEncodingMesh[] meshTable = ReadMeshTable(br, meshCount, meshTableOffset);
                PapaEncodingSkeleton[] skeletonTable = ReadSkeletonTable(br, skeletonCount, skeletonTableOffset);
                PapaEncodingModel[] modelTable = ReadModelTable(br, modelCount, modelTableOffset);
                PapaEncodingAnimation[] animationTable = ReadAnimationTable(br, animationCount, animationTableOffset);

                // Construct high-level objects from tables
                this.Textures = ConstructTextures(stringTable, textureTable);
                List<PapaMaterial> materials = ConstructMaterials(stringTable, materialTable, this.Textures);
                List<PapaMesh> meshes = ConstructMeshes(stringTable, meshTable, materials, vertexBufferTable, indexBufferTable);
                List<PapaSkeleton> skeletons = ConstructSkeletons(stringTable, skeletonTable);
                
                this.Animations = ConstructAnimations(stringTable, animationTable);
                this.Models = ConstructModels(stringTable, modelTable, meshes, skeletons);
            }
        }

        private string[] ReadStringTable(BinaryReader br, short count, long tableOffset)
        {
            string[] tableEntries = new string[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                uint stringLength = br.ReadUInt32();
                uint padding = br.ReadUInt32();
                long stringOffset = br.ReadInt64();

                long returnOffset = br.BaseStream.Position;
                br.BaseStream.Seek(stringOffset, SeekOrigin.Begin);

                tableEntries[i] = Encoding.UTF8.GetString(br.ReadBytes((int)stringLength));

                br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
            }

            return tableEntries;
        }
        private PapaEncodingTexture[] ReadTextureTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingTexture[] tableEntries = new PapaEncodingTexture[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingTexture(br);
            }

            return tableEntries;
        }
        private PapaEncodingVertexBuffer[] ReadVertexBufferTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingVertexBuffer[] tableEntries = new PapaEncodingVertexBuffer[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingVertexBuffer(br);
            }

            return tableEntries;
        }
        private PapaEncodingIndexBuffer[] ReadIndexBufferTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingIndexBuffer[] tableEntries = new PapaEncodingIndexBuffer[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingIndexBuffer(br);
            }

            return tableEntries;
        }
        private PapaEncodingMaterial[] ReadMaterialTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingMaterial[] tableEntries = new PapaEncodingMaterial[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingMaterial(br);
            }

            return tableEntries;
        }
        private PapaEncodingMesh[] ReadMeshTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingMesh[] tableEntries = new PapaEncodingMesh[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingMesh(br);
            }

            return tableEntries;
        }
        private PapaEncodingSkeleton[] ReadSkeletonTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingSkeleton[] tableEntries = new PapaEncodingSkeleton[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingSkeleton(br);
            }

            return tableEntries;
        }
        private PapaEncodingModel[] ReadModelTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingModel[] tableEntries = new PapaEncodingModel[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingModel(br);
            }

            return tableEntries;
        }
        private PapaEncodingAnimation[] ReadAnimationTable(BinaryReader br, short count, long tableOffset)
        {
            PapaEncodingAnimation[] tableEntries = new PapaEncodingAnimation[count];

            if (tableOffset < 0)
            {
                return tableEntries;
            }

            br.BaseStream.Seek(tableOffset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                tableEntries[i] = new PapaEncodingAnimation(br);
            }

            return tableEntries;
        }

        private List<PapaTexture> ConstructTextures(ICollection<string> strings, ICollection<PapaEncodingTexture> textureTable)
        {
            List<PapaTexture> textures = new(textureTable.Count);

            foreach (PapaEncodingTexture encodingTexture in textureTable)
            {
                textures.Add(new PapaTexture(strings, encodingTexture));
            }

            return textures;
        }
        private List<PapaMaterial> ConstructMaterials(ICollection<string> strings, ICollection<PapaEncodingMaterial> materialTable, ICollection<PapaTexture> textures)
        {
            List<PapaMaterial> materials = new(materialTable.Count);

            foreach (PapaEncodingMaterial encodingMaterial in materialTable)
            {
                materials.Add(new PapaMaterial(strings, encodingMaterial, textures));
            }

            return materials;
        }
        private List<PapaMesh> ConstructMeshes(
            ICollection<string> strings,
            ICollection<PapaEncodingMesh> meshTable,
            ICollection<PapaMaterial> materials,
            ICollection<PapaEncodingVertexBuffer> vertexBufferTuble,
            ICollection<PapaEncodingIndexBuffer> indexBufferTable)
        {
            List<PapaMesh> meshes = new(meshTable.Count);

            foreach (PapaEncodingMesh encodingMesh in meshTable)
            {
                meshes.Add(new PapaMesh(strings, encodingMesh, materials, vertexBufferTuble, indexBufferTable));
            }

            return meshes;
        }
        private List<PapaSkeleton> ConstructSkeletons(ICollection<string> strings, ICollection<PapaEncodingSkeleton> skeletonTable)
        {
            List<PapaSkeleton> skeletons = new(skeletonTable.Count);

            foreach (PapaEncodingSkeleton encodingSkeleton in skeletonTable)
            {
                skeletons.Add(new PapaSkeleton(strings, encodingSkeleton));
            }

            return skeletons;
        }
        private List<PapaModel> ConstructModels(ICollection<string> strings, ICollection<PapaEncodingModel> modelTable, ICollection<PapaMesh> meshes, ICollection<PapaSkeleton> skeletons)
        {
            List<PapaModel> models = new(modelTable.Count);

            foreach (PapaEncodingModel encodingModel in modelTable)
            {
                models.Add(new PapaModel(strings, encodingModel, skeletons, meshes));
            }

            return models;
        }
        private List<PapaAnimation> ConstructAnimations(ICollection<string> strings, ICollection<PapaEncodingAnimation> animationTable)
        {
            List<PapaAnimation> animations = new List<PapaAnimation>();

            foreach(PapaEncodingAnimation encodingAnimation in animationTable)
            {
                animations.Add(new PapaAnimation(strings, encodingAnimation));
            }

            return animations;
        }

        public static Papa Merge(params Papa[] papaFiles)
        {
            if(papaFiles is null)
            {
                throw new ArgumentNullException(nameof(papaFiles), "must not be null");
            }
            if(papaFiles.Length == 0)
            {
                throw new ArgumentException("please provide at least 1 papa file", nameof(papaFiles));
            }
            if(papaFiles.Length == 1)
            {
                return papaFiles[0];
            }

            Papa basePapa = papaFiles[0];
            for(int i = 1; i < papaFiles.Length; i++)
            {
                Papa papaToMerge = papaFiles[i];

                basePapa.Merge(papaToMerge);
            }

            return basePapa;
        }
        public Papa Merge(Papa papa)
        {
            if (papa is null)
            {
                throw new ArgumentNullException(nameof(papa), "must not be null");
            }

            foreach(PapaTexture texture in papa.Textures)
            {
                if(this.Textures.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name == texture.Name) is PapaTexture originalTexture)
                {
                    originalTexture.CopyDataFromTexture(texture);
                }
                else
                {
                    this.Textures.Add(texture);
                }
            }

            foreach(PapaAnimation animation in papa.Animations)
            {
                // Check if animation already exists
                if(this.Animations.Any(x => !string.IsNullOrEmpty(x.Name) && x.Name == animation.Name))
                {
                    throw new Exception("Found an already existing animation");
                }

                this.Animations.Add(animation);
            }

            foreach (PapaModel model in papa.Models)
            {
                if (this.Models.Any(x => !string.IsNullOrEmpty(x.Name) && x.Name == model.Name))
                {
                    throw new Exception("Found an already existing model");
                }

                this.Models.Add(model);
            }

            return this;
        }
    }
}
