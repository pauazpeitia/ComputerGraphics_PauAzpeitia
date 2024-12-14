# Documentation of the task "06-ImageRecoloring"

## Author
Pau Azpeitia Bergos

## Command line arguments
i: Direccion of the image to be modified
o: Name of the modified image
h: Specify by how many degrees (hsv) to change the Hue of pixels not detected as skin.

## Input data
The user's input data must be an image supported by ImageSharp (JPEG, PNG, GIF, BMP, TIFF, WebP, ICO); otherwise, the program will throw an exception.

## Algorithm
The program first ensures that the arguments it is working with are valid (input and output names). From there with two for loops it detects each pixel of the image (x,y) and with the function RgbToHsv it changes from rgb to Hsv format. After that it detects with the function IsSkinTone if the pixel has the normal human skin colors. Note that the parameters are impossible to make exact as they often depend also on the sharpness and dynamic range of the image. If it is not detected as skin color, the value of the Hue argument is moved by plus the Hue argument value (making sure that it is in the range 0-360). If it is detected as skin color, nothing happens in that iteration. After this change it is converted again from Hsv to Rgb with the function HsvToRgb, and the image is saved.

## Extra work / Bonuses
-

## Use of AI
I used ChatGPT to ask and try the correct parameters for the function IsSkinTone(Hsv hsv).
