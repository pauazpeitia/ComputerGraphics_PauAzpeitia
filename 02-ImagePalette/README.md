# Documentation of the task "02-ImagePalette"

## Author
Pau Azpeitia Bergós

## Command line arguments
-o Output file-name (SVG), Default '' will return colors in RGB; if '.svg' is provided, it will return colors in that format.
-i Input image file-name.
-c Desired number of colors. From 3 to 10 as specified.

## Input data
The user's input data must be an image supported by ImageSharp (JPEG, PNG, GIF, BMP, TIFF, WebP, ICO); otherwise, the program will throw an exception.

## Algorithm
The program consists of three main parts, each with its defined functions (all of which have comments for proper tracking):
  1. Analyze the image and store the colors in HSV format in a list.
  2. Work with the K-means clustering algorithm to group the hue of the main colors.
  3. Sort the colors and then return them in the specified format (either as RGB text or in a .svg file)

## Extra work / Bonuses
Consider that I haven't found any images that my program cannot support. Additionally, I sort the colors at the end to make the palette view as pleasant as possible.


## Use of AI
I have not used the help of any artificial intelligence.