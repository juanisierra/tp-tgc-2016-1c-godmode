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
        TgcBox meshLuz; //Linterna
        Luz miLuz= new Luz(); //Instancia de clase luz para la iluminacion de a linterna
        float temblorLuz;
        #endregion

        public override void init()
        {
            //GuiController.Instance.FullScreenEnable = true; //Pantalla Completa
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosDir;
            #region Linterna
            
           meshLuz = TgcBox.fromSize(new Vector3(10, 10, 10), Color.White);

           

            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 35);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.3f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);
            GuiController.Instance.Modifiers.addFloat("spotAngle", 0, 180, 39f);
            GuiController.Instance.Modifiers.addFloat("spotExponent", 0, 20, 7f);
            GuiController.Instance.Modifiers.addVertex3f("posLint", new Vector3(-100f, -100f, 0f), new Vector3(100f,100f, 150f),new Vector3(25f, -70f, 52.5f));
           
            //Modifiers de material
            GuiController.Instance.Modifiers.addColor("mEmissive", Color.Black);
            GuiController.Instance.Modifiers.addColor("mAmbient", Color.White);
            GuiController.Instance.Modifiers.addColor("mDiffuse", Color.White);
            GuiController.Instance.Modifiers.addColor("mSpecular", Color.White);
            #endregion

            #region Carga de la Escena
            TgcSceneLoader loader = new TgcSceneLoader(); //TgcsceneLoader para cargar el escenario
            tgcScene = loader.loadSceneFromFile(            //Carga el escenario
                alumnoMediaFolder + "GODMODE\\Media\\mapaCentrado-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            #endregion

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

            esferaCamara = new TgcBoundingSphere(camara.getPosition(), 15f); //Crea la esfera de la camara en la posicion de la camara
            #endregion

           meshLuz.Position = camara.getPosition() + new Vector3(25f, -70f, 52.5f);
           meshLuz.Rotation = new Vector3(Geometry.DegreeToRadian(-5f), Geometry.DegreeToRadian(-14f), Geometry.DegreeToRadian(0f));
        }


        // <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {

           //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;




            //tgcScene.renderAll(); //Renderiza la escena del TGCSceneLoader
            #region Colisiones
             esferaCamara.setCenter(camara.getPosition()); //TODO MOVER LA ESFERA PARA INCLUIR LA LINTERNA
          
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

            #region Luz Linterna
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            if (lightEnable)
            {
                foreach (TgcMesh mesh in tgcScene.Meshes)
                {
                    miLuz.prenderLuz(mesh);
                }
            }
            else
            {
                foreach (TgcMesh mesh in tgcScene.Meshes)
                {
                    miLuz.apagarLuz(mesh);
                }
            }



            //Actualzar posición de la luz
           
            Vector3 lightPos = camara.getPosition();
           

            //Normalizar direccion de la luz
        
           Vector3 lightDir = camara.target - camara.eye;
            lightDir.Normalize();            

            //Renderizar meshes
            foreach (TgcMesh mesh in tgcScene.Meshes)
            {
               if(lightEnable)
                {
                    miLuz.renderizarLuz(lightPos, lightDir, mesh, (float) GuiController.Instance.Modifiers["lightIntensity"]);
                }
                }



            //Renderizar mesh de luz
  
            temblorLuz = temblorLuz + elapsedTime; //Calcula movimientos del mesh de luz
            var random = FastMath.Cos(6 * temblorLuz);
            var random2 = FastMath.Cos(12 * temblorLuz);
            meshLuz.Rotation = new Vector3(Geometry.DegreeToRadian(-5f + random2), Geometry.DegreeToRadian(-5f + random), Geometry.DegreeToRadian(0f));



            var matrizView = GuiController.Instance.D3dDevice.Transform.View; //Al aplanar la matriz renderiza el mesh en la misma posicion siempre respecto a la camara
            GuiController.Instance.D3dDevice.Transform.View = Matrix.Identity;
           meshLuz.render();
            GuiController.Instance.D3dDevice.Transform.View = matrizView;

            #endregion

            esferaCamara.setRenderColor(Color.Aqua);
            esferaCamara.render();

            #region Ejemplo de input
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
            #endregion
        }
        public override void close()
        {
            tgcScene.disposeAll();
        }

    }
}
