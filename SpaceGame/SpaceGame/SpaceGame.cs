
using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

using NS_Camera;


namespace XNAFirstPersonCamera
{
    public class Demo : Microsoft.Xna.Framework.Game
    {
        private static void Main()
        {
            using (Demo demo = new Demo())
            {
                demo.Run();
            }
        }

        private const float WEAPON_SCALE = 0.03f;
        private const float WEAPON_X_OFFSET = 0.45f;
        private const float WEAPON_Y_OFFSET = -0.75f;
        private const float WEAPON_Z_OFFSET = 1.65f;

        private const float CAMERA_FOVX = 85.0f;
        private const float CAMERA_ZNEAR = 0.01f;
        private const float FLOOR_PLANE_SIZE = 1024.0f;
        private const float CAMERA_ZFAR = FLOOR_PLANE_SIZE * 2.0f;
        private const float CAMERA_PLAYER_EYE_HEIGHT = 110.0f;
        private const float CAMERA_ACCELERATION_X = 800.0f;
        private const float CAMERA_ACCELERATION_Y = 800.0f;
        private const float CAMERA_ACCELERATION_Z = 800.0f;
        private const float CAMERA_VELOCITY_X = 200.0f;
        private const float CAMERA_VELOCITY_Y = 200.0f;
        private const float CAMERA_VELOCITY_Z = 200.0f;
        private const float CAMERA_RUNNING_MULTIPLIER = 2.0f;
        private const float CAMERA_RUNNING_JUMP_MULTIPLIER = 1.5f;
        

        private const float CAMERA_BOUNDS_MIN_Y = 0.0f;
        private const float WALL_HEIGHT = 256.0f;
        private const float CAMERA_BOUNDS_MAX_Y = WALL_HEIGHT;

        private const float FLOOR_TILE_FACTOR = 8.0f;
        private const float FLOOR_CLIP_BOUNDS = FLOOR_PLANE_SIZE * 0.5f - 30.0f;
        private const float CAMERA_BOUNDS_PADDING = 30.0f;
        private const float CAMERA_BOUNDS_MIN_Z = -FLOOR_PLANE_SIZE / 2.0f + CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MAX_Z = FLOOR_PLANE_SIZE / 2.0f - CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MIN_X = -FLOOR_PLANE_SIZE / 2.0f + CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MAX_X = FLOOR_PLANE_SIZE / 2.0f - CAMERA_BOUNDS_PADDING;


        private GraphicsDeviceManager graphics;
        
        
        private KeyboardState currentKeyboardState;
        private FirstPersonCamera camera;
        
        private Model weapon;
        private Matrix[] weaponTransforms;
        private Matrix weaponWorldMatrix;
       
       
       
       
        private Vector2 fontPos;
        private int windowWidth;
        private int windowHeight;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private bool enableColorMap;
        private bool enableParallax;
        private bool displayHelp;


        private float shootingtime;

        private Debug.Stats DebugStats;
        private SystemInput.PCinput PCinput;
        private Bullets.Basic BasicBullets;
        private Effects.Default DefaultEffects;

        List<Bullets.Basic> bulletList;

        public Demo()
        {
           graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            camera = new FirstPersonCamera(this);
            Components.Add(camera);

            Window.Title = "XNA 4.0 First Person Camera";
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width / 2;
            windowHeight = GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            // Initially enable the diffuse color map texture.
            enableColorMap = true;

            // Initially enable parallax mapping.
            enableParallax = true;

            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);

            // Setup the camera.

            camera.Acceleration = new Vector3(
                CAMERA_ACCELERATION_X,
                CAMERA_ACCELERATION_Y,
                CAMERA_ACCELERATION_Z);
            camera.VelocityWalking = new Vector3(
                CAMERA_VELOCITY_X,
                CAMERA_VELOCITY_Y,
                CAMERA_VELOCITY_Z);
            camera.VelocityRunning = new Vector3(
                camera.VelocityWalking.X * CAMERA_RUNNING_MULTIPLIER,
                camera.VelocityWalking.Y * CAMERA_RUNNING_JUMP_MULTIPLIER,
                camera.VelocityWalking.Z * CAMERA_RUNNING_MULTIPLIER);
            camera.Perspective(
                CAMERA_FOVX,
                (float)windowWidth / (float)windowHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);

