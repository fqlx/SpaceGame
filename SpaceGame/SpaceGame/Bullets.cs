using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Bullets
{
    class Basic
    {
        private Model bulletModel;
        private Matrix[] bulletTransforms;
        private Matrix bulletWorldMatrix;
        private const float WEAPON_X_OFFSET = 0.45f;
        private const float WEAPON_Y_OFFSET = -0.75f;
        private const float WEAPON_Z_OFFSET = 1.65f;
        private const float WEAPON_SCALE = 0.03f;

        public Basic()
        {
            bulletWorldMatrix = new Matrix();
        }

        public void LoadContent(ContentManager content)
        {
            bulletModel = content.Load<Model>(@"Models\YellowSphere");
            bulletTransforms = new Matrix[bulletModel.Bones.Count];
            bulletWorldMatrix = Matrix.Identity;
        }

        private void DrawModel(Model model, Matrix[] effectWorldTransform, Matrix effectWorldMatrix, Matrix effectView, Matrix effectProjection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = effectWorldTransform[mesh.ParentBone.Index] * effectWorldMatrix;
                    effect.View = effectView; // camera.ViewMatrix;
                    effect.Projection = effectProjection;// camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }

        public void Draw(NS_Camera.FirstPersonCamera camera)
        {
            DrawModel(bulletModel, bulletTransforms, bulletWorldMatrix, camera.ViewMatrix, camera.ProjectionMatrix);
        }

        public void UpdateBullets(GameTime gameTime, NS_Camera.FirstPersonCamera camera, float bulletTime)
        {
                bulletModel.CopyAbsoluteBoneTransformsTo(bulletTransforms);

                bulletWorldMatrix = camera.SpawnWorldMatrix(WEAPON_X_OFFSET,
                    WEAPON_Y_OFFSET, WEAPON_Z_OFFSET * bulletTime, WEAPON_SCALE);
        }
    }
}
