//Input del Vertex Shader
struct VS_INPUT_DEFAULT 
{
   float4 Position : POSITION0;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};


//Vertex Shader
VS_OUTPUT_DEFAULT vs_default( VS_INPUT_DEFAULT Input )
{
   VS_OUTPUT_DEFAULT Output;

   //Proyectar posicion
   Output.Position = float4(Input.Position.xy, 0, 1);
   
   //Las Texcoord quedan igual
   Output.Texcoord = Input.Texcoord;

   return( Output );
}



//Textura del Render target 2D
texture render_target2D;
sampler RenderTarget = sampler_state
{
    Texture = (render_target2D);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	BorderColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
    MipFilter = NONE;
    MinFilter = NONE;
    MagFilter = NONE;
};


//Input del Pixel Shader
struct PS_INPUT_DEFAULT 
{
   float2 Texcoord : TEXCOORD0;
};

//Pixel Shader
float4 ps_default( PS_INPUT_DEFAULT Input ) : COLOR0
{      
	float4 color = tex2D( RenderTarget, Input.Texcoord );
	return color;
}



technique DefaultTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_default();
	  PixelShader = compile ps_2_0 ps_default();
   }
}

float tiempo;
float ondas_size;
float alarmaScaleFactor = 0.1;

//Textura alarma
texture textura_alarma;
sampler sampler_alarma = sampler_state
{
	Texture = (textura_alarma);
};
//Pixel Shader de Ondas
float4 ps_ondas( PS_INPUT_DEFAULT Input ) : COLOR0
{     
	//Alterar coordenadas de textura
	Input.Texcoord.y = Input.Texcoord.y + ( sin( Input.Texcoord.x + tiempo ) * ondas_size);
Input.Texcoord.x = Input.Texcoord.x + (sin(Input.Texcoord.y + tiempo ) * ondas_size);
	//Obtener color de textura
	float4 color = tex2D( RenderTarget, Input.Texcoord );
	//Obtener color de textura de alarma, escalado por un factor
	float4 color2 = tex2D(sampler_alarma, Input.Texcoord) * alarmaScaleFactor;
	return  color+color2*0.3;
}




technique OndasTechnique
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_default();
	  PixelShader = compile ps_2_0 ps_ondas();
   }
}