            // Initialize the weapon matrices.
            weaponTransforms = new Matrix[weapon.Bones.Count];
            weaponWorldMatrix = Matrix.Identity;

            // Get the initial keyboard state.
            currentKeyboardState = Keyboard.GetState();

           

            PCinput = new SystemInput.PCinput();

            List<Bullets.Basic> bulletList = new List<Bullets.Basic>();
            

        }

        protected override void LoadContent()
        {
            weapon = Content.Load<Model>(@"Models\weapon");

            DebugStats = new Debug.Stats();
            DebugStats.LoadContent(Content, GraphicsDevice);

            BasicBullets = new Bullets.Basic();
            BasicBullets.LoadContent(Content);

            DefaultEffects = new Effects.Default(GraphicsDevice);
            DefaultEffects.LoadContent(Content);
        }

        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Very simple camera collision detection logic to prevent the camera
        /// from moving below the floor and from moving outside the bounds of
        /// the room.
        /// </summary>
        private void PerformCameraCollisionDetection()
        {
            Vector3 newPos = camera.Position;

            if (camera.Position.X > CAMERA_BOUNDS_MAX_X)
                newPos.X = CAMERA_BOUNDS_MAX_X;

            if (camera.Position.X < CAMERA_BOUNDS_MIN_X)
                newPos.X = CAMERA_BOUNDS_MIN_X;

            if (camera.Position.Y > CAMERA_BOUNDS_MAX_Y)
                newPos.Y = CAMERA_BOUNDS_MAX_Y;

            if (camera.Position.Y < CAMERA_BOUNDS_MIN_Y)
                newPos.Y = CAMERA_BOUNDS_MIN_Y;

            if (camera.Position.Z > CAMERA_BOUNDS_MAX_Z)
                newPos.Z = CAMERA_BOUNDS_MAX_Z;

            if (camera.Position.Z < CAMERA_BOUNDS_MIN_Z)
                newPos.Z = CAMERA_BOUNDS_MIN_Z;

            camera.Position = newPos;
        }


        private void ProcessActions(GameTime gameTime)
        {
            bool Shoot = false;
            bool Exit = false;
            bool ToggleFull = false;
            bool MS = camera.EnableMouseSmoothing;

            //help,parallax,colormap are autotoggled
            PCinput.PCActions(ref Shoot, ref Exit, ref displayHelp, ref MS, ref enableParallax, ref enableColorMap, ref ToggleFull);

            if (Exit)
                this.Exit();

            //ms doesnt work from fpc.cs
            camera.EnableMouseSmoothing = MS;

            if (ToggleFull)
                DebugStats.ToggleFullScreen(ref graphics, camera, CAMERA_FOVX, CAMERA_ZNEAR, CAMERA_ZFAR);

            if (Shoot)
            {
                shootingtime += 0.05f; //* (float)gameTime.TotalGameTime.TotalSeconds;
                //run time error
                //bulletList.Add(new Bullets.Basic());
                //bulletList[0].UpdateBullets(gameTime, camera, shootingtime);
                BasicBullets.UpdateBullets(gameTime, camera, shootingtime);
            }
            else
            {
                shootingtime = 0.0f;
            }
        }

        private void UpdateWeapon()
        {

            weapon.CopyAbsoluteBoneTransformsTo(weaponTransforms);

            weaponWorldMatrix = camera.SpawnWorldMatrix(WEAPON_X_OFFSET,
                WEAPON_Y_OFFSET, WEAPON_Z_OFFSET, WEAPON_SCALE);
        }


        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            base.Update(gameTime);

            ProcessActions(gameTime);
            PerformCameraCollisionDetection();

            DefaultEffects.UpdateEffect(enableParallax, camera);

            UpdateWeapon();

            DebugStats.UpdateFrameRate(gameTime);

            base.Draw(gameTime);
            DebugStats.IncrementFrameCounter();

        }

        protected override void Draw(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            base.Draw(gameTime);

            DefaultEffects.DrawColorRoom(enableColorMap, GraphicsDevice);

            DebugStats.DrawText(displayHelp, enableParallax, camera);
            DebugStats.IncrementFrameCounter();

            BasicBullets.Draw(camera);
        }
    }
}

