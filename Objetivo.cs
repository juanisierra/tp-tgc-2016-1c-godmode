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
        public TgcMesh mesh;
        public Boolean encontrado;
        public Objetivo(String alumnoMediaFolder, String textura, Vector3 posicion,Vector3 escala)
        {
            cargador = new TgcSceneLoader();
            TgcScene nueva = cargador.loadSceneFromFile(alumnoMediaFolder + textura,
                alumnoMediaFolder + "GODMODE\\Media\\");
            mesh = nueva.Meshes[0];
            mesh.Scale = escala;
            mesh.Position = posicion;
            encontrado = false;
        }
  
            public void flotar(float random, float elapsedTime,float alturaOriginal)
        {
            mesh.Position = new Vector3(mesh.Position.X, alturaOriginal + random, mesh.Position.Z);
            if (!encontrado)
            {
                mesh.render();
            }
        }
    
    }
}
