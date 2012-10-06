using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SystemInput
{
    class PCinput
    {
        private KeyboardState currentKeyboardState;
        public PCinput()
        {
            enableMouseSmoothing = true;
            mouseMovement = new Vector2[2];
            mouseSmoothingSensitivity = 0.5f;
            mouseSmoothingCache = new Vector2[10];
            mouseMovement[0].X = 0.0f;
            mouseMovement[0].Y = 0.0f;
            mouseMovement[1].X = 0.0f;
            mouseMovement[1].Y = 0.0f;
            currentKeyboardState = Keyboard.GetState();
        }

        private void IsKeyHeldDown(KeyboardState currentKeyboardState, ref bool Shoot)
        {
            if (currentKeyboardState.IsKeyDown(Keys.Space))
                Shoot = true;
            else if (currentKeyboardState.IsKeyUp(Keys.Space))
                Shoot = false;
        }

        private bool KeyToggledPressed(Keys key, KeyboardState currentKeyboardState, KeyboardState prevKeyboardState)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        public void PCActions(ref bool Shoot, ref bool Exit, ref bool displayHelp, ref bool MouseSmoothing, ref bool enableParallax,
            ref bool enableColorMap, ref bool ToggleFullScreen)
        {
            KeyboardState prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            IsKeyHeldDown(currentKeyboardState, ref Shoot);

            if (KeyToggledPressed(Keys.Escape, currentKeyboardState, prevKeyboardState))
                Exit = true;

            if (KeyToggledPressed(Keys.H, currentKeyboardState, prevKeyboardState))
                displayHelp = !displayHelp;

            if (KeyToggledPressed(Keys.M, currentKeyboardState, prevKeyboardState))
                MouseSmoothing = !MouseSmoothing;

            if (KeyToggledPressed(Keys.P, currentKeyboardState, prevKeyboardState))
                enableParallax = !enableParallax;

            if (KeyToggledPressed(Keys.T, currentKeyboardState, prevKeyboardState))
                enableColorMap = !enableColorMap;

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) || currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyToggledPressed(Keys.Enter, currentKeyboardState, prevKeyboardState))
                    ToggleFullScreen = true;
            }
        }

        public void PCMovement(ref bool MoveForward, ref bool MoveBackward, ref bool MoveRight, ref bool MoveLeft)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                MoveForward = true;
            }

            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                MoveBackward = true;
            }

            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                MoveRight = true;
            }

            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                MoveLeft = true;
            }
        }

        bool enableMouseSmoothing = true;
        private int mouseIndex;
        private float mouseSmoothingSensitivity;
        private Vector2[] mouseMovement;
        private Vector2[] mouseSmoothingCache;

        public void PCLook(out Vector2 smoothedMouseMovement, Rectangle clientBounds)
        {
            MouseState currentMouseState = Mouse.GetState();

            int centerX = clientBounds.Width / 2;
            int centerY = clientBounds.Height / 2;
            int deltaX = centerX - currentMouseState.X;
            int deltaY = centerY - currentMouseState.Y;

            Mouse.SetPosition(centerX, centerY);

            if (enableMouseSmoothing)
            {
                PerformLookFiltering((float)deltaX, (float)deltaY, out smoothedMouseMovement);
                PerformLookSmoothing(smoothedMouseMovement.X, smoothedMouseMovement.Y, out smoothedMouseMovement);
            }
            else
            {
                smoothedMouseMovement.X = (float)deltaX;
                smoothedMouseMovement.Y = (float)deltaY;
            }
        }

        private void PerformLookSmoothing(float x, float y, out Vector2 smoothedMouseMovement)
        {
            mouseMovement[mouseIndex].X = x;
            mouseMovement[mouseIndex].Y = y;

            smoothedMouseMovement.X = (mouseMovement[0].X + mouseMovement[1].X) * 0.5f;
            smoothedMouseMovement.Y = (mouseMovement[0].Y + mouseMovement[1].Y) * 0.5f;

            mouseIndex ^= 1;
            mouseMovement[mouseIndex].X = 0.0f;
            mouseMovement[mouseIndex].Y = 0.0f;
        }

        private void PerformLookFiltering(float x, float y, out Vector2 smoothedMouseMovement)
        {
            // Shuffle all the entries in the cache.
            // Newer entries at the front. Older entries towards the back.
            for (int i = mouseSmoothingCache.Length - 1; i > 0; --i)
            {
                mouseSmoothingCache[i].X = mouseSmoothingCache[i - 1].X;
                mouseSmoothingCache[i].Y = mouseSmoothingCache[i - 1].Y;
            }

            // Store the current mouse movement entry at the front of cache.
            mouseSmoothingCache[0].X = x;
            mouseSmoothingCache[0].Y = y;

            float averageX = 0.0f;
            float averageY = 0.0f;
            float averageTotal = 0.0f;
            float currentWeight = 1.0f;

            // Filter the mouse movement with the rest of the cache entries.
            // Use a weighted average where newer entries have more effect than
            // older entries (towards the back of the cache).
            for (int i = 0; i < mouseSmoothingCache.Length; ++i)
            {
                averageX += mouseSmoothingCache[i].X * currentWeight;
                averageY += mouseSmoothingCache[i].Y * currentWeight;
                averageTotal += 1.0f * currentWeight;
                currentWeight *= mouseSmoothingSensitivity;
            }

            // Calculate the new smoothed mouse movement.
            smoothedMouseMovement.X = averageX / averageTotal;
            smoothedMouseMovement.Y = averageY / averageTotal;
        }
    }
}
