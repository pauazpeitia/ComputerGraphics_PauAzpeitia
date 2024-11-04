# Documentation of the task "01-AllTheColors"

## Author
Pau Azpeitia

## Command line arguments
--t recives '0' for not running the trivial mode and '1' for running it. (Default 1)
--r recives '0' for not running the random mode and '1' for running it. (Default 1)
--o recives '0' for not running the ornament mode and '1' for running it. (Default 1)
--w to specify the width (Default 4096)
--h to specify the height (Defualt 4096)
--trivialFileName for entering the name of the trivial mode .png file
--randomFileName for entering the name of the random mode .png file
--ornamentFileName for entering the name of the ornament mode .png file


## Input data
This program does not require any input data from the user. 

## Algorithm
For a hight * wight argument less than 2^24 pixels, the program rejects the input (not all colors can fit in the image).
The idea of the ornament mode is to go around the first pixel located in the middle of the image. There are comments in the code so that the algorithm can be understand.

## Extra work / Bonuses
-

## Use of AI
Chat Gpt helped with the implementation of the array and list in the ornament mode, as well as in understanding how to apply the directions (171-177).
