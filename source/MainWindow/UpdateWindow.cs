using System;
using System.Drawing;
using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;

using RB3_X360_Keyboard.Properties;

namespace Program
{
	public partial class MainWindow : Form
	{
		/// <summary>
		/// Array for the MIDI lights.
		/// </summary>
		/// <remarks>
		/// Indexing:
		/// 0 = Output enabled, 1 = Foot Controller, 2 = Channel Volume, 3 = Drum Mode
		/// </remarks>
		bool[] midiLight = new bool[4];
		/// <summary>
		/// Indicates if the MIDI lights are animating.
		/// </summary>
		bool animating = false;
		/// <summary>
		/// Current animation step.
		/// </summary>
		/// <remarks>
		/// See the AnimationSteps enum for number definitions.
		/// </remarks>
		int animationType = 0;

		/// <summary>
		/// Available animation steps.
		/// </summary>
		enum AnimationSteps
		{
			None = 0,
			End1 = 1,
			End2 = 2,
			Increasing1 = 10,
			Increasing2 = 11,
			Increasing3 = 12,
			Increasing4 = 13,
			Increasing5 = 14,
			Decreasing1 = 20,
			Decreasing2 = 21,
			Decreasing3 = 22,
			Decreasing4 = 23,
			Decreasing5 = 24,
			Reset1 = 30,
			Reset2 = 31,
			Reset3 = 32,
			Reset4 = 33,
		}


		/// <summary>
		/// Updates the values of the window controls.
		/// </summary>
		void UpdateValues()
		{
			// Toggle drum mode.
			if(checkbox_Setting_DrumMode.Enabled)
			{
				if(outputMode == (int)OutputModes.MIDI)
				{
					if(stateCurrent.dpadU != statePrevious.dpadU)
					{
						if(stateCurrent.dpadU)
						{
							drumMode = !drumMode;

							checkbox_Setting_DrumMode.CheckedChanged -= checkbox_SettingDrumMode_Changed;
							checkbox_Setting_DrumMode.Checked = drumMode;
							checkbox_Setting_DrumMode.CheckedChanged += checkbox_SettingDrumMode_Changed;
						}
					}
				}
			}

			// Set the analog pedal mode.
			if(radio_Pedal_Expression.Enabled)
			{
				if(stateCurrent.dpadL != statePrevious.dpadL)
				{
					if(stateCurrent.dpadL)
					{
						pedalMode = (int)PedalModes.Expression;
						radio_Pedal_Expression.Checked = true;
					}
				}
			}
			if(radio_Pedal_ChannelVolume.Enabled)
			{
				if(stateCurrent.dpadD != statePrevious.dpadD)
				{
					if(stateCurrent.dpadD)
					{
						pedalMode = (int)PedalModes.ChannelVolume;
						radio_Pedal_ChannelVolume.Checked = true;
					}
				}
			}
			if(radio_Pedal_FootController.Enabled)
			{
				if(stateCurrent.dpadR != statePrevious.dpadR)
				{
					if(stateCurrent.dpadR)
					{
						pedalMode = (int)PedalModes.FootController;
						radio_Pedal_FootController.Checked = true;
					}
				}
			}

			// Switch the program number.
			if(numUpDown_Setting_Program.Enabled)
			{
				if((stateCurrent.btnA != statePrevious.btnA) || (stateCurrent.btnY != statePrevious.btnY))
				{
					if(stateCurrent.btnA && stateCurrent.btnY)
					{
						program = 0;
						PlayAnimation(3);
					}
					else
					{
						if(stateCurrent.btnA)	
						{
							program -= 1;
							PlayAnimation(2);
						}
						if(stateCurrent.btnY)	
						{
							program += 1;
							PlayAnimation(1);
						}
					}
					program = Math.Clamp(program, (byte)0, (byte)127);
					numUpDown_Setting_Program.Value = program + 1;
				}
			}

			// Switch the octave number.
			if(numUpDown_Setting_Octave.Enabled)
			{
				if((stateCurrent.btnB != statePrevious.btnB) || (stateCurrent.btnX != statePrevious.btnX))
				{
					if(stateCurrent.btnB && stateCurrent.btnX)
					{
						octave = 4;
						PlayAnimation(3);
					}
					else
					{
						if(stateCurrent.btnX)
						{
							octave -= 1;
							PlayAnimation(2);
						}
						if(stateCurrent.btnB)
						{
							octave += 1;
							PlayAnimation(1);
						}
					}
					octave = Math.Clamp(octave, (byte)0, (byte)8);
					numUpDown_Setting_Octave.Value = octave;
				}
			}

			if(!animating)
			{
				midiLight[0] = outputStarted;
				midiLight[1] = pedalMode == (int)PedalModes.FootController;
				midiLight[2] = pedalMode == (int)PedalModes.ChannelVolume;
				midiLight[3] = drumMode;
			}

			if(outputStarted)
			{
				if(midiLight[0]) image_MidiLight1.BackColor = Color.Red;
				else image_MidiLight1.BackColor = Color.Black;

				if(midiLight[1]) image_MidiLight2.BackColor = Color.Red;
				else image_MidiLight2.BackColor = Color.Black;

				if(midiLight[2]) image_MidiLight3.BackColor = Color.Red;
				else image_MidiLight3.BackColor = Color.Black;

				if(midiLight[3]) image_MidiLight4.BackColor = Color.Red;
				else image_MidiLight4.BackColor = Color.Black;
			}
		}

