# Documentation of the task "03-SFC"

## Author
Pau Azpeitia Bergós

## Command line arguments
-o Output file-name (SVG).
-w Width.
-h Height.
-d Recursion depth
-c Choose Curve (1: "Hilbert", 2: "Dragon", 3: "Sierpinski")
-t Choose color set (1-8)

## Input data
-

## Algorithm
The program consists of three main parts, each with its defined functions (all of which have comments for proper tracking):
  1. Hilbert Curve.
    - I made a classic recursive implemenation based on various documentation / videos i found online. The recursive function does not depend on others. Initially it places the central axis in the middle of its corresponding quadrant and then draws the lines depending on the variable n in its corresponding place. The function calls itself 4 times due the 4 sub-quadrants to be filled in the next iterarion.
  2. Dragon Curve.
    - The Dragon Curve algorithm generates a fractal by recursively dividing a line segment into two smaller segments and rotating the midpoint by 90 degrees to form a right angle. The process starts with a base case: when no more iterations are needed, the segment is drawn directly as a line. Otherwise, the midpoint is calculated and adjusted geometrically to create the characteristic curve, and the function recursively processes the two new segments. With each iteration the number of segments doubles, forming an increasingly detailed fractal. Note that it is actually a failed attempt to recreate the dragon curve, but after several attempts I came up with this heart-shaped fractal curve, so I left it in the program anyway.
  3. Sierpinski Curve.
    - Starting with three points (x0, y0), (x1, y1), and (x2, y2), it calculates the midpoints of the triangle’s edges and creates three smaller triangles. For each smaller triangle, the function calls itself recursively, decreasing the recursion depth (iterations) with each call. When the recursion depth reaches zero, the algorithm stops dividing and renders the triangle using the DrawTriangle function, which in turn uses the DrawLine helper.
  To add variety, the algorithm uses the ColorRandomizer class to assign a random color from a specified palette, ensuring that each curve is uniquely styled. You can try from 1 to 8, my favourite is my dragon curve implementation with palette number 4.
  The three curves do not crashes in all window sizes you try to implement as the same function is used as in the example: int size = Math.Max(Math.Min(o.Width, o.Height) - 10, 5);. However for very small (and nonsense) windows and very large (and nonsense) iterations, at least with my browser I can't see the generated curve.
  For the Hilbert algorithm I don't recommend a dimension higher than 7 (the programme ends anyway but at least my browser can't show the curve). For the Dragon Curve I do not recommend a dimension higher than 20 for the same reason. And for the Sierpinski Curve I would put the limit at 10.

## Extra work / Bonuses
I find the "Dragon Curve" an interestic graphic design and leave it in the algorithm as an extra fractal curve.  
I also invested time in writing everything necessary for colour generation and adapting the code to it.


## Use of AI
I have not used the help of any artificial intelligence.