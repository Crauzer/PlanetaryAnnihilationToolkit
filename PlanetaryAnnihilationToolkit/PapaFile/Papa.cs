using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlanetaryAnnihilationToolkit.PapaFile
{
    public class Papa
    {
        public List<PapaModel> Models { get; private set; } = new();

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
                // TextureTable
                PapaEncodingVertexBuffer[] vertexBufferTable = ReadVertexBufferTable(br, vertexBufferCount, vertexBufferTableOffset);
                PapaEncodingIndexBuffer[] indexBufferTable = ReadIndexBufferTable(br, indexBufferCount, indexBufferTableOffset);
                PapaEncodingMaterial[] materialTable = ReadMaterialTable(br, materialCount, materialTableOffset);
                PapaEncodingMesh[] meshTable = ReadMeshTable(br, meshCount, meshTableOffset);
                PapaEncodingSkeleton[] skeletonTable = ReadSkeletonTable(br, skeletonCount, skeletonTableOffset);
                PapaEncodingModel[] modelTable = ReadModelTable(br, modelCount, modelTableOffset);
                // AnimationTable

                // Construct high-level objects from tables
                List<PapaMaterial> materials = ConstructMaterials(stringTable, materialTable);
                List<PapaMesh> meshes = ConstructMeshes(stringTable, meshTable, materials, vertexBufferTable, indexBufferTable);
                List<PapaSkeleton> skeletons = ConstructSkeletons(stringTable, skeletonTable);

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

        private List<PapaMaterial> ConstructMaterials(ICollection<string> strings, ICollection<PapaEncodingMaterial> materialTable)
        {
            List<PapaMaterial> materials = new(materialTable.Count);

            foreach (PapaEncodingMaterial encodingMaterial in materialTable)
            {
                materials.Add(new PapaMaterial(strings, encodingMaterial));
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
    }
}
