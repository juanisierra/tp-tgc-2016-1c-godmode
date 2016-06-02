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

    class Locker
    {
        private static TgcSceneLoader cargador;
        public TgcMesh mesh;
        public Boolean encontrado;
        public Vector3 posVista;
        public Vector3 lookAt;
        public Vector3 posAnterior;
        public Vector3 lookAtAnterior;
        public bool adentro = false;
        public Locker(String alumnoMediaFolder, Vector3 posicion,Vector3 escala)
        {
            cargador = new TgcSceneLoader();
            TgcScene nueva = cargador.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\locker-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            mesh = nueva.Meshes[0];
            mesh.Scale = escala;
            mesh.Position = posicion;
            encontrado = false;
        }
  
            public void manejarLocker(float elapsedTime)
        {
            mesh.render();
        }
        public Vector3 getPos()
        {
            return mesh.Position;
        }
        public void mover(Vector3 pos)
        {
            mesh.Position = pos;
        }
    
    }
}
