# Documentation of the task "05-Animation"

## Author
Pau Azpeitia Bergós

## Command line arguments
--h and --w to define height and widht

## Input data
-

## Algorithm
I took my Mandala-Code and saved images as Frames for every modification for the construction of the original Mandala. The code assambles this frames with the SaveFrame function to a Folder called Frames. Then we can use this frames to create a video with the Ffmpeg utility. Here is an example https://youtube.com/shorts/qIp6NGo1A9w?feature=share. For this example I used : "ffmpeg -framerate 4 -i out%04d.png -f avi -vcodec msmpeg4v2 -q:v 2 -y out.avi"


## Extra work / Bonuses
-

## Use of AI
I have not used the help of any artificial intelligence.