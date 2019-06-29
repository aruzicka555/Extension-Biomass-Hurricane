﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseHurricane
{
    public struct Line
    {
        public double Length { get; set; }
        public double Angle { get; set; }

        public Line(double len, double angle)
        { this.Length = len; this.Angle = angle; }

    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        { this.X = x; this.Y = y; }

        public override string ToString()
        {
            return $"X: {X:F2}, Y: {Y:F2}";
        }

        public static Point operator +(Point startPt, Line line)
        {
            double deltaX = 0.0; double deltaY = 0.0;
            if(Math.Abs(line.Angle - Math.Truncate(line.Angle / 180.0)) < 0.0000001)
            {
                deltaX = 0.0;
                deltaY = line.Length * Math.Cos(line.Angle * Math.PI / 180.0);
            }
            else if(Math.Abs(line.Angle - Math.Truncate((line.Angle + 90.0) / 180.0)) < 0.0000001)
            {
                deltaX = line.Length * Math.Sin(line.Angle * Math.PI / 180.0);
                deltaY = 0.0;
            }
            else
            {
                deltaX = line.Length * Math.Sin(line.Angle * Math.PI / 180.0);
                deltaY = line.Length * Math.Cos(line.Angle * Math.PI / 180.0);
            }

            double newX = startPt.X + deltaX;
            double newY = startPt.Y + deltaY;
            return new Point(newX, newY);
        }
    }

    /// <summary>
    /// ContinentalGrid is a class representing a cartesian plane with the origin
    /// set at the lower left point of the raster study area. 
    /// </summary>
    public class ContinentalGrid
    {
        public static double metersPerDegreeLat = 111000.0;

        public double CellSize { get; set; }
        public double CenterLatitude {get; set;}
        public int Columns { get; set; }
        public int Rows { get; set; }
        public double StudyAreaWidthMeters { get; set; }
        public double StudyAreaHeightMeters { get; set; }

        public Point CenterPoint { get; private set; }
        public Point CoastNearPoint { get; private set; }

        public double b_coastLine { get; private set; }
        public double b_coastLineLatitude { get; private set; }

        public ContinentalGrid(double centerLatitude, double cellSize, 
            double studyAreaWidthInCells, double studyAreaHeightInCells, 
            double centerPtDistanceInland_kilometers)
        {
            this.CenterLatitude = centerLatitude;
            this.CellSize = cellSize;
            this.Columns = (int) studyAreaWidthInCells;
            this.Rows = (int) studyAreaHeightInCells;
            this.StudyAreaWidthMeters = studyAreaWidthInCells * cellSize;
            this.StudyAreaHeightMeters = studyAreaHeightInCells * cellSize;
            this.CenterPoint = 
                new Point(this.StudyAreaWidthMeters / 2.0, this.StudyAreaHeightMeters / 2.0);
            this.CoastNearPoint = 
                this.CenterPoint + new Line(1000.0 * centerPtDistanceInland_kilometers, 135.0);

            this.b_coastLine = -this.CoastNearPoint.X;
            this.b_coastLineLatitude =
                centerLatitude - (this.CenterPoint.Y - this.b_coastLine) /
                                        metersPerDegreeLat;

            // test coordinate conversion
            //var aPoint = this.siteIndexToCoordinates(20, 20);
            //var convertBack = this.coordinatesToSiteIndex(aPoint);
        }

        /// <summary>
        /// Assumes 0, 0 is at the top left corner and positive row is down.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public Point siteIndexToCoordinates(int column, int row)
        {
            int upRow = this.Rows - row - 1;
            return new Point((double)column * this.CellSize, (double)upRow * this.CellSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns>IList where return[0] is x and return[1] is y.</returns>
        public IList<int> coordinatesToSiteIndex(Point pt)
        {
            double column = (pt.X / this.CellSize) + 0.5 ;
            double row = this.Rows - (pt.Y / this.CellSize) - 1;
            return new List<int>() { (int) column, (int) row };
        }
    }
}
