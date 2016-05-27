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
using System.Linq;
using System.Windows.Forms;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;

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
        List<TgcBoundingBox> objetosColisionables; //Lista de esferas colisionables
        List<TgcBoundingBox> objetosColisionablesCambiantes; //Lista de objetos que se calcula cada vez
        List<TgcBoundingBox> todosObjetosColisionables;
        List<TgcMesh> todosLosMeshesIluminables;
        List<TgcMesh> meshesParaNightVision;
        Camara camara;
        TgcBoundingSphere esferaCamara; //Esfera que rodea a la camara
        TgcScene linterna, vela, farol;
        List<TgcMesh> meshesExtra; //Otros meshes para iluminar
        TgcMesh meshLinterna, meshVela, meshFarol;
        Luz miLuz; //Instancia de clase luz para la iluminacion de la linterna
        float temblorLuz;
        int ObjetoIluminacion; //0 linterna 1 farol 2 vela
        float tiempo;
        float tiempoIluminacion;
        Puerta puerta1, puerta2, puerta3, puerta4, puerta5, puerta6, puerta7;
        List<Puerta> puertas;
        public static Boolean esperandoPuerta; //si esta en true no se mueve
        TgcSprite bateria, titulo, mancha, instrucciones,spriteLocker,spriteObjetivos;
        TgcSkeletalMesh meshEnemigo;
        Enemigo enemigo;
        bool mostrarInstrucciones;
        TgcRay rayo; //Rayo que conecta al enemigo con el jugador
        bool perdido;
        Vector3 direccionRayo;
        Vector3 lastKnownPos;
        string animacionSeleccionada;
        float tiempoBuscando;
        bool enemigoActivo;
        bool enWaypoints;
        bool enLocker;
        List<Tgc3dSound> sonidos;
        Tgc3dSound sonidoEnemigo;
        TgcStaticSound sonidoPilas;
        TgcStaticSound sonidoObjeto, sonidoPuertas, sonidoGrito;
        Recarga[] recargas;
        Objetivo copa, espada, locket, llave;
        int iteracion;
        Boolean enMenu;
        Boolean gameOver;
        Boolean ganado;
        TgcText2d textoEmpezarJuego;
        TgcText2d textoDescripcion;
        TgcText2d textoGameOver;
        TgcText2d textoSpace;
        TgcText2d textoGanador;
        int contadorDetecciones;
        List<Locker> listaLockers;
        Locker locker1, locker2, locker3;
        bool enemigoEsperandoPuerta;
        Effect effect,efectoMiedo;
        /*NightVision*/
        Surface g_pDepthStencil;     // Depth-stencil buffer 
        Texture g_pRenderTarget, g_pGlowMap, g_pRenderTarget4, g_pRenderTarget4Aux;
        VertexBuffer g_pVBV3D;
        int cant_pasadas;
        Boolean conNightVision;
        /*Miedo*/
        VertexBuffer screenQuadVB;
        Texture renderTarget2D;
        Surface pOldRT;
        #endregion

        string alumnoMediaFolder;

        const int VELOCIDAD_ENEMIGO = 75;
        const int VELOCIDAD_PATRULLA = 50;
        const float POSICION_INICIAL_ENEMIGO_X = 2135.981f;
        const float POSICION_INICIAL_ENEMIGO_Z = -780.9791f;
        const float TIEMPO_DE_BUSQUEDA = 15;
        const int DELAY_FRAMES_DETECCION = 4;

        public override void init()
        {
            enMenu = true;
            objetosColisionables = new List<TgcBoundingBox>(); //Lista de esferas colisionables
            objetosColisionablesCambiantes = new List<TgcBoundingBox>(); //Lista de objetos que se calcula cada vez
            todosObjetosColisionables = new List<TgcBoundingBox>();
            todosLosMeshesIluminables = new List<TgcMesh>();
            meshesParaNightVision = new List<TgcMesh>();
            meshesExtra = new List<TgcMesh>();
            miLuz = new Luz();
            enemigo = new Enemigo();
            mostrarInstrucciones = false;
            rayo = new TgcRay();
            GuiController.Instance.CustomRenderEnabled = true;
            perdido = true;
            direccionRayo = new Vector3();
            lastKnownPos = new Vector3();
            enemigoActivo = true;
            enWaypoints = true;
            enLocker = false;
            iteracion = 0;
            enMenu = true;
            gameOver = false;
            ganado = false;
            contadorDetecciones = 0;
            enemigoEsperandoPuerta = false;
            cant_pasadas = 3;
            conNightVision = false;
            #region Menu
            Size screenSize = GuiController.Instance.Panel3d.Size;
            GuiController.Instance.BackgroundColor = Color.Black;
            textoEmpezarJuego = new TgcText2d();
            textoEmpezarJuego.Text = "Presione Space para comenzar";
            textoEmpezarJuego.Color = Color.Maroon;
            textoEmpezarJuego.Align = TgcText2d.TextAlign.CENTER;
            textoEmpezarJuego.changeFont(new System.Drawing.Font("TimesNewRoman", 25, FontStyle.Bold));
            textoEmpezarJuego.Size = new Size(500, 120);
            textoEmpezarJuego.Position = new Point(FastMath.Max(screenSize.Width / 2 - textoEmpezarJuego.Size.Width / 2, 0), (int)FastMath.Max(screenSize.Height / 2 + textoEmpezarJuego.Size.Height / 0.8f, 0));

            textoDescripcion = new TgcText2d();
            textoDescripcion.Text = "   El objetivo del juego es encontrar los tres objetos malditos distribuídos por los distintos sectores del mapa. Sólo así se podrá atravesar la puerta final, en busca del objeto más preciado. Pero cuidado, habrá varios obstáculos en tu camino que deberás superar. Presiona H para ver la ayuda.";
            textoDescripcion.changeFont(new System.Drawing.Font("TimesNewRoman", 13, FontStyle.Bold));
            textoDescripcion.Color = Color.Gray;
            textoDescripcion.Align = TgcText2d.TextAlign.LEFT;
            textoDescripcion.Size = new Size(screenSize.Width - 200, screenSize.Height / 2);
            textoDescripcion.Position = new Point(screenSize.Width / 8, screenSize.Height / 2);

            textoGameOver = new TgcText2d();
            textoGameOver.Text = "GAME OVER";
            textoGameOver.Color = Color.Red;
            textoGameOver.Align = TgcText2d.TextAlign.CENTER;
            textoGameOver.changeFont(new System.Drawing.Font("TimesNewRoman", 60, FontStyle.Bold));
            textoGameOver.Size = new Size(500, 200);
            textoGameOver.Position = new Point(FastMath.Max(screenSize.Width / 2 - textoEmpezarJuego.Size.Width / 2, 0), (int)FastMath.Max(screenSize.Height / 2 - textoEmpezarJuego.Size.Height / 6f, 0));

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
            mancha.Position = new Vector2(FastMath.Max(screenSize.Width / 4 - textureSize.Width / 4, 0), FastMath.Max(screenSize.Height / 2 - textureSize.Height / 4f, 0));
            titulo = new TgcSprite();
            titulo.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\titulo.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            textureSize = titulo.Texture.Size;
            titulo.Scaling = new Vector2(0.7f, 0.7f);
            titulo.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width * 0.7f / 2, 0), FastMath.Max(screenSize.Height / 3 - textureSize.Height / 2.2f, 0));
            instrucciones = new TgcSprite();
            instrucciones.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\instrucciones.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            textureSize = instrucciones.Texture.Size;
            instrucciones.Scaling = new Vector2(1f, 1f);
            instrucciones.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textureSize.Height / 2, 0));


            #endregion

            tiempoBuscando = TIEMPO_DE_BUSQUEDA;
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
            meshEnemigo.Position = new Vector3(POSICION_INICIAL_ENEMIGO_X, 0, POSICION_INICIAL_ENEMIGO_Z);
            meshEnemigo.Scale = new Vector3(1.5f, 1.3f, 1.3f);
            meshEnemigo.rotateY(FastMath.PI / 2);
            enemigo.setMesh(meshEnemigo);
            lastKnownPos = enemigo.getPosicion();
            #endregion

            #region Modifiers
            //Modifiers de la luz
            GuiController.Instance.Modifiers.addBoolean("lightEnable", "lightEnable", true);
            GuiController.Instance.Modifiers.addVertex3f("posVista", new Vector3(-20f, 49f, -20f), new Vector3(20f, 51f, 20f), new Vector3(0, 50, 0));
            //Modifiers para desplazamiento del personaje
            GuiController.Instance.UserVars.addVar("posicion");
            GuiController.Instance.UserVars.addVar("lookAt");
            GuiController.Instance.UserVars.addVar("PosEnemigo", 0);
            GuiController.Instance.UserVars.addVar("lastKnown", 0);
            GuiController.Instance.UserVars.addVar("enWaypoints", 0);
            GuiController.Instance.UserVars.addVar("perdido", perdido);

            GuiController.Instance.UserVars.addVar("poder", 0);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 10000, 4000);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0, 500, 200);
            GuiController.Instance.Modifiers.addVertex3f("posicionS", new Vector3(-300, 0, -50), new Vector3(300, 100, 500), new Vector3(-140, 50, 246.74f));
            miLuz.posicionesDeLuces[0] = new Vector3(240, 60, 145.5f);
            miLuz.posicionesDeLuces[1] = new Vector3(-260, 60, -133.2f);
            miLuz.posicionesDeLuces[2] = new Vector3(997, 60, -645);
            miLuz.posicionesDeLuces[3] = new Vector3(-1314, 60, 1077);


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
            spriteLocker.Scaling = new Vector2(1, 1);
            spriteLocker.Position = new Vector2(-screenSize.Width / 1.9f, -screenSize.Height / 3f);
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
            bateria.Scaling = new Vector2(0.4f, 0.4f);
            bateria.Position = new Vector2(FastMath.Max(screenSize.Width / 5 - textureSize.Width / 4, 0), FastMath.Max(screenSize.Height - textureSize.Height / 1.7f, 0));
            #endregion
            #region Sprite Objetivos
            spriteObjetivos = new TgcSprite();
            spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\0.png");
            screenSize = GuiController.Instance.Panel3d.Size;
            textureSize = spriteObjetivos.Texture.Size;
            spriteObjetivos.Scaling = new Vector2(0.6f, 0.6f);
            spriteObjetivos.Position = new Vector2(FastMath.Max(screenSize.Width / 2.7f, 0), FastMath.Max(screenSize.Height / 1.2f, 0));
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

            puertas = new List<Puerta>();
            puertas.Add(puerta1); puertas.Add(puerta2); puertas.Add(puerta3); puertas.Add(puerta4);
            puertas.Add(puerta5); puertas.Add(puerta6); puertas.Add(puerta7);
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
            #region Cargo shader nightvision
            String compilationErrors;
            effect = Effect.FromFile(GuiController.Instance.D3dDevice,
                alumnoMediaFolder + "GODMODE\\Media\\Shaders\\GaussianBlur.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            effect.Technique = "DefaultTechnique";
            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                        d3dDevice.PresentationParameters.BackBufferHeight,
                                                                        DepthFormat.D24S8,
                                                                        MultiSampleType.None,
                                                                        0,
                                                                        true);

            // inicializo el render target
            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            g_pGlowMap = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            g_pRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4
                    , d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            g_pRenderTarget4Aux = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4
                    , d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            // Resolucion de pantalla
            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices = new CustomVertex.PositionTextured[]
            {
                new CustomVertex.PositionTextured( -1, 1, 1, 0,0),
                new CustomVertex.PositionTextured(1,  1, 1, 1,0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
                new CustomVertex.PositionTextured(1,-1, 1, 1,1)
            };
            //vertex buffer de los triangulos
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            g_pVBV3D.SetData(vertices, 0, LockFlags.None);
            #endregion
            #region Carga shader Miedo
            CustomVertex.PositionTextured[] screenQuadVertices = new CustomVertex.PositionTextured[]
           {
                new CustomVertex.PositionTextured( -1, 1, 1, 0,0),
                new CustomVertex.PositionTextured(1,  1, 1, 1,0),
                new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
                new CustomVertex.PositionTextured(1,-1, 1, 1,1)
           };
            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            //Creamos un Render Targer sobre el cual se va a dibujar la pantalla
            renderTarget2D = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);


            //Cargar shader con efectos de Post-Procesado
            efectoMiedo = TgcShaders.loadEffect(alumnoMediaFolder + "GODMODE\\Media\\Shaders\\Miedo.fx");

            //Configurar Technique dentro del shader
            efectoMiedo.Technique = "OndasTechnique";
            #endregion
        }

        // <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>

        public override void render(float elapsedTime)
        {

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
                foreach (Puerta puerta in puertas)
                {
                    if (puerta != puertas.Last())
                    manejarPuerta(puerta);
                }
                if ((llave.encontrado && locket.encontrado && espada.encontrado) || iteracion == 1)
                {
                    manejarPuerta(puertas.Last());
                }
                if (!puertas.Last().abierta) //Manejo de la ultima puerta para colisiones 
                {
                    puertas.Last().mesh.updateBoundingBox();
                    puertas.Last().mesh.BoundingBox.transform(puertas.Last().mesh.Transform); //rota el bounding box
                    objetosColisionablesCambiantes.Add(puertas.Last().mesh.BoundingBox);
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
                    bool colisionDetectada = false;
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
                            colisionDetectada = true;
                            break;
                        }
                    }

                    if (colisionDetectada && !perdido) //Indicar que se perdio al jugador al detectar colision, si no lo estaba
                    {
                        perdido = true;
                        contadorDetecciones = 0;
                    }

                    if (!colisionDetectada && iteracion != 1 && !enLocker) //En la primera iteracion no se carga bien el escenario y no funciona
                    {
                        contadorDetecciones++;
                        if (contadorDetecciones >= DELAY_FRAMES_DETECCION && !enemigoEsperandoPuerta)
                        {
                            lastKnownPos = esferaCamara.Position;
                            if (perdido && enWaypoints) sonidoGrito.play();
                            perdido = false; //Si se ve al jugador, indicar que se lo encontro
                            enWaypoints = false;
                            enemigoActivo = true; //RARO
                            tiempoBuscando = TIEMPO_DE_BUSQUEDA; //Reiniciar el tiempo que nos busca si no estamos
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
                #region Calculos Tiempo Iluminacion

                tiempoIluminacion -= elapsedTime;
                if (tiempoIluminacion <= 15)
                    tiempoIluminacion = 15;
                tiempo += elapsedTime;
                //temblorLuz = temblorLuz + elapsedTime; //Calcula movimientos del mesh de luz, ya se suma en otro lado
                var random = FastMath.Cos(6 * temblorLuz);
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
                    pila.flotar(random, elapsedTime,conNightVision);
                    GuiController.Instance.UserVars.setValue("posicion", camara.getPosition());
                    GuiController.Instance.UserVars.setValue("poder", tiempoIluminacion);
                }
                #endregion
                #region Mover Enemigo
                if (enemigoActivo)
                {
                    sonidoEnemigo.play();
                    if (!enWaypoints)
                    {
                        if (perdido)
                        {
                            tiempoBuscando -= elapsedTime;
                            foreach (Puerta puerta in puertas)
                            {
                                Vector3 ptoIntersec = new Vector3();
                                if (TgcCollisionUtils.intersectRayAABB(rayo, puerta.mesh.BoundingBox, out ptoIntersec) && (direccionRayo.Length() > (rayo.Origin - ptoIntersec).Length()) && !puerta.abierta)
                                {
                                    enWaypoints = true;
                                    enemigo.irAWaypointMasCercano();
                                }
                            }
                        }
                        enemigo.perseguir(lastKnownPos, VELOCIDAD_ENEMIGO * elapsedTime);
                    }
                    else
                    {
                        enemigoEsperandoPuerta = false;
                        foreach (Puerta puerta in puertas)
                        {
                            Vector3 posicionPuerta = puerta.mesh.Position; posicionPuerta.Y = 0;
                            if (Math.Abs(Vector3.Length(enemigo.getPosicion() - posicionPuerta)) < 110f && !puerta.abierta)
                            {
                                if (!puerta.girando)
                                {
                                    sonidoPuertas.play(false);
                                    puerta.girando = true;
                                }
                                enemigoEsperandoPuerta = true;
                                break;
                            }
                        }
                        if (!enemigoEsperandoPuerta)
                            enemigo.seguirWaypoints(VELOCIDAD_PATRULLA * elapsedTime);
                    }
                    //Retomar waypoints por tiempo de busqueda
                    if (!enWaypoints && tiempoBuscando <= 0)
                    {
                        tiempoBuscando = TIEMPO_DE_BUSQUEDA;
                        perdido = true;
                        enWaypoints = true;
                        enemigo.irAWaypointMasCercano();
                    }

                    //GAME OVER
                     /*   if ((Math.Abs(Vector3.Length(esferaCamara.Position - new Vector3(enemigo.getPosicion().X, 50, enemigo.getPosicion().Z))) < 30f))
                        {
                            gameOver = true;
                        }
                        */
                    enemigo.actualizarAnim();
                    
                }

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
                llave.flotar(random, elapsedTime, 40f,conNightVision);
                espada.flotar(random, elapsedTime, 10f, conNightVision);
                copa.flotar(random, elapsedTime, 30f, conNightVision);
                locket.flotar(random, elapsedTime, 30f, conNightVision);
                #endregion
                #region Renderizado
                todosLosMeshesIluminables.Clear();
                todosLosMeshesIluminables.AddRange(tgcScene.Meshes);
                todosLosMeshesIluminables.AddRange(meshesExtra);

                foreach(Locker locker in listaLockers)
                {
                    todosLosMeshesIluminables.Add(locker.mesh);
                }
                bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];
          

                //Actualzar posición de la luz
                Vector3 lightPos = camara.getPosition();
                //Normalizar direccion de la luz
                Vector3 lightDir = camara.target - camara.eye;
                lightDir.Normalize();
                if (!conNightVision && tiempoIluminacion!=15)
                {
                    renderizarMeshes(todosLosMeshesIluminables, lightEnable, lightPos, lightDir);
                    //Renderizar mesh de luz
                    enemigo.render();
                    renderizarObjetoIluminacion(elapsedTime);

                } else if(conNightVision)
                {
                    renderizarNightVision(elapsedTime);
                } else if(!conNightVision && tiempoIluminacion == 15){
                    renderizarMiedo(elapsedTime);
                }
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

                #region Sprites
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
                if(!llave.encontrado && !locket.encontrado && !espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\0.png");
                } else if(!llave.encontrado && ! locket.encontrado && espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\1.png");
                } else if(llave.encontrado && !locket.encontrado && espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\2.png");
                } else if(!llave.encontrado && locket.encontrado && espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\3.png");
                } else if (llave.encontrado && locket.encontrado && !espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\4.png");
                } else if(llave.encontrado && !locket.encontrado && !espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\5.png");
                } else if (!llave.encontrado && locket.encontrado && !espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\6.png");
                } else if(llave.encontrado && locket.encontrado && espada.encontrado)
                {
                    spriteObjetivos.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Objetivos\\7.png");
                }
                //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                bateria.render();
                spriteObjetivos.render();
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
                if (GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.R))
                {
                    conNightVision = !conNightVision;
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
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
        }

        public override void close()
        {
            camara.updateCamera();
            camara.LockCam = false;
            tgcScene.disposeAll();
             esferaCamara.dispose();
            sonidoEnemigo.dispose(); sonidoGrito.dispose(); sonidoPilas.dispose(); sonidoObjeto.dispose(); sonidoPuertas.dispose();
            enemigo.dispose();
            meshLinterna.dispose(); meshFarol.dispose(); meshVela.dispose();
            espada.mesh.dispose(); locket.mesh.dispose(); copa.mesh.dispose(); llave.mesh.dispose();
            for (int i = 0; i < 4; i++) recargas[i].dispose();
            foreach (Puerta puerta in puertas) puerta.mesh.dispose();
            puertas.Clear();
             meshesExtra.Clear();
            foreach (Locker locker in listaLockers) locker.mesh.dispose();
            listaLockers.Clear();
            
            todosLosMeshesIluminables.Clear();
              objetosColisionables.Clear();
             objetosColisionablesCambiantes.Clear();
             todosObjetosColisionables.Clear();
        }

        private void manejarPuerta(Puerta puerta)
        {
            if (Math.Abs(Vector3.Length(camara.eye - (puerta.mesh.Position + (new Vector3(0f, 50f, 0f))))) < 130f && GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.E)) //Sumo el vector para compensar la altura
            {
                if (!puerta.girando)
                {
                    sonidoPuertas.play(false);
                    puerta.girando = true;
                    esperandoPuerta = true;
                }
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
        private int renderizarMeshesConEfecto(List<TgcMesh> meshes,Effect effect, String technique)
        {
            int cantRenderizados = 0;
            foreach (TgcMesh m in meshes)
            {   
                
                //Solo mostrar la malla si colisiona contra el Frustum

                if (hayQueRenderizarlo(m.BoundingBox))
                {
                   m.Effect = effect;
                m.Technique = technique;
                    m.render();
                    cantRenderizados++;
                }
            }
            return cantRenderizados;
        }
        private void renderizarObjetoIluminacion(float elapsedTime)
        {

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
        }
        private void renderizarNightVision(float elapsedTime)
        {

            meshesParaNightVision.Clear();

            meshesParaNightVision.Add(espada.mesh);
            meshesParaNightVision.Add(locket.mesh);
            meshesParaNightVision.Add(copa.mesh);
            meshesParaNightVision.Add(llave.mesh);
            foreach (Recarga recarga in recargas)
            {
                meshesParaNightVision.Add(recarga.mesh);
            }
            Device device = GuiController.Instance.D3dDevice;
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;

            // dibujo la escena una textura 
            effect.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            Surface pOldRT = device.GetRenderTarget(0);
            Surface pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            Surface pOldDS = device.DepthStencilSurface;
            device.DepthStencilSurface = g_pDepthStencil;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();
            //Dibujamos todos los meshes del escenario
            renderizarMeshesConEfecto(todosLosMeshesIluminables, effect, "DefaultTechnique");
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];


            //Actualzar posición de la luz
            Vector3 lightPos = camara.getPosition();
            //Normalizar direccion de la luz
            Vector3 lightDir = camara.target - camara.eye;
            renderizarMeshes(todosLosMeshesIluminables, lightEnable, lightPos, lightDir);
            meshLinterna.Effect = effect;
            meshLinterna.Technique = "DefaultTechnique";
            meshVela.Effect = effect;
            meshVela.Technique = "DefaultTechnique";
            meshFarol.Effect = effect;
            meshFarol.Technique = "DefaultTechnique";
            renderizarObjetoIluminacion(elapsedTime);
            meshLinterna.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            meshLinterna.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(meshLinterna.RenderType);
            meshVela.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            meshVela.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(meshVela.RenderType);
            meshFarol.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            meshFarol.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(meshFarol.RenderType);
            //Render personames enemigos
            enemigo.render();
            device.EndScene();
            pSurf.Dispose();


            // dibujo el glow map
            effect.Technique = "DefaultTechnique";
            pSurf = g_pGlowMap.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            //Dibujamos SOLO los meshes que tienen glow brillantes
            //Render personaje brillante
            //Render personames enemigos
            enemigo.render();
            foreach(Recarga rec in recargas)
            {
                if(!rec.usada)
                {
                    rec.mesh.Effect = effect;
                    rec.mesh.Technique = "DefaultTechnique";
                    rec.mesh.render();
                }
            }
            if(!espada.encontrado)
            {
                espada.mesh.Effect = effect;
                espada.mesh.Technique = "DefaultTechnique";
                espada.mesh.render();
            }
            if (!locket.encontrado)
            {
                locket.mesh.Effect = effect;
                locket.mesh.Technique = "DefaultTechnique";
                locket.mesh.render();
            }
            if (!copa.encontrado)
            {
                copa.mesh.Effect = effect;
                copa.mesh.Technique = "DefaultTechnique";
                copa.mesh.render();
            }
            if (!llave.encontrado)
            {
                llave.mesh.Effect = effect;
                llave.mesh.Technique = "DefaultTechnique";
                llave.mesh.render();
            }
            // El resto opacos
            renderizarMeshesConEfecto(todosLosMeshesIluminables, effect, "DibujarObjetosOscuros");
            meshLinterna.Effect = effect;
            meshLinterna.Technique = "DibujarObjetosOscuros";
            meshVela.Effect = effect;
            meshVela.Technique = "DibujarObjetosOscuros";
            meshFarol.Effect = effect;
            meshFarol.Technique = "DibujarObjetosOscuros";
            renderizarObjetoIluminacion(elapsedTime);
            meshLinterna.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            meshLinterna.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(meshLinterna.RenderType);
            meshVela.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            meshVela.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(meshVela.RenderType);
            meshFarol.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            meshFarol.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(meshFarol.RenderType);
            device.EndScene();
            pSurf.Dispose();

            // Hago un blur sobre el glow map
            // 1er pasada: downfilter x 4
            // -----------------------------------------------------
            pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.BeginScene();
            effect.Technique = "DownFilter4";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pGlowMap);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            pSurf.Dispose();
            device.EndScene();
            device.DepthStencilSurface = pOldDS;

            // Pasadas de blur
            for (int P = 0; P < cant_pasadas; ++P)
            {
                // Gaussian blur Horizontal
                // -----------------------------------------------------
                pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf);
                // dibujo el quad pp dicho :
                device.BeginScene();
                effect.Technique = "GaussianBlurSeparable";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget4);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                pSurf.Dispose();
                device.EndScene();

                pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf);
                pSurf.Dispose();

                //  Gaussian blur Vertical
                // -----------------------------------------------------
                device.BeginScene();
                effect.Technique = "GaussianBlurSeparable";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget4Aux);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(1);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                device.EndScene();

            }


            //  To Gray Scale
            // -----------------------------------------------------
            // Ultima pasada vertical va sobre la pantalla pp dicha
            device.SetRenderTarget(0, pOldRT);
            //pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
            //device.SetRenderTarget(0, pSurf);

            device.BeginScene();
            effect.Technique = "GrayScale";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);
            effect.SetValue("g_GlowMap", g_pRenderTarget4Aux);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();


            device.EndScene();
        
    }
        static public bool hayQueRenderizarlo(TgcBoundingBox objeto)
        {
            TgcCollisionUtils.FrustumResult r = TgcCollisionUtils.classifyFrustumAABB(GuiController.Instance.Frustum, objeto);
            return (r != TgcCollisionUtils.FrustumResult.OUTSIDE);
        }
        void renderizarMiedo(float elapsedTime)
        {
            Device device  = GuiController.Instance.D3dDevice;
            //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
            //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
            pOldRT = device.GetRenderTarget(0);
            Surface pSurf = renderTarget2D.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);


            //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
            device.BeginScene();


            //Como estamos en modo CustomRenderEnabled, tenemos que dibujar todo nosotros, incluso el contador de FPS
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

            //Tambien hay que dibujar el indicador de los ejes cartesianos
            GuiController.Instance.AxisLines.render();
            bool lightEnable = (bool)GuiController.Instance.Modifiers["lightEnable"];


            //Actualzar posición de la luz
            Vector3 lightPos = camara.getPosition();
            //Normalizar direccion de la luz
            Vector3 lightDir = camara.target - camara.eye;
            lightDir.Normalize();
            //Dibujamos todos los meshes del escenario
            renderizarMeshes(todosLosMeshesIluminables, lightEnable, lightPos, lightDir);
            enemigo.render();
            renderizarObjetoIluminacion(elapsedTime);
            foreach (Recarga rec in recargas)
            {
                if (!rec.usada)
                {
                    
                    rec.mesh.render();
                }
            }
            if (!espada.encontrado)
            {
                espada.mesh.render();
            }
            if (!locket.encontrado)
            {
               
                locket.mesh.render();
            }
            if (!copa.encontrado)
            {
                
                copa.mesh.render();
            }
            if (!llave.encontrado)
            {
                
                llave.mesh.render();
            }
            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            device.EndScene();

            //Liberar memoria de surface de Render Target
            pSurf.Dispose();

            //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
            //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);


            //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
            device.SetRenderTarget(0, pOldRT);


            //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
            drawPostProcess(device,elapsedTime);
        }
        private void drawPostProcess(Device d3dDevice,float elapsedTime)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            efectoMiedo.Technique = "OndasTechnique";

            //Cargamos parametros en el shader de Post-Procesado
            efectoMiedo.SetValue("render_target2D", renderTarget2D);
            efectoMiedo.SetValue("ondas_vertical_length", 60);
            efectoMiedo.SetValue("ondas_size", 0.01f);


            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            efectoMiedo.Begin(FX.None);
            efectoMiedo.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            efectoMiedo.EndPass();
            efectoMiedo.End();

            //Terminamos el renderizado de la escena
            d3dDevice.EndScene();
        }
        private void reiniciarJuego()
        {
            enMenu = true;
            mostrarInstrucciones = false;
            GuiController.Instance.CustomRenderEnabled = true;
            perdido = true;
            enemigoActivo = true;
            enWaypoints = true;
            enLocker = false;
            iteracion = 0;
            enMenu = true;
            gameOver = false;
            ganado = false;
            contadorDetecciones = 0;
            enemigoEsperandoPuerta = false;
            cant_pasadas = 3;
            conNightVision = false;
            contadorDetecciones = 0;
            esperandoPuerta = false;
            ObjetoIluminacion = 0;
            tiempoIluminacion = 100;
            tiempo = 0;
            tiempoBuscando = TIEMPO_DE_BUSQUEDA;
            meshEnemigo.Position = new Vector3(POSICION_INICIAL_ENEMIGO_X, 0, POSICION_INICIAL_ENEMIGO_Z);
            enemigo.indiceActual = -1;
            enemigo.paso = 1;
            enemigoActivo = true;
            enWaypoints = true;
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
            foreach(Puerta puerta in puertas)
            {
                puerta.abierta = false;
                puerta.girando = false;
                puerta.angulo = 1.605f;
            }
            foreach(Recarga rec in recargas)
            {
                rec.usada = false;
            }
        }

    }
}
