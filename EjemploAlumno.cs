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
using TgcViewer.Utils.Sound;

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
        List<TgcBoundingBox> todosObjetosColisionables = new List<TgcBoundingBox>();
        Camara camara;
        TgcBoundingSphere esferaCamara; //Esfera que rodea a la camara
        TgcScene linterna, vela, farol;
        List<TgcMesh> meshesExtra = new List<TgcMesh>(); //Otros meshes para iluminar
        TgcMesh meshLinterna,meshVela,meshFarol;
        Luz miLuz= new Luz(); //Instancia de clase luz para la iluminacion de la linterna
        float temblorLuz;
        int ObjetoIluminacion; //0 linterna 1 farol 2 vela
        float tiempo;
        float tiempoIluminacion;
        Puerta puerta1, puerta2, puerta3, puerta4, puerta5, puerta6, puerta7;
        public static Boolean esperandoPuerta; //si esta en true no se mueve
        TgcSprite bateria;
        TgcSkeletalMesh meshEnemigo;
        Enemigo enemigo = new Enemigo();
        Microsoft.DirectX.DirectInput.Key correr = Microsoft.DirectX.DirectInput.Key.LeftShift; //Tecla para correr
        bool corriendo = false;
        TgcRay rayo = new TgcRay(); //Rayo que conecta al enemigo con el jugador
        bool perdido = true;
        Vector3 direccionRayo = new Vector3();
        Vector3 lastKnownPos = new Vector3();
        string animacionSeleccionada;
        float tiempoBuscando;
        Boolean enemigoActivo = false;
        List<Tgc3dSound> sonidos;
        Tgc3dSound sonidoEnemigo;
        TgcStaticSound sonidoPilas;
        TgcStaticSound sonidoObjeto,sonidoPuertas,sonidoGrito;
        Recarga[] recargas;
        Objetivo copa,espada,locket,llave;
        int iteracion = 0;
        Boolean enMenu = true;
        Boolean gameOver = false;
        Boolean ganado = false;
        TgcText2d textoEmpezarJuego;
        TgcText2d textoDescripcion;
        TgcText2d textoGameOver;
        TgcText2d textoSpace;
        TgcText2d textoGanador;
        #endregion

        string alumnoMediaFolder;

        const int VELOCIDAD_ENEMIGO = 70;

        public override void init()
        {
            
            #region Menu
            Size screenSize = GuiController.Instance.Panel3d.Size;
            GuiController.Instance.BackgroundColor = Color.Black;
            textoEmpezarJuego = new TgcText2d();
            textoEmpezarJuego.Text = "Iniciar Juego";
            textoEmpezarJuego.Color = Color.Yellow;
            textoEmpezarJuego.Align = TgcText2d.TextAlign.CENTER;
            textoEmpezarJuego.changeFont(new System.Drawing.Font("TimesNewRoman", 38, FontStyle.Bold | FontStyle.Italic));
            textoEmpezarJuego.Size = new Size(500, 120);
            textoEmpezarJuego.Position = new Point(FastMath.Max(screenSize.Width / 2 -textoEmpezarJuego.Size.Width/2  , 0), FastMath.Max(screenSize.Height / 2 + textoEmpezarJuego.Size.Height/2, 0));

            textoDescripcion = new TgcText2d();
            textoDescripcion.Text = "El objetivo del juego es encontrar los tres objetos malidtos distribuídos por los distintos sectores del mapa. Sólo así se podrá atravesar la puerta final, en busca del objeto más preciado. Pero cuidado, habrá varios obstáculos en tu camino que deberás superar. presionen space para jugar";
            textoDescripcion.Color = Color.White;
            textoDescripcion.Align = TgcText2d.TextAlign.LEFT;
            textoDescripcion.Size = new Size(screenSize.Width - 200, screenSize.Height / 2);
            textoDescripcion.Position = new Point(screenSize.Width / 8, screenSize.Height / 2);
            
            textoGameOver = new TgcText2d();
            textoGameOver.Text = "GAME OVER";
            textoGameOver.Color = Color.Red;
            textoGameOver.Align = TgcText2d.TextAlign.CENTER;
            textoGameOver.changeFont(new System.Drawing.Font("TimesNewRoman",50, FontStyle.Bold | FontStyle.Italic));
            textoGameOver.Size = new Size(500, 200);
            textoGameOver.Position = new Point(FastMath.Max(screenSize.Width / 2 - textoEmpezarJuego.Size.Width/2 , 0), FastMath.Max(screenSize.Height / 2 - textoEmpezarJuego.Size.Height / 2, 0));

            textoGanador = new TgcText2d();
            textoGanador.Text = "GANASTE";
            textoGanador.Color = Color.Green;
            textoGanador.Align = TgcText2d.TextAlign.CENTER;
            textoGanador.changeFont(new System.Drawing.Font("TimesNewRoman", 50, FontStyle.Bold | FontStyle.Italic));
            textoGanador.Size = new Size(500, 200);
            textoGanador.Position = new Point(FastMath.Max(screenSize.Width / 2 - textoEmpezarJuego.Size.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textoEmpezarJuego.Size.Height / 2, 0));
            
            textoSpace = new TgcText2d();
            textoSpace.Text = "Presione Space para Volver al menu";
            textoSpace.Color = Color.White;
            textoSpace.Align = TgcText2d.TextAlign.LEFT;
            textoSpace.Size = new Size(screenSize.Width - 200, screenSize.Height / 2);
            textoSpace.Position = new Point(screenSize.Width / 8, screenSize.Height / 2 + textoGameOver.Size.Height);
            #endregion

            tiempoBuscando = 15;
            esperandoPuerta = false;
            //GuiController.Instance.FullScreenEnable = true; //Pantalla Completa
            //GuiController.Instance: acceso principal a todas las herramientas del Framework
            GuiController.Instance.UserVars.addVar("a", 0);
            //Device de DirectX para crear primitivas
            Device d3dDevice = GuiController.Instance.D3dDevice;
            ObjetoIluminacion = 0;
            //Carpeta de archivos Media del alumno
            alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosDir;

            #region Carga de la Escena
            TgcSceneLoader loader = new TgcSceneLoader(); //TgcsceneLoader para cargar el escenario
            tgcScene = loader.loadSceneFromFile(            //Carga el escenario
                alumnoMediaFolder + "GODMODE\\Media\\mapaCentrado-TgcScene.xml",
                alumnoMediaFolder + "GODMODE\\Media\\");
            #endregion

            #region Inicializacion de Recargas
            recargas = new Recarga[4];
            recargas[0] = new Recarga(alumnoMediaFolder, new Vector3(-1508f, 20f, 107f));
            recargas[1] = new Recarga(alumnoMediaFolder, new Vector3(-1430.953f, 20f, 966.6811f));
            recargas[2] = new Recarga(alumnoMediaFolder, new Vector3(1100f, 20f, -850f));
            recargas[3] = new Recarga(alumnoMediaFolder, new Vector3(800f, 20f, 539f));
            tiempo = 0;
            tiempoIluminacion = 180; // 60 segundos * 3 = 3 minutos
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
            lastKnownPos = enemigo.getPosicion();
            #endregion

            #region Modifiers
            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            //Modifiers para desplazamiento del personaje
  
            GuiController.Instance.Modifiers.addBoolean("HabilitarGravedad", "Habilitar Gravedad", false);
            GuiController.Instance.Modifiers.addVertex3f("Gravedad", new Vector3(-50, -50, -50), new Vector3(50, 50, 50), new Vector3(0, -10, 0));
            GuiController.Instance.Modifiers.addFloat("SlideFactor", 1f, 200f,1.5f);
            GuiController.Instance.UserVars.addVar("poder", 0);
            GuiController.Instance.UserVars.addVar("posicion", 0);
            GuiController.Instance.UserVars.addVar("perdido", perdido);
           /* GuiController.Instance.Modifiers.addVertex3f("posPuerta", new Vector3(835f, 0f, -1122f), new Vector3(1975f, 1.95f, 667f), new Vector3(1740f, 1f, -248f));
            GuiController.Instance.Modifiers.addVertex3f("escaladoPuerta", new Vector3(-5f, -52.15f, -51f), new Vector3(10f, 52.15f, 51f), new Vector3(4.1f, 2.15f, 1f));*/
            
            #endregion

            #region Configuracion de camara
            //Camara
            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.RotCamera.Enable = false;
            camara = new Camara();
            // camara.setCamera(new Vector3(1f, 50f, 1f), new Vector3(1.9996f, 50f, 0.9754f));
            camara.setCamera(new Vector3(1710f, 50f, -269f), new Vector3(1.9996f, 50f, 0.9754f)); //cerca del final
           
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

            /* ACLARACION: para usar ListenerTracking es necesario pasar un mesh por parametro. Como la esfera del jugador no tiene, el envio
             * del sonido se hace de esfera a enemigo; es decir, el ListenerTracking se hace sobre el enemigo.*/
            #region Sonido
            sonidos = new List<Tgc3dSound>();
            sonidoEnemigo = new Tgc3dSound(alumnoMediaFolder + "GODMODE\\Media\\Sound\\pies, arrastrar.wav", esferaCamara.Position);
            sonidoEnemigo.MinDistance = 10f;
            sonidos.Add(sonidoEnemigo);
            GuiController.Instance.DirectSound.ListenerTracking = enemigo.getMesh();
           
            sonidoPilas = new TgcStaticSound();
            sonidoPilas.loadSound(alumnoMediaFolder + "GODMODE\\Media\\Sound\\torno 3.wav");
            sonidoObjeto = new TgcStaticSound();
            sonidoObjeto.loadSound(alumnoMediaFolder + "GODMODE\\Media\\Sound\\supersónico cueva.wav");
            sonidoPuertas = new TgcStaticSound();
            sonidoPuertas.loadSound(alumnoMediaFolder + "GODMODE\\Media\\Sound\\pisada crujiente izda.wav");
            sonidoGrito = new TgcStaticSound();
            sonidoGrito.loadSound(alumnoMediaFolder + "GODMODE\\Media\\Sound\\monstruo, grito.wav");
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
             screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = bateria.Texture.Size;
            bateria.Scaling = new Vector2(0.6f, 0.6f);
            bateria.Position = new Vector2(FastMath.Max(screenSize.Width / 4 - textureSize.Width / 4, 0), FastMath.Max(screenSize.Height - textureSize.Height / 1.7f, 0));
            #endregion

            #region Puertas
            puerta1 = new Puerta(alumnoMediaFolder, new Vector3(-251f, 1f, -71f), new Vector3(5.85f, 2.15f, 1f), new Vector3(0f, -0.05f, 0f));//puerta que esta atras nuestro cuando empezamos
            puerta2 = new Puerta(alumnoMediaFolder, new Vector3(50.4f, 1f, -252f), new Vector3(5.75f, 2.15f, 1f), new Vector3(0f, -1.6f, 0f)); // a nuestra derecha
            puerta3 = new Puerta(alumnoMediaFolder, new Vector3(251.5f, 1f, 61f), new Vector3(5.85f, 2.15f, 1f), new Vector3(0f, -3.17f, 0f)); //puerta frente a la cual empezamos
            puerta4 = new Puerta(alumnoMediaFolder, new Vector3(-51f, 1f, 649.04f), new Vector3(5.75f, 2.15f, 1f), new Vector3(0f, 1.55f, 0f)); // a nuestra izquierda
            puerta5 = new Puerta(alumnoMediaFolder, new Vector3(-1360f, 1f, 432f), new Vector3(5.75f, 2.15f, 1f), new Vector3(0f, 1.55f, 0f)); // siguiendo el camino indicado por la 3
            puerta6 = new Puerta(alumnoMediaFolder, new Vector3(1200.8f, 1f, -749f), new Vector3(4.65f, 2.15f, 1f), new Vector3(0f, 3.1f, 0f)); // siguiendo el camino indicado por la 2
            puerta7 = new Puerta(alumnoMediaFolder, new Vector3(1740f, 1f, -248f), new Vector3(4.05f, 2.15f, 1f), new Vector3(0f, 1.54f, 0f)); //ULTIMOA PUERTA
            meshesExtra.Add(puerta1.mesh);
            meshesExtra.Add(puerta2.mesh);
            meshesExtra.Add(puerta3.mesh);
            meshesExtra.Add(puerta4.mesh);
            meshesExtra.Add(puerta5.mesh);
            meshesExtra.Add(puerta6.mesh);
            meshesExtra.Add(puerta7.mesh);

            #endregion

            #region Inicializacion del rayo
            direccionRayo = camara.getPosition() - enemigo.getPosicion();
            rayo.Origin = enemigo.getPosicion();
            rayo.Direction = direccionRayo;
            #endregion

            #region Objetos a buscar
            copa = new Objetivo(alumnoMediaFolder, "GODMODE\\Media\\copa-TgcScene.xml", new Vector3(1782.22f, 30f,-5.51f), new Vector3(0.1f, 0.1f, 0.1f));
            espada = new Objetivo(alumnoMediaFolder, "GODMODE\\Media\\espada-TgcScene.xml", new Vector3(829f, 0f, 821f), new Vector3(0.1f, 0.1f, 0.1f));
            locket = new Objetivo(alumnoMediaFolder, "GODMODE\\Media\\locket-TgcScene.xml", new Vector3(-1447f, 30f,1023f), new Vector3(0.02f,0.02f,0.02f));
            llave = new Objetivo(alumnoMediaFolder, "GODMODE\\Media\\llave-TgcScene.xml", new Vector3(1274f, 40f, -458f), new Vector3(0.1f, 0.1f, 0.1f));
            locket.mesh.rotateY(-0.7f);
            espada.mesh.rotateZ(1f);
            #endregion
        }


        // <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
        public override void render(float elapsedTime)
        {   if (enMenu) {
               
                textoEmpezarJuego.render();
                textoDescripcion.render();
                if(GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                {
                    enMenu = false;
                }
            } if (gameOver)
            {
                textoGameOver.render();
                textoSpace.render();
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                {
                    enMenu = true;
                    gameOver = false;
                    reiniciarJuego();
                }
            }
            if (ganado)
            {
                textoGanador.render();
                textoSpace.render();
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                {
                    enMenu = true;
                    gameOver = false;
                    ganado = false;
                    reiniciarJuego();
                }
            }

            else if (!enMenu && !gameOver && !ganado) {
                iteracion++;
                objetosColisionablesCambiantes.Clear();
                todosObjetosColisionables.Clear();
                GuiController.Instance.UserVars.setValue("perdido", perdido);

                #region Manejo de Puertas
                manejarPuerta(puerta1);
                manejarPuerta(puerta2); //Hacer foreach
                manejarPuerta(puerta3);
                manejarPuerta(puerta4);
                manejarPuerta(puerta5);
                manejarPuerta(puerta6);
                if ((llave.encontrado && locket.encontrado && espada.encontrado) || iteracion == 1)
                {
                    manejarPuerta(puerta7);
                }
                if (!puerta7.abierta) //Manejo de la ultima puerta para colisiones 
                {
                    puerta7.mesh.updateBoundingBox();
                    puerta7.mesh.BoundingBox.transform(puerta7.mesh.Transform); //rota el bounding box
                    objetosColisionablesCambiantes.Add(puerta7.mesh.BoundingBox);
                }


                #endregion

                //Device de DirectX para renderizar
                Device d3dDevice = GuiController.Instance.D3dDevice;


                GuiController.Instance.UserVars.setValue("a", tiempo);
                //tgcScene.renderAll(); //Renderiza la escena del TGCSceneLoader

                #region Camara, Colisiones y Deteccion

                todosObjetosColisionables.AddRange(objetosColisionables);
                todosObjetosColisionables.AddRange(objetosColisionablesCambiantes);
                camara.objetosColisionables = todosObjetosColisionables;
                camara.characterSphere = esferaCamara;
                if (!esperandoPuerta)
                {
                    camara.updateCamera();
                }

                #region Deteccion del jugador

                if (enemigoActivo)
                {
                    int cantColisiones = 0;
                    direccionRayo = camara.getPosition() - enemigo.getPosicion();
                    rayo.Origin = enemigo.getPosicion();
                    rayo.Direction = direccionRayo;
                    foreach (TgcBoundingBox obstaculo in todosObjetosColisionables)
                    {
                        if (TgcCollisionUtils.intersectRayAABB(rayo, obstaculo, out direccionRayo))
                            cantColisiones++;
                    }

                    if (cantColisiones > 2 && !perdido) //Si se pierde de vista al jugador y no venia perdido, almacenar la ultima posicion conocida
                    {
                        lastKnownPos = esferaCamara.Position;
                        perdido = true;
                    }

                    if (cantColisiones <= 2 && iteracion != 1) //En la primera iteracion no se carga bien el escenario y no funciona
                    {
                        if (perdido) sonidoGrito.play();
                        perdido = false; //Si se ve al jugador, indicar que se lo encontro
                        enemigoActivo = true; //RARO
                        tiempoBuscando = 15; //Reiniciar el tiempo que nos busca si no estamos
                    }
                }
                #endregion

                #endregion

                sonidoEnemigo.Position = esferaCamara.Position; //Actualizar posicion del origen del sonido.


                #region Luz Linterna
                List<TgcMesh> todosLosMeshesIluminables = new List<TgcMesh>();
                todosLosMeshesIluminables.AddRange(tgcScene.Meshes);
                todosLosMeshesIluminables.AddRange(meshesExtra);
                bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
                if (lightEnable)
                {
                    foreach (TgcMesh mesh in todosLosMeshesIluminables)
                    {
                        miLuz.prenderLuz(ObjetoIluminacion, mesh);
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
                    if (lightEnable)
                    {
                        if (ObjetoIluminacion == 0)
                        { //miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh,  70f*(tiempoIluminacion/180), temblorLuz);
                            miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh, 70f, temblorLuz);
                        }
                        else
                        {
                            miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh, 37f * (tiempoIluminacion / 180), temblorLuz);
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
                if (ObjetoIluminacion == 0)
                {
                    meshLinterna.render();
                }
                else if (ObjetoIluminacion == 1)
                {
                    meshFarol.render();
                }
                else if (ObjetoIluminacion == 2)
                {
                    meshVela.render();
                }

                GuiController.Instance.D3dDevice.Transform.View = matrizView;

                #endregion

                esferaCamara.setRenderColor(Color.Aqua);
                esferaCamara.render();


                #region Calculos Tiempo Iluminacion

                tiempoIluminacion -= elapsedTime;
                if (tiempoIluminacion <= 15)
                    tiempoIluminacion = 15;
                tiempo += elapsedTime;

                foreach (Recarga pila in recargas)
                {
                    if (Math.Abs(Vector3.Length(camara.eye - pila.mesh.Position)) < 30f)
                    {
                        if (!pila.usada)
                        {
                            tiempoIluminacion = 180f;
                            sonidoPilas.play();
                        }
                        pila.usada = true;
                        tiempoIluminacion = 180f;

                    }
                    pila.flotar(random, elapsedTime);
                    GuiController.Instance.UserVars.setValue("posicion", esferaCamara.Center);
                    GuiController.Instance.UserVars.setValue("poder", tiempoIluminacion);
                }
                #endregion

                #region Sprite de Bateria
                if (tiempoIluminacion <= 30)
                {
                    bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria0.png");
                }
                else if (tiempoIluminacion > 30 && tiempoIluminacion < 100)
                {
                    bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria1.png");
                }
                else if (tiempoIluminacion >= 100 && tiempoIluminacion <= 150)
                {
                    bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria2.png");
                }
                else
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

                #region Ejemplo de input teclado
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

                #region Aparecer enemigo
                if (Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(-1288,50,372))) < 400f || Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(-1299, 50, 986))) < 400f)
                {
                    ponerEnemigo(new Vector3(-1317f, 0f,778.95f)); 
                }
                if (Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(10.55f, 50, -805.52f))) < 400f )
                {
                    ponerEnemigo(new Vector3(460.2749f, 0f, -791.8f));
                }
                if (Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(1274f, 50f, -458f))) < 300f || Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(-1299, 50, 986))) < 400f)
                {
                    ponerEnemigo(new Vector3(1615f, 0f, -525f));
                }

                #endregion

                #region Manejo de Objetos a Buscar
                if (Math.Abs(Vector3.Length(camara.eye - copa.mesh.Position)) < 30f)
                {
                    if (copa.encontrado == false) sonidoObjeto.play(false);
                    copa.encontrado = true;
                    ganado = true;
                }
                if (Math.Abs(Vector3.Length(camara.eye - espada.mesh.Position)) < 50f)
                {
                    if (espada.encontrado == false) sonidoObjeto.play(false);
                    if ((!enemigoActivo) && (espada.encontrado == false))
                    {
                        ponerEnemigo(new Vector3(489.047f, 0f, 843.8695f)); //PONER ENEMIGO
                    }
                    espada.encontrado = true;
                    
                }
                if (Math.Abs(Vector3.Length(camara.eye - locket.mesh.Position)) < 40f)
                {
                    if (locket.encontrado == false) sonidoObjeto.play(false);
                    locket.encontrado = true;
                }
                if (Math.Abs(Vector3.Length(camara.eye - llave.mesh.Position)) < 40f)
                {
                    if (locket.encontrado == false) sonidoObjeto.play(false);
                    llave.encontrado = true;
                }
                llave.flotar(random, elapsedTime, 40f);
                espada.flotar(random, elapsedTime, 10f);
                copa.flotar(random, elapsedTime, 30f);
                locket.flotar(random, elapsedTime, 30f);
                #endregion

                #region Mover Enemigo
                if (enemigoActivo)
                {
                    if (Math.Abs(Vector3.Length(esferaCamara.Position - enemigo.getPosicion())) < 700f)
                    {
                        sonidoEnemigo.play();
                        if (!perdido)
                            enemigo.perseguir(esferaCamara.Position, VELOCIDAD_ENEMIGO * elapsedTime);
                        else
                        {
                            enemigo.perseguir(lastKnownPos, VELOCIDAD_ENEMIGO * elapsedTime);
                            tiempoBuscando -= elapsedTime;
                        }
                    }
                    enemigo.actualizarAnim();
                    enemigo.render();
                    if (Math.Abs(Vector3.Length(esferaCamara.Position - enemigo.getPosicion())) > 700f || tiempoBuscando <= 0)
                    {
                        tiempoBuscando = 15;
                        meshEnemigo.Position = new Vector3(500, 0, 0);
                        enemigoActivo = false;
                    }
                    if (Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(enemigo.getPosicion().X, 50, enemigo.getPosicion().Z))) < 20f)
                    {
                        gameOver = true;
                    }
                }
                #endregion

                if ((esferaCamara.Position.X > 1775 && esferaCamara.Position.X < 1776) && (esferaCamara.Position.Z > -23 && esferaCamara.Position.Z < -22))
                {
                    ganado = true;
                }
            }
        }

        public override void close()
        {
            tgcScene.disposeAll();
            sonidoEnemigo.dispose();
            sonidoPilas.dispose();
            enemigo.getMesh().dispose();
            for (int i = 0; i < 4; i++)
                recargas[i].dispose();
        }

        private void manejarPuerta(Puerta puerta)
         {
             if (Math.Abs(Vector3.Length(camara.eye - (puerta.mesh.Position + (new Vector3(0f, 50f, 0f))))) < 130f && GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.E)) //Sumo el vector para compensar la altura
             {
                sonidoPuertas.play(false);
                 puerta.girando = true;
                 esperandoPuerta = true;
             }
             puerta.actualizarPuerta(GuiController.Instance.ElapsedTime);
            if (!puerta.abierta)
            {
                puerta.mesh.updateBoundingBox();
                puerta.mesh.BoundingBox.transform(puerta.mesh.Transform); //rota el bounding box
                objetosColisionablesCambiantes.Add(puerta.mesh.BoundingBox);
            }
         }

        private void ponerEnemigo(Vector3 posicion)
        {   if (!enemigoActivo)
            {
                enemigo.position(posicion); //PONER ENEMIGO
                lastKnownPos = enemigo.getPosicion();
                enemigoActivo = true;
                //sonidoGrito.play(); //Opcional: grita cuando aparece, aunque no vea al jugador
            }
        }

        private void reiniciarJuego()
        {
            tiempoBuscando = 15;
            esperandoPuerta = false;
            ObjetoIluminacion = 0;
            tiempoIluminacion = 180;
            tiempo = 0;
            tiempoBuscando = 15;
            meshEnemigo.Position = new Vector3(500, 0, 0);
            enemigoActivo = true;
            iteracion = 0;
            perdido = true;
            lastKnownPos = enemigo.getPosicion();
            camara.setCamera(new Vector3(1f, 50f, 1f), new Vector3(1.9996f, 50f, 0.9754f));
            esferaCamara.setCenter(camara.getPosition());
            #region Inicializacion del rayo
            direccionRayo = camara.getPosition() - enemigo.getPosicion();
            rayo.Origin = enemigo.getPosicion();
            rayo.Direction = direccionRayo;
            #endregion
            copa.encontrado = false;
            locket.encontrado = false;
            espada.encontrado = false;
            llave.encontrado = false;
            puerta1.abierta = false;
            puerta1.angulo = 1.605f;
            puerta2.abierta = false;
            puerta2.angulo = 1.605f;
            puerta3.abierta = false;
            puerta3.angulo = 1.605f;
            puerta4.abierta = false;
            puerta4.angulo = 1.605f;
            puerta5.abierta = false;
            puerta5.angulo = 1.605f;
            puerta6.abierta = false;
            puerta6.angulo = 1.605f;
            puerta7.abierta = false;
            puerta7.angulo = 1.605f;
        }

    }
}
