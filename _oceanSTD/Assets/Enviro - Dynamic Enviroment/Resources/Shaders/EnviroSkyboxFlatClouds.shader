// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Enviro/SkyboxFlatClouds"
{
	Properties
	{
		_Stars("Stars Cubemap", Cube) = "black" {}
		_Galaxy("Galaxy Cubemap", Cube) = "black" {}
		_SatTex("Satellites Tex", 2D) = "black" {}
		_MoonTex("Moon Tex", 2D) = "black" {}
	}
	SubShader 
	{
	    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "IgnoreProjector"="True" }
	    Cull Back     
		Fog {Mode Off} 
		ZWrite Off    

    	Pass 
    	{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			#include "UnityCG.cginc"

			uniform float3 _Br;
			uniform float3 _Bm;
			uniform float3 _mieG;
			uniform float  _SunIntensity;
			uniform float _Exposure;
			uniform float _SkyLuminance;
			uniform float _scatteringPower;
			uniform float _SunDiskSize;
		    uniform float _SunDiskIntensity;
		    uniform float _StarsIntensity;
			uniform float4 _scatteringColor;
			uniform float4 _sunDiskColor;
			uniform samplerCUBE _Stars;
			uniform float4x4 _StarsMatrix;
			uniform float _SkyColorPower;
			uniform float3 _SunDir;
			uniform float3 _MoonDir;
			//uniform float _noiseScale;
			//uniform half _noiseIntensity;
			uniform float4 _weatherSkyMod;
			uniform float _hdr;
			uniform float _moonGlowStrenght;
			uniform float4 _moonGlowColor;
			uniform sampler2D _MoonTex;
			uniform sampler2D _SatTex;
			uniform float4 _MoonColor;
			uniform float _MoonSize;
			uniform float _MoonBrightness;
			uniform float _GalaxyIntensity;
			uniform samplerCUBE _Galaxy;
			uniform int _blackGround;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 Position : SV_POSITION;
				float2 moonPos : TEXCOORD0;
				float4 sky : TEXCOORD1;
				float night : TEXCOORD2;
				float3 texcoord : TEXCOORD3;
				float3 starPos : TEXCOORD4;
				float4 screenUV : TEXCOORD5;
			//	float4 WorldPosition : TEXCOORD6;
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.Position = UnityObjectToClipPos(v.vertex);
				//o.WorldPosition = normalize(mul((float4x4)unity_ObjectToWorld, v.vertex)).xyzw;
				float3 viewDir = normalize(v.texcoord + float3(0.0, 0.1, 0.0));
				o.sky.x = saturate(_SunDir.y + 0.25);
				o.sky.y = saturate(clamp(1.0 - _SunDir.y, 0.0, 0.5));
				o.sky.z = saturate(dot(-_MoonDir.xyz, viewDir));
				float3 r = normalize(cross(_MoonDir.xyz, float3(0, 0, 1)));
				float3 u = cross(_MoonDir.xyz, r);
				o.moonPos = float2(dot(r, v.vertex.xyz), dot(u, v.vertex.xyz)) * (21.0 - _MoonSize) - 0.5;
				o.moonPos *= -1;
				o.starPos = mul((float3x3)_StarsMatrix, v.vertex.xyz);
				o.night = pow(max(0.0, viewDir.y), 1.25);
				o.texcoord = v.texcoord;
				o.screenUV = ComputeScreenPos(o.Position);
				return o;
			}



			float4 frag(v2f i) : SV_Target
			{
				float2 screenPosition = (i.screenUV.xy / i.screenUV.w);
				float3 viewDir = normalize(i.texcoord);
				float cosTheta = dot(viewDir,_SunDir);
				viewDir = normalize(i.texcoord + float3(0.0,0.1,0.0));
				float zen = acos(saturate(viewDir.y));
				float alb = (cos(zen) + 0.5 * pow(93.885 - ((zen * 180.0) / 3.141592), -0.253));
				float3 fex = exp(-(_Br * (4 / alb) + _Bm * (1.25 / alb)));
				float rayPhase = 2.5 + pow(cosTheta,2);
				float miePhase = _mieG.x / pow(_mieG.y - _mieG.z * cosTheta, 1);
				float3 BrTheta = 0.059683 * _Br * rayPhase;
				float3 BmTheta = 0.079577  * _Bm * miePhase;
				float3 BrmTheta = (BrTheta + BmTheta * 2.0) / ((_Bm + _Br) * 0.75);
				float3 scattering = BrmTheta * _SunIntensity * (1.0 - fex);
				float3 sunClr = lerp(fex, _sunDiskColor.rgb, 0.75) * _SunDiskIntensity;
				float3 sunDisk = (min(2, pow((1 - cosTheta) * (_SunDiskSize * 100), -2)) * sunClr) * saturate(_sunDiskColor);
				float4 moonSampler = tex2D(_MoonTex, i.moonPos.xy);
				//half3 moonNormal = UnpackNormal(tex2D(_MoonNormal, i.moonPos.xy*-1));
				//float3 lightVector = normalize(_SunPos - _MoonPos);
				//float d = saturate(max(0.0,dot(moonNormal,lightVector)) * 4);
				//moonSampler = lerp(float4(0,0,0,0),moonSampler,d);
				moonSampler = (moonSampler * _MoonColor) * _MoonBrightness * 2;
				float3 moonArea = pow(dot(viewDir, -_MoonDir), (21.0 - _MoonSize) * 10);
				float3 moonBright = saturate((float3(1,1,1)) * moonArea * i.night);
				float3 skyFinalize = saturate((pow(1.0 - fex, 2.0) * 0.234) * (1 - i.sky.x)) * _SkyLuminance;
				skyFinalize = saturate(lerp(float3(0.1,0.1,0.1), skyFinalize, saturate(dot(viewDir.y + 0.3, float3(0,1,0)))) * (1 - fex));
				float fadeStar = i.night * _StarsIntensity * 50;
				float3 starsMap = texCUBE(_Stars, i.starPos.xyz);
				float starsBehindMoon = 1 - clamp((moonBright * 5), 0, 1);
				float3 stars = clamp((starsMap * fadeStar) * starsBehindMoon,0,4);
				float3 galaxyMap = texCUBE(_Galaxy, i.starPos.xyz);
				float3 galaxy = galaxyMap * (i.night * _GalaxyIntensity);
				scattering *= saturate((lerp(float3(_scatteringPower, _scatteringPower, _scatteringPower), pow(2000.0f * BrmTheta * fex, 0.75f), i.sky.y) * 0.05));
				scattering *= (_SkyLuminance * _scatteringColor.rgb) * pow((1 - fex), 2) * i.sky.x;
				float3 skyScattering = (scattering + sunDisk) + (skyFinalize + galaxy + stars + ((moonBright * _moonGlowColor)*_moonGlowStrenght));
				float4 satSampler = tex2D(_SatTex, screenPosition);
				skyScattering = satSampler.rgb + skyScattering * (1 - satSampler.a);
				skyScattering += moonSampler.rgb * i.sky.z;

				//Tonemapping
				if (_hdr == 0)
					skyScattering = saturate(1.0 - exp(-_Exposure * skyScattering));

				skyScattering = pow(skyScattering,_SkyColorPower);
				skyScattering = lerp(skyScattering, (lerp(skyScattering,_weatherSkyMod.rgb,_weatherSkyMod.a)),_weatherSkyMod.a);

				#if defined(UNITY_COLORSPACE_GAMMA)
				skyScattering = pow(skyScattering,0.454545);
				#endif

				if (viewDir.y < 0 && _blackGround > 0)
					skyScattering = 0;

				//Dithering Disabled for now because of issue in OpenGl on Mac
				//float2 wcoord = (i.WorldPosition.xy / i.WorldPosition.w) * _noiseScale;
				//float4 dither = (dot(float2(171.0f, 231.0f), wcoord.xy));
				//dither.rgb = frac(dither / float3(103.0f, 71.0f, 97.0f)) - float3(0.5f, 0.5f, 0.5f);

				return float4(skyScattering, 1.0);// +(dither / 255.0f) * _noiseIntensity;
			}
				ENDCG
		}
		//----------------------------------------------
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag

		uniform sampler2D _CloudMap;
		uniform float _CloudAlpha;
		uniform float _CloudCoverage;
		uniform float _CloudAltitude;
		uniform float4 _CloudColor;
		uniform float _CloudColorPower;
		uniform float2 _CloudAnimation;
		uniform float _CloudAnimationSpeed;

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 Position : SV_POSITION;
			float4 uv : TEXCOORD0;
			float3 worldPos : TEXCOORD1;
		};

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			o.Position = UnityObjectToClipPos(v.vertex);
			o.worldPos = normalize(mul((float4x4)unity_ObjectToWorld, v.vertex)).xyz;
			float3 viewDir = normalize(o.worldPos + float3(0,1,0));
			o.worldPos.y *= 1 - dot(viewDir.y + _CloudAltitude, float3(0,-0.15,0));
			return o;
		}

		float4 frag(v2f i) : SV_Target
		{
			float3 uvs = normalize(i.worldPos);
			float2 uv1 = (uvs.xz * 0.2) + _CloudAnimation;
			float2 uv2 = (uvs.xz * 0.4) + _CloudAnimation;
			float4 clouds1 = tex2D(_CloudMap, uv1.xy);
			float4 clouds2 = tex2D(_CloudMap, uv2.xy);
			float color1 = pow(clouds1.g + clouds2.g, 0.1);
			float color2 = pow(clouds2.b * clouds1.r, 0.2);
			float4 finalClouds = lerp(clouds1, clouds2, color1 * color2);
			float cloudExtinction = pow(uvs.y , 2);
			finalClouds.a *= _CloudAlpha;
			finalClouds.a *= cloudExtinction;
			if (uvs.y < 0)
				finalClouds.a = 0;
			finalClouds.rgb = finalClouds.a * pow(_CloudColor,_CloudColorPower);
			finalClouds.rgb = pow(finalClouds.rgb,1 - _CloudCoverage);
			return finalClouds;
		}
			ENDCG
		}



		// -----------------------------------------------

    	Pass 
    	{
    		Blend SrcAlpha OneMinusSrcAlpha
    		
		CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _Cloud1Map;
			uniform float _Cloud1Alpha;
			uniform float _Cloud1Coverage;
			uniform float _Cloud1Altitude;
			uniform float4 _Cloud1Color;
		    uniform float _Cloud1ColorPower;
			uniform float2 _CloudAnimation;
		    uniform float _CloudAnimationSpeed;
		    uniform float _gameTime;

			struct appdata{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			};

			struct v2f 
			{
    			float4 Position : SV_POSITION;
				float3 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
    			v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.Position = UnityObjectToClipPos(v.vertex);
				o.worldPos = normalize(mul((float4x4)unity_ObjectToWorld, v.vertex)).xyz;
				o.uv = normalize(mul((float4x4)unity_ObjectToWorld, v.vertex)).xyz;
				float3 viewDir = normalize(o.worldPos + float3(0,0,0));
				o.uv.y *= dot(viewDir.y + _Cloud1Altitude, float3(0,-0.15,0));
				return o;
			}

			float PhaseHenyeyGreenStein(float inScatteringAngle, float g)
			{
				return ((1.0 - g * g) / pow((1.0 + g * g - 2.0 * g * inScatteringAngle), 3.0 / 2.0)) / (4.0 * 3.14159);
			}
			
			float4 frag(v2f i) : SV_Target
			{

				float3 LightDirection = _WorldSpaceLightPos0;
				float inScatteringAngle = dot(i.worldPos, LightDirection);

			    float3 uvs = normalize(i.uv);
				float4 clouds1 = tex2D(_Cloud1Map, uvs.xz * 0.5 + 0.5);
				float cloudExtinction = pow(uvs.y , 2);
				float alpha = clouds1.a;

				alpha *= _Cloud1Alpha;
				alpha *= cloudExtinction;
				alpha = clamp(alpha, 0, 1);

				float3 finalClouds = clouds1.rgb * _Cloud1Color ;

				finalClouds = pow(finalClouds, _Cloud1ColorPower);
				float hg = PhaseHenyeyGreenStein(inScatteringAngle, 0.75);

				if(uvs.y > 0)
					alpha = 0;

				finalClouds += (pow(_Cloud1Color,2.5) * hg) * (1-alpha);

				return float4(finalClouds.rgb * 1.5, alpha);
			}
			ENDCG
    	}

    }
}