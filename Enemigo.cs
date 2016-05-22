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
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace AlumnoEjemplos.GODMODE
{
    class Enemigo
    {
        private const int CANT_WAYPOINTS = 1;
        public TgcSkeletalMesh cuerpo;
       /* static Vector3[] waypoints = new Vector3[]
              {new Vector3(1f, 0f,1f), new Vector3(795.7219f,0f, 9.3205f), new Vector3(815.5751f, 0f, 766.1295f),
               new Vector3(28.48632f, 0f, 817.5837f), new Vector3(-802.072f, 0f, 765.6752f), new Vector3(-804.0176f, 0f, -23.41319f),
               new Vector3(-1503.234f, 0f, -20.93158f), new Vector3(-1504.088f, 0f, 377.9283f)};*/
        static Vector3[] waypoints = new Vector3[] { new Vector3(-1503.234f, 0f, -20.93158f), new Vector3(-1504.088f, 0f, 377.9283f) };
        Vector3 waypointObjetivo = waypoints[0];
        int indiceActual = -1;
        int paso = 1;

        public Vector3 getPosicion ()
        {
            return cuerpo.Position;
        }

        public void mover(Vector3 posicion)
        {
            cuerpo.move(posicion);
        }

        public void position(Vector3 posicion)
        {
            cuerpo.Position = posicion;
        }

        public void setMesh(TgcSkeletalMesh meshNuevo)
        {
            cuerpo = meshNuevo;
        }

        public TgcSkeletalMesh getMesh()
        {
            return cuerpo;
        }

        public void actualizarAnim()
        {
            cuerpo.updateAnimation();
        }

        public void render()
        {
            cuerpo.render();
        }

        public void perseguir(Vector3 objetivo, float velocidad)
        {
            Vector3 direccion = new Vector3(0, 0, 0);

            direccion = Vector3.Normalize(objetivo - this.getPosicion());
            direccion.Y = 0;
            cuerpo.rotateY((float)Math.Atan2(direccion.X, direccion.Z) - cuerpo.Rotation.Y - Geometry.DegreeToRadian(180f));
            this.mover(direccion * velocidad);
        }

        public void seguirWaypoints(float velocidad)
        {
            if (Math.Abs(Vector3.Length(this.getPosicion() - waypointObjetivo)) < 10f)
            {
                if (indiceActual >= CANT_WAYPOINTS - 1) paso = -1;
                if (indiceActual <= 0) paso = 1;
                indiceActual += paso;
                waypointObjetivo = waypoints[indiceActual];
            }
            this.perseguir(waypointObjetivo, velocidad);
        }
       public void moverAUltimoWaypoint()
        {
            this.mover(waypointObjetivo);
       }
    }
}
