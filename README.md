# LibPngDotNet

`LibPngDotNet` is a .net png library based on [libpng](https://github.com/glennrp/libpng). It can encode and decode images in png format.

## Features

* Decode from all png supported bit depth (1, 2, 4, 8, 16) and color types (gray scale, gray scale with alpha, rgb, rgba, palette)
* Decode pixels to custom struct, even if the struct is not the same as the one stored in png file. 
* Encode custom pixel struct into the corresponding png bit depth (8, 16) and color types  (gray scale, gray scale with alpha, rgb, rgba).
* Custom pixel struct supports these layout with 8 or 16 bits per channel:
    * Gray Scale
    * GA, AG (gray scale with alpha)
    * RGB, BGR
    * RGBA, ARGB, BGRA, ABGR
* 2 release versions:
    * .Net5: higher performance based on `System.Span`
    * .NetFrameWork4.8: better compatibility, such as can be used in Unity projects 


## Quick Start

### Decode

```c#
// Get PngDecoder instance.
using var decoder = PngDecoder.Open("File Path");

// Access Png properties.
var width = decoder.Width;
var colorType = decoder.ColorType;

// Set the decoding settings to what you need.
decoder.Settings.InvertY = true;

// Read pixels into your custom struct.
var pixels = decoder.ReadPixels<Rgba>();
```

### Encode

```c#
// Get PngEncoder instance.
using var encoder = PngEncoder.Open("File Path");

// Set the encoding settings to what you need.
encoder.Settings.InvertY = true;

// Write image content.
// Png format and depth will depend on your pixel struct.
encoder.WriteImage(width, height, pixels);
```

## Custom Pixel Struct

Currently, `LibPngDotNet` has not provided any struct for describing the pixel layout. Therefore, you have to use your custom pixel struct like this:

```c#
[PngPixel(nameof(PixelLayout.Rgb))]
public struct Rgb
{
    public byte R;
    public byte G;
    public byte B;
}
```

`PngPixelAttribute` describe the pixel layout info about your pixel struct, like channel count, channel order, bit depths.

If it is inconvenient to add `PngPixelAttribute` on the pixel struct, you can pass a `PixelLayout` argument in `PngDecoder.ReadPixels` or `PngEncoder.WriteImage`:

```c#
// Decode to a byte array with RGBA format
var bytes = decoder.ReadPixels<byte>(PixelLayout.Rgba);
```



