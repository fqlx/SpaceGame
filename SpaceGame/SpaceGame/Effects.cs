using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Effects
{
    class Default
    {
        /// A light. This light structure is the same as the one defined in
        /// the parallax_normal_mapping.fx file. The only difference is the
        /// LightType enum.
        private struct Light
        {
            public enum LightType
            {
                DirectionalLight,
                PointLight,
                SpotLight
            }

            public LightType Type;
            public Vector3 Direction;
            public Vector3 Position;
            public Color Ambient;
            public Color Diffuse;
            public Color Specular;
            public float SpotInnerConeRadians;
            public float SpotOuterConeRadians;
            public float Radius;
        }

        private Light light;
        private Color globalAmbient;
        private const float WALL_HEIGHT = 256.0f;
        
        private Vector2 scaleBias;
        private Effect effect;
       
        /// A material. This material structure is the same as the one defined
        /// in the parallax_normal_mapping.fx file. We use the Color type here
        /// instead of a four element floating point array.
        private struct Material
        {
            public Color Ambient;
            public Color Diffuse;
            public Color Emissive;
            public Color Specular;
            public float Shininess;
        }
        private Material material;
        private MappingUtils.NormalMappedRoom room;

        private const float FLOOR_PLANE_SIZE = 324.0f;
        private const float CEILING_TILE_FACTOR = 8.0f;

        private const float FLOOR_TILE_FACTOR = 8.0f;
        private const float FLOOR_CLIP_BOUNDS = FLOOR_PLANE_SIZE * 0.5f - 30.0f;
        private const float CAMERA_BOUNDS_PADDING = 30.0f;
        private const float CAMERA_BOUNDS_MIN_Z = -FLOOR_PLANE_SIZE / 2.0f + CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MAX_Z = FLOOR_PLANE_SIZE / 2.0f - CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MIN_X = -FLOOR_PLANE_SIZE / 2.0f + CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MAX_X = FLOOR_PLANE_SIZE / 2.0f - CAMERA_BOUNDS_PADDING;
        private const float WALL_TILE_FACTOR_X = 8.0f;
        private const float WALL_TILE_FACTOR_Y = 2.0f;

        public Default(GraphicsDevice graphicsDevice)
        {
             // Parallax mapping height scale and bias values.
            scaleBias = new Vector2(0.04f, -0.03f);

            // Initialize point lighting for the scene.
            globalAmbient = new Color(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            light.Type = Light.LightType.PointLight;
            light.Direction = Vector3.Zero;
            light.Position = new Vector3(0.0f, WALL_HEIGHT - (0.25f * WALL_HEIGHT), 0.0f);
            light.Ambient = Color.White;
            light.Diffuse = Color.White;
            light.Specular = Color.White;
            light.SpotInnerConeRadians = MathHelper.ToRadians(40.0f);
            light.SpotOuterConeRadians = MathHelper.ToRadians(70.0f);
            light.Radius = Math.Max(FLOOR_PLANE_SIZE, WALL_HEIGHT);

            
            // Initialize material settings. Just a plain lambert material.
            material.Ambient = new Color(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            material.Diffuse = new Color(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            material.Emissive = Color.Black;
            material.Specular = Color.Black;
            material.Shininess = 0.0f;


            // Create the room.
            room = new MappingUtils.NormalMappedRoom(graphicsDevice,
                    FLOOR_PLANE_SIZE, WALL_HEIGHT, FLOOR_TILE_FACTOR,
                    CEILING_TILE_FACTOR, WALL_TILE_FACTOR_X, WALL_TILE_FACTOR_Y);

            // Create an empty white texture. This will be bound to the
            // colorMapTexture shader parameter when the user wants to
            // disable the color map texture. This trick will allow the
            // same shader to be used for when textures are enabled and
            // disabled.

            nullTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);

            Color[] pixels = { Color.White };

            nullTexture.SetData(pixels);
        }

        private Texture2D nullTexture;
        private Texture2D brickColorMap;
        private Texture2D brickNormalMap;
        private Texture2D brickHeightMap;
        private Texture2D stoneColorMap;
        private Texture2D stoneNormalMap;
        private Texture2D stoneHeightMap;
        private Texture2D woodColorMap;
        private Texture2D woodNormalMap;
        private Texture2D woodHeightMap;

        public void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>(@"Effects\parallax_normal_mapping");
            effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingPointLighting"];

            brickColorMap = content.Load<Texture2D>(@"Textures\brick_color_map");
            brickNormalMap = content.Load<Texture2D>(@"Textures\brick_normal_map");
            brickHeightMap = content.Load<Texture2D>(@"Textures\brick_height_map");

            stoneColorMap = content.Load<Texture2D>(@"Textures\stone_color_map");
            stoneNormalMap = content.Load<Texture2D>(@"Textures\stone_normal_map");
            stoneHeightMap = content.Load<Texture2D>(@"Textures\stone_height_map");

            woodColorMap = content.Load<Texture2D>(@"Textures\wood_color_map");
            woodNormalMap = content.Load<Texture2D>(@"Textures\wood_normal_map");
            woodHeightMap = content.Load<Texture2D>(@"Textures\wood_height_map");
        }

        public void UpdateEffect(bool enableParallax, NS_Camera.FirstPersonCamera camera)
        {
            if (enableParallax)
                effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingPointLighting"];
            else
                effect.CurrentTechnique = effect.Techniques["NormalMappingPointLighting"];

            effect.Parameters["worldMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldInverseTransposeMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldViewProjectionMatrix"].SetValue(camera.ViewMatrix * camera.ProjectionMatrix);

            effect.Parameters["cameraPos"].SetValue(camera.Position);
            effect.Parameters["globalAmbient"].SetValue(globalAmbient.ToVector4());
            effect.Parameters["scaleBias"].SetValue(scaleBias);

            effect.Parameters["light"].StructureMembers["dir"].SetValue(light.Direction);
            effect.Parameters["light"].StructureMembers["pos"].SetValue(light.Position);
            effect.Parameters["light"].StructureMembers["ambient"].SetValue(light.Ambient.ToVector4());
            effect.Parameters["light"].StructureMembers["diffuse"].SetValue(light.Diffuse.ToVector4());
            effect.Parameters["light"].StructureMembers["specular"].SetValue(light.Specular.ToVector4());
            effect.Parameters["light"].StructureMembers["spotInnerCone"].SetValue(light.SpotInnerConeRadians);
            effect.Parameters["light"].StructureMembers["spotOuterCone"].SetValue(light.SpotOuterConeRadians);
            effect.Parameters["light"].StructureMembers["radius"].SetValue(light.Radius);

            effect.Parameters["material"].StructureMembers["ambient"].SetValue(material.Ambient.ToVector4());
            effect.Parameters["material"].StructureMembers["diffuse"].SetValue(material.Diffuse.ToVector4());
            effect.Parameters["material"].StructureMembers["emissive"].SetValue(material.Emissive.ToVector4());
            effect.Parameters["material"].StructureMembers["specular"].SetValue(material.Specular.ToVector4());
            effect.Parameters["material"].StructureMembers["shininess"].SetValue(material.Shininess);

        }


        public void DrawColorRoom(bool enableColorMap, GraphicsDevice graphicsDevice)
        {
            // Draw the room.
            if (enableColorMap)
            {
                room.Draw(graphicsDevice, effect,
                    "colorMapTexture", "normalMapTexture", "heightMapTexture",
                    brickColorMap, brickNormalMap, brickHeightMap,
                    stoneColorMap, stoneNormalMap, stoneHeightMap,
                    woodColorMap, woodNormalMap, woodHeightMap);
            }
            else
            {
                room.Draw(graphicsDevice, effect,
                    "colorMapTexture", "normalMapTexture", "heightMapTexture",
                    nullTexture, brickNormalMap, brickHeightMap,
                    nullTexture, stoneNormalMap, stoneHeightMap,
                    nullTexture, woodNormalMap, woodHeightMap);
            }
        }
    }
}
