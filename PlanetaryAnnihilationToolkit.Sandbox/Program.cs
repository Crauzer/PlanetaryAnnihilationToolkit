using PlanetaryAnnihilationToolkit.PapaFile;
using SharpGLTF.Schema2;
using System;

namespace PlanetaryAnnihilationToolkit.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Papa papa = new Papa("assault_bot_adv_anim_idle.papa");
            ModelRoot gltf = papa.ToGLTF();
            
            gltf.SaveGLTF("assault_bot_adv_anim_idle.papa.gltf");

            Console.WriteLine("Hello World!");
        }
    }
}
