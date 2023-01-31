using System;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Emgu.CV.Util;
using System.Drawing;
using UnityEngine;

public class ImageRecognitionPipeline
{
   
    // find contours of all red, blue obstacles and boundery in image                           h
    public List<List<Rectangle>> getObstaclePosition(byte[] image)
    {
        var imageDecoded = new Emgu.CV.Mat();
        var imageHsv = new Emgu.CV.Mat();
        var inRange = new Emgu.CV.Mat();
        Image<Gray, Byte> boundImage = new Mat().ToImage<Gray, Byte>();
        var redRange = new Emgu.CV.Mat();
        Image<Gray, Byte> redImage = new Mat().ToImage<Gray, Byte>();
        var blueRange = new Emgu.CV.Mat();
        Image<Gray, Byte> blueImage = new Mat().ToImage<Gray, Byte>();



        CvInvoke.Imdecode(image, Emgu.CV.CvEnum.ImreadModes.Unchanged, imageDecoded);
        var height = imageDecoded.Height;
        var width = imageDecoded.Width;
        CvInvoke.CvtColor(imageDecoded, imageHsv, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

        //detect bounderies
        ScalarArray lowerBound = new ScalarArray(new MCvScalar(3, 106, 0));
        ScalarArray upperBound = new ScalarArray(new MCvScalar(60, 202, 165));

        CvInvoke.InRange(imageHsv, lowerBound, upperBound, inRange);
        CvInvoke.Threshold(inRange, boundImage, 70, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

        //detect red obstacles
        ScalarArray lowerRed = new ScalarArray(new MCvScalar(4, 0, 19));
        ScalarArray upperRed = new ScalarArray(new MCvScalar(136, 234, 205));


        CvInvoke.InRange(imageHsv, lowerRed, upperRed, redRange);
        CvInvoke.Threshold(redRange, redImage, 70, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

        //detect blue obstacles
        ScalarArray lowerBlue = new ScalarArray(new MCvScalar(110, 0, 99));
        ScalarArray upperBlue = new ScalarArray(new MCvScalar(114, 231, 138));


        CvInvoke.InRange(imageHsv, lowerBlue, upperBlue, blueRange);
        CvInvoke.Threshold(blueRange, blueImage, 70, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

        // find contours boundaries
        VectorOfVectorOfPoint boundObstacleContours = new VectorOfVectorOfPoint();
        Mat boundObstaclesHierarchy = new Emgu.CV.Mat();

        CvInvoke.FindContours(boundImage, boundObstacleContours, boundObstaclesHierarchy,
            Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);


        // find contours red obstacles
        VectorOfVectorOfPoint redObstacleContours = new VectorOfVectorOfPoint();
        Mat redObstaclesHierarchy = new Emgu.CV.Mat();

        CvInvoke.FindContours(redImage, redObstacleContours, redObstaclesHierarchy,
            Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);


        // find contours blue obstacles
        VectorOfVectorOfPoint blueObstacleContours = new VectorOfVectorOfPoint();
        Mat blueObstaclesHierarchy = new Emgu.CV.Mat();

        CvInvoke.FindContours(blueImage, blueObstacleContours, blueObstaclesHierarchy,
            Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);



        // calculate area and remove small elements (potential wrong detected areas)

        // bounderies
        Point[][] boundObstacleContoursArray = boundObstacleContours.ToArrayOfArray();

        List<Rectangle> bounderies = new List<Rectangle>();

        foreach (var contour in boundObstacleContoursArray) {
           double area = CvInvoke.ContourArea(new VectorOfPoint(contour));
           if (area > 10)
            {
                Rectangle boundingBox = CvInvoke.BoundingRectangle(contour);
                bounderies.Add(boundingBox);
            } 
        }

        // blue Obstacle rectangles

        Point[][] blueObstacleContoursArray = blueObstacleContours.ToArrayOfArray();

        List<Rectangle> blueObstacles = new List<Rectangle>();

        foreach (var contour in blueObstacleContoursArray)
        {
            double area = CvInvoke.ContourArea(new VectorOfPoint(contour));
            if (area > 10)
            {
                Rectangle boundingBox = CvInvoke.BoundingRectangle(contour);
                blueObstacles.Add(boundingBox);
            }
        }

        //red obstacles rectangles
        Point[][] redObstacleContoursArray = blueObstacleContours.ToArrayOfArray();

        List<Rectangle> redObstacles = new List<Rectangle>();

        foreach (var contour in redObstacleContoursArray)
        {
            double area = CvInvoke.ContourArea(new VectorOfPoint(contour));
            if (area > 10)
            {
                Rectangle boundingBox = CvInvoke.BoundingRectangle(contour);
                redObstacles.Add(boundingBox);
            }
        }

        // order important: [redObstacles, blueObstacles, bounderies]
        List<List<Rectangle>> allContours = new List<List<Rectangle>>();
        allContours.Add(redObstacles);
        allContours.Add(blueObstacles);
        allContours.Add(bounderies);

        return allContours;

    }

    public List<List<Vector3>> GetCooridnates10ClosestObstacles(Vector3 position, byte[] image)
    {
        // versuche die 5 nächsten Roten, Gelben und Blauen auf y achse zu return (was er auf bild sieht geradeaus müsste y entfernung sein)
        //überlegung: nach height objeckte sortieren, weil je größer desto näher dran
        var allContours = this.getObstaclePosition(image);
        List<Vector3> obstaclePositions = new List<Vector3>();
        List<List<Vector3>> obstaclesSorted = new List<List<Vector3>>();

        // sorts by Y ascending
        foreach( List<Rectangle> obs in allContours )
        {
            obs.Sort((x, y) => x.Y.CompareTo(y.Y));
            List<Vector3> vectorList = new List<Vector3>();
            // create Vector from Rectangle object
            foreach(Rectangle rect in obs)
            {
                Vector3 vec = new Vector3();
                vec.x = rect.X;
                vec.y = rect.Y;
                vec.z = rect.Width;
                vectorList.Add(vec);
            }

            for (int i = 0; i < 10; i++)
            {
                if (vectorList.Count < 10)
                {
                    Vector3 defaultVec = new Vector3() { x = -99, y = -99, z = -99 };
                    vectorList.Add(defaultVec);
                }
            }

            obstaclesSorted.Add(vectorList.GetRange(0, 10));
        }

        //fill until list is length 10 with default argument



        return obstaclesSorted;
        

    }

    public void saveImageToPath(byte[] image, String filepath = "test.png")
    {
        var image1 = new Emgu.CV.Mat();
        CvInvoke.Imdecode(image, Emgu.CV.CvEnum.ImreadModes.Unchanged, image1);
        image1.Save(filepath);

    }

}


// contours python script: bluecontours =  list 10 of shape [n,1,2] arrays i.e. [2,1,2] -> [[[276  47]],, [[276  51]]]