		/// <summary>
		/// Plays one of the MIDI light animations.
		/// </summary>
		private void PlayAnimation(int animation)
		{
			switch(animation)
			{
				case 1:
					animationTimer.Enabled = true;
					animating = true;
					animationType = 10;
					break;

				case 2:
					animationTimer.Enabled = true;
					animating = true;
					animationType = 20;
					break;

				case 3:
					animationTimer.Enabled = true;
					animating = true;
					animationType = 30;
					break;
			}
		}

		/// <summary>
		/// Progresses through one of the MIDI light animations.
		/// </summary>
		private void animationTimer_Elapsed(Object source, ElapsedEventArgs e)
		{
			switch(animationType)
			{
				default:
					break;

				// Animation end
				// 0000
				case (int)AnimationSteps.End1:
					midiLight[0] = false;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.End2;
					break;

				// 0000 for an additional 100 ms
				case (int)AnimationSteps.End2:
					midiLight[0] = false;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.None;
					animationTimer.Enabled = false;
					animating = false;
					break;

				// Increase
				// 1000
				case (int)AnimationSteps.Increasing1:
					midiLight[0] = true;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Increasing2;
					break;

				// 1100
				case (int)AnimationSteps.Increasing2:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Increasing3;
					break;

				// 1110
				case (int)AnimationSteps.Increasing3:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Increasing4;
					break;

				// 1111
				case (int)AnimationSteps.Increasing4:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = true;
					animationType = (int)AnimationSteps.Increasing5;
					break;

				// 1111 for an additional 100 ms
				case (int)AnimationSteps.Increasing5:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = true;
					animationType = (int)AnimationSteps.End1;
					break;

				// Decrease
				// 1111
				case (int)AnimationSteps.Decreasing1:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = true;
					animationType = (int)AnimationSteps.Decreasing2;
					break;

				// 1110
				case (int)AnimationSteps.Decreasing2:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Decreasing3;
					break;

				// 1100
				case (int)AnimationSteps.Decreasing3:
					midiLight[0] = true;
					midiLight[1] = true;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Decreasing4;
					break;

				// 1000
				case (int)AnimationSteps.Decreasing4:
					midiLight[0] = true;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = 24;
					break;

				// 1000 for an additional 100 ms
				case (int)AnimationSteps.Decreasing5:
					midiLight[0] = true;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.End1;
					break;

				// Reset
				// 0110
				case (int)AnimationSteps.Reset1:
					midiLight[0] = false;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Reset2;
					break;

				// 0110 for an additional 100 ms
				case (int)AnimationSteps.Reset2:
					midiLight[0] = false;
					midiLight[1] = true;
					midiLight[2] = true;
					midiLight[3] = false;
					animationType = (int)AnimationSteps.Reset3;
					break;

				// 1001
				case (int)AnimationSteps.Reset3:
					midiLight[0] = true;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = true;
					animationType = (int)AnimationSteps.Reset4;
					break;

				// 1001
				case (int)AnimationSteps.Reset4:
					midiLight[0] = true;
					midiLight[1] = false;
					midiLight[2] = false;
					midiLight[3] = true;
					animationType = (int)AnimationSteps.End1;
					break;
			}
		}

