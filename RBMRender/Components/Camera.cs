﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Input;

namespace RBMRender.Components
{
	public class Camera : GameSystem
	{
		private const float MovementScale = 0.1f;
		private const float MovementUpscale = 0.8f;
		private readonly KeyboardManager _keyboardManager;
		private readonly MouseManager _mouseManager;
		public Vector3 LookAt = new Vector3(0f, 0f, 0f);
		public float Pitch;
		public float Yaw;
		private KeyboardState _keyboardState;
		private Point _lastMousePosition = new Point(0, 0);
		private MouseState _mouseState;

		/// <summary>
		///     Initializes a new instance of the <see cref="T:SharpDX.Toolkit.GameSystem" /> class.
		/// </summary>
		/// <param name="game">The game.</param>
		public Camera(Game game)
			: base(game)
		{
			World = Matrix.Identity;
			_mouseManager = new MouseManager(Game);
			_keyboardManager = new KeyboardManager(Game);

			UpdateOrder = 1;

			Enabled = true;

			View = Matrix.LookAtRH(Position, Position + 10, Vector3.Up);
		}

		public Matrix World { get; set; }
		public Matrix Projection { get; set; }
		public Matrix View { get; set; }
		public Vector3 Position { get; set; }

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out Point lpPoint);

		#region Overrides of GameSystem

		public Matrix CameraRotation = Matrix.Identity;
		private bool _shiftPressed;
		public float Fov = (float)Math.PI / 4.0f;

		private void UpdateViewMatrix()
		{
			_keyboardState = _keyboardManager.GetState();
			var keys = new List<Keys>();
			_keyboardState.GetDownKeys(keys);
			_shiftPressed = keys.Any(k => k == Keys.RightShift || k == Keys.LeftShift);

			foreach (Keys pressedKey in keys)
			{
				switch (pressedKey)
				{
					case Keys.Q:
						Yaw += 0.02f;
						break;
					case Keys.E:
						Yaw -= 0.02f;
						break;

					case Keys.W:
					case Keys.Up:
						Move(CameraRotation.Forward);
						break;
					case Keys.S:
					case Keys.Down:
						Move(CameraRotation.Backward);
						break;
					case Keys.D:
					case Keys.Right:
						Move(CameraRotation.Right);
						break;
					case Keys.A:
					case Keys.Left:
						Move(CameraRotation.Left);
						break;
					case Keys.PageUp:
					case Keys.Space:
						Move(CameraRotation.Up);
						break;
					case Keys.PageDown:
					case Keys.LeftControl:
					case Keys.RightControl:
						Move(CameraRotation.Down);
						break;
				}
			}

			CameraRotation.Forward.Normalize();
			CameraRotation.Right.Normalize();
			CameraRotation.Up.Normalize();

			CameraRotation *= Matrix.RotationY(Yaw);
			CameraRotation *= Matrix.RotationAxis(CameraRotation.Right, Pitch);
			Vector3 target = Position + CameraRotation.Forward;
			View = Matrix.LookAtRH(Position, target, CameraRotation.Up);
		}

		/// <summary>
		///     This method is called when this game component is updated.
		/// </summary>
		/// <param name="gameTime">The current timing.</param>
		public override void Update(GameTime gameTime)
		{
			Yaw = Pitch = 0f;
			_mouseState = _mouseManager.GetState();
			Point mousePos;
			GetCursorPos(out mousePos);
			if (_mouseState.RightButton.Down)
			{
				int deltaX = _lastMousePosition.X - mousePos.X;
				int deltaY = _lastMousePosition.Y - mousePos.Y;
				_lastMousePosition = mousePos;


				Yaw += deltaX*.01f;
				Pitch += deltaY*.01f;
			}
			else
			{
				_lastMousePosition = mousePos;
			}

			UpdateViewMatrix();
			float aspectRatio = GraphicsDevice.BackBuffer.Width/(float) GraphicsDevice.BackBuffer.Height;
			Projection = Matrix.PerspectiveFovRH(Fov, aspectRatio, 0.01f, 10000.0f);

			// Handle base.Update
			base.Update(gameTime);
		}

		private void Move(Vector3 v)
		{
			float speed = _shiftPressed ? MovementUpscale : MovementScale;
			Position += speed*v;
		}

		#endregion
	}
}