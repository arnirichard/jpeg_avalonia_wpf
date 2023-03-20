# jpeg_avalonia_wpf
Jpeg 8x8px block compression demo with both Avalonia and WPF and WriteableBitmap.
The Encoder/Decoder works for Baseline Jpeg with chroma subsampling (to Bitmap) but only common formats are supported yet.

This purpose of this project:

1. Show graphically how fundamentals of the jpeg compression works: Discrete Cosine Transform (DCT), Quantization, Zigzag, Progressive
2. Write Jpeg decoder (basline/progressive/chrome subsampling) and encoder (baseline only)

The graphical part is implemented in both Avalonia and WPF (.Net) using WriteableBitmap

This project shows some problems in Avalonia

1. Performance problems with multiple TextBlocks in Canvas
2. Grid columns that should be equally sized are not, due to overflowing TextBlocks. This can cause Grid columns to resize infinitely as demonstrated in the project JpegAvaloniaAsync.

WriteableBitmap in Avalonia has the advantage over WPF that it can be created in worker thread for better performance
(that does not apply to the Textblocks in Canvas though).

Project JpegAvaloniaAsync shows worker thread (async) processing of WriteableBitmap.

![Alt text](screenshot.png?raw=true)
![Alt text](screenshot_progressive.png?raw=true)

If you want to learn more about the Jpeg format I recommend Daniel Harding's series
https://www.youtube.com/watch?v=CPT4FSkFUgs&ab_channel=DanielHarding
Some of the code was ported from his repo (C++)
https://github.com/dannye/jed
