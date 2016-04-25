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
       public TgcSkeletalMesh cuerpo;

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
    }
}
