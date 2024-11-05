# Documentation of the task "01-AllTheColors"

## Author
Pau Azpeitia

## Command line arguments
--o for entering the name of the .png file.
--m for choosing the mode of the program (1 for trivial, 2 for random, 3 for ornament)
--w to specify the width (Default 4096)
--h to specify the height (Defualt 4096)

## Input data
This program does not require any input data from the user. 

## Algorithm
For a height * widht argument less than 2^24 pixels, the program rejects the input (not all colors can fit in the image).
For a height or widht argument greater than 32768, the program rejects the input (ImageSharp has limit buffer of 4294967296, InvalidMemoryOperationException).
The idea of the ornament mode is to go around the first pixel located in the middle of the image. There are comments in the code so that the algorithm can be understand.

## Extra work / Bonuses
-

## Use of AI
Chat Gpt helped with the implementation of the array and list in the ornament mode, as well as in understanding how to apply the directions (171-177).
