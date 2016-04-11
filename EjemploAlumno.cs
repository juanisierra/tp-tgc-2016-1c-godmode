using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Input;
namespace AlumnoEjemplos.GODMODE
{
    #region Descripcion Ejemplo
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {   ///Configuraciones del Tgc-Viewer
        ///Configuraciones del Ejemplo
        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Grupo GODMODE";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "Survival Horror";
        }
        #endregion

        
        #region Variables Globales
        TgcScene tgcScene; // Crea la escena
        List<TgcBoundingBox> objetosColisionables = new List<TgcBoundingBox>(); //Lista de esferas colisionables
        Vector3 posCamaraAnterior;
        Vector3 lookAtAnterior;
        Camara camara;
        TgcBoundingSphere esferaCamara; //Esfera que rodea a la camara
        #endregion

        public override void init()
        {
            GuiController.Instance.FullScreenEnable = true; //Pantalla Completa
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosDir;

            
            TgcSceneLoader loader = new TgcSceneLoader(); //TgcsceneLoader para cargar el escenario
            tgcScene = loader.loadSceneFromFile(            //Carga el escenario
                alumnoMediaFolder + "GODMODE\\Media\\mapaCentrado-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");

            #region Configuracion de camara
            //Camara
            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.RotCamera.Enable = false;
            camara = new Camara();
            camara.setCamera(new Vector3(1f, 50f, 1f), new Vector3(1.9996f, 50f, 0.9754f));
            camara.MovementSpeed = 100f;
            camara.RotationSpeed = 2f;
            camara.JumpSpeed = 30f;
            #endregion
            #region Control de Colisiones

            objetosColisionables.Clear(); 
                        foreach (TgcMesh mesh in tgcScene.Meshes) //Agrega una caja a cada mesh que haya en la escena
                        {
                            objetosColisionables.Add(mesh.BoundingBox);
                        }

                         esferaCamara = new TgcBoundingSphere(camara.getPosition(), 5f); //Crea la esfera de la camara en la posicion de la camara
            #endregion

        }


        // <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {

           //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;
            tgcScene.renderAll(); //Renderiza la escena del TGCSceneLoader
            #region Colisiones
            esferaCamara.setCenter(camara.getPosition()); //Movemos la esfera a la posicion de la camara
            //Detectar colisiones
            bool collide = false;
            foreach(TgcBoundingBox obstaculo in objetosColisionables)
            {
               if( TgcCollisionUtils.testSphereAABB(esferaCamara,obstaculo)) // Probamos cada box de la escena contra la esfera de la camara
                {
                    collide = true;
                    break;
                }
            }
            //Si hubo colision, restaurar la posicion anterior
            if (collide)
            {
                camara.setCamera(posCamaraAnterior, lookAtAnterior);
                camara.updateCamera();

            }
            else {
                camara.updateCamera();
                posCamaraAnterior = camara.getPosition();
                lookAtAnterior = camara.getLookAt();
            }
            #endregion


            ///////////////INPUT//////////////////

            /*
            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
            {
              
            }

            //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado
            }
   
       */
        }
        public override void close()
        {
            tgcScene.disposeAll();
        }

    }
}
