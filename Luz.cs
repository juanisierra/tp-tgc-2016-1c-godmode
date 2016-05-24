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

namespace AlumnoEjemplos.GODMODE
{
    class Luz
    {
       public Vector3[] posicionesDeLuces = new Vector3[4];
        Effect godmodeSpotlightMultiDiffuse = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosDir + "GODMODE\\Media\\Shaders\\SpotlightMultiDifuse.fx");
        ColorValue[] lightColors = new ColorValue[5];
        Vector4[] pointLightPositions = new Vector4[5];
        float[] pointLightIntensity = new float[5];
        float[] pointLightAttenuation = new float[5];

        public Effect currentShader;
        public Luz()
        {
            this.currentShader = godmodeSpotlightMultiDiffuse;
        }
        public void prenderLuz(int tipo, TgcMesh mesh)
        {
            
            
            if (tipo == 0)
            {
                currentShader = godmodeSpotlightMultiDiffuse;
                mesh.Effect = currentShader;
            } else if(tipo == 1 || tipo ==2)
            {
                currentShader = GuiController.Instance.Shaders.TgcMeshPointLightShader;
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
        public void renderizarLuz(int tipo, Vector3 posicionCamara, Vector3 direccionDeLuz, TgcMesh mesh, float intensidad,float temblor,Boolean shadow)
        {
            var random = FastMath.Cos(6 * temblor);

            //Actualzar posición de la luz
            Vector3 lightPos = posicionCamara;
            //Cargar variables shader de la luz
            if (shadow)
            { mesh.Technique = "RenderShadow";
            } else
            {
                mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
            }
            if (tipo == 0)
            {   lightColors[0] = ColorValue.FromColor(Color.White);
                pointLightPositions[0] = TgcParserUtils.vector3ToVector4(lightPos);
                pointLightIntensity[0] = intensidad*2;
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
                mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(direccionDeLuz));
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
            {
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("lightIntensity", intensidad*2);
                mesh.Effect.SetValue("lightAttenuation", 0.67f);

                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialSpecularExp",33f);
                mesh.render();
            }
            else if (tipo==2)
            {
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.LightGoldenrodYellow));//PeachPuff  Orange
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("lightIntensity", intensidad+random*3);
                mesh.Effect.SetValue("lightAttenuation", 0.67f);

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
