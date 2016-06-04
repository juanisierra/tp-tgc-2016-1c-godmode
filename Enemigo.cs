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
        private const int CANT_WAYPOINTS = 10;
        public TgcSkeletalMesh cuerpo;
        static Vector3[] waypoints = new Vector3[]
              {new Vector3(2135.981f, 0, -780.9791f), new Vector3(1121.311f, 0, -795.8734f), new Vector3(1119.649f, 0, -54.46498f),
               new Vector3(798.399f, 0, -18.12312f), new Vector3(676.7241f, 0, 8.804145f), new Vector3(5.406677f, 0, -7.967796f),
               new Vector3(-3.416103f, 0, 770.7191f), new Vector3(-807.0411f, 0, 763.6401f), new Vector3(-804.649f, 0, -21.21243f),
               new Vector3(-1493.659f, 0, -21.32377f)};
        // static Vector3[] waypoints = new Vector3[] { new Vector3(-1503.234f, 0f, -20.93158f), new Vector3(-1504.088f, 0f, 377.9283f) };
        Vector3 waypointObjetivo = waypoints[0];
       public int indiceActual = -1;
        public int paso = 1;

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

        public void dispose()
        {
            cuerpo.dispose();
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
            //this.irAWaypointMasCercano();
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

        public void irAWaypointMasCercano()
        {
            float min = 10000f;
            int i = 0;
            foreach (Vector3 waypoint in waypoints)
            {
                if (Math.Abs(Vector3.Length(this.getPosicion() - waypoint)) < min)
                {
                    min = Math.Abs(Vector3.Length(this.getPosicion() - waypoint));
                    waypointObjetivo = waypoint;
                    indiceActual = i;
                }
                i++;
            }
        }
    }
}
