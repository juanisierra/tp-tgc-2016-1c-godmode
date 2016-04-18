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
    
    class Recarga 
    { private static TgcSceneLoader cargador;
        public float tiempoAparicion = 15;
        public TgcMesh mesh;
        public Boolean usada = false;
        public Recarga(String alumnoMediaFolder, Vector3 posicion)
        {
            cargador = new TgcSceneLoader();
            TgcScene nueva = cargador.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\battery-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            mesh = nueva.Meshes[0];
            mesh.Scale = new Vector3(0.07f, 0.07f, 0.07f);
            mesh.Position = posicion;
        }/*
        public TgcMesh nuevaRecarga(String alumnoMediaFolder, Vector3 posicion)
        {
            TgcScene nueva = cargador.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\battery-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            TgcMesh mesh = nueva.Meshes[0];
            mesh.Scale = new Vector3(0.07f, 0.07f, 0.07f);
            mesh.Position = posicion;
            return mesh;
        }*/
        public void flotar(float random,float elapsedTime)
        {
            mesh.Position = new Vector3(mesh.Position.X, 30f + random, mesh.Position.Z);
            if (!usada)
            {
                mesh.render();
            } else
            {
                tiempoAparicion -= elapsedTime;
                if (tiempoAparicion <= 0)
                {
                    usada = false;
                    tiempoAparicion = 15;
                }
            }
        }
    }
}
