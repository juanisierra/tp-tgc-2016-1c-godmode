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
    class Luz
    {
        
        public void prenderLuz(int tipo, TgcMesh mesh)
        {
            Effect currentShader;
            if (tipo == 0)
            {
                currentShader = GuiController.Instance.Shaders.TgcMeshSpotLightShader;
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
        public void renderizarLuz(int tipo, Vector3 posicionCamara, Vector3 direccionDeLuz, TgcMesh mesh, float intensidad,float temblor)
        {
            var random = FastMath.Cos(6 * temblor);

            //Actualzar posición de la luz
            Vector3 lightPos = posicionCamara;

            //Normalizar direccion de la luz

            Vector3 lightDir = direccionDeLuz;
            lightDir.Normalize();
            //Cargar variables shader de la luz
            if (tipo == 0)
            {
                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
                mesh.Effect.SetValue("lightIntensity", intensidad);
                mesh.Effect.SetValue("lightAttenuation", 0.48f);
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
