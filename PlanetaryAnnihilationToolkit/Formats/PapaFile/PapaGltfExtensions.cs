using PlanetaryAnnihilationToolkit.Helpers.Structures;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PlanetaryAnnihilationToolkit.Formats.PapaFile
{
    using VERTEX = VertexBuilder<VertexPositionNormal, VertexColor2Texture2, VertexEmpty>;
    using VERTEX_SKINNED = VertexBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>;

    public static class PapaGltfExtensions
    {
        public static ModelRoot ToGLTF(this Papa papa)
        {
            ModelRoot root = ModelRoot.CreateModel();
            Scene scene = root.UseScene("default");
            Node rootNode = scene
                .CreateNode()
                .WithLocalRotation(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(-90 * (Math.PI / 180))));

            foreach (PapaModel papaModel in papa.Models)
            {
                Node modelNode = rootNode
                    .CreateNode();

                // Skinned model
                if (papaModel.Skeleton != null)
                {
                    List<(Node, Matrix4x4)> skeleton = CreateSkeleton(modelNode, papaModel.Skeleton);

                    foreach (PapaMeshBinding meshBinding in papaModel.MeshBindings)
                    {
                        modelNode
                            .CreateNode(papaModel.Name)
                            .WithSkinnedMesh(root.CreateMesh(BuildSkinnedMesh(meshBinding)), skeleton.ToArray());
                    }

                    if(papa.Animations.Count != 0)
                    {
                        CreateAnimations(root, skeleton, papa.Animations);
                    }
                }
                else
                {
                    foreach (PapaMeshBinding meshBinding in papaModel.MeshBindings)
                    {
                        var meshBuilder = BuildMesh(meshBinding);

                        modelNode
                            .CreateNode(meshBinding.Name)
                            .WithMesh(root.CreateMesh(meshBuilder));
                    }
                }
            }

            return root;
        }

        private static IMeshBuilder<MaterialBuilder> BuildMesh(PapaMeshBinding meshBinding)
        {
            var meshBuilder = VERTEX.CreateCompatibleMesh();

            foreach (PapaMaterialGroup materialGroup in meshBinding.Mesh.MaterialGroups)
            {
                // Skip empty materials
                if (materialGroup.PrimitiveCount == 0)
                {
                    continue;
                }

                MaterialBuilder materialBuiler = new MaterialBuilder(materialGroup.Name).WithSpecularGlossinessShader();
                var materialPrimitive = meshBuilder.UsePrimitive(materialBuiler);

                // Check for DiffuseColor material parameter
                if (materialGroup.Material.VectorParameters.Any(x => x.Name == "DiffuseColor"))
                {
                    PapaVectorParameter diffuseColor = materialGroup.Material.VectorParameters.FirstOrDefault(x => x.Name == "DiffuseColor");

                    materialBuiler.UseChannel(KnownChannel.Diffuse).Parameter = diffuseColor.Value;
                }

                int minVertex = int.MaxValue;
                int maxVertex = int.MinValue;
                for (uint i = materialGroup.FirstIndex; i < materialGroup.PrimitiveCount * 3; i++)
                {
                    uint index = meshBinding.Mesh.Indices[(int)i];

                    if (index < minVertex) minVertex = (int)index;
                    if (index > maxVertex) maxVertex = (int)index;
                }

                int vertexCount = maxVertex - minVertex + 1;
                List<VERTEX> vertices = new(vertexCount);
                for (int i = minVertex; i < maxVertex + 1; i++)
                {
                    PapaVertex papaVertex = meshBinding.Mesh.Vertices[i];
                    VERTEX vertex = new VERTEX();

                    vertex.Geometry = new VertexPositionNormal()
                    {
                        Position = papaVertex.Position.Value,
                        Normal = papaVertex.Normal.HasValue ? papaVertex.Normal.Value : new Vector3()
                    };

                    vertex.Material = new VertexColor2Texture2()
                    {
                        Color0 = papaVertex.Color1.HasValue ? papaVertex.Color1.Value : new Color(),
                        Color1 = papaVertex.Color2.HasValue ? papaVertex.Color2.Value : new Color(),
                        TexCoord0 = papaVertex.TexCoord1.HasValue ? papaVertex.TexCoord1.Value : new Vector2(),
                        TexCoord1 = papaVertex.TexCoord2.HasValue ? papaVertex.TexCoord2.Value : new Vector2(),
                    };

                    vertices.Add(vertex);
                }

                for (uint i = materialGroup.FirstIndex; i < materialGroup.PrimitiveCount * 3; i += 3)
                {
                    int index0 = (int)meshBinding.Mesh.Indices[(int)i + 0] - minVertex;
                    int index1 = (int)meshBinding.Mesh.Indices[(int)i + 1] - minVertex;
                    int index2 = (int)meshBinding.Mesh.Indices[(int)i + 2] - minVertex;

                    VERTEX vertex0 = vertices[index0];
                    VERTEX vertex1 = vertices[index1];
                    VERTEX vertex2 = vertices[index2];

                    materialPrimitive.AddTriangle(vertex0, vertex1, vertex2);
                }
            }

            return meshBuilder;
        }
        private static IMeshBuilder<MaterialBuilder> BuildSkinnedMesh(PapaMeshBinding meshBinding)
        {
            var meshBuilder = VERTEX_SKINNED.CreateCompatibleMesh();

            foreach (PapaMaterialGroup materialGroup in meshBinding.Mesh.MaterialGroups)
            {
                // Skip empty materials
                if (materialGroup.PrimitiveCount == 0)
                {
                    continue;
                }

                MaterialBuilder materialBuiler = new MaterialBuilder(materialGroup.Name).WithSpecularGlossinessShader();
                var materialPrimitive = meshBuilder.UsePrimitive(materialBuiler);

                // Check for DiffuseColor material parameter
                if (materialGroup.Material.VectorParameters.Any(x => x.Name == "DiffuseColor"))
                {
                    PapaVectorParameter diffuseColor = materialGroup.Material.VectorParameters.FirstOrDefault(x => x.Name == "DiffuseColor");

                    materialBuiler.UseChannel(KnownChannel.Diffuse).Parameter = diffuseColor.Value;
                }

                int minVertex = int.MaxValue;
                int maxVertex = int.MinValue;
                for (uint i = materialGroup.FirstIndex; i < materialGroup.PrimitiveCount * 3; i++)
                {
                    uint index = meshBinding.Mesh.Indices[(int)i];

                    if (index < minVertex) minVertex = (int)index;
                    if (index > maxVertex) maxVertex = (int)index;
                }

                int vertexCount = maxVertex - minVertex + 1;
                List<VERTEX_SKINNED> vertices = new(vertexCount);
                for (int i = minVertex; i < maxVertex + 1; i++)
                {
                    PapaVertex papaVertex = meshBinding.Mesh.Vertices[i];
                    VERTEX_SKINNED vertex = new VERTEX_SKINNED();

                    vertex.Geometry = new VertexPositionNormal()
                    {
                        Position = papaVertex.Position.Value,
                        Normal = papaVertex.Normal.HasValue ? papaVertex.Normal.Value : new Vector3()
                    };

                    vertex.Material = new VertexColor2Texture2()
                    {
                        Color0 = papaVertex.Color1.HasValue ? papaVertex.Color1.Value : new Color(),
                        Color1 = papaVertex.Color2.HasValue ? papaVertex.Color2.Value : new Color(),
                        TexCoord0 = papaVertex.TexCoord1.HasValue ? papaVertex.TexCoord1.Value : new Vector2(),
                        TexCoord1 = papaVertex.TexCoord2.HasValue ? papaVertex.TexCoord2.Value : new Vector2(),
                    };

                    vertex.Skinning = new VertexJoints4(new (int, float)[]
                    {
                        (meshBinding.BoneMappings[papaVertex.Bones[0]], papaVertex.Weights[0]),
                        (meshBinding.BoneMappings[papaVertex.Bones[1]], papaVertex.Weights[1]),
                        (meshBinding.BoneMappings[papaVertex.Bones[2]], papaVertex.Weights[2]),
                        (meshBinding.BoneMappings[papaVertex.Bones[3]], papaVertex.Weights[3]),
                    });

                    vertices.Add(vertex);
                }

                for (uint i = materialGroup.FirstIndex; i < materialGroup.PrimitiveCount * 3; i += 3)
                {
                    int index0 = (int)meshBinding.Mesh.Indices[(int)i + 0] - minVertex;
                    int index1 = (int)meshBinding.Mesh.Indices[(int)i + 1] - minVertex;
                    int index2 = (int)meshBinding.Mesh.Indices[(int)i + 2] - minVertex;

                    VERTEX_SKINNED vertex0 = vertices[index0];
                    VERTEX_SKINNED vertex1 = vertices[index1];
                    VERTEX_SKINNED vertex2 = vertices[index2];

                    materialPrimitive.AddTriangle(vertex0, vertex1, vertex2);
                }
            }

            return meshBuilder;
        }

        private static List<(Node, Matrix4x4)> CreateSkeleton(Node skeletonParent, PapaSkeleton skeleton)
        {
            List<(Node, Matrix4x4)> bones = new(skeleton.Bones.Count);

            foreach (PapaBone papaBone in skeleton.Bones)
            {
                if (papaBone.ParentId == -1)
                {
                    Node boneNode = skeletonParent
                        .CreateNode(papaBone.Name)
                        .WithLocalTranslation(papaBone.Translation)
                        .WithLocalRotation(papaBone.Rotation)
                        .WithLocalScale(new Vector3(papaBone.ShearAndScale.M11, papaBone.ShearAndScale.M22, papaBone.ShearAndScale.M33));

                    bones.Add((boneNode, papaBone.Bind2BoneTransform));
                }
                else
                {
                    Node parentNode = bones[papaBone.ParentId].Item1;
                    Node boneNode = parentNode
                        .CreateNode(papaBone.Name)
                        .WithLocalTranslation(papaBone.Translation)
                        .WithLocalRotation(papaBone.Rotation)
                        .WithLocalScale(new Vector3(papaBone.ShearAndScale.M11, papaBone.ShearAndScale.M22, papaBone.ShearAndScale.M33));

                    bones.Add((boneNode, papaBone.Bind2BoneTransform));
                }
            }

            return bones;
        }
        private static void CreateAnimations(ModelRoot root, List<(Node, Matrix4x4)> skeleton, ICollection<PapaAnimation> animations)
        {
            // Create a list of animation names
            List<string> animationNames = new(animations.Count);
            for(int i = 0; i < animations.Count; i++)
            {
                PapaAnimation animation = animations.ElementAt(i);
                if (string.IsNullOrEmpty(animation.Name)) animationNames.Add("Animation" + i);
                else animationNames.Add(animation.Name);
            }

            for(int i = 0; i < animations.Count; i++)
            {
                Animation gltfAnimation = root.UseAnimation(animationNames[i]);
                PapaAnimation papaAnimation = animations.ElementAt(i);

                float frameDuration = 1 / papaAnimation.FPS;
                foreach(var boneTransforms in papaAnimation.BoneFrameTransforms)
                {
                    Dictionary<float, Vector3> translations = new();
                    Dictionary<float, Quaternion> rotations = new();
                    Node boneNode = skeleton[boneTransforms.Key].Item1;

                    // Build transform maps
                    float frameTime = 0;
                    foreach(PapaAnimationTransform transform in boneTransforms.Value)
                    {
                        translations.Add(frameTime, transform.Translation);
                        rotations.Add(frameTime, transform.Rotation);

                        frameTime += frameDuration;
                    }

                    // Create channels
                    gltfAnimation.CreateTranslationChannel(boneNode, translations, false);
                    gltfAnimation.CreateRotationChannel(boneNode, rotations, false);
                }
            }
        }
    }
}
