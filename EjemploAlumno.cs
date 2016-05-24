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
        List<TgcMesh> todosLosMeshesIluminables = new List<TgcMesh>();
        Camara camara;
        TgcBoundingSphere esferaCamara; //Esfera que rodea a la camara
        TgcScene linterna, vela, farol;
        List<TgcMesh> meshesExtra = new List<TgcMesh>(); //Otros meshes para iluminar
        TgcMesh meshLinterna, meshVela, meshFarol;
        Luz miLuz = new Luz(); //Instancia de clase luz para la iluminacion de la linterna
        float temblorLuz;
        int ObjetoIluminacion; //0 linterna 1 farol 2 vela
        float tiempo;
        float tiempoIluminacion;
        Puerta puerta1, puerta2, puerta3, puerta4, puerta5, puerta6, puerta7;
        public static Boolean esperandoPuerta; //si esta en true no se mueve
        TgcSprite bateria, titulo, mancha, instrucciones,spriteLocker;
        TgcSkeletalMesh meshEnemigo;
        Enemigo enemigo = new Enemigo();
        Microsoft.DirectX.DirectInput.Key correr = Microsoft.DirectX.DirectInput.Key.LeftShift; //Tecla para correr
        bool corriendo = false;
        bool mostrarInstrucciones = false;
        TgcRay rayo = new TgcRay(); //Rayo que conecta al enemigo con el jugador
        bool perdido = true;
        Vector3 direccionRayo = new Vector3();
        Vector3 lastKnownPos = new Vector3();
        string animacionSeleccionada;
        float tiempoBuscando;
        bool enemigoActivo = true;
        bool enWaypoints = true;
        bool enLocker = false;
        List<Tgc3dSound> sonidos;
        Tgc3dSound sonidoEnemigo;
        TgcStaticSound sonidoPilas;
        TgcStaticSound sonidoObjeto, sonidoPuertas, sonidoGrito;
        Recarga[] recargas;
        Objetivo copa, espada, locket, llave;
        int iteracion = 0;
        Boolean enMenu = true;
        Boolean gameOver = false;
        Boolean ganado = false;
        TgcText2d textoEmpezarJuego;
        TgcText2d textoDescripcion;
        TgcText2d textoGameOver;
        TgcText2d textoSpace;
        TgcText2d textoGanador;
        TgcText2d objetosAgarrados;
        int contadorDetecciones = 0;
        List<Locker> listaLockers;
        Locker locker1, locker2, locker3;
        TgcBox pruebaLuz;
        #endregion

        string alumnoMediaFolder;

        const int VELOCIDAD_ENEMIGO = 75;
        const int VELOCIDAD_PATRULLA = 50; 
        public override void init()
        {
            pruebaLuz = TgcBox.fromSize(new Vector3(10, 10, 10), Color.White);

            #region Menu
            Size screenSize = GuiController.Instance.Panel3d.Size;
            GuiController.Instance.BackgroundColor = Color.Black;
            textoEmpezarJuego = new TgcText2d();
            textoEmpezarJuego.Text = "Presione Space para comenzar";
            textoEmpezarJuego.Color = Color.Maroon;
            textoEmpezarJuego.Align = TgcText2d.TextAlign.CENTER;
            textoEmpezarJuego.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));
            textoEmpezarJuego.Size = new Size(500, 120);
            textoEmpezarJuego.Position = new Point(FastMath.Max(screenSize.Width / 2 -textoEmpezarJuego.Size.Width/2  , 0), (int)FastMath.Max(screenSize.Height /2 + textoEmpezarJuego.Size.Height/0.8f, 0));

            textoDescripcion = new TgcText2d();
            textoDescripcion.Text = "   El objetivo del juego es encontrar los tres objetos malditos distribuídos por los distintos sectores del mapa. Sólo así se podrá atravesar la puerta final, en busca del objeto más preciado. Pero cuidado, habrá varios obstáculos en tu camino que deberás superar. Presiona H para ver la ayuda.";
            textoDescripcion.changeFont(new System.Drawing.Font("TimesNewRoman", 13, FontStyle.Bold));
            textoDescripcion.Color = Color.Gray;
            textoDescripcion.Align = TgcText2d.TextAlign.LEFT;
            textoDescripcion.Size = new Size(screenSize.Width - 200, screenSize.Height / 2);
            textoDescripcion.Position = new Point(screenSize.Width / 8, screenSize.Height /2 );
            
            textoGameOver = new TgcText2d();
            textoGameOver.Text = "GAME OVER";
            textoGameOver.Color = Color.Red;
            textoGameOver.Align = TgcText2d.TextAlign.CENTER;
            textoGameOver.changeFont(new System.Drawing.Font("TimesNewRoman",60, FontStyle.Bold));
            textoGameOver.Size = new Size(500, 200);
            textoGameOver.Position = new Point(FastMath.Max(screenSize.Width / 2 - textoEmpezarJuego.Size.Width/2 , 0), (int)FastMath.Max(screenSize.Height / 2 - textoEmpezarJuego.Size.Height/6f, 0));

            textoGanador = new TgcText2d();
            textoGanador.Text = "Felicitaciones, Ganaste";
            textoGanador.Color = Color.Green;
            textoGanador.Align = TgcText2d.TextAlign.CENTER;
            textoGanador.changeFont(new System.Drawing.Font("TimesNewRoman", 50, FontStyle.Bold));
            textoGanador.Size = new Size(500, 200);
            textoGanador.Position = new Point(FastMath.Max(screenSize.Width / 2 - textoEmpezarJuego.Size.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textoEmpezarJuego.Size.Height / 2, 0));
            
            textoSpace = new TgcText2d();
            textoSpace.Text = "Presione Space para Volver al menu";
            textoSpace.Color = Color.White;
            textoSpace.Align = TgcText2d.TextAlign.LEFT;
            textoSpace.Size = new Size(screenSize.Width - 200, screenSize.Height / 2);
            textoSpace.Position = new Point(screenSize.Width / 8, screenSize.Height / 2 + textoGameOver.Size.Height);

            mancha = new TgcSprite();
            mancha.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\mancha1.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = mancha.Texture.Size;
            mancha.Scaling = new Vector2(0.6f, 0.6f);
            mancha.Position = new Vector2(FastMath.Max(screenSize.Width / 4 - textureSize.Width / 4, 0), FastMath.Max(screenSize.Height/2 - textureSize.Height / 4f, 0));
            titulo = new TgcSprite();
            titulo.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\titulo.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            textureSize =titulo.Texture.Size;
            titulo.Scaling = new Vector2(0.7f,0.7f);
            titulo.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width*0.7f / 2, 0), FastMath.Max(screenSize.Height/3  - textureSize.Height / 2.2f, 0));
            instrucciones = new TgcSprite();
            instrucciones.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\instrucciones.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            textureSize = instrucciones.Texture.Size;
            instrucciones.Scaling = new Vector2(1f, 1f);
            instrucciones.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width  / 2, 0), FastMath.Max(screenSize.Height / 2 - textureSize.Height  / 2, 0));


            #endregion

            tiempoBuscando = 15;
            esperandoPuerta = false;
            //GuiController.Instance.FullScreenEnable = true; //Pantalla Completa
            //GuiController.Instance: acceso principal a todas las herramientas del Framework
            GuiController.Instance.UserVars.addVar("enLocker");
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
            tiempoIluminacion = 100; // 60 segundos * 3 = 3 minutos
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
            meshEnemigo.Position = new Vector3(2135.981f, 0, -780.9791f);
            meshEnemigo.Scale = new Vector3(1.5f, 1.3f, 1.3f);
            meshEnemigo.rotateY(FastMath.PI / 2);
            enemigo.setMesh(meshEnemigo);
            lastKnownPos = enemigo.getPosicion();
            #endregion

            #region Modifiers
            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addBoolean("optimizar", "optimizar", true);
            GuiController.Instance.Modifiers.addVertex3f("posVista", new Vector3(-20f, 49f, -20f), new Vector3(20f, 51f, 20f), new Vector3(0, 50, 0));
            //Modifiers para desplazamiento del personaje
            GuiController.Instance.UserVars.addVar("posicion");
            GuiController.Instance.UserVars.addVar("lookAt");
            GuiController.Instance.UserVars.addVar("PosEnemigo", 0);
            GuiController.Instance.UserVars.addVar("lastKnown", 0);
            GuiController.Instance.UserVars.addVar("enWaypoints", 0);
            GuiController.Instance.UserVars.addVar("perdido", perdido);
            
            GuiController.Instance.UserVars.addVar("poder", 0);
            GuiController.Instance.Modifiers.addFloat("lightIntensity",0,10000,950);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0, 500, 100);
            GuiController.Instance.Modifiers.addVertex3f("posicionS", new Vector3(-300, 0, -50), new Vector3(300, 100, 500), new Vector3(-140, 50, 246.74f));
            miLuz.posicionesDeLuces[0] = (Vector3)GuiController.Instance.Modifiers["posicionS"];
            miLuz.posicionesDeLuces[1] = new Vector3(-150, 55, 252.5f);
            miLuz.posicionesDeLuces[2] = new Vector3(1000, 1000, 1000);
            miLuz.posicionesDeLuces[3] = new Vector3(1000, 1000, 1000);


            /* GuiController.Instance.Modifiers.addVertex3f("posPuerta", new Vector3(-151f, 1f, 549.04f), new Vector3(-11f, 1f, 749.04f), new Vector3(-51f, 1f, 649.04f));
             GuiController.Instance.Modifiers.addVertex3f("escaladoPuerta", new Vector3(-5f, -52.15f, -51f), new Vector3(10f, 52.15f, 51f), new Vector3(4.1f, 2.15f, 1f));*/

            #endregion

            #region Configuracion de camara
            //Camara
            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.RotCamera.Enable = false;
            camara = new Camara();
            camara.setCamera(new Vector3(1f, 50f, 1f), new Vector3(1.9996f, 50f, 0.9754f));
           // camara.setCamera(new Vector3(1710f, 50f, -269f), new Vector3(1.9996f, 50f, 0.9754f)); //cerca del final
           
            camara.MovementSpeed = 100f;
            camara.RotationSpeed = 2f;
            camara.JumpSpeed = 30f;
            camara.init();
            #endregion

            #region Lockers
            
            spriteLocker = new TgcSprite();
            spriteLocker.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\spriteLocker.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            textureSize = spriteLocker.Texture.Size;
            spriteLocker.Scaling = new Vector2(1,1);
            spriteLocker.Position = new Vector2(-screenSize.Width /1.9f, -screenSize.Height / 3f);
            //spriteLocker.Position = new Vector2(FastMath.Max(screenSize.Width / 2 -textureSize.Width/5, 0), FastMath.Max(screenSize.Height /2 - textureSize.Width/4 , 0));

            listaLockers = new List<Locker>();
            locker1 = new Locker(alumnoMediaFolder, new Vector3(-245f, 0, -240f), new Vector3(0.4f, 0.17f, 0.4f));
            locker1.posVista = new Vector3(-225.6352f, 50f, -197.2892f);
            locker1.lookAt = new Vector3(-225.6328f, 49.94f, -196.7892f);
            listaLockers.Add(locker1);

            locker2 = new Locker(alumnoMediaFolder, new Vector3(805f, 0, -890f), new Vector3(0.4f, 0.17f, 0.4f));
            locker2.posVista = new Vector3(82.59220f, 50f, -853.2103f);
            locker2.lookAt = new Vector3(820.009f, 49.97f, -850.4145f);
            listaLockers.Add(locker2);

            locker3 = new Locker(alumnoMediaFolder, new Vector3(-1170.824f, 0, 940f), new Vector3(0.4f, 0.17f, 0.4f));
            locker3.posVista = new Vector3(-1161.415f, 50f, 980.6960f);
            locker3.lookAt = new Vector3(-1161.45f, 50f, 981.1906f);
            listaLockers.Add(locker3);

            #endregion

            #region Control de Colisiones

            objetosColisionables.Clear(); 
                        foreach (TgcMesh mesh in tgcScene.Meshes) //Agrega una caja a cada mesh que haya en la escena
                        {
                            objetosColisionables.Add(mesh.BoundingBox);
                        }
                        foreach(Locker locker in listaLockers)
                          {
                         objetosColisionables.Add(locker.mesh.BoundingBox);
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
            textureSize = bateria.Texture.Size;
            bateria.Scaling = new Vector2(0.6f, 0.6f);
            bateria.Position = new Vector2(FastMath.Max(screenSize.Width / 4 - textureSize.Width / 4, 0), FastMath.Max(screenSize.Height - textureSize.Height / 1.7f, 0));
            #endregion

            #region ContadorDeObjetos
            objetosAgarrados = new TgcText2d();
           objetosAgarrados.Text = "0/4";
            objetosAgarrados.Color = Color.White;
            objetosAgarrados.Align = TgcText2d.TextAlign.LEFT;
            objetosAgarrados.Size = new Size(100,100);
            objetosAgarrados.changeFont(new System.Drawing.Font("TimesNewRoman", 40, FontStyle.Bold ));
            objetosAgarrados.Position = new Point((int) FastMath.Max(screenSize.Width / 1.3f , 0), (int) FastMath.Max(screenSize.Height/1.2f, 0));

            #endregion

            #region Puertas
            puerta1 = new Puerta(alumnoMediaFolder, new Vector3(-251f, 1f, -71f), new Vector3(5.85f, 2.15f, 1f), new Vector3(0f, -0.05f, 0f));//puerta que esta atras nuestro cuando empezamos
            puerta2 = new Puerta(alumnoMediaFolder, new Vector3(50.4f, 1f, -252f), new Vector3(5.75f, 2.15f, 1f), new Vector3(0f, -1.6f, 0f)); // a nuestra derecha
            puerta3 = new Puerta(alumnoMediaFolder, new Vector3(251.5f, 1f, 61f), new Vector3(5.85f, 2.15f, 1f), new Vector3(0f, -3.17f, 0f)); //puerta frente a la cual empezamos
            puerta4 = new Puerta(alumnoMediaFolder, new Vector3(51f, 1f, 648.04f), new Vector3(5.75f, 2.15f, 1f), new Vector3(0f, -1.59f, 0f)); // a nuestra izquierda
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
        {
            miLuz.posicionesDeLuces[0] = (Vector3)GuiController.Instance.Modifiers["posicionS"];
            pruebaLuz.Position = (Vector3)GuiController.Instance.Modifiers["posicionS"];
            pruebaLuz.render();
            #region enMenu
            if (enMenu) {

                
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                titulo.render();
                mancha.render();
                
                //Finalizar el dibujado de Sprites
                GuiController.Instance.Drawer2D.endDrawSprite();
                textoEmpezarJuego.render();
                textoDescripcion.render();
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                {
                    enMenu = false;
                }
            }
            #endregion
            #region Gameover 
            if (gameOver)
            {
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                titulo.render();
                mancha.render();

                //Finalizar el dibujado de Sprites
                GuiController.Instance.Drawer2D.endDrawSprite();
                textoGameOver.render();
                textoSpace.render();
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
                {
                    enMenu = true;
                    gameOver = false;
                    reiniciarJuego();
                }
            }
            #endregion
            #region ganado

            if (ganado)
            {
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                titulo.render();
                

                //Finalizar el dibujado de Sprites
                GuiController.Instance.Drawer2D.endDrawSprite();
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
            #endregion

            else if (!enMenu && !gameOver && !ganado) {
                iteracion++;
                objetosColisionablesCambiantes.Clear();
                todosObjetosColisionables.Clear();
                GuiController.Instance.UserVars.setValue("perdido", perdido);
                GuiController.Instance.UserVars.setValue("PosEnemigo", enemigo.getPosicion());
                GuiController.Instance.UserVars.setValue("lastKnown", lastKnownPos);
                GuiController.Instance.UserVars.setValue("enWaypoints", enWaypoints);
                GuiController.Instance.UserVars.setValue("enLocker", enLocker);
                GuiController.Instance.UserVars.setValue("lookAt", camara.getLookAt());
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
                //tgcScene.renderAll(); //Renderiza la escena del TGCSceneLoader

                #region Camara y Colisiones

                todosObjetosColisionables.AddRange(objetosColisionables);
                todosObjetosColisionables.AddRange(objetosColisionablesCambiantes);
                camara.objetosColisionables = todosObjetosColisionables;
                camara.characterSphere = esferaCamara;
                if (!esperandoPuerta && !enLocker)
                {
                    camara.updateCamera();
                }
                #endregion

                #region Deteccion del jugador

                if (enemigoActivo)
                {
                    int cantColisiones = 0;
                    Vector3 origenRayo = enemigo.getPosicion();
                    origenRayo.Y = 20;
                    direccionRayo = camara.getPosition() - enemigo.getPosicion();
                    direccionRayo.Y = 0;
                    rayo.Origin = origenRayo;
                    rayo.Direction = direccionRayo;
                    Vector3 ptoIntersec;
                    foreach (TgcBoundingBox obstaculo in todosObjetosColisionables)
                    {
                        if (TgcCollisionUtils.intersectRayAABB(rayo, obstaculo, out ptoIntersec) && (direccionRayo.Length() > (rayo.Origin - ptoIntersec).Length()))
                        {
                            cantColisiones++;
                            break;
                        }
                    }

                    if (cantColisiones > 0 && !perdido && !enLocker) //Si se pierde de vista al jugador y no venia perdido, almacenar la ultima posicion conocida
                    {
                        lastKnownPos = esferaCamara.Position;
                        perdido = true;
                        contadorDetecciones = 0;
                    }

                    if (cantColisiones == 0 && iteracion != 1 &&!enLocker) //En la primera iteracion no se carga bien el escenario y no funciona
                    {
                        contadorDetecciones++;
                        if (contadorDetecciones == 2)
                        {
                            if (perdido && enWaypoints) sonidoGrito.play();
                            perdido = false; //Si se ve al jugador, indicar que se lo encontro
                            enWaypoints = false;
                            enemigoActivo = true; //RARO
                            tiempoBuscando = 15; //Reiniciar el tiempo que nos busca si no estamos
                            contadorDetecciones = 0;
                        }
                    }                   
                }
                #endregion

                sonidoEnemigo.Position = esferaCamara.Position; //Actualizar posicion del origen del sonido.

                #region manejo de lockers
                manejarLocker(locker1);
                manejarLocker(locker2);
                manejarLocker(locker3);

                #endregion

                #region Luz Linterna
                todosLosMeshesIluminables.Clear();
                todosLosMeshesIluminables.AddRange(tgcScene.Meshes);
                //tgcScene.setMeshesEnabled(false);
                todosLosMeshesIluminables.AddRange(meshesExtra);
               /* foreach(TgcMesh mesh in meshesExtra)
                {
                    mesh.Enabled = false;
                }*/
                foreach(Locker locker in listaLockers)
                {
                    todosLosMeshesIluminables.Add(locker.mesh);
                   // locker.mesh.Enabled = false;
                }
                bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
          

                //Actualzar posición de la luz

                Vector3 lightPos = camara.getPosition();


                //Normalizar direccion de la luz

                Vector3 lightDir = camara.target - camara.eye;
                lightDir.Normalize();
                if ((bool)GuiController.Instance.Modifiers["optimizar"])
                {
                    renderizarMeshes(todosLosMeshesIluminables, lightEnable, lightPos, lightDir);
                } else
                {   foreach (TgcMesh m in todosLosMeshesIluminables)
                    {
                        //m.Enabled = true;
                        if (lightEnable)
                        {
                            miLuz.prenderLuz(ObjetoIluminacion, m);
                            if (ObjetoIluminacion == 0)
                            {
                                
                                miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, m, 70f * (tiempoIluminacion / 100), temblorLuz);
                                // miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh, 70f, temblorLuz);
                            }
                            else
                            {
                                miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, m, 37f * (tiempoIluminacion / 100), temblorLuz);
                            }

                        }
                        else
                        {
                            miLuz.apagarLuz(m);
                            m.render();
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
                if (ObjetoIluminacion == 0 && !enLocker)
                {
                    meshLinterna.render();
                }
                else if (ObjetoIluminacion == 1 && !enLocker)
                {
                    meshFarol.render();
                }
                else if (ObjetoIluminacion == 2 && !enLocker) 
                {
                    meshVela.render();
                }

                GuiController.Instance.D3dDevice.Transform.View = matrizView;

#endregion

               /* esferaCamara.setRenderColor(Color.Aqua);
                esferaCamara.render();
                */

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
                            tiempoIluminacion = 100f;
                            sonidoPilas.play();
                        }
                        pila.usada = true;
                        tiempoIluminacion = 100f;

                    }
                    pila.flotar(random, elapsedTime);
                    GuiController.Instance.UserVars.setValue("posicion", camara.getPosition());
                    GuiController.Instance.UserVars.setValue("poder", tiempoIluminacion);
                }
#endregion
#region Sprite locker
                if (enLocker)
                {
                     //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                spriteLocker.render();

                //Finalizar el dibujado de Sprites
                GuiController.Instance.Drawer2D.endDrawSprite();
                }
#endregion
#region Sprite de Bateria
                if (tiempoIluminacion <= 40)
                {
                    bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria0.png");
                }
                else if (tiempoIluminacion > 40 && tiempoIluminacion < 60)
                {
                    bateria.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Bateria1.png");
                }
                else if (tiempoIluminacion >= 60 && tiempoIluminacion <= 80)
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

                //REGION COMENTADA
#region Aparecer enemigo
                /* Aparicion aleatoria de enemigo           
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
                */               
#endregion

#region Manejo de Objetos a Buscar
                if (Math.Abs(Vector3.Length(camara.eye - copa.mesh.Position)) < 30f)
                {
                    if (!copa.encontrado) sonidoObjeto.play(false);
                    copa.encontrado = true;
                    ganado = true;
                }
                if (Math.Abs(Vector3.Length(camara.eye - espada.mesh.Position)) < 50f)
                {
                    if (!espada.encontrado) sonidoObjeto.play(false);
                   /* if ((!enemigoActivo) && (!espada.encontrado))
                    {
                        ponerEnemigo(new Vector3(489.047f, 0f, 843.8695f)); //PONER ENEMIGO
                    }*/
                    espada.encontrado = true;
                    
                }
                if (Math.Abs(Vector3.Length(camara.eye - locket.mesh.Position)) < 40f)
                {
                    if (!locket.encontrado) sonidoObjeto.play(false);
                    locket.encontrado = true;
                }
                if (Math.Abs(Vector3.Length(camara.eye - llave.mesh.Position)) < 40f)
                {
                    if (!llave.encontrado) sonidoObjeto.play(false);
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
                    sonidoEnemigo.play();
                    if (!perdido && !enLocker)
                    {
                        enemigo.perseguir(esferaCamara.Position, VELOCIDAD_ENEMIGO * elapsedTime);
                    }
                    else
                    {
                        if (!enWaypoints)
                        {
                            enemigo.perseguir(lastKnownPos, VELOCIDAD_ENEMIGO * elapsedTime);
                            tiempoBuscando -= elapsedTime;
                            lastKnownPos.Y = 0;
                        }
                        else {
                            enemigo.seguirWaypoints(VELOCIDAD_PATRULLA * elapsedTime);
                        }
                    }
                    //Ocultar enemigo
                    if (!enWaypoints && tiempoBuscando <= 0)
                    {
                        tiempoBuscando = 15;
                        perdido = true;
                        enWaypoints = true;
                        enemigo.irAWaypointMasCercano();
                    }
                    
                    //GAME OVER
                /*    if ((Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(enemigo.getPosicion().X, 50, enemigo.getPosicion().Z))) < 30f))
                    {
                        gameOver = true;
                    }
                    */
                    enemigo.actualizarAnim();
                    enemigo.render();
                }
                
#endregion

#region Contador Objetos
                int cantidadObjetos = 0;
                if (llave.encontrado) cantidadObjetos++;
                if (locket.encontrado) cantidadObjetos++;
                if (espada.encontrado) cantidadObjetos++;
                objetosAgarrados.Text = String.Concat(cantidadObjetos.ToString(),"/3");
                objetosAgarrados.render();
#endregion
                
                
            }
            if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.H))
            {
                mostrarInstrucciones = !mostrarInstrucciones;
            }
            if(mostrarInstrucciones)
            {
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                instrucciones.render();

                //Finalizar el dibujado de Sprites
                GuiController.Instance.Drawer2D.endDrawSprite();
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
        private void manejarLocker(Locker locker)
        {
            if (Math.Abs(Vector3.Length(camara.eye - (locker.getPos() + (new Vector3(0f, 50f, 0f))))) < 100f && !enLocker && GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F) && perdido) //Sumo el vector para compensar la altura
            {
                enLocker = true;
                locker.adentro = true;
                locker.posAnterior = camara.getPosition();
                locker.lookAtAnterior = camara.getLookAt();
                camara.setCamera(locker.posVista,locker.lookAt);
                camara.updateCamera();
                esferaCamara.setCenter(camara.getPosition());
            }
            else {
                if (locker.adentro && GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.F))
                {
                    enLocker = false;
                    locker.adentro = false;
                    camara.setCamera(locker.posAnterior, locker.lookAtAnterior);
                    camara.updateCamera();
                    esferaCamara.setCenter(camara.getPosition());
                }
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
        private int renderizarMeshes(List<TgcMesh> meshes, bool lightEnable, Vector3 lightPos, Vector3 lightDir)
        {
            int cantRenderizados = 0;
            foreach (TgcMesh m in meshes)
            {

                //Solo mostrar la malla si colisiona contra el Frustum

                if (hayQueRenderizarlo(m.BoundingBox))
                {
                   // m.Enabled = true;
                    if (lightEnable)
                    {
                        miLuz.prenderLuz(ObjetoIluminacion, m);
                        if (ObjetoIluminacion == 0)
                        {
                            miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, m, 70f * (tiempoIluminacion / 100), temblorLuz);
                            // miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, mesh, 70f, temblorLuz);
                        }
                        else
                        {
                            miLuz.renderizarLuz(ObjetoIluminacion, lightPos, lightDir, m, 37f * (tiempoIluminacion / 100), temblorLuz);
                        }
                        
                    }
                    else
                    {
                        miLuz.apagarLuz(m);
                        m.render();
                    }
                    cantRenderizados++;
                    //m.Enabled = false;
                }
            }
            return cantRenderizados;
        }
        static public bool hayQueRenderizarlo(TgcBoundingBox objeto)
        {
            TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(GuiController.Instance.Frustum, objeto);
            return (r != TgcCollisionUtils.FrustumResult.OUTSIDE);
        }

        private void reiniciarJuego()
        {
            contadorDetecciones = 0;
            esperandoPuerta = false;
            ObjetoIluminacion = 0;
            tiempoIluminacion = 100;
            tiempo = 0;
            tiempoBuscando = 15;
            meshEnemigo.Position = new Vector3(500, 0, 0);
            enemigoActivo = false;
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
            puerta1.girando = false;
            puerta1.angulo = 1.605f;
            puerta2.abierta = false;
            puerta2.angulo = 1.605f;
            puerta2.girando = false;
            puerta3.abierta = false;
            puerta3.angulo = 1.605f;
            puerta3.girando = false;
            puerta4.abierta = false;
            puerta4.angulo = 1.605f;
            puerta4.girando = false;
            puerta5.abierta = false;
            puerta5.angulo = 1.605f;
            puerta5.girando = false;
            puerta6.abierta = false;
            puerta6.angulo = 1.605f;
            puerta6.girando = false;
            puerta7.abierta = false;
            puerta7.angulo = 1.605f;
            puerta7.girando = false;
        }

    }
}
