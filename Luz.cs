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
    {   public void prenderLuz(TgcMesh mesh)
        {
            Effect currentShader;
            currentShader = GuiController.Instance.Shaders.TgcMeshSpotLightShader;
            mesh.Effect = currentShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
        }
        public void apagarLuz(TgcMesh mesh)
        {
            mesh.Effect = GuiController.Instance.Shaders.TgcMeshShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);
        }
       public void renderizarLuz(Vector3 posicionCamara, Vector3 direccionDeLuz,TgcMesh mesh,float intensidad)
        {
            //Actualzar posición de la luz
            Vector3 lightPos = posicionCamara;

            //Normalizar direccion de la luz

            Vector3 lightDir= direccionDeLuz;
            lightDir.Normalize();
            //Cargar variables shader de la luz
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
                    mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                    mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
                    mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(lightDir));
                    mesh.Effect.SetValue("lightIntensity", intensidad);
                    mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
                    mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad((float)GuiController.Instance.Modifiers["spotAngle"]));
                    mesh.Effect.SetValue("spotLightExponent", (float)GuiController.Instance.Modifiers["spotExponent"]);
                    //Variables de los materiales
                    mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
                    mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
                    mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
                    mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
                    mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);
            mesh.render();
        }
            
            
    }
}
