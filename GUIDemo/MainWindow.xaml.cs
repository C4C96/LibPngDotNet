using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace LibPngDotNet.GUIDemo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		private string? _decodeFilePath;
		public string? DecodeFilePath
		{
			get => _decodeFilePath;
			set
			{
				if (_decodeFilePath == value) return;
				_decodeFilePath = value;
				OnPropertyChanged();
			}
		}

		private string? _encodeFilePath;
		public string? EncodeFilePath
		{
			get => _encodeFilePath;
			set
			{
				if (_encodeFilePath == value) return;
				_encodeFilePath = value;
				OnPropertyChanged();
			}
		}

		public PixelLayout PixelLayout { get; set; } = PixelLayout.Rgb;

		public IEnumerable<PixelLayout> PixelLayouts => _dict.Keys;

		private void DecodeButton_OnClick(object sender, RoutedEventArgs e)
		{
			using var decoder = PngDecoder.Open(DecodeFilePath);
			var width = decoder.Width;
			var height = decoder.Height;
			var pixelBytes = decoder.ReadPixels<byte>(PixelLayout);

			var image = new WriteableBitmap(width, height, 96f, 96f, _dict[PixelLayout], null);
			var rect = new Int32Rect(0, 0, width, height);
			image.WritePixels(rect, pixelBytes, PixelLayout.PixelBits * width / 8, 0);

			Image.Source = image;
		}

		private static readonly Dictionary<PixelLayout, PixelFormat> _dict = new Dictionary<PixelLayout, PixelFormat>
		{
			{ PixelLayout.Gray, PixelFormats.Gray8 },
			{ PixelLayout.Rgb, PixelFormats.Rgb24 },
			{ PixelLayout.Bgr, PixelFormats.Bgr24 },
			{ PixelLayout.Gbra, PixelFormats.Bgra32 },
			{ PixelLayout.Gray16, PixelFormats.Gray16 },
			{ PixelLayout.Rgb16, PixelFormats.Rgb48 },
			{ PixelLayout.Rgba16, PixelFormats.Rgba64 },
		};

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OpenDecodeFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.Multiselect = false;
			dialog.Filter = "png file|*.png";
			if (dialog.ShowDialog(this) == true)
			{
				DecodeFilePath = dialog.FileName;
			}
		}

		private unsafe void EncodeButton_OnClick(object sender, RoutedEventArgs e)
		{
			var image = Image.Source as WriteableBitmap;
			if (image == null)
				return;

			using var encoder = PngEncoder.Open(EncodeFilePath);
			encoder.Width = image.PixelWidth;
			encoder.Height = image.PixelHeight;
			var pixels = new ReadOnlySpan<byte>((byte*)image.BackBuffer, image.BackBufferStride * image.PixelHeight);
			encoder.WriteImage(PixelLayout, pixels);
		}

		private void OpenEncodeFileButton_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog();
			dialog.Filter = "png file|*.png";
			if (dialog.ShowDialog(this) == true)
			{
				EncodeFilePath = dialog.FileName;
			}
		}
	}
}