		/// <summary>
		/// Updates the state of the images on the window.
		/// </summary>
		void UpdateImages()
		{
			// Update the state of the button/key images based on the current state of each button.
			// Buttons
			if(stateCurrent.btnA) image_AButton.Image = Resources.aPressed_cropped;
			else image_AButton.Image = Resources.a_cropped;

			if(stateCurrent.btnB) image_BButton.Image = Resources.bPressed_cropped;
			else image_BButton.Image = Resources.b_cropped;

			if(stateCurrent.btnX) image_XButton.Image = Resources.xPressed_cropped;
			else image_XButton.Image = Resources.x_cropped;

			if(stateCurrent.btnY) image_YButton.Image = Resources.yPressed_cropped;
			else image_YButton.Image = Resources.y_cropped;

			if(stateCurrent.btnSt) image_StartButton.Image = Resources.startPressed_cropped;
			else image_StartButton.Image = Resources.start_cropped;

			if(stateCurrent.btnBk) image_BackButton.Image = Resources.backPressed_cropped;
			else image_BackButton.Image = Resources.back_cropped;

			if(stateCurrent.overdrive) image_OverdriveButton.Image = Resources.overdrivePressed_cropped;
			else image_OverdriveButton.Image = Resources.overdrive_cropped;

			if(stateCurrent.pedalDigital) image_Pedal.Image = Resources.pedalPressed_small;
			else image_Pedal.Image = Resources.pedal_small;

			// D-pad
			switch(stateCurrent.dpadU, stateCurrent.dpadD, stateCurrent.dpadL, stateCurrent.dpadR)
			{
				// Not pressed
				default:
				case (false, false, false, false):
					image_Dpad.Image = Resources.dpad_cropped;
					break;

				// Up
				case (true, false, false, false):
					image_Dpad.Image = Resources.dpadUp_cropped;
					break;
				// Down
				case (false, true, false, false):
					image_Dpad.Image = Resources.dpadDown_cropped;
					break;
				// Left
				case (false, false, true, false):
					image_Dpad.Image = Resources.dpadLeft_cropped;
					break;
				// Right
				case (false, false, false, true):
					image_Dpad.Image = Resources.dpadRight_cropped;
					break;
				
				// Up + Down (impossible under normal circumstances)
				case (true, true, false, false):
					image_Dpad.Image = Resources.dpadUpDown_cropped;
					break;
				// Up + Left
				case (true, false, true, false):
					image_Dpad.Image = Resources.dpadUpLeft_cropped;
					break;
				// Up + Right
				case (true, false, false, true):
					image_Dpad.Image = Resources.dpadUpRight_cropped;
					break;
				// Down + Left
				case (false, true, true, false):
					image_Dpad.Image = Resources.dpadDownLeft_cropped;
					break;
				// Down + Right
				case (false, true, false, true):
					image_Dpad.Image = Resources.dpadDownRight_cropped;
					break;
				// Left + Right (impossible under normal circumstances)
				case (false, false, true, true):
					image_Dpad.Image = Resources.dpadDownRight_cropped;
					break;

				// Up + Down + Left (impossible under normal circumstances)
				case (true, true, true, false):
					image_Dpad.Image = Resources.dpadDownLeftUp_cropped;
					break;
				// Up + Down + Right (impossible under normal circumstances)
				case (true, true, false, true):
					image_Dpad.Image = Resources.dpadUpRightDown_cropped;
					break;
				// Up + Left + Right (impossible under normal circumstances)
				case (true, false, true, true):
					image_Dpad.Image = Resources.dpadUpLeftRight_cropped;
					break;
				// Down + Left + Right (impossible under normal circumstances)
				case (false, true, true, true):
					image_Dpad.Image = Resources.dpadDownLeftRight_cropped;
					break;

				// Up + Down + Left + Right (impossible under normal circumstances)
				case (true, true, true, true):
					image_Dpad.Image = Resources.dpadUpDownLeftRight_cropped;
					break;
			}

			// Guide button
			label_guideConnectionStatus.Visible = !controllerConnected;
			switch (playerIndex)
			{
				default:
				case -1:
					if(stateCurrent.btnGuide) image_GuideButton.Image = Resources.guidePressedLightsOff_cropped;
					else image_GuideButton.Image = Resources.guideLightsOff_cropped;
					break;
				case 0:
					if(stateCurrent.btnGuide) image_GuideButton.Image = Resources.guidePressedPlayer1_cropped;
					else image_GuideButton.Image = Resources.guidePlayer1_cropped;
					break;
				case 1:
					if(stateCurrent.btnGuide) image_GuideButton.Image = Resources.guidePressedPlayer2_cropped;
					else image_GuideButton.Image = Resources.guidePlayer2_cropped;
					break;
				case 2:
					if(stateCurrent.btnGuide) image_GuideButton.Image = Resources.guidePressedPlayer3_cropped;
					else image_GuideButton.Image = Resources.guidePlayer3_cropped;
					break;
				case 3:
					if(stateCurrent.btnGuide) image_GuideButton.Image = Resources.guidePressedPlayer4_cropped;
					else image_GuideButton.Image = Resources.guidePlayer4_cropped;
					break;
			}

			// Keys
			if(stateCurrent.key[0]) image_KeyC1.Image = (Image)Resources.keyLeftPressed_cropped.Clone();
			else image_KeyC1.Image = (Image)Resources.keyLeft_cropped.Clone();

			if(stateCurrent.key[1]) image_KeyDb1.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyDb1.Image = (Image)Resources.keyBlack_cropped.Clone();
			
			if(stateCurrent.key[2]) image_KeyD1.Image = (Image)Resources.keyCenterPressed_cropped.Clone();
			else image_KeyD1.Image = (Image)Resources.keyCenter_cropped.Clone();

			if(stateCurrent.key[3]) image_KeyEb1.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyEb1.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[4]) image_KeyE1.Image = (Image)Resources.keyRightPressed_cropped.Clone();
			else image_KeyE1.Image = (Image)Resources.keyRight_cropped.Clone();

