using PlanetaryAnnihilationToolkit.PapaFile;
using SharpGLTF.Schema2;
using System;

namespace PlanetaryAnnihilationToolkit.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Papa papa = new Papa("lava_crack_01.papa");
            ModelRoot gltf = papa.ToGLTF();
            
            gltf.SaveGLTF("lava_crack_01.papa.gltf");

            Console.WriteLine("Hello World!");
        }
    }
}
