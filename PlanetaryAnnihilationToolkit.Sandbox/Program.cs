using PlanetaryAnnihilationToolkit.Formats.PapaFile;
using SharpGLTF.Schema2;
using System;

namespace PlanetaryAnnihilationToolkit.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Papa animIdle = new Papa("assault_bot_adv_anim_idle.papa");
            Papa mainModel = new Papa("assault_bot_adv.papa");
            Papa diffuseTexture = new Papa("assault_bot_adv_diffuse.papa");

            Papa mergedPapa = Papa.Merge(animIdle, mainModel, diffuseTexture);

            ModelRoot gltf = mergedPapa.ToGLTF();
            
            gltf.SaveGLTF("assault_bot_adv.papa.gltf");

            Console.WriteLine("Hello World!");
        }
    }
}
