# Documentation of the task "03-SFC"

## Author
Pau Azpeitia Bergós

## Command line arguments
-o: output file name
-w: width
-h: height
-d: Recursion depth
-c: "Choose curve (1(Hilbert Curve),2(Hearth Curve),3(Sierpinski Curve),4(Dragon Curve))."
-t: Choose color set (try between 1 and 8)
## Input data
-

## Algorithm

Hilbert Curve: 
    The HilbertFunction function generates a Hilbert curve using the SVG format using a recursive approach. The process starts with the creation of a group <g> within the SVG document, to which a transformation is applied to move it to a specific location (using the parameters cx and cy for translation) and rotate it by the specified angle. Then, it is determined whether the curve should be split into recursive subcurves. If the order of the curve is greater than 1, if the order of the curve is greater than 1, the function is called recursively four times, each time with a reduced size (size / 2) and a different angle, to create the subcurves that form the Hilbert.The recursion is organized so that each subcurve is generated in a different position, forming a fractal pattern. As the function reaches a base level of recursion (when the order is 1 or less), it begins to generate the lines that make up the Hilbert curve. These lines are represented by <line> elements within the SVG. The coordinates of each line are calculated in relation to the size of the curve, and the lines are drawn in different positions and orientations, forming part of the fractal figure.
Hearth Curve:
    The HearthFunction generates a heart-shaped fractal figure .The generation is done recursively, dividing each line segment into two parts and modifying their orientation, gradually creating a heart-shaped pattern. 
    If the number of iterations reaches zero, the function generates a line between two points defined by the coordinates x0, y0 and x1, y1, creating a <line> element in the SVG document. Each line is assigned attributes such as start and end coordinates (x1, y1, x2, y2), randomly generated color and line thickness. 
    This line is added to the group of SVG elements passed as a parameter.
    If the number of iterations is greater than zero, the function calculates the intermediate points (midX, midY) to divide the line segment in two. Then, the function is called recursively to generate two new line segments between the original points and the intermediate points, reducing the number of iterations in each call. This recursive process creates the fractal pattern, where each subdivision becomes a more detailed part of the heart-shaped figure. Thus, the function continues to divide the segments and generate more lines until it reaches the base level of iteration, completing the design of the heart in the SVG.
Dragon Curve:
    The sequence of the dragon curve is generated with the GenerateDragonSequence(depth) function. This sequence contains the turns that the curve will follow: 'R' for a 90 degree turn to the right and 'L' for a 90 degree turn to the left.
    For each turn: The direction in which the line should be drawn is calculated using the current angle.
    Calculate the distance the line should extend (using scale). The DrawLineD function is used to draw a line from the current coordinates (x, y) to the newly calculated coordinates (x + dx, y + dy). After each line, the values of the minimum and maximum coordinates (minX, maxX, minY, maxY) are updated to determine the limits of the area occupied by the curve. Once all the lines have been drawn, the necessary offsets are calculated to center the figure in the visible area of the SVG. The offsets (offsetX and offsetY) are calculated taking into account the size of the curve and the available area in the SVG. Finally, a transformation is applied to the group of SVG elements to move the curve to the appropriate position within the visible area.
Sierpinski Curve:
    When the iterations reach 0, the function draws a triangle using the three points (x0, y0), (x1, y1), and (x2, y2) provided. The triangle is drawn using the DrawTriangle function, which probably creates a <polygon> element in the SVG to represent it. If the iterations have not reached 0, the function performs a subdivision of the triangle. It calculates the midpoints of the three sides of the original triangle:
        midX01 and midY01: the midpoint between the vertices (x0, y0) and (x1, y1).
        midX12 and midY12: the midpoint between the vertices (x1, y1) and (x2, y2).
        midX20 and midY20: the midpoint between the vertices (x2, y2) and (x0, y0).
    After calculating the midpoints, the function makes three recursive calls to itself, each with a triangle formed by the calculated midpoints and an original vertex:
        A triangle with vertices (x0, y0), (midX01, midY01), and (midX20, midY20).
        A triangle with vertices (midX01, midY01), (x1, y1), and (midX12, midY12).
        A triangle with vertices (midX20, midY20), (midX12, midY12), and (x2, y2).
    This process of subdividing triangles into three subtriangles is recursively repeated for the specified number of iterations. At each level of recursion, the triangles are subdivided into three smaller parts, generating the characteristic Sierpinski curve pattern.

In all functions, a ColorRandomizer object is created that generates random colors. I want to make it clear that the Hearth Curve was a failed attempt to develop the Dragon Curve, but I have left it in the code because of its curious appearance. If it does not meet the properties or is not really a SFC I understand that it is not valued.

## Extra work / Bonuses
-

## Use of AI
I have not used the help of any artificial intelligence.