/*
MIT License

Copyright (c) 2022 Rafael Bordoni

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

///////////////////////////////////////////////////////////////////////////////
//                                                                           //
// Planar Reflections Probe for Unity                                        //
//                                                                           //
// Author: Rafael Bordoni                                                    //
// Date: January 25, 2022                                                    //
// Last Update: April 14, 2023                                               //
// Email: rafaelbordoni00@gmail.com                                          //
// Repository: https://github.com/eldskald/planar-reflections-unity          //
//                                                                           //
///////////////////////////////////////////////////////////////////////////////

// https://www.youtube.com/watch?v=w84-l3IEhXM

namespace Environment.PixelWater
{
    using UnityEngine;
    public static class PlanarReflectionProbe
    {
        public static Vector3 GetPosition(Vector3 cameraPosition, Vector3 planePosition, Vector3 planeNormal)
        {
            // Debug.Log("Calcualating probe position with camera: " + cameraPosition + " plane " + planePosition + "normal" + planeNormal);
            var cameraToPlane = planeNormal * Vector3.Dot(planeNormal, cameraPosition - planePosition);
            return cameraPosition - 2 * cameraToPlane;
        }

        public static Matrix4x4 GetObliqueProjection(Camera probeCamera, Vector3 planePosition, Vector3 planeNormal)
        {
            var viewMatrix = probeCamera.worldToCameraMatrix;
            var viewPosition = viewMatrix.MultiplyPoint(planePosition);
            var viewNormal = viewMatrix.MultiplyVector(planeNormal);
            var plane = new Vector4(viewNormal.x,
                                    viewNormal.y,
                                    viewNormal.z,
                                    -Vector3.Dot(viewPosition, viewNormal));
            return probeCamera.CalculateObliqueMatrix(plane);
        }
    }
}