using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NAudio.Wave;
using System.Windows.Input;

namespace wave
{
    public partial class MainWindow : Window
    {
        private const int GridRows = 9;
        private const int GridColumns = 9;
        private const double ButtonSize = 50; // Adjusted for square buttons
        private const double ButtonMargin = 5;

        // Dictionary to map buttons to sound file paths
        private Dictionary<Button, string> buttonSoundMap = new Dictionary<Button, string>();

        // Global volume (0-100)
        private float globalVolume = 0.5f; // Default 50%

        // NAudio WaveOutEvent for playback
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        public MainWindow()
        {
            InitializeComponent();
            GenerateSoundButtons();
        }

        /// <summary>
        /// Dynamically generates 81 square sound buttons with random colors.
        /// </summary>
        private void GenerateSoundButtons()
        {
            Random rand = new Random();

            for (int i = 1; i <= GridRows * GridColumns; i++)
            {
                Button soundButton = new Button
                {
                    Width = ButtonSize,
                    Height = ButtonSize,
                    Margin = new Thickness(ButtonMargin),
                    Style = (Style)this.FindResource("SoundButtonStyle"),
                    Tag = $"Sound{i}" // Assign a unique identifier
                };

                // Assign a random color to the button's background
                Color randomColor = Color.FromRgb(
                    (byte)rand.Next(50, 256), // Avoid very dark colors
                    (byte)rand.Next(50, 256),
                    (byte)rand.Next(50, 256));
                soundButton.Background = new SolidColorBrush(randomColor);

                // Optional: Store the color for later use
                // soundButton.Tag = randomColor;

                soundButton.Click += SoundButton_Click;

                // Optionally, set tooltip or content to indicate the sound number
                soundButton.ToolTip = $"Sound {i}";

                // Add the button to the grid
                SoundButtonsGrid.Children.Add(soundButton);

                // Map the button to a default sound file or leave empty
                // For now, we'll leave it empty and prompt user to assign sounds later
                buttonSoundMap[soundButton] = string.Empty;
            }
        }

        /// <summary>
        /// Event handler for sound button clicks.
        /// Plays the associated sound if available.
        /// </summary>
        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton == null)
                return;

            string soundFilePath = buttonSoundMap[clickedButton];

            if (string.IsNullOrEmpty(soundFilePath) || !File.Exists(soundFilePath))
            {
                // Prompt user to assign a sound file
                AssignSoundToButton(clickedButton);
            }
            else
            {
                PlaySound(soundFilePath, clickedButton);
            }
        }

        /// <summary>
        /// Prompts the user to select a sound file and assigns it to the button.
        /// </summary>
        /// <param name="button">The button to assign the sound to.</param>
        private void AssignSoundToButton(Button button)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Sound File",
                Filter = "Audio Files|*.wav;*.mp3;*.aiff;*.flac;*.ogg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string selectedFile = openFileDialog.FileName;
                buttonSoundMap[button] = selectedFile;

                // Optionally, change the button's tooltip or content to reflect the assigned sound
                button.ToolTip = Path.GetFileName(selectedFile);
            }
        }

        /// <summary>
        /// Plays the specified sound file using NAudio.
        /// </summary>
        /// <param name="filePath">Path to the audio file.</param>
        /// <param name="button">The button that was clicked.</param>
        private void PlaySound(string filePath, Button button)
        {
            try
            {
                StopSound(); // Stop any currently playing sound

                audioFile = new AudioFileReader(filePath)
                {
                    Volume = globalVolume
                };
                outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFile);
                outputDevice.PlaybackStopped += OnPlaybackStopped;
                outputDevice.Play();

                // Highlight the button to indicate it's playing
                HighlightButton(button);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing sound:\n{ex.Message}", "Playback Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Stops any currently playing sound.
        /// </summary>
        private void StopSound()
        {
            if (outputDevice != null)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            // Remove highlight from all buttons
            foreach (var btn in buttonSoundMap.Keys)
            {
                UnhighlightButton(btn);
            }
        }

        /// <summary>
        /// Event handler for when playback stops.
        /// Removes button highlight.
        /// </summary>
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var btn in buttonSoundMap.Keys)
                {
                    UnhighlightButton(btn);
                }
            });
        }

        /// <summary>
        /// Highlights the button by increasing its opacity.
        /// </summary>
        /// <param name="button">The button to highlight.</param>
        private void HighlightButton(Button button)
        {
            button.Opacity = 1.0;
        }

        /// <summary>
        /// Removes highlight from the button by resetting its opacity.
        /// </summary>
        /// <param name="button">The button to unhighlight.</param>
        private void UnhighlightButton(Button button)
        {
            button.Opacity = 0.85; // Default opacity as per style
        }

        /// <summary>
        /// Event handler for Perform navigation button.
        /// Sets the application to Perform mode.
        /// </summary>
        private void PerformButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform mode is default; no action needed unless toggling modes
            MessageBox.Show("Switched to Perform Mode", "Mode Change", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Event handler for Edit navigation button.
        /// Opens the editor window.
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Edit mode logic
            MessageBox.Show("Edit Mode Clicked", "Mode Change", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Event handler for Options navigation button.
        /// Opens the options/settings window.
        /// </summary>
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Options window
            MessageBox.Show("Options Clicked", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Event handler for Volume slider value changes.
        /// Adjusts the global volume.
        /// </summary>
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            globalVolume = (float)(e.NewValue / 100.0);

            if (audioFile != null)
            {
                audioFile.Volume = globalVolume;
            }
        }

        /// <summary>
        /// Event handler for Effect Knob 1 (Reverb).
        /// Placeholder for future effect implementation.
        /// </summary>
        private void EffectKnob1_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Reverb effect logic
            MessageBox.Show("Reverb Effect Applied", "Effect", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Event handler for Effect Knob 2 (Delay).
        /// Placeholder for future effect implementation.
        /// </summary>
        private void EffectKnob2_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Delay effect logic
            MessageBox.Show("Delay Effect Applied", "Effect", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Event handler for Effect Knob 3 (Pitch Shift).
        /// Placeholder for future effect implementation.
        /// </summary>
        private void EffectKnob3_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Pitch Shift effect logic
            MessageBox.Show("Pitch Shift Effect Applied", "Effect", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Overrides the OnClosing method to ensure resources are disposed.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopSound();
            base.OnClosing(e);
        }
    }
}
