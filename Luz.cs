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
using TgcViewer.Utils.Shaders;
using System.Windows.Forms;

namespace AlumnoEjemplos.GODMODE
{
    class Luz
    {
       public Vector3[] posicionesDeLuces = new Vector3[4];
        Effect godmodeSpotlightMultiDiffuse = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Shaders\\SpotlightMultiDifuse.fx");
        Effect godmodePointLightMultiDiffuse = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Shaders\\PointLightMultiDifuse.fx");
        Effect godmodeTextureProyection = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Shaders\\TextureProyection.fx");
        ColorValue[] lightColors = new ColorValue[5];
        Vector4[] pointLightPositions = new Vector4[5];
        float[] pointLightIntensity = new float[5];
        float[] pointLightAttenuation = new float[5];
        // Shadow map
        readonly int SHADOWMAP_SIZE = 1024;
        Texture g_pShadowMap;    // Texture to which the shadow map is rendered
        Texture miTex;
        Surface g_pDSShadow;     // Depth-stencil buffer for rendering to shadow map
        Matrix g_mShadowProj;    // Projection matrix for shadow map
        Vector3 g_LightPos;						// posicion de la luz actual (la que estoy analizando)
        Vector3 g_LightDir;						// direccion de la luz actual
        Matrix g_LightView;						// matriz de view del light
        float near_plane = 2f;
        float far_plane = 1500f;
        public Luz()
        {   
            miTex = TextureLoader.FromFile(GuiController.Instance.D3dDevice, GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\proy.png");
            Control panel3d = GuiController.Instance.Panel3d;
            float aspectRatio = (float)panel3d.Width / (float)panel3d.Height;
            g_mShadowProj = Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(80),
                aspectRatio, 2, 1500);
            GuiController.Instance.D3dDevice.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(45.0f),
                aspectRatio, near_plane, far_plane);


        }

        public void prenderLuz(int tipo, TgcMesh mesh)
        {
            Effect currentShader;

            if (tipo == 0)
            {
                currentShader = godmodeSpotlightMultiDiffuse;
                mesh.Effect = currentShader;
            }
            else if (tipo == 1)
            {
                currentShader = godmodeTextureProyection;
                mesh.Effect = currentShader;
            }
            else if (tipo == 2)
            {
                currentShader = godmodePointLightMultiDiffuse;
                mesh.Effect = currentShader;
            } 
            
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
        }
        public void apagarLuz(TgcMesh mesh)
        {
            mesh.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
        }
        public void renderizarLuz(int tipo, Vector3 posicionCamara, Vector3 direccionDeLuz, TgcMesh mesh, float intensidad,float temblor)
        {
            g_LightPos = posicionCamara;
            //g_LightDir = direccionDeLuz - g_LightPos;
            g_LightDir = direccionDeLuz;
            g_LightDir.Normalize();

            var random = FastMath.Cos(6 * temblor);

            //Actualzar posición de la luz
            Vector3 lightPos = posicionCamara;

            //Normalizar direccion de la luz

            Vector3 lightDir = direccionDeLuz;
            lightDir.Normalize();
            //Cargar variables shader de la luz
            if (tipo == 0)
            {   lightColors[0] = ColorValue.FromColor(Color.White);
                pointLightPositions[0] = TgcParserUtils.vector3ToVector4(lightPos);
                pointLightIntensity[0] = intensidad;
                pointLightAttenuation[0] = 0.48f;
                for (int i = 1; i < 5; i++)
                {
                    lightColors[i] = ColorValue.FromColor(Color.White);
                    pointLightPositions[i] = TgcParserUtils.vector3ToVector4(posicionesDeLuces[i - 1]);
                    pointLightIntensity[i] = (float)GuiController.Instance.Modifiers["lightIntensity"];
                    pointLightAttenuation[i] = (float)GuiController.Instance.Modifiers["lightAttenuation"];
                }

                mesh.Effect.SetValue("lightColor", lightColors);
                mesh.Effect.SetValue("lightPosition", pointLightPositions);
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
                mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(39f));
                mesh.Effect.SetValue("spotLightExponent", 14f);

              
                    
                
                //Variables de los materiales
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularExp", 11f);
                mesh.render();
            }
            else if (tipo == 1)
            {    // Calculo la matriz de view de la luz

                //mesh.Effect.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
                mesh.Effect.SetValue("g_vLightPos", new Vector4(posicionCamara.X, posicionCamara.Y, posicionCamara.Z, 1));
                //mesh.Effect.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
                mesh.Effect.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
                mesh.Effect.SetValue("texProy", miTex);
                g_LightView = Matrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new Vector3(0, 0, 1));
                // inicializacion standard: 
                //mesh.Effect.SetValue("g_mProjLight", g_mShadowProj);
                mesh.Effect.SetValue("g_mViewLightProj", g_LightView * g_mShadowProj);
                lightColors[0] = ColorValue.FromColor(Color.Yellow);
                pointLightPositions[0] = TgcParserUtils.vector3ToVector4(lightPos);
                pointLightIntensity[0] = intensidad; //POR 2??    
                pointLightAttenuation[0] = 0.67f;
                for (int i = 1; i < 5; i++)
                {
                    lightColors[i] = ColorValue.FromColor(Color.White);
                    pointLightPositions[i] = TgcParserUtils.vector3ToVector4(posicionesDeLuces[i - 1]);
                    pointLightIntensity[i] = (float)GuiController.Instance.Modifiers["lightIntensity"];
                    pointLightAttenuation[i] = (float)GuiController.Instance.Modifiers["lightAttenuation"];
                }

                mesh.Effect.SetValue("lightColor", lightColors);
                mesh.Effect.SetValue("lightPosition", pointLightPositions);
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);

                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialSpecularExp",33f);
                mesh.render();
            }
            else if (tipo==2)
            {
                lightColors[0] = ColorValue.FromColor(Color.LightGoldenrodYellow);
                pointLightPositions[0] = TgcParserUtils.vector3ToVector4(lightPos);
                pointLightIntensity[0] = intensidad + random * 3; 
                pointLightAttenuation[0] = 0.67f;

                for (int i = 1; i < 5; i++)
                {
                    lightColors[i] = ColorValue.FromColor(Color.White);
                    pointLightPositions[i] = TgcParserUtils.vector3ToVector4(posicionesDeLuces[i - 1]);
                    pointLightIntensity[i] = (float)GuiController.Instance.Modifiers["lightIntensity"];
                    pointLightAttenuation[i] = (float)GuiController.Instance.Modifiers["lightAttenuation"];
                }

                mesh.Effect.SetValue("lightColor", lightColors);
                mesh.Effect.SetValue("lightPosition", pointLightPositions);
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("lightIntensity", pointLightIntensity);
                mesh.Effect.SetValue("lightAttenuation", pointLightAttenuation);
                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.Orange));
                mesh.Effect.SetValue("materialSpecularExp", 23f);
                mesh.render();
            }


        }
    }
}
