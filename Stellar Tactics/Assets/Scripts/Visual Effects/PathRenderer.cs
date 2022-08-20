/* PathRenderer
 * Author:      Brett Loewen
 * Date:        August 19, 2022
 * Purpose:     Use a LineRenderer to display a passed path
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    #region Variables

    private LineRenderer lineRenderer;  //The LineRenderer component used to create the visual effect

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        //Get the LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
    }//end Awake

    #endregion //end Unity Control Methods

    #region Enable/Disable

    /// <summary>
    /// Turn off the PathRenderer
    /// </summary>
    public void Disable()
    {
        //Disable the LineRenderer
        lineRenderer.enabled = false;
    }//end Disable

    /// <summary>
    /// Turn on the PathRenderer
    /// </summary>
    public void Enable()
    {
        //Enable the LineRenderer
        lineRenderer.enabled = true;
    }//end Enable

    #endregion //end Enable/Disable

    #region Configure Display

    /// <summary>
    /// Display the passed path of Tiles
    /// </summary>
    /// <param name="path">The Stack of Tiles which defines the path to display</param>
    public void DisplayPath(Stack<Tile> path)
    {
        //Turn on the PathRenderer
        Enable();

        //Convert the passed Stack of Tiles to an Array of Tiles
        Tile[] tiles = path.ToArray();

        //Set the LineRenderer's position count to have the necessary number of points
        lineRenderer.positionCount = tiles.Length;

        //Loop through the path of Tiles
        for (int i = 0; i < tiles.Length; i++)
        {
            //Add a Tile's position to the LineRenderer
            lineRenderer.SetPosition(i, (tiles[i].transform.position + new Vector3(0f, 0.1f, 0f)));
        }
    }//end DisplayPath

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <param name="arcYCurve"></param>
    /// <param name="samplePoints"></param>
    public void DisplayArc(Vector3 startPosition, Vector3 endPosition, AnimationCurve arcYCurve, int samplePoints)
    {
        //Turn on the PathRenderer
        Enable();

        //Create the start XZ position (Y position will be calculated separately
        Vector3 positionXZ = startPosition;
        positionXZ.y = 0f;
        
        //Create a variable to hold the current Y position
        float positionY;

        //Calculate the total distance (along the ground) from the start position to the end position
        float totalDistance = Vector3.Distance(positionXZ, endPosition);

        //Create a variable to hold the current distance to the target
        float distance;
        
        //Create a variable to hold the total normalized distance to the target (0: just started, 1: reached target)
        float distanceNormalized;

        //Calculate the distance (along the ground) between sample points (as a percentage from 0 to 1, ~0: lots of points, ~1: not many points)
        float segmentLength = 1f / (samplePoints + 1f);

        //Calculate the movement direction for the path
        Vector3 moveDirection = (endPosition - positionXZ).normalized;

        //Set the LineRenderer's position count according to the number of sample points
        lineRenderer.positionCount = samplePoints + 1;

        //Loop to calculate the position of all sample points
        for (int i = 0; i <= samplePoints; i++)
        {
            //Update the XZ position using the move direction and length of a segment
            positionXZ += moveDirection * segmentLength * totalDistance;

            //Calculate the distance (along the ground) between the current position and the end position
            distance = Vector3.Distance(positionXZ, endPosition);

            //Calculate the normalized distance
            distanceNormalized = 1 - (distance / totalDistance);

            //Sample the Y position from the passed animation curve using the normalized distance
            positionY = arcYCurve.Evaluate(distanceNormalized);

            //Use the above calculations to set this point of the LineRenderer
            lineRenderer.SetPosition(i, new Vector3(positionXZ.x, positionY, positionXZ.z));
        }
    }//end DisplayArc

    #endregion //end Configure Display
}