			if(stateCurrent.key[5]) image_KeyF1.Image = (Image)Resources.keyLeftPressed_cropped.Clone();
			else image_KeyF1.Image = (Image)Resources.keyLeft_cropped.Clone();

			if(stateCurrent.key[6]) image_KeyGb1.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyGb1.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[7]) image_KeyG1.Image = (Image)Resources.keyMiddleLeftPressed_cropped.Clone();
			else image_KeyG1.Image = (Image)Resources.keyMiddleLeft_cropped.Clone();

			if(stateCurrent.key[8]) image_KeyAb1.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyAb1.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[9]) image_KeyA1.Image = (Image)Resources.keyMiddleRightPressed_cropped.Clone();
			else image_KeyA1.Image = (Image)Resources.keyMiddleRight_cropped.Clone();

			if(stateCurrent.key[10]) image_KeyBb1.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyBb1.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[11]) image_KeyB1.Image = (Image)Resources.keyRightPressed_cropped.Clone();
			else image_KeyB1.Image = (Image)Resources.keyRight_cropped.Clone();

			if(stateCurrent.key[12]) image_KeyC2.Image = (Image)Resources.keyLeftPressed_cropped.Clone();
			else image_KeyC2.Image = (Image)Resources.keyLeft_cropped.Clone();

			if(stateCurrent.key[13]) image_KeyDb2.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyDb2.Image = (Image)Resources.keyBlack_cropped.Clone();
			
			if(stateCurrent.key[14]) image_KeyD2.Image = (Image)Resources.keyCenterPressed_cropped.Clone();
			else image_KeyD2.Image = (Image)Resources.keyCenter_cropped.Clone();

			if(stateCurrent.key[15]) image_KeyEb2.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyEb2.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[16]) image_KeyE2.Image = (Image)Resources.keyRightPressed_cropped.Clone();
			else image_KeyE2.Image = (Image)Resources.keyRight_cropped.Clone();

			if(stateCurrent.key[17]) image_KeyF2.Image = (Image)Resources.keyLeftPressed_cropped.Clone();
			else image_KeyF2.Image = (Image)Resources.keyLeft_cropped.Clone();

			if(stateCurrent.key[18]) image_KeyGb2.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyGb2.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[19]) image_KeyG2.Image = (Image)Resources.keyMiddleLeftPressed_cropped.Clone();
			else image_KeyG2.Image = (Image)Resources.keyMiddleLeft_cropped.Clone();

			if(stateCurrent.key[20]) image_KeyAb2.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyAb2.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[21]) image_KeyA2.Image = (Image)Resources.keyMiddleRightPressed_cropped.Clone();
			else image_KeyA2.Image = (Image)Resources.keyMiddleRight_cropped.Clone();

			if(stateCurrent.key[22]) image_KeyBb2.Image = (Image)Resources.keyBlackPressed_cropped.Clone();
			else image_KeyBb2.Image = (Image)Resources.keyBlack_cropped.Clone();

			if(stateCurrent.key[23]) image_KeyB2.Image = (Image)Resources.keyRightPressed_cropped.Clone();
			else image_KeyB2.Image = (Image)Resources.keyRight_cropped.Clone();

			if(stateCurrent.key[24]) image_KeyC3.Image = (Image)Resources.keyEndPressed_cropped;
			else image_KeyC3.Image = (Image)Resources.keyEnd_cropped;
		}
	}
}