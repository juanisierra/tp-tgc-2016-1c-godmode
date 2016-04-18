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
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcSkeletalAnimation;

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
        List<TgcBoundingBox> objetosColisionablesCambiantes = new List<TgcBoundingBox>(); //Lista de objetos que se calcula cada vez
        Camara camara;
        TgcBoundingSphere esferaCamara; //Esfera que rodea a la camara
        TgcScene linterna, vela, farol;
        List<TgcMesh> meshesExtra = new List<TgcMesh>(); //Otros meshes para iluminar
        TgcMesh meshLinterna,meshVela,meshFarol,meshPila1;
        Luz miLuz= new Luz(); //Instancia de clase luz para la iluminacion de a linterna
        float temblorLuz;
        int ObjetoIluminacion; //0 linterna 1 farol 2 vela
        float tiempo;
        float tiempoIluminacion;
        Recarga miRecarga;
        Puerta miPuerta;
        public static Boolean esperandoPuerta; //si esta en true no se mueve
        TgcSprite bateria;
        TgcSkeletalMesh meshEnemigo;
        Enemigo enemigo = new Enemigo();
        Microsoft.DirectX.DirectInput.Key correr = Microsoft.DirectX.DirectInput.Key.LeftShift; //Tecla para correr
        bool corriendo = false;
        TgcRay rayo = new TgcRay(); //Rayo que conecta al enemigo con el jugador
        bool perdido = false;
        Vector3 direccionRayo = new Vector3();
        Vector3 lastKnownPos = new Vector3();
        string animacionSeleccionada;
        #endregion

        const int VELOCIDAD_ENEMIGO = 100;

        public override void init()
        {
           
            esperandoPuerta = false;
            //GuiController.Instance.FullScreenEnable = true; //Pantalla Completa
            //GuiController.Instance: acceso principal a todas las herramientas del Framework
            GuiController.Instance.UserVars.addVar("a", 0);
            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;
            ObjetoIluminacion = 0;
            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosDir;
            #region Carga de la Escena
            TgcSceneLoader loader = new TgcSceneLoader(); //TgcsceneLoader para cargar el escenario
            tgcScene = loader.loadSceneFromFile(            //Carga el escenario
                alumnoMediaFolder + "GODMODE\\Media\\mapaCentrado-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            #endregion

            #region Recargas
            miRecarga = new Recarga();
            tiempo = 0;
            tiempoIluminacion = 60 * 3;
            meshPila1 = miRecarga.nuevaRecarga(alumnoMediaFolder, new Vector3(15f, 20f, 15f));
            #endregion

            #region Carga de Mesh para Enemigo
            string pathMesh = alumnoMediaFolder + "GODMODE\\Media\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml";
            string mediaPath = alumnoMediaFolder + "GODMODE\\Media\\BasicHuman\\";
            TgcSkeletalLoader enemigos = new TgcSkeletalLoader();
            string[] animaciones = { "Walk" };
            animacionSeleccionada = animaciones[0];
            for (int i = 0; i < animaciones.Length; i++)
            {
                animaciones[i] = mediaPath + "Animations\\" + animaciones[i] + "-TgcSkeletalAnim.xml";
            }
            meshEnemigo = enemigos.loadMeshAndAnimationsFromFile(pathMesh, mediaPath, animaciones);
            meshEnemigo.playAnimation(animacionSeleccionada, true);
            meshEnemigo.Position = new Vector3(500, 0, 0);
            meshEnemigo.Scale = new Vector3(1.5f, 1.3f, 1.3f);
            meshEnemigo.rotateY(FastMath.PI / 2);
            enemigo.setMesh(meshEnemigo);
            #endregion

            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);


            //Modifiers para desplazamiento del personaje
  
            GuiController.Instance.Modifiers.addBoolean("HabilitarGravedad", "Habilitar Gravedad", false);
            GuiController.Instance.Modifiers.addVertex3f("Gravedad", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, -10, 0));
            GuiController.Instance.Modifiers.addFloat("SlideFactor", 1f, 200f,1.5f);
            GuiController.Instance.UserVars.addVar("poder", 0);
            GuiController.Instance.UserVars.addVar("posicion", 0);

            #region Configuracion de camara
            //Camara
            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.RotCamera.Enable = false;
            camara = new Camara();
            camara.setCamera(new Vector3(1f, 50f, 1f), new Vector3(1.9996f, 50f, 0.9754f));
            camara.MovementSpeed = 100f;
            camara.RotationSpeed = 2f;
            camara.JumpSpeed = 30f;
            camara.init();
            #endregion

            #region Control de Colisiones

            objetosColisionables.Clear(); 
                        foreach (TgcMesh mesh in tgcScene.Meshes) //Agrega una caja a cada mesh que haya en la escena
                        {
                            objetosColisionables.Add(mesh.BoundingBox);
                        }

            esferaCamara = new TgcBoundingSphere(camara.getPosition(), 20f); //Crea la esfera de la camara en la posicion de la camara
            #endregion

            #region Meshes Objetos Iluminacion

            linterna = loader.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\linterna-TgcScene.xml",
             alumnoMediaFolder + "GODMODE\\Media\\");
            meshLinterna = linterna.Meshes[0];
            vela = loader.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\vela con fuego-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            meshVela = vela.Meshes[0];
            farol = loader.loadSceneFromFile(alumnoMediaFolder + "GODMODE\\Media\\farol-TgcScene.xml",
             alumnoMediaFolder + "GODMODE\\Media\\");
            meshFarol = farol.Meshes[0];

            meshLinterna.Position = camara.getPosition() + new Vector3(10f,-70f,52.5f);
            meshLinterna.Rotation = new Vector3(Geometry.DegreeToRadian(-5f), Geometry.DegreeToRadian(90f), Geometry.DegreeToRadian(-5f));
            meshLinterna.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            meshVela.Position = camara.getPosition() + new Vector3(10f, -80f, 52.5f);
            meshVela.Rotation = new Vector3(Geometry.DegreeToRadian(-5f), Geometry.DegreeToRadian(90f), Geometry.DegreeToRadian(-5f));
            meshVela.Scale = new Vector3(0.08f, 0.08f, 0.08f);
            meshFarol.Position = camara.getPosition() + new Vector3(15f, -80f, 52.5f);
            meshFarol.Rotation = new Vector3(Geometry.DegreeToRadian(-5f), Geometry.DegreeToRadian(90f), Geometry.DegreeToRadian(-5f));
            meshFarol.Scale = new Vector3(0.2f,0.2f,0.2f);
            #endregion

            #region Sprite Bateria
            bateria = new TgcSprite();
            bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria3.png");
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = bateria.Texture.Size;
            bateria.Scaling = new Vector2(0.6f, 0.6f);
            bateria.Position = new Vector2(FastMath.Max(screenSize.Width / 4 - textureSize.Width / 4, 0), FastMath.Max(screenSize.Height - textureSize.Height / 1.7f, 0));
            #endregion

            #region Prueba puerta
            miPuerta = new Puerta(alumnoMediaFolder, new Vector3(-253f, 1f, -69f), new Vector3(5.7f, 2.15f, 1f), new Vector3(0f, 0f, 0f));

            #endregion
            meshesExtra.Add(miPuerta.mesh);

            #region Inicializacion del rayo
            direccionRayo = camara.getPosition() - enemigo.getPosicion();
            rayo.Origin = enemigo.getPosicion();
            rayo.Direction = direccionRayo;
            #endregion

        }


        // <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {
            #region Manejo de Una puerta
            if (Math.Abs(Vector3.Length(camara.eye - (miPuerta.mesh.Position + (new Vector3(0f,50f,0f)) ))) < 130f && GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.T)) //Sumo el vector para compensar la altura
            {
                miPuerta.girando = true;
                esperandoPuerta = true;
            }
            miPuerta.actualizarPuerta(elapsedTime);
            miPuerta.mesh.updateBoundingBox();
         
                miPuerta.mesh.BoundingBox.transform(miPuerta.mesh.Transform); //rota el bounding box
           
            miPuerta.mesh.BoundingBox.setRenderColor(Color.White);
            miPuerta.mesh.BoundingBox.render();
            objetosColisionablesCambiantes.Add(miPuerta.mesh.BoundingBox);
            #endregion

            //Device de DirectX para renderizar
            Device d3dDevice = GuiController.Instance.D3dDevice;

            
            GuiController.Instance.UserVars.setValue("a",tiempo);
            //tgcScene.renderAll(); //Renderiza la escena del TGCSceneLoader
            
            #region Camara, Colisiones y Deteccion
            List<TgcBoundingBox> todosObjetosColisionables = new List<TgcBoundingBox>();
            todosObjetosColisionables.AddRange(objetosColisionables);
            todosObjetosColisionables.AddRange(objetosColisionablesCambiantes);
            camara.objetosColisionables = todosObjetosColisionables;
            camara.characterSphere = esferaCamara;
            if (!esperandoPuerta)
            {
                camara.updateCamera();
            }

            #region Deteccion del jugador

            int cantColisiones = 0;
            direccionRayo = camara.getPosition() - enemigo.getPosicion();
            rayo.Origin = enemigo.getPosicion();
            rayo.Direction = direccionRayo;
            foreach (TgcBoundingBox obstaculo in objetosColisionables)
            {
                if (TgcCollisionUtils.intersectRayAABB(rayo, obstaculo, out direccionRayo))
                    cantColisiones++;
            }

            if (cantColisiones > 2 && !perdido) //Si se pierde de vista al jugador y no venia perdido, almacenar la ultima posicion conocida
            {
                lastKnownPos = esferaCamara.Position;
                perdido = true;
            }

            if (cantColisiones <= 2) perdido = false; //Si se ve al jugador, indicar que se lo encontro

            #endregion

            #endregion


            #region Luz Linterna
            List<TgcMesh> todosLosMeshesIluminables = new List<TgcMesh>();
            todosLosMeshesIluminables.AddRange(tgcScene.Meshes);
            todosLosMeshesIluminables.AddRange(meshesExtra);
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
            if (lightEnable)
            {
                foreach (TgcMesh mesh in todosLosMeshesIluminables)
                {
                    miLuz.prenderLuz(ObjetoIluminacion,mesh);
                }
            }
            else
            {
                foreach (TgcMesh mesh in todosLosMeshesIluminables)
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
            foreach (TgcMesh mesh in todosLosMeshesIluminables)
            {
               if(lightEnable)
                {   if(ObjetoIluminacion==0) { miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh,  70f*(tiempoIluminacion/180), temblorLuz); } else
                    {
                        miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh, 37f* (tiempoIluminacion / 180), temblorLuz);
                    }
                     
                }
            }

            
            //Renderizar mesh de luz
  
            temblorLuz = temblorLuz + elapsedTime; //Calcula movimientos del mesh de luz
            var random = FastMath.Cos(6 * temblorLuz);
            var random2 = FastMath.Cos(12 * temblorLuz);
            meshLinterna.Rotation = new Vector3(Geometry.DegreeToRadian(-5f + random2), Geometry.DegreeToRadian(90f + random), Geometry.DegreeToRadian(-5f));
            meshVela.Rotation = new Vector3(Geometry.DegreeToRadian(-5f + random2), Geometry.DegreeToRadian(90f + random), Geometry.DegreeToRadian(-5f));
            meshFarol.Rotation = new Vector3(Geometry.DegreeToRadian(-5f + random2), Geometry.DegreeToRadian(90f + random), Geometry.DegreeToRadian(-5f));

            var matrizView = GuiController.Instance.D3dDevice.Transform.View; //Al aplanar la matriz renderiza el mesh en la misma posicion siempre respecto a la camara
            GuiController.Instance.D3dDevice.Transform.View = Matrix.Identity;
            if(ObjetoIluminacion==0)
            {
                meshLinterna.render();
            } else if (ObjetoIluminacion==1){
                meshFarol.render();
            } else if (ObjetoIluminacion==2) {
                meshVela.render();
            }

            GuiController.Instance.D3dDevice.Transform.View = matrizView;

            #endregion

            esferaCamara.setRenderColor(Color.Aqua);
            esferaCamara.render();

            #region Mover Enemigo
            if (!perdido)
                enemigo.perseguir(esferaCamara.Position, VELOCIDAD_ENEMIGO * elapsedTime);
            else
                enemigo.perseguir(lastKnownPos, VELOCIDAD_ENEMIGO * elapsedTime);

            enemigo.actualizarAnim();
            enemigo.render();
            #endregion

            #region Calculos Tiempo Iluminacion
            tiempoIluminacion -= elapsedTime;
            if (tiempoIluminacion <= 15) tiempoIluminacion = 15;
            tiempo += elapsedTime;

            miRecarga.flotar(meshPila1, random);
            meshPila1.render();
            if(Math.Abs(Vector3.Length(camara.eye - meshPila1.Position)) <30f)
            {
                tiempoIluminacion = 180f;
            }
            GuiController.Instance.UserVars.setValue("posicion",esferaCamara.Center);
            GuiController.Instance.UserVars.setValue("poder", tiempoIluminacion);
            #endregion

            #region Sprite de Bateria
            if (tiempoIluminacion <= 30)
            {
                bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria0.png");
            } else if (tiempoIluminacion >30 && tiempoIluminacion<100)
            {
                bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria1.png");
            } else if (tiempoIluminacion >=100 && tiempoIluminacion<=150)
            {
                bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria2.png");
            } else
            {
                bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria3.png");
            }
            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
           bateria.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();
            #endregion

            #region Ejemplo de input
            ///////////////INPUT//////////////////


            //Capturar Input teclado 
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.D1))
            {
                ObjetoIluminacion = 0;
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.D2))
            {
                ObjetoIluminacion = 1;
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.D3))
            {
                ObjetoIluminacion = 2;
            }

           /* //Capturar Input Mouse
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Boton izq apretado
            }*/
   
       
            #endregion
        }

        public override void close()
        {
            tgcScene.disposeAll();
        }

    }
}
