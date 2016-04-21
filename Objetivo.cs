using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
namespace AlumnoEjemplos.GODMODE
{

    class Objetivo
    {
        private static TgcSceneLoader cargador;
        public float tiempoAparicion = 15;
        public TgcMesh mesh;

        public Objetivo(String alumnoMediaFolder, Vector3 posicion)
        {
            cargador = new TgcSceneLoader();
            TgcScene nueva = cargador.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\battery-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            mesh = nueva.Meshes[0];
            mesh.Scale = new Vector3(0.03f, 0.03f, 0.03f);
            mesh.Position = posicion;
        }
    }
}
