# jpeg_avalonia_wpf
Jpeg 8x8px block demo with both Avalonia and WPF and WriteableBitmap

This primary purpose of this project is to show graphically how fundamentals of the jpeg compression words, Discrete Cosine Transform (DCT), Quantization, Zigzag.

This is implemented in both Avalonia and WPF (.Net) using WriteableBitmap

This project shows some problems in Avalonia

1. Performance problems with multiple TextBlocks in Canvas
2. Grid columns that should be equally sized are not, due to overflowing TextBlocks.

WriteableBitmap in Avalonia has the advantage over WPF that it can be created in worker thread for better performance
(that does not apply to the Textblocks in Canvas though).

Project JpegAvaloniaAsync show worker thread processing of WriteableBitmap.
