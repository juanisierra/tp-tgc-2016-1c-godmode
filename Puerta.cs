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
    class Puerta
    {
        public TgcMesh mesh;
        public Boolean girando;
        public Boolean abierta;
        public float angulo;
        public float anguloOriginal;

        TgcSceneLoader loader;
        public Puerta(String alumnoMediaFolder, Vector3 posicion, Vector3 escala, Vector3 rotation)
        {
            this.girando = false;
            this.loader = new TgcSceneLoader();
            this.angulo = 1.6f;
            this.abierta = false;
            TgcScene escena = loader.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\puerta-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            anguloOriginal = rotation.Y;
             this.mesh = escena.Meshes[0];
            this.mesh.Position = posicion;
            this.mesh.Scale = escala;
            this.mesh.Rotation = rotation;

        }

        public void actualizarPuerta(float elapsedTime) // poner girando en true para que gire
        {   if (girando == true && abierta == true)
            {   
                angulo += (elapsedTime / 3) * 1.6f;
                if (angulo >= 1.6) {
                    angulo = 1.6f;
                    girando = false;
                    EjemploAlumno.esperandoPuerta = false;
                    abierta = false;
                }
                
            }
        if(girando == true && abierta ==false)
            {
                angulo -= (elapsedTime / 3) * 1.6f;
                if (angulo <= 0)
                {
                    angulo = 0f;
                    girando = false;
                    EjemploAlumno.esperandoPuerta = false;
                    abierta = true;
                }
            }
            mesh.Rotation = new Vector3(mesh.Rotation.X, anguloOriginal + angulo, mesh.Rotation.Z);
        }
    }
}
